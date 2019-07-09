using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Specifies type attributes.
	/// </summary>
	public static class TypeDefFlags
	{
		#region Visibility flags (binary mask 0x00000007)

		public const int VisibilityMask = 0x00000007;

		/// <summary>
		/// The type is not visible outside the assembly. This is the default.
		/// </summary>
		public const int Private = 0;

		/// <summary>
		/// The type is visible outside the assembly.
		/// </summary>
		public const int Public = 0x1;

		/// <summary>
		/// The nested type has public visibility.
		/// </summary>
		public const int NestedPublic = 0x2;

		/// <summary>
		/// The nested type has private visibility, it is not visible outside the enclosing class.
		/// </summary>
		public const int NestedPrivate = 0x3;

		/// <summary>
		/// The nested type has family visibility—that is, it is visible to descendants
		/// of the enclosing class only.
		/// </summary>
		public const int NestedFamily = 0x4;

		/// <summary>
		/// The nested type is visible within the assembly only.
		/// </summary>
		public const int NestedAssembly = 0x5;

		/// <summary>
		/// The nested type is visible to the descendants of the enclosing class residing in the same assembly.
		/// </summary>
		public const int NestedFamAndAssem = 0x6;

		/// <summary>
		/// The nested type is visible to the descendants of the enclosing class either within
		/// or outside the assembly and to every type within the assembly with no regard to "lineage".
		/// </summary>
		public const int NestedFamOrAssem = 0x7;

		#endregion

		#region Layout flags (binary mask 0x00000018)

		public const int LayoutMask = 0x00000018;

		/// <summary>
		/// The type fields are laid out automatically, at the loader’s discretion. This is the default.
		/// </summary>
		public const int Auto = 0;

		/// <summary>
		/// The loader shall preserve the order of the instance fields.
		/// </summary>
		public const int Sequential = 0x8;

		/// <summary>
		/// The type layout is specified explicitly, and the loader shall follow it.
		/// </summary>
		public const int Explicit = 0x10;

		#endregion

		#region Type semantics flags (binary mask 0x000005A0)

		public const int SemanticsMask = 0x000005A0;

		/// <summary>
		/// The type is an interface. If this flag is not specified, the type is presumed to be a class
		/// or a value type, if this flag is specified, the default parent (the class that is assumed to be
		/// the parent if the extends clause is not specified, usually [mscorlib]System.Object) is set to nil.
		/// </summary>
		public const int Interface = 0x20;

		/// <summary>
		/// The class is abstract—for example, it has abstract member methods. As such, this class
		/// cannot be instantiated and can be used only as a parent of another type or types.
		/// This flag is invalid for value types.
		/// </summary>
		public const int Abstract = 0x80;

		/// <summary>
		/// No types can be derived from this type. All value types and enumerations must carry this flag.
		/// </summary>
		public const int Sealed = 0x100;

		/// <summary>
		/// The type has a special name. How special it is depends on the name itself. This flag indicates
		/// to the metadata API and the loader that the name has a meaning in which they might be
		/// interested—for instance, _Deleted*.
		/// </summary>
		public const int SpecialName = 0x400;

		#endregion

		#region Type implementation flags (binary mask 0x00103000)

		public const int ImplementationMask = 0x00103000;

		/// <summary>
		/// The type (a class or an interface) is imported from a COM type library.
		/// </summary>
		public const int Import = 0x1000;

		/// <summary>
		/// The type can be serialized into sequential data by the serializer provided in the
		/// Microsoft .NET Framework class library.
		/// </summary>
		public const int Serializable = 0x2000;

		/// <summary>
		/// The type can be initialized (its .cctor run) any time before the first access to a static field.
		/// If this flag is not set, the type is initialized before the first access to one of its
		/// static fields or methods or before the first instantiation of the type.
		/// </summary>
		public const int BeforeFieldInit = 0x100000;

		#endregion

		#region String formatting flags (binary mask 0x00030000)

		public const int CharSetMask = 0x00030000;

		/// <summary>
		/// When interoperating with native methods, the managed strings are by default marshaled to
		/// and from ANSI strings. Managed strings are instances of the System.String class defined
		/// in the .NET Framework class library. Marshaling is a general term for data conversion on the
		/// managed and unmanaged code boundaries.
		/// </summary>
		public const int Ansi = 0;

		/// <summary>
		/// By default, managed strings are marshaled to and from Unicode (UTF-16).
		/// </summary>
		public const int Unicode = 0x10000;

		/// <summary>
		/// The default string marshaling is defined by the underlying platform.
		/// </summary>
		public const int Autochar = 0x20000;

		#endregion

		#region Reserved flags (binary mask 0x0004080)

		public const int ReservedMask = 0x0004080;

		/// <summary>
		/// The name is reserved by the common language runtime and has a special meaning.
		/// This flag is legal only in combination with the specialname flag. The keyword rtspecialname
		/// has no effect in ILAsm and is provided for informational purposes only.
		/// The IL disassembler uses this keyword to  show the presence of this reserved flag.
		/// Reserved flags cannot be set at will—this flag, for example, is set automatically by the
		/// metadata emission API when it emits an item with the specialname flag set and the name
		/// recognized as specific to the common language runtime, for example .ctor or .cctor.
		/// </summary>
		public const int RTSpecialName = 0x800;

		/// <summary>
		/// The type has declarative security metadata associated with it. This flag is set by the
		/// metadata emission API when respective declarative security metadata is emitted.
		/// </summary>
		public const int HasSecurity = 0x40000;

		/// <summary>
		/// This flag is used by exported type only when type is defined in another assembly.
		/// </summary>
		public const int Forwarder = 0x00200000;

		#endregion
	}
}
