using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	public class MetadataBuilder : BuildTask
	{
		#region Fields

		private MetadataScope _metadata;
		private BuildBlob _blob;
		private TableCompressionInfo _compressionInfo;
		private int[] _tableOffsets;
		private int[] _rowSizes;
		private State _state;
		private string _sectionName = PESectionNames.Text;
		private int _blobPriority = 1000;

		#endregion

		#region Properties

		public MetadataScope Metadata
		{
			get { return _metadata; }
			set { _metadata = value; }
		}

		public BuildBlob Blob
		{
			get { return _blob; }
		}

		public TableCompressionInfo CompressionInfo
		{
			get { return _compressionInfo; }
		}

		public int[] TableOffsets
		{
			get { return _tableOffsets; }
		}

		public int[] RowSizes
		{
			get { return _rowSizes; }
		}

		public string SectionName
		{
			get { return _sectionName; }
			set { _sectionName = value; }
		}

		public int BlobPriority
		{
			get { return _blobPriority; }
			set { _blobPriority = value; }
		}

		#endregion

		#region Methods

		public override void Build()
		{
			if (_metadata == null)
				return;

			FinalizeBuildModule();

			Initialize();

			SortTables();

			DoBuild();

			Write();

			// Add fixups
			PE.Fixups.Add(new MetadataFixup(this));

			// Add blobs
			var section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);

			_state = null;
		}

		private void FinalizeBuildModule()
		{
			var moduleBuilder = PE.Tasks.Get<ModuleBuilder>();
			if (moduleBuilder != null)
			{
				moduleBuilder.FinalizeBuild();
			}
		}

		private void Initialize()
		{
			_blob = new BuildBlob();
			_tableOffsets = new int[MetadataConstants.TableCount];
			_rowSizes = new int[MetadataConstants.TableCount];
			_state = new State();
		}

		private void SortTables()
		{
			int[][] map = _metadata.Tables.Sort();

			var moduleBuilder = PE.Tasks.Get<ModuleBuilder>(true);

			// FieldDataPoints
			{
				int[] rids = map[SortableTableType.FieldRVA];
				if (rids != null)
				{
					var fieldDataPoints = moduleBuilder.FieldDataPoints;
					if (fieldDataPoints != null)
					{
						for (int i = 0; i < fieldDataPoints.Length; i++)
						{
							int newIndex = rids[i] - 1;
							if (i == newIndex)
								continue;

							var point = fieldDataPoints[i];
							fieldDataPoints[i] = fieldDataPoints[newIndex];
							fieldDataPoints[newIndex] = point;
						}
					}
				}
			}
		}

		private void DoBuild()
		{
			_compressionInfo = TableCompressionInfo.Create(_metadata);

			// Signature
			_state.StreamPos += 16;
			_state.StreamPos += Encoding.UTF8.GetByteCount((_metadata.FrameworkVersionMoniker ?? "")).Align(4);

			// Storage header
			_state.StreamPos += 4;

			// Tables
			{
				_state.TablesLength = GetTableStreamLength();
				string tableName = _metadata.Tables.IsOptimized ? MetadataConstants.StreamTable : MetadataConstants.StreamTableUnoptimized;
				_state.StreamPos += 8 + (tableName.Length + 1).Align(4);
				_state.StreamCount++;
			}

			// Strings
			if (_metadata.Strings.Length > 0)
			{
				_state.StreamPos += 8 + (MetadataConstants.StreamStrings.Length + 1).Align(4);
				_state.StreamCount++;
			}

			if (_metadata.UserStrings.Length > 0)
			{
				_state.StreamPos += 8 + (MetadataConstants.StreamUserStrings.Length + 1).Align(4);
				_state.StreamCount++;
			}

			if (_metadata.Guids.Length > 0)
			{
				_state.StreamPos += 8 + (MetadataConstants.StreamGuid.Length + 1).Align(4);
				_state.StreamCount++;
			}

			if (_metadata.Blobs.Length > 0)
			{
				_state.StreamPos += 8 + (MetadataConstants.StreamBlob.Length + 1).Align(4);
				_state.StreamCount++;
			}

			foreach (var externalStream in _metadata.ExternalStreams)
			{
				_state.StreamPos += 8 + ((externalStream.Name ?? "").Length + 1).Align(4);
				_state.StreamCount++;
			}
		}

		private int GetTableStreamLength()
		{
			var tables = _metadata.Tables;
			var compressionInfo = _compressionInfo;

			int length = 24; // table stream header

			for (int tableType = 0; tableType < MetadataConstants.TableCount; tableType++)
			{
				var table = tables[tableType];
				if (table.Count == 0)
					continue;

				// Row size integer.
				length += 4;

				// Columns

				int rowSize = GetTableRowSize(table);
				_rowSizes[tableType] = rowSize;
				length += (rowSize * table.Count);
			}

			return length;
		}

		private int GetTableRowSize(MetadataTable table)
		{
			var tableSchema = table.GetSchema();
			var columnSchemas = tableSchema.Columns;

			int columnOffset = 0;
			for (int columnIndex = 0; columnIndex < columnSchemas.Length; columnIndex++)
			{
				int columnSize;
				var columnSchema = columnSchemas[columnIndex];
				if (columnSchema.IsTableIndex)
				{
					columnSize = (_compressionInfo.TableRowIndexSize4[columnSchema.Type] ? 4 : 2);
				}
				else if (columnSchema.IsCodedToken)
				{
					columnSize = (_compressionInfo.CodedTokenDataSize4[columnSchema.Type - 64] ? 4 : 2);
				}
				else
				{
					switch (columnSchema.Type)
					{
						case MetadataColumnType.Byte2:
						case MetadataColumnType.Int16:
						case MetadataColumnType.UInt16:
							columnSize = 2;
							break;

						case MetadataColumnType.Int32:
						case MetadataColumnType.UInt32:
							columnSize = 4;
							break;

						case MetadataColumnType.String:
							columnSize = (_compressionInfo.StringHeapOffsetSize4 ? 4 : 2);
							break;

						case MetadataColumnType.Guid:
							columnSize = (_compressionInfo.GuidHeapOffsetSize4 ? 4 : 2);
							break;

						case MetadataColumnType.Blob:
							columnSize = (_compressionInfo.BlobHeapOffsetSize4 ? 4 : 2);
							break;

						default:
							throw new InvalidOperationException();
					}
				}

				columnOffset += columnSize;
			}

			return columnOffset;
		}

		private ulong GetMaskValid(MetadataTableStream tables)
		{
			ulong mask = 0;
			for (int i = 0; i < MetadataConstants.TableCount; i++)
			{
				var table = tables[i];
				if (table.Count > 0)
				{
					mask |= 1UL << i;
				}
			}

			return mask;
		}

		private ulong GetMaskSorted(MetadataTableStream tables)
		{
			return tables.SortMask;
		}

		private void Write()
		{
			// Signature
			_blob.Write(ref _state.Pos, (uint)MetadataConstants.MetadataHeaderSignature);
			_blob.Write(ref _state.Pos, (short)1); // Major version, 1 (ignore on read)
			_blob.Write(ref _state.Pos, (short)1); // Minor version, 1 (ignore on read)
			_blob.Write(ref _state.Pos, (int)0); // Reserved

			// Version
			byte[] versionBytes = Encoding.UTF8.GetBytes(_metadata.FrameworkVersionMoniker ?? "");
			int versionAlignedLength = versionBytes.Length.Align(4);
			_blob.Write(ref _state.Pos, versionAlignedLength); // length
			_blob.Write(ref _state.Pos, versionBytes);

			int versionAlignCount = versionAlignedLength - versionBytes.Length;
			if (versionAlignCount > 0)
			{
				_blob.Write(ref _state.Pos, 0, versionAlignCount);
			}

			// Storage header
			_blob.Write(ref _state.Pos, (short)0); // Reserved
			_blob.Write(ref _state.Pos, (short)_state.StreamCount);

			WriteTables(_metadata.Tables);

			if (_metadata.Strings.Length > 0)
			{
				WriteStrings(_metadata.Strings);
			}

			if (_metadata.UserStrings.Length > 0)
			{
				WriteUserStrings(_metadata.UserStrings);
			}

			if (_metadata.Guids.Length > 0)
			{
				WriteGuids(_metadata.Guids);
			}

			if (_metadata.Blobs.Length > 0)
			{
				WriteBlobs(_metadata.Blobs);
			}

			foreach (var externalStream in _metadata.ExternalStreams)
			{
				WriteExternalStream(externalStream);
			}
		}

		private void WriteStreamHeader(string name, int length)
		{
			_blob.Write(ref _state.Pos, (int)_state.StreamPos);
			_blob.Write(ref _state.Pos, (int)length);

			// Name
			name = (name ?? "") + '\0';
			_blob.Write(ref _state.Pos, name, Encoding.ASCII);

			int nameAlignCount = name.Length.Align(4) - name.Length;
			if (nameAlignCount > 0)
			{
				_blob.Write(ref _state.Pos, 0, nameAlignCount);
			}
		}

		private void WriteTables(MetadataTableStream tables)
		{
			string tableName = tables.IsOptimized ? MetadataConstants.StreamTable : MetadataConstants.StreamTableUnoptimized;
			WriteStreamHeader(tableName, _state.TablesLength);

			var compressionInfo = _compressionInfo;

			// Header
			_blob.Write(ref _state.StreamPos, (int)0); // Reserved, 4 bytes

			_blob.Write(ref _state.StreamPos, (byte)tables.SchemaMajorVersion);
			_blob.Write(ref _state.StreamPos, (byte)tables.SchemaMinorVersion);

			// Flags
			var heapFlags = HeapOffsetFlags.None;
			if (compressionInfo.StringHeapOffsetSize4)
				heapFlags |= HeapOffsetFlags.StringHeap4;
			if (compressionInfo.GuidHeapOffsetSize4)
				heapFlags |= HeapOffsetFlags.GuidHeap4;
			if (compressionInfo.BlobHeapOffsetSize4)
				heapFlags |= HeapOffsetFlags.BlobHeap4;

			_blob.Write(ref _state.StreamPos, (byte)heapFlags);

			_blob.Write(ref _state.StreamPos, (byte)1); // Reserved, 1 bytes

			// Mask
			_blob.Write(ref _state.StreamPos, (ulong)GetMaskValid(tables));
			_blob.Write(ref _state.StreamPos, (ulong)GetMaskSorted(tables));

			// Row counts
			for (int i = 0; i < MetadataConstants.TableCount; i++)
			{
				int count = tables[i].Count;
				if (count > 0)
				{
					_blob.Write(ref _state.StreamPos, (int)count);
				}
			}

			// Tables
			for (int i = 0; i < MetadataConstants.TableCount; i++)
			{
				var table = tables[i];
				if (table.Count > 0)
				{
					_tableOffsets[(int)table.Type] = _state.StreamPos;
					table.Write(_blob, ref _state.StreamPos, compressionInfo);
				}
			}
		}

		private void WriteStrings(MetadataStringStream strings)
		{
			strings.Blob.Align(4, 0);

			WriteStreamHeader(MetadataConstants.StreamStrings, strings.Length);
			_blob.Write(ref _state.StreamPos, strings.Blob.GetBuffer(), 0, strings.Blob.Length);
		}

		private void WriteUserStrings(MetadataUserStringStream userStrings)
		{
			userStrings.Blob.Align(4, 0);

			WriteStreamHeader(MetadataConstants.StreamUserStrings, userStrings.Length);
			_blob.Write(ref _state.StreamPos, userStrings.Blob.GetBuffer(), 0, userStrings.Blob.Length);
		}

		private void WriteGuids(MetadataGuidStream guids)
		{
			WriteStreamHeader(MetadataConstants.StreamGuid, guids.Length);
			_blob.Write(ref _state.StreamPos, guids.Blob.GetBuffer(), 0, guids.Blob.Length);
		}

		private void WriteBlobs(MetadataBlobStream blobs)
		{
			blobs.Blob.Align(4, 0);

			WriteStreamHeader(MetadataConstants.StreamBlob, blobs.Length);
			_blob.Write(ref _state.StreamPos, blobs.Blob.GetBuffer(), 0, blobs.Blob.Length);
		}

		private void WriteExternalStream(MetadataExternalStream externalStream)
		{
			WriteStreamHeader(externalStream.Name, externalStream.Blob.Length);
			_blob.Write(ref _state.StreamPos, externalStream.Blob.GetBuffer(), 0, externalStream.Blob.Length);
		}

		#endregion

		#region Nested types

		private class MetadataFixup : BuildFixup
		{
			private MetadataBuilder _builder;

			internal MetadataFixup(MetadataBuilder builder)
			{
				_builder = builder;
			}

			public override void ApplyFixup()
			{
				FixMethodBody();
				FixFieldData();
			}

			private void FixMethodBody()
			{
				var metadata = _builder._metadata;
				if (metadata == null)
					return;

				var metadataBlob = _builder._blob;
				if (metadataBlob == null)
					return;

				var methodTable = metadata.Tables.MethodTable;
				if (methodTable.Count == 0)
					return;

				int[] tableOffsets = _builder._tableOffsets;
				if (tableOffsets == null)
					return;

				int[] rowSizes = _builder._rowSizes;
				if (rowSizes == null)
					return;

				int tableOffset = tableOffsets[MetadataTableType.MethodDef];
				int rowSize = rowSizes[MetadataTableType.MethodDef];
				if (rowSize == 0)
					return;

				var moduleBuilder = PE.Tasks.Get<ModuleBuilder>(true);

				var methodBodyPoints = moduleBuilder.MethodBodyPoints;

				for (int i = 0; i < methodTable.Count; i++)
				{
					var point = methodBodyPoints[i];
					if (point.Blob == null)
						continue;

					int rowPos = tableOffset + (rowSize * i);
					metadataBlob.Write(ref rowPos, (uint)point.RVA);
				}
			}

			private void FixFieldData()
			{
				var metadata = _builder._metadata;
				if (metadata == null)
					return;

				var metadataBlob = _builder._blob;
				if (metadataBlob == null)
					return;

				var fieldRVATable = metadata.Tables.FieldRVATable;
				if (fieldRVATable.Count == 0)
					return;

				int[] tableOffsets = _builder._tableOffsets;
				if (tableOffsets == null)
					return;

				int[] rowSizes = _builder._rowSizes;
				if (rowSizes == null)
					return;

				int tableOffset = tableOffsets[MetadataTableType.FieldRVA];
				int rowSize = rowSizes[MetadataTableType.FieldRVA];
				if (rowSize == 0)
					return;

				var moduleBuilder = PE.Tasks.Get<ModuleBuilder>(true);

				var fieldDataPoints = moduleBuilder.FieldDataPoints;
				var dataBlob = moduleBuilder.FieldDataBlob;
				var cliDataBlob = moduleBuilder.FieldCLIDataBlob;
				var tlsDataBlob = moduleBuilder.FieldTLSDataBlob;

				for (int i = 0; i < fieldRVATable.Count; i++)
				{
					var point = fieldDataPoints[i];

					uint blobRVA;
					switch (point.Type)
					{
						case FieldDataType.Data:
							blobRVA = dataBlob.RVA;
							break;

						case FieldDataType.Cli:
							blobRVA = cliDataBlob.RVA;
							break;

						case FieldDataType.Tls:
							blobRVA = tlsDataBlob.RVA;
							break;

						default:
							throw new InvalidOperationException();
					}

					int rowPos = tableOffset + (rowSize * i);
					metadataBlob.Write(ref rowPos, (uint)(blobRVA + point.Offset));
				}
			}
		}

		private class State
		{
			internal int Pos;
			internal int StreamPos;
			internal int StreamCount;
			internal int TablesLength;
		}

		#endregion
	}
}
