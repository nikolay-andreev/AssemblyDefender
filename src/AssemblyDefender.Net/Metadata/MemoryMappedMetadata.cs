using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	public class MemoryMappedMetadata : IMetadata
	{
		#region Fields

		private string _frameworkVersionMoniker;
		private bool _isOptimized;
		private byte _tableSchemaMajorVersion = 2;
		private byte _tableSchemaMinorVersion = 0;
		private long _offset;
		private int _stringsOffset;
		private int _stringsSize;
		private int _userStringsOffset;
		private int _userStringsSize;
		private int _guidsOffset;
		private int _guidsSize;
		private int _blobsOffset;
		private int _blobsSize;
		private ulong _sortTableMask;
		private bool _stringHeapOffsetSize4;
		private bool _guidHeapOffsetSize4;
		private bool _blobHeapOffsetSize4;
		private bool[] _tableRowIndexSize4;
		private bool[] _codedTokenDataSize4;
		private TableInfo[] _tables;
		private IBinaryAccessor _accessor;

		#endregion

		#region Ctors

		public MemoryMappedMetadata(IBinaryAccessor accessor)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			_accessor = accessor;
			Read();
		}

		#endregion

		#region Properties

		public string FrameworkVersionMoniker
		{
			get { return _frameworkVersionMoniker; }
		}

		public bool IsOptimized
		{
			get { return _isOptimized; }
		}

		public byte TableSchemaMajorVersion
		{
			get { return _tableSchemaMajorVersion; }
		}

		public byte TableSchemaMinorVersion
		{
			get { return _tableSchemaMinorVersion; }
		}

		#endregion

		#region Methods

		public bool IsTokenExists(int token)
		{
			int type = MetadataToken.GetType(token);
			int tableType = MetadataToken.GetTableTypeByTokenType(type);
			if (tableType < 0 || tableType >= MetadataConstants.TableCount)
				return false;

			int rowCount = _tables[tableType].RowCount;

			int rid = MetadataToken.GetRID(token);
			if (rid < 1 || rid > rowCount)
				return false;

			return true;
		}

		public string GetString(int id)
		{
			if (id < 0 || id >= _stringsSize)
				throw new ArgumentOutOfRangeException("id");

			if (id == 0)
				return null;

			_accessor.Position = _offset + _stringsOffset + id;
			return _accessor.ReadNullTerminatedString();
		}

		public string GetUserString(int id)
		{
			if (id < 0 || id >= _userStringsSize)
				throw new ArgumentOutOfRangeException("id");

			if (id == 0)
				return string.Empty;

			_accessor.Position = _offset + _userStringsOffset + id;

			int length = ReadCompressedInteger();

			// There is an additional terminal byte.
			if (length < 1)
				return string.Empty;

			byte[] buffer = _accessor.ReadBytes(length - 1);
			return Encoding.Unicode.GetString(buffer);
		}

		public Guid GetGuid(int id)
		{
			if (id == 0)
				return Guid.Empty;

			int startPos = id - 1;
			if (startPos < 0 || startPos + 16 > _guidsSize)
				throw new ArgumentOutOfRangeException("id");

			_accessor.Position = _offset + _guidsOffset + startPos;

			var buffer = _accessor.ReadBytes(16);
			return new Guid(buffer);
		}

		public byte[] GetBlob(int id)
		{
			if (id < 0 || id >= _blobsSize)
				throw new ArgumentOutOfRangeException("id");

			if (id == 0)
				return BufferUtils.EmptyArray;

			_accessor.Position = _offset + _blobsOffset + id;

			int length = ReadCompressedInteger();
			if (length == 0)
				return BufferUtils.EmptyArray;

			return _accessor.ReadBytes(length);
		}

		public byte[] GetBlob(int id, int offset, int size)
		{
			if (id < 0 || id >= _blobsSize)
				throw new ArgumentOutOfRangeException("id");

			if (id == 0)
				return BufferUtils.EmptyArray;

			_accessor.Position = _offset + _blobsOffset + id;

			int length = ReadCompressedInteger();
			if (length < offset + size)
				throw new ArgumentOutOfRangeException("size");

			if (size == 0)
				return BufferUtils.EmptyArray;

			_accessor.Position += offset;

			return _accessor.ReadBytes(size);
		}

		public byte GetBlobByte(int id, int offset)
		{
			if (id < 0 || id >= _blobsSize)
				throw new ArgumentOutOfRangeException("id");

			if (id == 0)
			{
				if (offset != 0)
					throw new ArgumentOutOfRangeException("offset");

				return 0;
			}

			_accessor.Position = _offset + _blobsOffset + id + offset;
			return _accessor.ReadByte();
		}

		public IBinaryAccessor OpenBlob(int id)
		{
			if (id < 0 || id >= _blobsSize)
				throw new ArgumentOutOfRangeException("id");

			if (id == 0)
				return new BlobAccessor();

			_accessor.Position = _offset + _blobsOffset + id;

			int length = ReadCompressedInteger();
			if (length == 0)
				return new BlobAccessor();

			return _accessor.Map(_accessor.Position, length);
		}

		private void Read()
		{
			_offset = _accessor.Position;

			// Signature
			uint signature = _accessor.ReadUInt32();
			if (signature != MetadataConstants.MetadataHeaderSignature)
			{
				throw new BadImageFormatException(SR.MetadataHeaderNotValid);
			}

			// Major version, 2 bytes (ignore on read)
			// Minor version, 2 bytes (ignore on read)
			// Reserved, always 0, 4 bytes
			_accessor.Position += 8;

			ReadVersionString();

			// Reserved, 2 bytes
			_accessor.Position += 2;

			ReadStreams();
		}

		private void ReadVersionString()
		{
			int versionLength = _accessor.ReadInt32();
			if (versionLength == 0)
				return;

			byte[] buffer = _accessor.ReadBytes(versionLength);
			int count = 0;
			for (int i = 0; i < buffer.Length; i++)
			{
				if (buffer[i] == 0)
					break;

				count++;
			}

			_frameworkVersionMoniker = Encoding.UTF8.GetString(buffer, 0, count);
		}

		private void ReadStreams()
		{
			int numberOfStream = _accessor.ReadUInt16();

			int[] offsets = new int[numberOfStream];
			int[] sizes = new int[numberOfStream];
			string[] names = new string[numberOfStream];

			for (int i = 0; i < numberOfStream; i++)
			{
				offsets[i] = _accessor.ReadInt32();
				sizes[i] = _accessor.ReadInt32();

				// Name of the stream; a zero-terminated ASCII string no longer than 31 characters (plus zero terminator).
				// The name might be shorter, in which case the size of the stream header is correspondingly reduced,
				// padded to the 4-byte boundary.
				long startPos = _accessor.Position;
				names[i] = _accessor.ReadNullTerminatedString(Encoding.ASCII);
				_accessor.Align(startPos, 4);
			}

			for (int i = 0; i < numberOfStream; i++)
			{
				int offset = offsets[i];
				int size = sizes[i];
				string name = names[i];

				if (name == MetadataConstants.StreamTable)
				{
					_isOptimized = true;
					_accessor.Position = offset + _offset;
					ReadTables();
				}
				else if (name == MetadataConstants.StreamTableUnoptimized)
				{
					_isOptimized = false;
					_accessor.Position = offset + _offset;
					ReadTables();
				}
				else if (name == MetadataConstants.StreamStrings)
				{
					_stringsOffset = offset;
					_stringsSize = size;
				}
				else if (name == MetadataConstants.StreamUserStrings)
				{
					_userStringsOffset = offset;
					_userStringsSize = size;
				}
				else if (name == MetadataConstants.StreamGuid)
				{
					_guidsOffset = offset;
					_guidsSize = size;
				}
				else if (name == MetadataConstants.StreamBlob)
				{
					_blobsOffset = offset;
					_blobsSize = size;
				}
			}
		}

		private void ReadTables()
		{
			_tables = new TableInfo[MetadataConstants.TableCount];
			_tableRowIndexSize4 = new bool[MetadataConstants.TableCount];

			// Reserved, 4 bytes
			_accessor.ReadInt32();

			_tableSchemaMajorVersion = _accessor.ReadByte();
			_tableSchemaMinorVersion = _accessor.ReadByte();

			byte heapFlags = _accessor.ReadByte();

			// Reserved, 1 bytes
			_accessor.ReadByte();

			ulong maskValid = _accessor.ReadUInt64();
			_sortTableMask = _accessor.ReadUInt64();

			// Row counts
			int[] rowCounts = new int[MetadataConstants.TableCount];
			for (int tableIndex = 0; tableIndex < MetadataConstants.TableCount; tableIndex++)
			{
				if ((maskValid & (1UL << tableIndex)) != 0)
				{
					int rowCount = _accessor.ReadInt32();
					rowCounts[tableIndex] = rowCount;
					_tableRowIndexSize4[tableIndex] = (rowCount >= (1 << 16));
				}
			}

			// Heap offset sizes.
			_stringHeapOffsetSize4 = ((heapFlags & HeapOffsetFlags.StringHeap4) == HeapOffsetFlags.StringHeap4);
			_guidHeapOffsetSize4 = ((heapFlags & HeapOffsetFlags.GuidHeap4) == HeapOffsetFlags.GuidHeap4);
			_blobHeapOffsetSize4 = ((heapFlags & HeapOffsetFlags.BlobHeap4) == HeapOffsetFlags.BlobHeap4);

			// Coded token size.
			_codedTokenDataSize4 = new bool[MetadataConstants.CodedTokenCount];
			for (int i = 0; i < MetadataConstants.CodedTokenCount; i++)
			{
				int maxRowCount = 0;
				int codedTokenType = i + CodedTokenType.TypeDefOrRef;
				var token = CodedTokenInfo.Get(codedTokenType);

				for (int j = 0; j < token.TokenTypes.Length; j++)
				{
					int tableType = MetadataToken.GetTableTypeByTokenType(token.TokenTypes[j]);

					int rowCount = rowCounts[tableType];
					if (maxRowCount < rowCount)
						maxRowCount = rowCount;
				}

				_codedTokenDataSize4[i] = (maxRowCount >= (1 << (16 - token.Tag)));
			}

			int stringHeapOffsetSize = (_stringHeapOffsetSize4 ? 4 : 2);
			int guidHeapOffsetSize = (_guidHeapOffsetSize4 ? 4 : 2);
			int blobHeapOffsetSize = (_blobHeapOffsetSize4 ? 4 : 2);

			// Create tables
			var tableSchemas = MetadataSchema.Tables;
			int offset = (int)(_accessor.Position - _offset);
			for (int tableIndex = 0; tableIndex < MetadataConstants.TableCount; tableIndex++)
			{
				var tableSchema = tableSchemas[tableIndex];
				var columnSchemas = tableSchema.Columns;

				int rowCount = rowCounts[tableIndex];

				var columns = new ColumnInfo[columnSchemas.Length];
				int columnOffset = 0;
				for (int columnIndex = 0; columnIndex < columnSchemas.Length; columnIndex++)
				{
					int columnSize;
					int columnPadding = 0;
					var columnSchema = columnSchemas[columnIndex];
					if (columnSchema.IsTableIndex)
					{
						columnSize = (_tableRowIndexSize4[columnSchema.Type] ? 4 : 2);
					}
					else if (columnSchema.IsCodedToken)
					{
						columnSize = (_codedTokenDataSize4[columnSchema.Type - 64] ? 4 : 2);
					}
					else
					{
						switch (columnSchema.Type)
						{
							case MetadataColumnType.Int16:
							case MetadataColumnType.UInt16:
								columnSize = 2;
								break;

							case MetadataColumnType.Int32:
							case MetadataColumnType.UInt32:
								columnSize = 4;
								break;

							case MetadataColumnType.Byte2:
								columnSize = 1;
								columnPadding = 1;
								break;

							case MetadataColumnType.String:
								columnSize = stringHeapOffsetSize;
								break;

							case MetadataColumnType.Guid:
								columnSize = guidHeapOffsetSize;
								break;

							case MetadataColumnType.Blob:
								columnSize = blobHeapOffsetSize;
								break;

							default:
								throw new InvalidOperationException();
						}
					}

					columns[columnIndex] = new ColumnInfo()
					{
						Offset = (short)columnOffset,
						Size = (short)columnSize,
					};

					columnOffset += (columnSize + columnPadding);
				}

				int rowSize = columnOffset;

				_tables[tableIndex] = new TableInfo()
				{
					Offset = offset,
					RowCount = rowCount,
					RowSize = (short)rowSize,
					Columns = columns,
				};

				offset += (rowSize * rowCount);
			}
		}

		private int ReadValue(bool size4)
		{
			if (size4)
				return _accessor.ReadInt32();
			else
				return _accessor.ReadUInt16();
		}

		private int ReadCompressedInteger()
		{
			int result = 0;
			byte b = _accessor.ReadByte();
			if ((b & 0x80) == 0)
			{
				// 1 byte
				result = b;
			}
			else if ((b & 0x40) == 0)
			{
				// 2 byte
				result = (b & ~0x80) << 8;
				result |= _accessor.ReadByte();
			}
			else
			{
				// 4 byte
				result = (b & ~0xc0) << 24;
				result |= _accessor.ReadByte() << 16;
				result |= _accessor.ReadByte() << 8;
				result |= _accessor.ReadByte();
			}

			return result;
		}

		#endregion

		#region Tables

		public bool IsTableSorted(int tableType)
		{
			return ((_sortTableMask & (1UL << tableType)) != 0);
		}

		public int GetTableRowCount(int tableType)
		{
			return _tables[tableType].RowCount;
		}

		public int GetTableValue(int tableType, int rid, int columnID)
		{
			var table = _tables[tableType];
			var column = table.Columns[columnID];
			_accessor.Position = _offset + table.Offset + (table.RowSize * (rid - 1)) + column.Offset;

			if (column.Size == 4)
				return _accessor.ReadInt32();
			else if (column.Size == 2)
				return _accessor.ReadUInt16();
			else
				return _accessor.ReadByte();
		}

		public void GetTableValues(int tableType, int rid, int columnID, int count, int[] values)
		{
			var table = _tables[tableType];
			var column = table.Columns[columnID];

			int rowSize = table.RowSize;
			bool size4 = (column.Size == 4 ? true : false);
			long basePosition = _offset + table.Offset + column.Offset;

			for (int i = 0, j = rid - 1; i < count; i++, j++)
			{
				_accessor.Position = basePosition + (j * rowSize);
				values[i] = size4 ? _accessor.ReadInt32() : _accessor.ReadUInt16();
			}
		}

		public void UnloadTable(int tableType)
		{
		}

		public void GetAssembly(int rid, out AssemblyRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.Assembly, rid);
			row.HashAlgId = (HashAlgorithm)_accessor.ReadInt32();
			row.MajorVersion = (int)_accessor.ReadUInt16();
			row.MinorVersion = (int)_accessor.ReadUInt16();
			row.BuildNumber = (int)_accessor.ReadUInt16();
			row.RevisionNumber = (int)_accessor.ReadUInt16();
			row.Flags = _accessor.ReadInt32();
			row.PublicKey = ReadValue(_blobHeapOffsetSize4);
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Locale = ReadValue(_stringHeapOffsetSize4);
		}

		public void GetAssemblyOS(int rid, out AssemblyOSRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.AssemblyOS, rid);
			row.OSPlatformId = (uint)_accessor.ReadInt32();
			row.OSMajorVersion = (uint)_accessor.ReadInt32();
			row.OSMinorVersion = (uint)_accessor.ReadInt32();
		}

		public void GetAssemblyProcessor(int rid, out AssemblyProcessorRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.AssemblyProcessor, rid);
			row.Processor = (uint)_accessor.ReadInt32();
		}

		public void GetAssemblyRef(int rid, out AssemblyRefRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.AssemblyRef, rid);
			row.MajorVersion = (int)_accessor.ReadUInt16();
			row.MinorVersion = (int)_accessor.ReadUInt16();
			row.BuildNumber = (int)_accessor.ReadUInt16();
			row.RevisionNumber = (int)_accessor.ReadUInt16();
			row.Flags = _accessor.ReadInt32();
			row.PublicKeyOrToken = ReadValue(_blobHeapOffsetSize4);
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Locale = ReadValue(_stringHeapOffsetSize4);
			row.HashValue = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetAssemblyRefOS(int rid, out AssemblyRefOSRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.AssemblyRefOS, rid);
			row.OSPlatformId = (uint)_accessor.ReadInt32();
			row.OSMajorVersion = (uint)_accessor.ReadInt32();
			row.OSMinorVersion = (uint)_accessor.ReadInt32();
			row.AssemblyRef = ReadValue(_tableRowIndexSize4[MetadataTableType.AssemblyRef]);
		}

		public void GetAssemblyRefProcessor(int rid, out AssemblyRefProcessorRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.AssemblyRefProcessor, rid);
			row.Processor = (uint)_accessor.ReadInt32();
			row.AssemblyRef = ReadValue(_tableRowIndexSize4[MetadataTableType.AssemblyRef]);
		}

		public void GetClassLayout(int rid, out ClassLayoutRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.ClassLayout, rid);
			row.PackingSize = (int)_accessor.ReadUInt16();
			row.ClassSize = _accessor.ReadInt32();
			row.Parent = ReadValue(_tableRowIndexSize4[MetadataTableType.TypeDef]);
		}

		public void GetConstant(int rid, out ConstantRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.Constant, rid);
			row.Type = (ConstantTableType)_accessor.ReadByte();
			_accessor.Position++; // 1 byte padding
			row.Parent = ReadValue(_codedTokenDataSize4[1]);
			row.Value = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetCustomAttribute(int rid, out CustomAttributeRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.CustomAttribute, rid);
			row.Parent = ReadValue(_codedTokenDataSize4[2]);
			row.Type = ReadValue(_codedTokenDataSize4[10]);
			row.Value = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetDeclSecurity(int rid, out DeclSecurityRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.DeclSecurity, rid);
			row.Action = (SecurityAction)_accessor.ReadUInt16();
			row.Parent = ReadValue(_codedTokenDataSize4[4]);
			row.PermissionSet = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetENCLog(int rid, out ENCLogRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.ENCLog, rid);
			row.Token = (uint)_accessor.ReadInt32();
			row.FuncCode = (uint)_accessor.ReadInt32();
		}

		public void GetENCMap(int rid, out ENCMapRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.ENCMap, rid);
			row.Token = (uint)_accessor.ReadInt32();
		}

		public void GetEvent(int rid, out EventRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.Event, rid);
			row.Flags = _accessor.ReadUInt16();
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.EventType = ReadValue(_codedTokenDataSize4[0]);
		}

		public void GetEventMap(int rid, out EventMapRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.EventMap, rid);
			row.Parent = ReadValue(_tableRowIndexSize4[MetadataTableType.TypeDef]);
			row.EventList = ReadValue(_tableRowIndexSize4[MetadataTableType.Event]);
		}

		public void GetEventPtr(int rid, out int value)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.EventPtr, rid);
			value = ReadValue(_tableRowIndexSize4[MetadataTableType.Event]);
		}

		public void GetExportedType(int rid, out ExportedTypeRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.ExportedType, rid);
			row.Flags = _accessor.ReadInt32();
			row.TypeDefId = _accessor.ReadInt32();
			row.TypeName = ReadValue(_stringHeapOffsetSize4);
			row.TypeNamespace = ReadValue(_stringHeapOffsetSize4);
			row.Implementation = ReadValue(_codedTokenDataSize4[9]);
		}

		public void GetField(int rid, out FieldRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.Field, rid);
			row.Flags = _accessor.ReadUInt16();
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Signature = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetFieldLayout(int rid, out FieldLayoutRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.FieldLayout, rid);
			row.OffSet = _accessor.ReadInt32();
			row.Field = ReadValue(_tableRowIndexSize4[MetadataTableType.Field]);
		}

		public void GetFieldMarshal(int rid, out FieldMarshalRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.FieldMarshal, rid);
			row.Parent = ReadValue(_codedTokenDataSize4[3]);
			row.NativeType = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetFieldPtr(int rid, out int value)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.FieldPtr, rid);
			value = ReadValue(_tableRowIndexSize4[MetadataTableType.Field]);
		}

		public void GetFieldRVA(int rid, out FieldRVARow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.FieldRVA, rid);
			row.RVA = (uint)_accessor.ReadInt32();
			row.Field = ReadValue(_tableRowIndexSize4[MetadataTableType.Field]);
		}

		public void GetFile(int rid, out FileRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.File, rid);
			row.Flags = _accessor.ReadInt32();
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.HashValue = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetGenericParam(int rid, out GenericParamRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.GenericParam, rid);
			row.Number = (int)_accessor.ReadUInt16();
			row.Flags = _accessor.ReadUInt16();
			row.Owner = ReadValue(_codedTokenDataSize4[12]);
			row.Name = ReadValue(_stringHeapOffsetSize4);
		}

		public void GetGenericParamConstraint(int rid, out GenericParamConstraintRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.GenericParamConstraint, rid);
			row.Owner = ReadValue(_tableRowIndexSize4[MetadataTableType.GenericParam]);
			row.Constraint = ReadValue(_codedTokenDataSize4[0]);
		}

		public void GetImplMap(int rid, out ImplMapRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.ImplMap, rid);
			row.MappingFlags = _accessor.ReadUInt16();
			row.MemberForwarded = ReadValue(_codedTokenDataSize4[8]);
			row.ImportName = ReadValue(_stringHeapOffsetSize4);
			row.ImportScope = ReadValue(_tableRowIndexSize4[MetadataTableType.ModuleRef]);
		}

		public void GetInterfaceImpl(int rid, out InterfaceImplRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.InterfaceImpl, rid);
			row.Class = ReadValue(_tableRowIndexSize4[MetadataTableType.TypeDef]);
			row.Interface = ReadValue(_codedTokenDataSize4[0]);
		}

		public void GetManifestResource(int rid, out ManifestResourceRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.ManifestResource, rid);
			row.Offset = _accessor.ReadInt32();
			row.Flags = _accessor.ReadInt32();
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Implementation = ReadValue(_codedTokenDataSize4[9]);
		}

		public void GetMemberRef(int rid, out MemberRefRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.MemberRef, rid);
			row.Class = ReadValue(_codedTokenDataSize4[5]);
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Signature = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetMethod(int rid, out MethodRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.MethodDef, rid);
			row.RVA = (uint)_accessor.ReadInt32();
			row.ImplFlags = _accessor.ReadUInt16();
			row.Flags = _accessor.ReadUInt16();
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Signature = ReadValue(_blobHeapOffsetSize4);
			row.ParamList = ReadValue(_tableRowIndexSize4[MetadataTableType.Param]);
		}

		public void GetMethodImpl(int rid, out MethodImplRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.MethodImpl, rid);
			row.Class = ReadValue(_tableRowIndexSize4[MetadataTableType.TypeDef]);
			row.MethodBody = ReadValue(_codedTokenDataSize4[7]);
			row.MethodDeclaration = ReadValue(_codedTokenDataSize4[7]);
		}

		public void GetMethodPtr(int rid, out int value)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.MethodPtr, rid);
			value = ReadValue(_tableRowIndexSize4[MetadataTableType.MethodDef]);
		}

		public void GetMethodSemantics(int rid, out MethodSemanticsRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.MethodSemantics, rid);
			row.Semantic = _accessor.ReadUInt16();
			row.Method = ReadValue(_tableRowIndexSize4[MetadataTableType.MethodDef]);
			row.Association = ReadValue(_codedTokenDataSize4[6]);
		}

		public void GetMethodSpec(int rid, out MethodSpecRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.MethodSpec, rid);
			row.Method = ReadValue(_codedTokenDataSize4[7]);
			row.Instantiation = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetModule(int rid, out ModuleRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.Module, rid);
			row.Generation = _accessor.ReadUInt16();
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Mvid = ReadValue(_guidHeapOffsetSize4);
			row.EncId = ReadValue(_guidHeapOffsetSize4);
			row.EncBaseId = ReadValue(_guidHeapOffsetSize4);
		}

		public void GetModuleRef(int rid, out ModuleRefRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.ModuleRef, rid);
			row.Name = ReadValue(_stringHeapOffsetSize4);
		}

		public void GetNestedClass(int rid, out NestedClassRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.NestedClass, rid);
			row.NestedClass = ReadValue(_tableRowIndexSize4[MetadataTableType.TypeDef]);
			row.EnclosingClass = ReadValue(_tableRowIndexSize4[MetadataTableType.TypeDef]);
		}

		public void GetParam(int rid, out ParamRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.Param, rid);
			row.Flags = _accessor.ReadUInt16();
			row.Sequence = _accessor.ReadUInt16();
			row.Name = ReadValue(_stringHeapOffsetSize4);
		}

		public void GetParamPtr(int rid, out int value)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.ParamPtr, rid);
			value = ReadValue(_tableRowIndexSize4[MetadataTableType.Param]);
		}

		public void GetProperty(int rid, out PropertyRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.Property, rid);
			row.Flags = _accessor.ReadUInt16();
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Type = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetPropertyMap(int rid, out PropertyMapRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.PropertyMap, rid);
			row.Parent = ReadValue(_tableRowIndexSize4[MetadataTableType.TypeDef]);
			row.PropertyList = ReadValue(_tableRowIndexSize4[MetadataTableType.Property]);
		}

		public void GetPropertyPtr(int rid, out int value)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.PropertyPtr, rid);
			value = ReadValue(_tableRowIndexSize4[MetadataTableType.Property]);
		}

		public void GetStandAloneSig(int rid, out StandAloneSigRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.StandAloneSig, rid);
			row.Signature = ReadValue(_blobHeapOffsetSize4);
		}

		public void GetTypeDef(int rid, out TypeDefRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.TypeDef, rid);
			row.Flags = _accessor.ReadInt32();
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Namespace = ReadValue(_stringHeapOffsetSize4);
			row.Extends = ReadValue(_codedTokenDataSize4[0]);
			row.FieldList = ReadValue(_tableRowIndexSize4[MetadataTableType.Field]);
			row.MethodList = ReadValue(_tableRowIndexSize4[MetadataTableType.MethodDef]);
		}

		public void GetTypeRef(int rid, out TypeRefRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.TypeRef, rid);
			row.ResolutionScope = ReadValue(_codedTokenDataSize4[11]);
			row.Name = ReadValue(_stringHeapOffsetSize4);
			row.Namespace = ReadValue(_stringHeapOffsetSize4);
		}

		public void GetTypeSpec(int rid, out TypeSpecRow row)
		{
			_accessor.Position = GetTableRowOffset(MetadataTableType.TypeSpec, rid);
			row.Signature = ReadValue(_blobHeapOffsetSize4);
		}

		private long GetTableRowOffset(int tableType, int rid)
		{
			var table = _tables[tableType];
			if (rid < 1 || rid > table.RowCount)
				throw new ArgumentOutOfRangeException("rid");

			return _offset + table.Offset + (table.RowSize * (rid - 1));
		}

		#endregion

		#region Static

		public static MemoryMappedMetadata Load(PEImage pe)
		{
			CorHeader corHeader;
			return Load(pe, out corHeader);
		}

		public static MemoryMappedMetadata Load(PEImage pe, out CorHeader corHeader)
		{
			var dd = pe.Directories[DataDirectories.CLIHeader];
			if (dd.IsNull)
			{
				throw new BadImageFormatException(string.Format(SR.MetadataImageNotValid, pe.Location));
			}

			MemoryMappedMetadata metadata;
			using (var accessor = pe.OpenImageToSectionData(dd.RVA))
			{
				corHeader = CorHeader.Read(accessor, pe.Location);

				if (corHeader.Metadata.IsNull)
				{
					throw new BadImageFormatException(string.Format(SR.MetadataImageNotValid, pe.Location));
				}

				long position = pe.ResolvePositionToSectionData(corHeader.Metadata.RVA);
				metadata = new MemoryMappedMetadata(accessor.Map(position, corHeader.Metadata.Size));
			}

			return metadata;
		}

		#endregion

		#region Nested types

		private struct TableInfo
		{
			internal int Offset;
			internal int RowCount;
			internal short RowSize;
			internal ColumnInfo[] Columns;
		}

		private struct ColumnInfo
		{
			internal short Offset;
			internal short Size;
		}

		#endregion
	}
}
