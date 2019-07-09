using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The 22 tables that do not have associated token types are not intended to be accessed
	/// from "outside", through metadata APIs or from IL code. These tables are of an auxiliary or
	/// intermediate nature and should be accessed indirectly only, through the references contained
	/// in the "exposed" tables, which have associated token types.
	/// </summary>
	public static class MetadataTokenType
	{
		public const int Null = 0;

		/// <summary>
		/// Table Module
		/// </summary>
		public const int Module = 0x00000000;

		/// <summary>
		/// Table TypeRef
		/// </summary>
		public const int TypeRef = 0x01000000;

		/// <summary>
		/// Table TypeDef
		/// </summary>
		public const int TypeDef = 0x02000000;

		/// <summary>
		/// Table Field
		/// </summary>
		public const int Field = 0x04000000;

		/// <summary>
		/// Table Method
		/// </summary>
		public const int Method = 0x06000000;

		/// <summary>
		/// Table Param
		/// </summary>
		public const int Param = 0x08000000;

		/// <summary>
		/// Table InterfaceImpl
		/// </summary>
		public const int InterfaceImpl = 0x09000000;

		/// <summary>
		/// Table MemberRef
		/// </summary>
		public const int MemberRef = 0x0A000000;

		/// <summary>
		/// Table CustomAttribute
		/// </summary>
		public const int CustomAttribute = 0x0C000000;

		/// <summary>
		/// Table DeclSecurity
		/// </summary>
		public const int DeclSecurity = 0x0E000000;

		/// <summary>
		/// Table StandAloneSig
		/// </summary>
		public const int Signature = 0x11000000;

		/// <summary>
		/// Table Event
		/// </summary>
		public const int Event = 0x14000000;

		/// <summary>
		/// Table Property
		/// </summary>
		public const int Property = 0x17000000;

		/// <summary>
		/// Table ModuleRef
		/// </summary>
		public const int ModuleRef = 0x1A000000;

		/// <summary>
		/// Table TypeSpec
		/// </summary>
		public const int TypeSpec = 0x1B000000;

		/// <summary>
		/// Table Assembly
		/// </summary>
		public const int Assembly = 0x20000000;

		/// <summary>
		/// Table AssemblyRef
		/// </summary>
		public const int AssemblyRef = 0x23000000;

		/// <summary>
		/// Table File
		/// </summary>
		public const int File = 0x26000000;

		/// <summary>
		/// Table ExportedType
		/// </summary>
		public const int ExportedType = 0x27000000;

		/// <summary>
		/// Table ManifestResource
		/// </summary>
		public const int ManifestResource = 0x28000000;

		/// <summary>
		/// Table GenericParam
		/// </summary>
		public const int GenericParam = 0x2A000000;

		/// <summary>
		/// Table MethodSpec
		/// </summary>
		public const int MethodSpec = 0x2B000000;

		/// <summary>
		/// Table GenericParamConstraint
		/// </summary>
		public const int GenericParamConstraint = 0x2C000000;

		/// <summary>
		/// Internal. User string heap and rid is an index.
		/// </summary>
		public const int String = 0x70000000;

		/// <summary>
		/// Internal.
		/// </summary>
		public const int Name = 0x71000000;
	}
}
