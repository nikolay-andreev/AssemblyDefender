using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Metadata is, by definition, data that describes data. Like any general definition, however, this
	/// one is hardly informative. In the context of the common language runtime, metadata means
	/// a system of descriptors of all items that are declared or referenced in a module. The common
	/// language runtime programming model is inherently object oriented, so the items represented
	/// in metadata are classes and their members, with their accompanying attributes, properties,
	/// and relationships.
	/// </summary>
	public class MetadataScope : IMetadata
	{
		#region Fields

		private string _frameworkVersionMoniker;
		private MetadataTableStream _tables;
		private MetadataStringStream _strings;
		private MetadataUserStringStream _userStrings;
		private MetadataGuidStream _guids;
		private MetadataBlobStream _blobs;
		private List<MetadataExternalStream> _externalStreams;

		#endregion

		#region Ctors

		public MetadataScope()
		{
			_frameworkVersionMoniker = MicrosoftNetFramework.VersionMonikerLatest;
			_tables = new MetadataTableStream(this);
			_strings = new MetadataStringStream();
			_userStrings = new MetadataUserStringStream();
			_guids = new MetadataGuidStream();
			_blobs = new MetadataBlobStream();
		}

		#endregion

		#region Properties

		public string FrameworkVersionMoniker
		{
			get { return _frameworkVersionMoniker; }
			set { _frameworkVersionMoniker = value; }
		}

		public MetadataTableStream Tables
		{
			get { return _tables; }
		}

		public MetadataStringStream Strings
		{
			get { return _strings; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("Strings");

				_strings = value;
			}
		}

		public MetadataUserStringStream UserStrings
		{
			get { return _userStrings; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("UserStrings");

				_userStrings = value;
			}
		}

		public MetadataGuidStream Guids
		{
			get { return _guids; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("Guids");

				_guids = value;
			}
		}

		public MetadataBlobStream Blobs
		{
			get { return _blobs; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("Blobs");

				_blobs = value;
			}
		}

		public List<MetadataExternalStream> ExternalStreams
		{
			get
			{
				if (_externalStreams == null)
				{
					_externalStreams = new List<MetadataExternalStream>();
				}

				return _externalStreams;
			}
		}

		#endregion

		#region Methods

		public bool IsTokenExists(int token)
		{
			int type = MetadataToken.GetType(token);
			int tableType = MetadataToken.GetTableTypeByTokenType(type);
			if (tableType < 0 || tableType >= MetadataConstants.TableCount)
				return false;

			var table = Tables[tableType];

			int rid = MetadataToken.GetRID(token);
			if (!table.Exists(rid))
				return false;

			return true;
		}

		private void Read(IBinaryAccessor accessor)
		{
			long metadataOffset = accessor.Position;

			// Signature
			uint signature = accessor.ReadUInt32();
			if (signature != MetadataConstants.MetadataHeaderSignature)
			{
				throw new BadImageFormatException(SR.MetadataHeaderNotValid);
			}

			// Major version, 2 bytes (ignore on read)
			// Minor version, 2 bytes (ignore on read)
			// Reserved, always 0, 4 bytes
			accessor.Position += 8;

			ReadVersionString(accessor);

			// Reserved, 2 bytes
			accessor.Position += 2;

			ReadStreams(accessor, metadataOffset);
		}

		private void ReadVersionString(IBinaryAccessor accessor)
		{
			int versionLength = accessor.ReadInt32();
			if (versionLength == 0)
				return;

			byte[] buffer = accessor.ReadBytes(versionLength);
			int count = 0;
			for (int i = 0; i < buffer.Length; i++)
			{
				if (buffer[i] == 0)
					break;

				count++;
			}

			_frameworkVersionMoniker = Encoding.UTF8.GetString(buffer, 0, count);
		}

		private void ReadStreams(IBinaryAccessor accessor, long metadataOffset)
		{
			int numberOfStream = accessor.ReadUInt16();

			int[] offsets = new int[numberOfStream];
			int[] sizes = new int[numberOfStream];
			string[] names = new string[numberOfStream];

			for (int i = 0; i < numberOfStream; i++)
			{
				offsets[i] = accessor.ReadInt32();
				sizes[i] = accessor.ReadInt32();

				// Name of the stream; a zero-terminated ASCII string no longer than 31 characters (plus zero terminator).
				// The name might be shorter, in which case the size of the stream header is correspondingly reduced,
				// padded to the 4-byte boundary.
				long startPos = accessor.Position;
				names[i] = accessor.ReadNullTerminatedString(Encoding.ASCII);
				accessor.Align(startPos, 4);
			}

			int tableIndex = -1;

			for (int i = 0; i < numberOfStream; i++)
			{
				int offset = offsets[i];
				int size = sizes[i];
				string name = names[i];

				if (name == MetadataConstants.StreamTable)
				{
					tableIndex = i;
					_tables.IsOptimized = true;
				}
				else if (name == MetadataConstants.StreamTableUnoptimized)
				{
					tableIndex = i;
					_tables.IsOptimized = false;
				}
				else if (name == MetadataConstants.StreamStrings)
				{
					accessor.Position = offset + metadataOffset;
					_strings.Blob = new Blob(accessor.ReadBytes(size));
				}
				else if (name == MetadataConstants.StreamUserStrings)
				{
					accessor.Position = offset + metadataOffset;
					_userStrings.Blob = new Blob(accessor.ReadBytes(size));
				}
				else if (name == MetadataConstants.StreamGuid)
				{
					accessor.Position = offset + metadataOffset;
					_guids.Blob = new Blob(accessor.ReadBytes(size));
				}
				else if (name == MetadataConstants.StreamBlob)
				{
					accessor.Position = offset + metadataOffset;
					_blobs.Blob = new Blob(accessor.ReadBytes(size));
				}
				else
				{
					accessor.Position = offset + metadataOffset;
					var stream = new MetadataExternalStream(name, new Blob(accessor.ReadBytes(size)));
					ExternalStreams.Add(stream);
				}
			}

			if (tableIndex >= 0)
			{
				// Read table last as it relies on heaps.
				accessor.Position = offsets[tableIndex] + metadataOffset;
				_tables.Read(accessor);
			}
		}

		#endregion

		#region IMetadata Members

		bool IMetadata.IsOptimized
		{
			get { return _tables.IsOptimized; }
		}

		byte IMetadata.TableSchemaMajorVersion
		{
			get { return _tables.SchemaMajorVersion; }
		}

		byte IMetadata.TableSchemaMinorVersion
		{
			get { return _tables.SchemaMinorVersion; }
		}

		string IMetadata.GetString(int id)
		{
			return _strings.Get(id);
		}

		string IMetadata.GetUserString(int id)
		{
			return _userStrings.Get(id);
		}

		Guid IMetadata.GetGuid(int id)
		{
			return _guids.Get(id);
		}

		byte[] IMetadata.GetBlob(int id)
		{
			return _blobs.Get(id);
		}

		byte[] IMetadata.GetBlob(int id, int offset, int size)
		{
			return _blobs.Get(id, offset, size);
		}

		byte IMetadata.GetBlobByte(int id, int offset)
		{
			return _blobs.GetByte(id, offset);
		}

		IBinaryAccessor IMetadata.OpenBlob(int id)
		{
			return _blobs.OpenBlob(id);
		}

		bool IMetadata.IsTableSorted(int tableType)
		{
			return _tables.IsSorted(tableType);
		}

		int IMetadata.GetTableRowCount(int tableType)
		{
			return _tables[tableType].Count;
		}

		int IMetadata.GetTableValue(int tableType, int rid, int columnID)
		{
			return _tables[tableType].Get(rid, columnID);
		}

		void IMetadata.GetTableValues(int tableType, int rid, int columnID, int count, int[] values)
		{
			_tables[tableType].Get(rid, columnID, count, values);
		}

		void IMetadata.UnloadTable(int tableType)
		{
			_tables[tableType].Clear();
		}

		void IMetadata.GetAssembly(int rid, out AssemblyRow row)
		{
			_tables.AssemblyTable.Get(rid, out row);
		}

		void IMetadata.GetAssemblyOS(int rid, out AssemblyOSRow row)
		{
			_tables.AssemblyOSTable.Get(rid, out row);
		}

		void IMetadata.GetAssemblyProcessor(int rid, out AssemblyProcessorRow row)
		{
			_tables.AssemblyProcessorTable.Get(rid, out row);
		}

		void IMetadata.GetAssemblyRef(int rid, out AssemblyRefRow row)
		{
			_tables.AssemblyRefTable.Get(rid, out row);
		}

		void IMetadata.GetAssemblyRefOS(int rid, out AssemblyRefOSRow row)
		{
			_tables.AssemblyRefOSTable.Get(rid, out row);
		}

		void IMetadata.GetAssemblyRefProcessor(int rid, out AssemblyRefProcessorRow row)
		{
			_tables.AssemblyRefProcessorTable.Get(rid, out row);
		}

		void IMetadata.GetClassLayout(int rid, out ClassLayoutRow row)
		{
			_tables.ClassLayoutTable.Get(rid, out row);
		}

		void IMetadata.GetConstant(int rid, out ConstantRow row)
		{
			_tables.ConstantTable.Get(rid, out row);
		}

		void IMetadata.GetCustomAttribute(int rid, out CustomAttributeRow row)
		{
			_tables.CustomAttributeTable.Get(rid, out row);
		}

		void IMetadata.GetDeclSecurity(int rid, out DeclSecurityRow row)
		{
			_tables.DeclSecurityTable.Get(rid, out row);
		}

		void IMetadata.GetENCLog(int rid, out ENCLogRow row)
		{
			_tables.ENCLogTable.Get(rid, out row);
		}

		void IMetadata.GetENCMap(int rid, out ENCMapRow row)
		{
			_tables.ENCMapTable.Get(rid, out row);
		}

		void IMetadata.GetEvent(int rid, out EventRow row)
		{
			_tables.EventTable.Get(rid, out row);
		}

		void IMetadata.GetEventMap(int rid, out EventMapRow row)
		{
			_tables.EventMapTable.Get(rid, out row);
		}

		void IMetadata.GetEventPtr(int rid, out int value)
		{
			value = _tables.EventPtrTable.Get(rid);
		}

		void IMetadata.GetExportedType(int rid, out ExportedTypeRow row)
		{
			_tables.ExportedTypeTable.Get(rid, out row);
		}

		void IMetadata.GetField(int rid, out FieldRow row)
		{
			_tables.FieldTable.Get(rid, out row);
		}

		void IMetadata.GetFieldLayout(int rid, out FieldLayoutRow row)
		{
			_tables.FieldLayoutTable.Get(rid, out row);
		}

		void IMetadata.GetFieldMarshal(int rid, out FieldMarshalRow row)
		{
			_tables.FieldMarshalTable.Get(rid, out row);
		}

		void IMetadata.GetFieldPtr(int rid, out int value)
		{
			value = _tables.FieldPtrTable.Get(rid);
		}

		void IMetadata.GetFieldRVA(int rid, out FieldRVARow row)
		{
			_tables.FieldRVATable.Get(rid, out row);
		}

		void IMetadata.GetFile(int rid, out FileRow row)
		{
			_tables.FileTable.Get(rid, out row);
		}

		void IMetadata.GetGenericParam(int rid, out GenericParamRow row)
		{
			_tables.GenericParamTable.Get(rid, out row);
		}

		void IMetadata.GetGenericParamConstraint(int rid, out GenericParamConstraintRow row)
		{
			_tables.GenericParamConstraintTable.Get(rid, out row);
		}

		void IMetadata.GetImplMap(int rid, out ImplMapRow row)
		{
			_tables.ImplMapTable.Get(rid, out row);
		}

		void IMetadata.GetInterfaceImpl(int rid, out InterfaceImplRow row)
		{
			_tables.InterfaceImplTable.Get(rid, out row);
		}

		void IMetadata.GetManifestResource(int rid, out ManifestResourceRow row)
		{
			_tables.ManifestResourceTable.Get(rid, out row);
		}

		void IMetadata.GetMemberRef(int rid, out MemberRefRow row)
		{
			_tables.MemberRefTable.Get(rid, out row);
		}

		void IMetadata.GetMethod(int rid, out MethodRow row)
		{
			_tables.MethodTable.Get(rid, out row);
		}

		void IMetadata.GetMethodImpl(int rid, out MethodImplRow row)
		{
			_tables.MethodImplTable.Get(rid, out row);
		}

		void IMetadata.GetMethodPtr(int rid, out int value)
		{
			value = _tables.MethodPtrTable.Get(rid);
		}

		void IMetadata.GetMethodSemantics(int rid, out MethodSemanticsRow row)
		{
			_tables.MethodSemanticsTable.Get(rid, out row);
		}

		void IMetadata.GetMethodSpec(int rid, out MethodSpecRow row)
		{
			_tables.MethodSpecTable.Get(rid, out row);
		}

		void IMetadata.GetModule(int rid, out ModuleRow row)
		{
			_tables.ModuleTable.Get(rid, out row);
		}

		void IMetadata.GetModuleRef(int rid, out ModuleRefRow row)
		{
			_tables.ModuleRefTable.Get(rid, out row);
		}

		void IMetadata.GetNestedClass(int rid, out NestedClassRow row)
		{
			_tables.NestedClassTable.Get(rid, out row);
		}

		void IMetadata.GetParam(int rid, out ParamRow row)
		{
			_tables.ParamTable.Get(rid, out row);
		}

		void IMetadata.GetParamPtr(int rid, out int value)
		{
			value = _tables.ParamPtrTable.Get(rid);
		}

		void IMetadata.GetProperty(int rid, out PropertyRow row)
		{
			_tables.PropertyTable.Get(rid, out row);
		}

		void IMetadata.GetPropertyMap(int rid, out PropertyMapRow row)
		{
			_tables.PropertyMapTable.Get(rid, out row);
		}

		void IMetadata.GetPropertyPtr(int rid, out int value)
		{
			value = _tables.PropertyPtrTable.Get(rid);
		}

		void IMetadata.GetStandAloneSig(int rid, out StandAloneSigRow row)
		{
			_tables.StandAloneSigTable.Get(rid, out row);
		}

		void IMetadata.GetTypeDef(int rid, out TypeDefRow row)
		{
			_tables.TypeDefTable.Get(rid, out row);
		}

		void IMetadata.GetTypeRef(int rid, out TypeRefRow row)
		{
			_tables.TypeRefTable.Get(rid, out row);
		}

		void IMetadata.GetTypeSpec(int rid, out TypeSpecRow row)
		{
			_tables.TypeSpecTable.Get(rid, out row);
		}

		#endregion

		#region Static

		public static MetadataScope Load(PEImage pe)
		{
			CorHeader corHeader;
			return Load(pe, out corHeader);
		}

		public unsafe static MetadataScope Load(PEImage pe, out CorHeader corHeader)
		{
			var dd = pe.Directories[DataDirectories.CLIHeader];
			if (dd.IsNull)
			{
				throw new BadImageFormatException(string.Format(SR.MetadataImageNotValid, pe.Location));
			}

			var metadata = new MetadataScope();
			using (var accessor = pe.OpenImageToSectionData(dd.RVA))
			{
				fixed (byte* pBuff = accessor.ReadBytes(sizeof(CorHeader)))
				{
					corHeader = *(CorHeader*)pBuff;
				}

				if (corHeader.Cb != MetadataConstants.CorHeaderSignature)
				{
					throw new BadImageFormatException(string.Format(SR.MetadataImageNotValid, pe.Location));
				}

				accessor.Position = pe.ResolvePositionToSectionData(corHeader.Metadata.RVA);
				metadata.Read(accessor);
			}

			return metadata;
		}

		#endregion
	}
}
