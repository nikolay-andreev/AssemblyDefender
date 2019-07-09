using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Define all 45 tabeles with value as zero based table index.
	/// </summary>
	public static class MetadataTableType
	{
		/// <summary>
		/// The current module descriptor.
		/// </summary>
		public const int Module = 0;

		/// <summary>
		/// Class reference descriptors.
		/// </summary>
		public const int TypeRef = 1;

		/// <summary>
		/// Class or interface definition descriptors.
		/// </summary>
		public const int TypeDef = 2;

		/// <summary>
		/// A class-to-fields lookup table, which does not exist in optimized metadata
		/// </summary>
		public const int FieldPtr = 3;

		/// <summary>
		/// Field definition descriptors.
		/// </summary>
		public const int Field = 4;

		/// <summary>
		/// A class-to-methods lookup table, which does not exist in optimized metadata (#~ stream).
		/// </summary>
		public const int MethodPtr = 5;

		/// <summary>
		/// Method definition descriptors.
		/// </summary>
		public const int MethodDef = 6;

		/// <summary>
		/// A method-to-parameters lookup table, which does not exist in optimized metadata (#~ stream).
		/// </summary>
		public const int ParamPtr = 7;

		/// <summary>
		/// Parameter definition descriptors.
		/// </summary>
		public const int Param = 8;

		/// <summary>
		/// Interface implementation descriptors.
		/// </summary>
		public const int InterfaceImpl = 9;

		/// <summary>
		/// Member (field or method) reference descriptors.
		/// </summary>
		public const int MemberRef = 10;

		/// <summary>
		/// Constant value descriptors that map the default values stored in the #Blob
		/// stream to respective fields, parameters, and properties.
		/// </summary>
		public const int Constant = 11;

		/// <summary>
		/// Custom attribute descriptors.
		/// </summary>
		public const int CustomAttribute = 12;

		/// <summary>
		/// Field or parameter marshaling descriptors for managed/unmanaged interoperations.
		/// </summary>
		public const int FieldMarshal = 13;

		/// <summary>
		/// Security descriptors.
		/// </summary>
		public const int DeclSecurity = 14;

		/// <summary>
		/// Class layout descriptors that hold information about how the loader
		/// should lay out respective classes.
		/// </summary>
		public const int ClassLayout = 15;

		/// <summary>
		/// Field layout descriptors that specify the offset or ordinal of individual fields.
		/// </summary>
		public const int FieldLayout = 16;

		/// <summary>
		/// Stand-alone signature descriptors. Signatures per se are used in two
		/// capacities: as composite signatures of local variables of methods and as parameters of the
		/// call indirect (calli) IL instruction.
		/// </summary>
		public const int StandAloneSig = 17;

		/// <summary>
		/// A class-to-events mapping table. This is not an intermediate lookup table,
		/// and it does exist in optimized metadata.
		/// </summary>
		public const int EventMap = 18;

		/// <summary>
		/// An event map to events lookup table, which does not exist in optimized metadata (#~ stream).
		/// </summary>
		public const int EventPtr = 19;

		/// <summary>
		/// Event descriptors.
		/// </summary>
		public const int Event = 20;

		/// <summary>
		/// A class-to-properties mapping table. This is not an intermediate
		/// lookup table, and it does exist in optimized metadata.
		/// </summary>
		public const int PropertyMap = 21;

		/// <summary>
		/// A property map to properties lookup table, which does not exist in
		/// optimized metadata (#~ stream).
		/// </summary>
		public const int PropertyPtr = 22;

		/// <summary>
		/// Property descriptors.
		/// </summary>
		public const int Property = 23;

		/// <summary>
		/// Method semantics descriptors that hold information about which
		/// method is associated with a specific property or event and in what capacity.
		/// </summary>
		public const int MethodSemantics = 24;

		/// <summary>
		/// Method implementation descriptors.
		/// </summary>
		public const int MethodImpl = 25;

		/// <summary>
		/// Module reference descriptors.
		/// </summary>
		public const int ModuleRef = 26;

		/// <summary>
		/// Type specification descriptors.
		/// </summary>
		public const int TypeSpec = 27;

		/// <summary>
		/// Implementation map descriptors used for the platform invocation
		/// (P/Invoke) type of managed/unmanaged code interoperation.
		/// </summary>
		public const int ImplMap = 28;

		/// <summary>
		/// Field-to-data mapping descriptors.
		/// </summary>
		public const int FieldRVA = 29;

		/// <summary>
		/// Edit-and-continue log descriptors that hold information about what
		/// changes have been made to specific metadata items during in-memory editing. This
		/// table does not exist in optimized metadata (#~ stream).
		/// </summary>
		public const int ENCLog = 30;

		/// <summary>
		/// Edit-and-continue mapping descriptors. This table does not exist in
		/// optimized metadata (#~ stream).
		/// </summary>
		public const int ENCMap = 31;

		/// <summary>
		/// The current assembly descriptor, which should appear only in the prime module metadata.
		/// </summary>
		public const int Assembly = 32;

		/// <summary>
		/// This table is unused.
		/// </summary>
		public const int AssemblyProcessor = 33;

		/// <summary>
		/// This table is unused.
		/// </summary>
		public const int AssemblyOS = 34;

		/// <summary>
		/// Assembly reference descriptors.
		/// </summary>
		public const int AssemblyRef = 35;

		/// <summary>
		/// This table is unused.
		/// </summary>
		public const int AssemblyRefProcessor = 36;

		/// <summary>
		/// This table is unused.
		/// </summary>
		public const int AssemblyRefOS = 37;

		/// <summary>
		/// File descriptors that contain information about other files in the current assembly.
		/// </summary>
		public const int File = 38;

		/// <summary>
		/// Exported type descriptors that contain information about public
		/// classes exported by the current assembly, which are declared in other modules of the
		/// assembly. Only the prime module of the assembly should carry this table.
		/// </summary>
		public const int ExportedType = 39;

		/// <summary>
		/// Managed resource descriptors.
		/// </summary>
		public const int ManifestResource = 40;

		/// <summary>
		/// Nested class descriptors that provide mapping of nested classes to their
		/// respective enclosing classes.
		/// </summary>
		public const int NestedClass = 41;

		/// <summary>
		/// Type parameter descriptors for generic (parameterized) classes and methods.
		/// </summary>
		public const int GenericParam = 42;

		/// <summary>
		/// Generic method instantiation descriptors.
		/// </summary>
		public const int MethodSpec = 43;

		/// <summary>
		/// Descriptors of constraints specified for type parameters of generic classes and methods.
		/// </summary>
		public const int GenericParamConstraint = 44;
	}
}
