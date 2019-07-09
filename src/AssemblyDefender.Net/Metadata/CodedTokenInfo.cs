using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public struct CodedTokenInfo
	{
		#region Fields

		private int _tag;
		private int[] _tokenTypes;

		#endregion

		#region Ctors

		public CodedTokenInfo(int tag, int[] tokenTypes)
		{
			_tag = tag;
			_tokenTypes = tokenTypes;
		}

		#endregion

		#region Properties

		public int Tag
		{
			get { return _tag; }
		}

		public int[] TokenTypes
		{
			get { return _tokenTypes; }
		}

		#endregion

		#region Static

		public static CodedTokenInfo Get(int type)
		{
			return _tokens[type - 64];
		}

		public static string PrintType(int type)
		{
			switch (type)
			{
				case CodedTokenType.TypeDefOrRef:
					return "TypeDefOrRef";

				case CodedTokenType.HasConstant:
					return "HasConstant";

				case CodedTokenType.HasCustomAttribute:
					return "HasCustomAttribute";

				case CodedTokenType.HasFieldMarshal:
					return "HasFieldMarshal";

				case CodedTokenType.HasDeclSecurity:
					return "HasDeclSecurity";

				case CodedTokenType.MemberRefParent:
					return "MemberRefParent";

				case CodedTokenType.HasSemantic:
					return "HasSemantic";

				case CodedTokenType.MethodDefOrRef:
					return "MethodDefOrRef";

				case CodedTokenType.MemberForwarded:
					return "MemberForwarded";

				case CodedTokenType.Implementation:
					return "Implementation";

				case CodedTokenType.CustomAttributeType:
					return "CustomAttributeType";

				case CodedTokenType.ResolutionScope:
					return "ResolutionScope";

				case CodedTokenType.TypeOrMethodDef:
					return "TypeOrMethodDef";

				default:
					return string.Format("Unknown({0})", type);
			}
		}

		private static CodedTokenInfo[] _tokens = new CodedTokenInfo[]
		{
				new CodedTokenInfo(
					2,
					new int[]
					{
						MetadataTokenType.TypeDef,
						MetadataTokenType.TypeRef,
						MetadataTokenType.TypeSpec,
					}),

				new CodedTokenInfo(
					2,
					new int[]
					{
						MetadataTokenType.Field,
						MetadataTokenType.Param,
						MetadataTokenType.Property,
					}),

				new CodedTokenInfo(
					5,
					new int[]
					{
						MetadataTokenType.Method,
						MetadataTokenType.Field,
						MetadataTokenType.TypeRef,
						MetadataTokenType.TypeDef,
						MetadataTokenType.Param,
						MetadataTokenType.InterfaceImpl,
						MetadataTokenType.MemberRef,
						MetadataTokenType.Module,
						MetadataTokenType.DeclSecurity,
						MetadataTokenType.Property,
						MetadataTokenType.Event,
						MetadataTokenType.Signature,
						MetadataTokenType.ModuleRef,
						MetadataTokenType.TypeSpec,
						MetadataTokenType.Assembly,
						MetadataTokenType.AssemblyRef,
						MetadataTokenType.File,
						MetadataTokenType.ExportedType,
						MetadataTokenType.ManifestResource,
						MetadataTokenType.GenericParam,
						MetadataTokenType.GenericParamConstraint,
						MetadataTokenType.MethodSpec,
					}),

				new CodedTokenInfo(
					1,
					new int[]
					{
						MetadataTokenType.Field,
						MetadataTokenType.Param,
					}),

				new CodedTokenInfo(
					2,
					new int[]
					{
						MetadataTokenType.TypeDef,
						MetadataTokenType.Method,
						MetadataTokenType.Assembly,
					}),

				new CodedTokenInfo(
					3,
					new int[]
					{
						MetadataTokenType.TypeDef,
						MetadataTokenType.TypeRef,
						MetadataTokenType.ModuleRef,
						MetadataTokenType.Method,
						MetadataTokenType.TypeSpec,
					}),

				new CodedTokenInfo(
					1,
					new int[]
					{
						MetadataTokenType.Event,
						MetadataTokenType.Property,
					}),

				new CodedTokenInfo(
					1,
					new int[]
					{
						MetadataTokenType.Method,
						MetadataTokenType.MemberRef,
					}),

				new CodedTokenInfo(
					1,
					new int[]
					{
						MetadataTokenType.Field,
						MetadataTokenType.Method,
					}),

				new CodedTokenInfo(
					2,
					new int[]
					{
						MetadataTokenType.File,
						MetadataTokenType.AssemblyRef,
						MetadataTokenType.ExportedType,
					}),

				new CodedTokenInfo(
					3,
					new int[]
					{
						MetadataTokenType.Method,
						MetadataTokenType.MemberRef,
					}),

				new CodedTokenInfo(
					2,
					new int[]
					{
						MetadataTokenType.Module,
						MetadataTokenType.ModuleRef,
						MetadataTokenType.AssemblyRef,
						MetadataTokenType.TypeRef,
					}),

				new CodedTokenInfo(
					1,
					new int[]
					{
						MetadataTokenType.TypeDef,
						MetadataTokenType.Method,
					}),
		};

		#endregion
	}
}
