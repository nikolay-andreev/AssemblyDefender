using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public static class MetadataSchema
	{
		private static MetadataTableInfo[] _tables;

		static MetadataSchema()
		{
			InitializeTables();
		}

		public static MetadataTableInfo[] Tables
		{
			get { return _tables; }
		}

		private static void InitializeTables()
		{
			_tables = new MetadataTableInfo[]
			{
				new MetadataTableInfo(
					"Module",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Generation", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Mvid", MetadataColumnType.Guid),
						new MetadataColumnInfo("EncId", MetadataColumnType.Guid),
						new MetadataColumnInfo("EncBaseId", MetadataColumnType.Guid),
					}),
				new MetadataTableInfo(
					"TypeRef",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("ResolutionScope", CodedTokenType.ResolutionScope),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Namespace", MetadataColumnType.String),
					}),
				new MetadataTableInfo(
					"TypeDef",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt32),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Namespace", MetadataColumnType.String),
						new MetadataColumnInfo("Extends", CodedTokenType.TypeDefOrRef),
						new MetadataColumnInfo("FieldList", MetadataTableType.Field),
						new MetadataColumnInfo("MethodList", MetadataTableType.MethodDef),
					}),
				new MetadataTableInfo(
					"FieldPtr",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Field", MetadataTableType.Field),
					}),
				new MetadataTableInfo(
					"Field",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Signature", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"MethodPtr",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Method", MetadataTableType.MethodDef),
					}),
				new MetadataTableInfo(
					"Method",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("RVA", MetadataColumnType.UInt32),
						new MetadataColumnInfo("ImplFlags", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Signature", MetadataColumnType.Blob),
						new MetadataColumnInfo("ParamList", MetadataTableType.Param),
					}),
				new MetadataTableInfo(
					"ParamPtr",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Param", MetadataTableType.Param),
					}),
				new MetadataTableInfo(
					"Param",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Sequence", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
					}),
				new MetadataTableInfo(
					"InterfaceImpl",
					0,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Class", MetadataTableType.TypeDef),
						new MetadataColumnInfo("Interface", CodedTokenType.TypeDefOrRef),
					}),
				new MetadataTableInfo(
					"MemberRef",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Class", CodedTokenType.MemberRefParent),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Signature", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"Constant",
					1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Type", MetadataColumnType.Byte2),
						new MetadataColumnInfo("Parent", CodedTokenType.HasConstant),
						new MetadataColumnInfo("Value", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"CustomAttribute",
					0,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Parent", CodedTokenType.HasCustomAttribute),
						new MetadataColumnInfo("Type", CodedTokenType.CustomAttributeType),
						new MetadataColumnInfo("Value", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"FieldMarshal",
					0,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Parent", CodedTokenType.HasFieldMarshal),
						new MetadataColumnInfo("NativeType", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"DeclSecurity",
					1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Action", MetadataColumnType.Int16),
						new MetadataColumnInfo("Parent", CodedTokenType.HasDeclSecurity),
						new MetadataColumnInfo("PermissionSet", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"ClassLayout",
					2,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("PackingSize", MetadataColumnType.UInt16),
						new MetadataColumnInfo("ClassSize", MetadataColumnType.UInt32),
						new MetadataColumnInfo("Parent", MetadataTableType.TypeDef),
					}),
				new MetadataTableInfo(
					"FieldLayout",
					1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("OffSet", MetadataColumnType.UInt32),
						new MetadataColumnInfo("Field", MetadataTableType.Field),
					}),
				new MetadataTableInfo(
					"StandAloneSig",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Signature", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"EventMap",
					0,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Parent", MetadataTableType.TypeDef),
						new MetadataColumnInfo("EventList", MetadataTableType.Event),
					}),
				new MetadataTableInfo(
					"EventPtr",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Event", MetadataTableType.Event),
					}),
				new MetadataTableInfo(
					"Event",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("EventFlags", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("EventType", CodedTokenType.TypeDefOrRef),
					}),
				new MetadataTableInfo(
					"PropertyMap",
					0,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Parent", MetadataTableType.TypeDef),
						new MetadataColumnInfo("PropertyList", MetadataTableType.Property),
					}),
				new MetadataTableInfo(
					"PropertyPtr",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Property", MetadataTableType.Property),
					}),
				new MetadataTableInfo(
					"Property",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("PropFlags", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Type", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"MethodSemantics",
					2,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Semantic", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Method", MetadataTableType.MethodDef),
						new MetadataColumnInfo("Association", CodedTokenType.HasSemantic),
					}),
				new MetadataTableInfo(
					"MethodImpl",
					0,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Class", MetadataTableType.TypeDef),
						new MetadataColumnInfo("MethodBody", CodedTokenType.MethodDefOrRef),
						new MetadataColumnInfo("MethodDeclaration", CodedTokenType.MethodDefOrRef),
					}),
				new MetadataTableInfo(
					"ModuleRef",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Name", MetadataColumnType.String),
					}),
				new MetadataTableInfo(
					"TypeSpec",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Signature", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"ImplMap",
					1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("MappingFlags", MetadataColumnType.UInt16),
						new MetadataColumnInfo("MemberForwarded", CodedTokenType.MemberForwarded),
						new MetadataColumnInfo("ImportName", MetadataColumnType.String),
						new MetadataColumnInfo("ImportScope", MetadataTableType.ModuleRef),
					}),
				new MetadataTableInfo(
					"FieldRVA",
					1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("RVA", MetadataColumnType.UInt32),
						new MetadataColumnInfo("Field", MetadataTableType.Field),
					}),
				new MetadataTableInfo(
					"ENCLog",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Token", MetadataColumnType.UInt32),
						new MetadataColumnInfo("FuncCode", MetadataColumnType.UInt32),
					}),
				new MetadataTableInfo(
					"ENCMap",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Token", MetadataColumnType.UInt32),
					}),
				new MetadataTableInfo(
					"Assembly",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("HashAlgId", MetadataColumnType.UInt32),
						new MetadataColumnInfo("MajorVersion", MetadataColumnType.UInt16),
						new MetadataColumnInfo("MinorVersion", MetadataColumnType.UInt16),
						new MetadataColumnInfo("BuildNumber", MetadataColumnType.UInt16),
						new MetadataColumnInfo("RevisionNumber", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt32),
						new MetadataColumnInfo("PublicKey", MetadataColumnType.Blob),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Locale", MetadataColumnType.String),
					}),
				new MetadataTableInfo(
					"AssemblyProcessor",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Processor", MetadataColumnType.UInt32),
					}),
				new MetadataTableInfo(
					"AssemblyOS",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("OSPlatformId", MetadataColumnType.UInt32),
						new MetadataColumnInfo("OSMajorVersion", MetadataColumnType.UInt32),
						new MetadataColumnInfo("OSMinorVersion", MetadataColumnType.UInt32),
					}),
				new MetadataTableInfo(
					"AssemblyRef",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("MajorVersion", MetadataColumnType.UInt16),
						new MetadataColumnInfo("MinorVersion", MetadataColumnType.UInt16),
						new MetadataColumnInfo("BuildNumber", MetadataColumnType.UInt16),
						new MetadataColumnInfo("RevisionNumber", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt32),
						new MetadataColumnInfo("PublicKeyOrToken", MetadataColumnType.Blob),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Locale", MetadataColumnType.String),
						new MetadataColumnInfo("HashValue", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"AssemblyRefProcessor",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Processor", MetadataColumnType.UInt32),
						new MetadataColumnInfo("AssemblyRef", MetadataTableType.AssemblyRef),
					}),
				new MetadataTableInfo(
					"AssemblyRefOS",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("OSPlatformId", MetadataColumnType.UInt32),
						new MetadataColumnInfo("OSMajorVersion", MetadataColumnType.UInt32),
						new MetadataColumnInfo("OSMinorVersion", MetadataColumnType.UInt32),
						new MetadataColumnInfo("AssemblyRef", MetadataTableType.AssemblyRef),
					}),
				new MetadataTableInfo(
					"File",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt32),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("HashValue", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"ExportedType",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt32),
						new MetadataColumnInfo("TypeDefId", MetadataColumnType.UInt32),
						new MetadataColumnInfo("TypeName", MetadataColumnType.String),
						new MetadataColumnInfo("TypeNamespace", MetadataColumnType.String),
						new MetadataColumnInfo("Implementation", CodedTokenType.Implementation),
					}),
				new MetadataTableInfo(
					"ManifestResource",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Offset", MetadataColumnType.UInt32),
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt32),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
						new MetadataColumnInfo("Implementation", CodedTokenType.Implementation),
					}),
				new MetadataTableInfo(
					"NestedClass",
					0,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("NestedClass", MetadataTableType.TypeDef),
						new MetadataColumnInfo("EnclosingClass", MetadataTableType.TypeDef),
					}),
				new MetadataTableInfo(
					"GenericParam",
					2,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Number", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Flags", MetadataColumnType.UInt16),
						new MetadataColumnInfo("Owner", CodedTokenType.TypeOrMethodDef),
						new MetadataColumnInfo("Name", MetadataColumnType.String),
					}),
				new MetadataTableInfo(
					"MethodSpec",
					-1,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Method", CodedTokenType.MethodDefOrRef),
						new MetadataColumnInfo("Instantiation", MetadataColumnType.Blob),
					}),
				new MetadataTableInfo(
					"GenericParamConstraint",
					0,
					new MetadataColumnInfo[]
					{
						new MetadataColumnInfo("Owner", MetadataTableType.GenericParam),
						new MetadataColumnInfo("Constraint", CodedTokenType.TypeDefOrRef),
					}),
			};
		}
	}
}
