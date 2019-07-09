using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public static class MetadataToken
	{
		public static readonly int Null = MetadataTokenType.Module;

		public static bool IsValid(int token)
		{
			int type = GetType(token);
			int tableType = type >> 24;
			if (tableType < 0 || tableType >= MetadataConstants.TableCount)
				return false;

			if (!_tokenTypeValid[tableType])
				return false;

			return true;
		}

		public static bool IsNull(int token)
		{
			return GetRID(token) == 0;
		}

		public static int Get(int type, int rid)
		{
			return type | rid;
		}

		public static int GetNull(int type)
		{
			return type;
		}

		public static int GetRID(int token)
		{
			return (int)(token & 0x00ffffff);
		}

		public static int GetType(int token)
		{
			return (int)(token & 0xff000000);
		}

		public static int GetTableTypeByTokenType(int tokenType)
		{
			return tokenType >> 24;
		}

		public static int GetTokenTypeByTableType(int tableType)
		{
			return tableType << 24;
		}

		public static string Print(int token)
		{
			return PrintType(GetType(token)) + ":" + GetRID(token).ToString();
		}

		public static string PrintType(int type)
		{
			switch (type)
			{
				case MetadataTokenType.Module:
					return "Module";

				case MetadataTokenType.TypeRef:
					return "TypeRef";

				case MetadataTokenType.TypeDef:
					return "TypeDef";

				case MetadataTokenType.Field:
					return "Field";

				case MetadataTokenType.Method:
					return "Method";

				case MetadataTokenType.Param:
					return "Param";

				case MetadataTokenType.InterfaceImpl:
					return "InterfaceImpl";

				case MetadataTokenType.MemberRef:
					return "MemberRef";

				case MetadataTokenType.CustomAttribute:
					return "CustomAttribute";

				case MetadataTokenType.DeclSecurity:
					return "DeclSecurity";

				case MetadataTokenType.Signature:
					return "Signature";

				case MetadataTokenType.Event:
					return "Event";

				case MetadataTokenType.Property:
					return "Property";

				case MetadataTokenType.ModuleRef:
					return "ModuleRef";

				case MetadataTokenType.TypeSpec:
					return "TypeSpec";

				case MetadataTokenType.Assembly:
					return "Assembly";

				case MetadataTokenType.AssemblyRef:
					return "AssemblyRef";

				case MetadataTokenType.File:
					return "File";

				case MetadataTokenType.ExportedType:
					return "ExportedType";

				case MetadataTokenType.ManifestResource:
					return "ManifestResource";

				case MetadataTokenType.GenericParam:
					return "GenericParam";

				case MetadataTokenType.MethodSpec:
					return "MethodSpec";

				case MetadataTokenType.GenericParamConstraint:
					return "GenericParamConstraint";

				case MetadataTokenType.String:
					return "String";

				case MetadataTokenType.Name:
					return "Name";

				default:
					return string.Format("Unknown({0})", type);
			}
		}

		#region Compress

		public static int Compress(int token, int type)
		{
			switch (type)
			{
				case CodedTokenType.TypeDefOrRef:
					return CompressTypeDefOrRef(token);

				case CodedTokenType.HasConstant:
					return CompressHasConstant(token);

				case CodedTokenType.HasCustomAttribute:
					return CompressHasCustomAttribute(token);

				case CodedTokenType.HasFieldMarshal:
					return CompressHasFieldMarshal(token);

				case CodedTokenType.HasDeclSecurity:
					return CompressHasDeclSecurity(token);

				case CodedTokenType.MemberRefParent:
					return CompressMemberRefParent(token);

				case CodedTokenType.HasSemantic:
					return CompressHasSemantic(token);

				case CodedTokenType.MethodDefOrRef:
					return CompressMethodDefOrRef(token);

				case CodedTokenType.MemberForwarded:
					return CompressMemberForwarded(token);

				case CodedTokenType.Implementation:
					return CompressImplementation(token);

				case CodedTokenType.CustomAttributeType:
					return CompressCustomAttributeType(token);

				case CodedTokenType.ResolutionScope:
					return CompressResolutionScope(token);

				case CodedTokenType.TypeOrMethodDef:
					return CompressTypeOrMethodDef(token);

				default:
					return 0;
			}
		}

		public static int CompressTypeDefOrRef(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 2;
			switch (GetType(token))
			{
				case MetadataTokenType.TypeDef:
					return value | 0;
				case MetadataTokenType.TypeRef:
					return value | 1;
				case MetadataTokenType.TypeSpec:
					return value | 2;
				default:
					return MetadataTokenType.TypeRef;
			}
		}

		public static int CompressHasConstant(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 2;
			switch (GetType(token))
			{
				case MetadataTokenType.Field:
					return value | 0;
				case MetadataTokenType.Param:
					return value | 1;
				case MetadataTokenType.Property:
					return value | 2;
				default:
					return 0;
			}
		}

		public static int CompressHasCustomAttribute(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 5;
			switch (GetType(token))
			{
				case MetadataTokenType.Method:
					return value | 0;
				case MetadataTokenType.Field:
					return value | 1;
				case MetadataTokenType.TypeRef:
					return value | 2;
				case MetadataTokenType.TypeDef:
					return value | 3;
				case MetadataTokenType.Param:
					return value | 4;
				case MetadataTokenType.InterfaceImpl:
					return value | 5;
				case MetadataTokenType.MemberRef:
					return value | 6;
				case MetadataTokenType.Module:
					return value | 7;
				case MetadataTokenType.DeclSecurity:
					return value | 8;
				case MetadataTokenType.Property:
					return value | 9;
				case MetadataTokenType.Event:
					return value | 10;
				case MetadataTokenType.Signature:
					return value | 11;
				case MetadataTokenType.ModuleRef:
					return value | 12;
				case MetadataTokenType.TypeSpec:
					return value | 13;
				case MetadataTokenType.Assembly:
					return value | 14;
				case MetadataTokenType.AssemblyRef:
					return value | 15;
				case MetadataTokenType.File:
					return value | 16;
				case MetadataTokenType.ExportedType:
					return value | 17;
				case MetadataTokenType.ManifestResource:
					return value | 18;
				case MetadataTokenType.GenericParam:
					return value | 19;
				default:
					return 0;
			}
		}

		public static int CompressHasFieldMarshal(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 1;
			switch (GetType(token))
			{
				case MetadataTokenType.Field:
					return value | 0;
				case MetadataTokenType.Param:
					return value | 1;
				default:
					return 0;
			}
		}

		public static int CompressHasDeclSecurity(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 2;
			switch (GetType(token))
			{
				case MetadataTokenType.TypeDef:
					return value | 0;
				case MetadataTokenType.Method:
					return value | 1;
				case MetadataTokenType.Assembly:
					return value | 2;
				default:
					return 0;
			}
		}

		public static int CompressMemberRefParent(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 3;
			switch (GetType(token))
			{
				case MetadataTokenType.TypeDef:
					return value | 0;
				case MetadataTokenType.TypeRef:
					return value | 1;
				case MetadataTokenType.ModuleRef:
					return value | 2;
				case MetadataTokenType.Method:
					return value | 3;
				case MetadataTokenType.TypeSpec:
					return value | 4;
				default:
					return 0;
			}
		}

		public static int CompressHasSemantic(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 1;
			switch (GetType(token))
			{
				case MetadataTokenType.Event:
					return value | 0;
				case MetadataTokenType.Property:
					return value | 1;
				default:
					return 0;
			}
		}

		public static int CompressMethodDefOrRef(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 1;
			switch (GetType(token))
			{
				case MetadataTokenType.Method:
					return value | 0;
				case MetadataTokenType.MemberRef:
					return value | 1;
				default:
					return 0;
			}
		}

		public static int CompressMemberForwarded(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 1;
			switch (GetType(token))
			{
				case MetadataTokenType.Field:
					return value | 0;
				case MetadataTokenType.Method:
					return value | 1;
				default:
					return 0;
			}
		}

		public static int CompressImplementation(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 2;
			switch (GetType(token))
			{
				case MetadataTokenType.File:
					return value | 0;
				case MetadataTokenType.AssemblyRef:
					return value | 1;
				case MetadataTokenType.ExportedType:
					return value | 2;
				default:
					return 0;
			}
		}

		public static int CompressCustomAttributeType(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 3;
			switch (GetType(token))
			{
				case MetadataTokenType.Method:
					return value | 2;
				case MetadataTokenType.MemberRef:
					return value | 3;
				default:
					return 0;
			}
		}

		public static int CompressResolutionScope(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 2;
			switch (GetType(token))
			{
				case MetadataTokenType.Module:
					return value | 0;
				case MetadataTokenType.ModuleRef:
					return value | 1;
				case MetadataTokenType.AssemblyRef:
					return value | 2;
				case MetadataTokenType.TypeRef:
					return value | 3;
				default:
					return 0;
			}
		}

		public static int CompressTypeOrMethodDef(int token)
		{
			int rid = GetRID(token);
			if (rid == 0)
				return 0;

			int value = rid << 1;
			switch (GetType(token))
			{
				case MetadataTokenType.TypeDef:
					return value | 0;
				case MetadataTokenType.Method:
					return value | 1;
				default:
					return 0;
			}
		}

		#endregion

		#region Decompress

		public static int Decompress(int type, int data)
		{
			switch (type)
			{
				case CodedTokenType.TypeDefOrRef:
					return DecompressTypeDefOrRef(data);

				case CodedTokenType.HasConstant:
					return DecompressHasConstant(data);

				case CodedTokenType.HasCustomAttribute:
					return DecompressHasCustomAttribute(data);

				case CodedTokenType.HasFieldMarshal:
					return DecompressHasFieldMarshal(data);

				case CodedTokenType.HasDeclSecurity:
					return DecompressHasDeclSecurity(data);

				case CodedTokenType.MemberRefParent:
					return DecompressMemberRefParent(data);

				case CodedTokenType.HasSemantic:
					return DecompressHasSemantic(data);

				case CodedTokenType.MethodDefOrRef:
					return DecompressMethodDefOrRef(data);

				case CodedTokenType.MemberForwarded:
					return DecompressMemberForwarded(data);

				case CodedTokenType.Implementation:
					return DecompressImplementation(data);

				case CodedTokenType.CustomAttributeType:
					return DecompressCustomAttributeType(data);

				case CodedTokenType.ResolutionScope:
					return DecompressResolutionScope(data);

				case CodedTokenType.TypeOrMethodDef:
					return DecompressTypeOrMethodDef(data);

				default:
					return MetadataToken.Null;
			}
		}

		public static int DecompressTypeDefOrRef(int data)
		{
			int rid = (data >> 2);
			if (rid > 0)
			{
				switch (data & 3)
				{
					case 0:
						return MetadataTokenType.TypeDef | rid;
					case 1:
						return MetadataTokenType.TypeRef | rid;
					case 2:
						return MetadataTokenType.TypeSpec | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressHasConstant(int data)
		{
			int rid = (data >> 2);
			if (rid > 0)
			{
				switch (data & 3)
				{
					case 0:
						return MetadataTokenType.Field | rid;
					case 1:
						return MetadataTokenType.Param | rid;
					case 2:
						return MetadataTokenType.Property | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressHasCustomAttribute(int data)
		{
			int rid = (data >> 5);
			if (rid > 0)
			{
				switch (data & 31)
				{
					case 0:
						return MetadataTokenType.Method | rid;
					case 1:
						return MetadataTokenType.Field | rid;
					case 2:
						return MetadataTokenType.TypeRef | rid;
					case 3:
						return MetadataTokenType.TypeDef | rid;
					case 4:
						return MetadataTokenType.Param | rid;
					case 5:
						return MetadataTokenType.InterfaceImpl | rid;
					case 6:
						return MetadataTokenType.MemberRef | rid;
					case 7:
						return MetadataTokenType.Module | rid;
					case 8:
						return MetadataTokenType.DeclSecurity | rid;
					case 9:
						return MetadataTokenType.Property | rid;
					case 10:
						return MetadataTokenType.Event | rid;
					case 11:
						return MetadataTokenType.Signature | rid;
					case 12:
						return MetadataTokenType.ModuleRef | rid;
					case 13:
						return MetadataTokenType.TypeSpec | rid;
					case 14:
						return MetadataTokenType.Assembly | rid;
					case 15:
						return MetadataTokenType.AssemblyRef | rid;
					case 16:
						return MetadataTokenType.File | rid;
					case 17:
						return MetadataTokenType.ExportedType | rid;
					case 18:
						return MetadataTokenType.ManifestResource | rid;
					case 19:
						return MetadataTokenType.GenericParam | rid;
					case 20:
						return MetadataTokenType.GenericParamConstraint | rid;
					case 21:
						return MetadataTokenType.MethodSpec | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressHasFieldMarshal(int data)
		{
			int rid = (data >> 1);
			if (rid > 0)
			{
				switch (data & 1)
				{
					case 0:
						return MetadataTokenType.Field | rid;
					case 1:
						return MetadataTokenType.Param | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressHasDeclSecurity(int data)
		{
			int rid = (data >> 2);
			if (rid > 0)
			{
				switch (data & 3)
				{
					case 0:
						return MetadataTokenType.TypeDef | rid;
					case 1:
						return MetadataTokenType.Method | rid;
					case 2:
						return MetadataTokenType.Assembly | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressMemberRefParent(int data)
		{
			int rid = (data >> 3);
			if (rid > 0)
			{
				switch (data & 7)
				{
					case 0:
						return MetadataTokenType.TypeDef | rid;
					case 1:
						return MetadataTokenType.TypeRef | rid;
					case 2:
						return MetadataTokenType.ModuleRef | rid;
					case 3:
						return MetadataTokenType.Method | rid;
					case 4:
						return MetadataTokenType.TypeSpec | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressHasSemantic(int data)
		{
			int rid = (data >> 1);
			if (rid > 0)
			{
				switch (data & 1)
				{
					case 0:
						return MetadataTokenType.Event | rid;
					case 1:
						return MetadataTokenType.Property | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressMethodDefOrRef(int data)
		{
			int rid = (data >> 1);
			if (rid > 0)
			{
				switch (data & 1)
				{
					case 0:
						return MetadataTokenType.Method | rid;
					case 1:
						return MetadataTokenType.MemberRef | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressMemberForwarded(int data)
		{
			int rid = (data >> 1);
			if (rid > 0)
			{
				switch (data & 1)
				{
					case 0:
						return MetadataTokenType.Field | rid;
					case 1:
						return MetadataTokenType.Method | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressImplementation(int data)
		{
			int rid = (data >> 2);
			if (rid > 0)
			{
				switch (data & 3)
				{
					case 0:
						return MetadataTokenType.File | rid;
					case 1:
						return MetadataTokenType.AssemblyRef | rid;
					case 2:
						return MetadataTokenType.ExportedType | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressCustomAttributeType(int data)
		{
			int rid = (data >> 3);
			if (rid > 0)
			{
				switch (data & 7)
				{
					case 2:
						return MetadataTokenType.Method | rid;
					case 3:
						return MetadataTokenType.MemberRef | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressResolutionScope(int data)
		{
			int rid = (data >> 2);
			if (rid > 0)
			{
				switch (data & 3)
				{
					case 0:
						return MetadataTokenType.Module | rid;
					case 1:
						return MetadataTokenType.ModuleRef | rid;
					case 2:
						return MetadataTokenType.AssemblyRef | rid;
					case 3:
						return MetadataTokenType.TypeRef | rid;
				}
			}

			return MetadataToken.Null;
		}

		public static int DecompressTypeOrMethodDef(int data)
		{
			int rid = (data >> 1);
			if (rid > 0)
			{
				switch (data & 1)
				{
					case 0:
						return MetadataTokenType.TypeDef | rid;
					case 1:
						return MetadataTokenType.Method | rid;
				}
			}

			return MetadataToken.Null;
		}

		#endregion

		private static readonly bool[] _tokenTypeValid = new bool[]
		{
			true, // Module,
			true, // TypeRef
			true, // TypeDef
			false, // FieldPtr
			true, // Field
			false, // MethodPtr
			true, // Method
			false, // ParamPtr
			true, // Param
			true, // InterfaceImpl
			true, // MemberRef
			false, // Constant
			true, // CustomAttribute
			false, // FieldMarshal
			true, // DeclSecurity
			false, // ClassLayout
			false, // FieldLayout
			true, // Signature
			false, // EventMap
			false, // EventPtr
			true, // Event
			false, // PropertyMap
			false, // PropertyPtr
			true, // Property
			false, // MethodSemantics
			false, // MethodImpl
			true, // ModuleRef
			true, // TypeSpec
			false, // ImplMap
			false, // FieldRVA
			false, // ENCLog
			false, // ENCMap
			true, // Assembly
			false, // AssemblyProcessor
			false, // AssemblyOS
			true, // AssemblyRef
			false, // AssemblyRefProcessor
			false, // AssemblyRefOS
			true, // File
			true, // ExportedType
			true, // ManifestResource
			false, // NestedClass
			true, // GenericParam,
			true, // MethodSpec
			true, // GenericParamConstraint
		};
	}
}
