using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	public interface IMetadata
	{
		string FrameworkVersionMoniker
		{
			get;
		}

		bool IsOptimized
		{
			get;
		}

		byte TableSchemaMajorVersion
		{
			get;
		}

		byte TableSchemaMinorVersion
		{
			get;
		}

		bool IsTokenExists(int token);

		#region Heaps

		string GetString(int id);

		string GetUserString(int id);

		Guid GetGuid(int id);

		byte[] GetBlob(int id);

		byte[] GetBlob(int id, int offset, int size);

		byte GetBlobByte(int id, int offset);

		IBinaryAccessor OpenBlob(int id);

		#endregion

		#region  Tables

		bool IsTableSorted(int tableType);

		int GetTableRowCount(int tableType);

		int GetTableValue(int tableType, int rid, int columnID);

		void GetTableValues(int tableType, int rid, int columnID, int count, int[] values);

		void UnloadTable(int tableType);

		void GetAssembly(int rid, out AssemblyRow row);

		void GetAssemblyOS(int rid, out AssemblyOSRow row);

		void GetAssemblyProcessor(int rid, out AssemblyProcessorRow row);

		void GetAssemblyRef(int rid, out AssemblyRefRow row);

		void GetAssemblyRefOS(int rid, out AssemblyRefOSRow row);

		void GetAssemblyRefProcessor(int rid, out AssemblyRefProcessorRow row);

		void GetClassLayout(int rid, out ClassLayoutRow row);

		void GetConstant(int rid, out ConstantRow row);

		void GetCustomAttribute(int rid, out CustomAttributeRow row);

		void GetDeclSecurity(int rid, out DeclSecurityRow row);

		void GetENCLog(int rid, out ENCLogRow row);

		void GetENCMap(int rid, out ENCMapRow row);

		void GetEvent(int rid, out EventRow row);

		void GetEventMap(int rid, out EventMapRow row);

		void GetEventPtr(int rid, out int value);

		void GetExportedType(int rid, out ExportedTypeRow row);

		void GetField(int rid, out FieldRow row);

		void GetFieldLayout(int rid, out FieldLayoutRow row);

		void GetFieldMarshal(int rid, out FieldMarshalRow row);

		void GetFieldPtr(int rid, out int value);

		void GetFieldRVA(int rid, out FieldRVARow row);

		void GetFile(int rid, out FileRow row);

		void GetGenericParam(int rid, out GenericParamRow row);

		void GetGenericParamConstraint(int rid, out GenericParamConstraintRow row);

		void GetImplMap(int rid, out ImplMapRow row);

		void GetInterfaceImpl(int rid, out InterfaceImplRow row);

		void GetManifestResource(int rid, out ManifestResourceRow row);

		void GetMemberRef(int rid, out MemberRefRow row);

		void GetMethod(int rid, out MethodRow row);

		void GetMethodImpl(int rid, out MethodImplRow row);

		void GetMethodPtr(int rid, out int value);

		void GetMethodSemantics(int rid, out MethodSemanticsRow row);

		void GetMethodSpec(int rid, out MethodSpecRow row);

		void GetModule(int rid, out ModuleRow row);

		void GetModuleRef(int rid, out ModuleRefRow row);

		void GetNestedClass(int rid, out NestedClassRow row);

		void GetParam(int rid, out ParamRow row);

		void GetParamPtr(int rid, out int value);

		void GetProperty(int rid, out PropertyRow row);

		void GetPropertyMap(int rid, out PropertyMapRow row);

		void GetPropertyPtr(int rid, out int value);

		void GetStandAloneSig(int rid, out StandAloneSigRow row);

		void GetTypeDef(int rid, out TypeDefRow row);

		void GetTypeRef(int rid, out TypeRefRow row);

		void GetTypeSpec(int rid, out TypeSpecRow row);

		#endregion
	}
}
