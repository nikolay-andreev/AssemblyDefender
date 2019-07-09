using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Represents compressed metadata token.
	/// </summary>
	public static class CodedTokenType
	{
		/// <summary>
		/// 3 referenced tables; tag size 2
		/// </summary>
		public const int TypeDefOrRef = 64;

		/// <summary>
		/// 3 referenced tables; tag size 2
		/// </summary>
		public const int HasConstant = 65;

		/// <summary>
		/// 22 referenced tables; tag size 5
		/// </summary>
		public const int HasCustomAttribute = 66;

		/// <summary>
		/// 2 referenced tables; tag size 1
		/// </summary>
		public const int HasFieldMarshal = 67;

		/// <summary>
		/// 3 referenced tables; tag size 2
		/// </summary>
		public const int HasDeclSecurity = 68;

		/// <summary>
		/// 5 referenced tables; tag size 3
		/// </summary>
		public const int MemberRefParent = 69;

		/// <summary>
		/// 2 referenced tables; tag size 1
		/// </summary>
		public const int HasSemantic = 70;

		/// <summary>
		/// 2 referenced tables; tag size 1
		/// </summary>
		public const int MethodDefOrRef = 71;

		/// <summary>
		/// 2 referenced tables; tag size 1
		/// </summary>
		public const int MemberForwarded = 72;

		/// <summary>
		/// 3 referenced tables; tag size 2
		/// </summary>
		public const int Implementation = 73;

		/// <summary>
		/// 5 referenced tables; tag size 3
		/// </summary>
		public const int CustomAttributeType = 74;

		/// <summary>
		/// 4 referenced tables; tag size 2
		/// </summary>
		public const int ResolutionScope = 75;

		/// <summary>
		/// 2 referenced tables; tag size 1
		/// </summary>
		public const int TypeOrMethodDef = 76;
	}
}
