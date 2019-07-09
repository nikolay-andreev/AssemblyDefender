using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Predefined field attributes.
	/// </summary>
	public static class FieldFlags
	{
		#region Visibility flags (mask 0x0007)

		public const int VisibilityMask = 0x0007;

		/// <summary>
		/// This is the default accessibility. A private scope field is exempt from the requirement of
		/// having a unique triad of owner, name, and signature and hence must always be referenced by a
		/// FieldDef token and never by a MemberRef token (0x0A000000), because member references are resolved
		/// to the definitions by exactly this triad. The privatescope fields are accessible from anywhere
		/// within  current module.
		/// keyword: privatescope
		/// </summary>
		public const int PrivateScope = 0;

		/// <summary>
		/// The field is accessible from its owner and from classes nested in the field’s owner.
		/// Global private fields are accessible from anywhere within current module.
		/// keyword: private
		/// </summary>
		public const int Private = 0x1;

		/// <summary>
		/// The field is accessible from types belonging to the owner’s family defined in the
		/// current assembly. The term family here means the type itself and all its descendants.
		/// keyword: famandassem
		/// </summary>
		public const int FamAndAssem = 0x2;

		/// <summary>
		/// The field is accessible from types defined in the current assembly.
		/// keyword: assembly
		/// </summary>
		public const int Assembly = 0x3;

		/// <summary>
		/// The field is accessible from the owner’s family (defined in this or any other assembly).
		/// keyword: family
		/// </summary>
		public const int Family = 0x4;

		/// <summary>
		/// The field is accessible from the owner’s family (defined in this or any other assembly)
		/// and from all types (of the owner’s family or not) defined in the current assembly.
		/// keyword: famorassem
		/// </summary>
		public const int FamOrAssem = 0x5;

		/// <summary>
		/// The field is accessible from any type.
		/// keyword: public
		/// </summary>
		public const int Public = 0x6;

		#endregion

		#region Contract flags (mask 0x02F0)

		public const int ContractMask = 0x02F0;

		/// <summary>
		/// The field is static, shared by all instances of the type. Global fields must be static.
		/// keyword: static
		/// </summary>
		public const int Static = 0x10;

		/// <summary>
		/// The field can be initialized only and cannot be written to later. Initialization takes place
		/// in an instance constructor (.ctor) for instance fields and in a class constructor (.cctor)
		/// for static fields. This flag is not enforced by the CLR, it exists for the compilers’ reference only.
		/// keyword: initonly
		/// </summary>
		public const int InitOnly = 0x20;

		/// <summary>
		/// The field is a compile-time constant. The loader does not lay out this field and does not
		/// create an internal handle for it. The field cannot be directly addressed from IL and can be
		/// used only as a Reflection reference to retrieve an associated metadata-held constant.
		/// If you try to access a literal field directly—for example, through the ldsfld instruction —
		/// the JIT compiler throws a MissingField exception and aborts the task.
		/// keyword: literal
		/// </summary>
		public const int Literal = 0x40;

		/// <summary>
		/// The field is not serialized when the owner is remoted. This flag has meaning only for
		/// instance fields of the serializable types.
		/// keyword: notserialized
		/// </summary>
		public const int NotSerialized = 0x80;

		/// <summary>
		/// The field is special in some way, as defined by the name.
		/// An example is field value__ of an enumeration type.
		/// keyword: specialname
		/// </summary>
		public const int SpecialName = 0x200;

		#endregion

		#region Reserved flags (cannot be set explicitly, mask 0x9500)

		public const int ReservedMask = 0x9500;

		/// <summary>
		/// The field has a special name that is reserved for the internal use of the common language runtime.
		/// Two field names are reserved: value_, for instance fields in enumerations, and _Deleted*,
		/// for fields marked for deletion but not actually removed from metadata. The keyword rtspecialname
		/// is ignored by the IL assembler (the flag is actually set automatically by the metadata emission API)
		/// and is displayed by the IL disassembler for informational purposes only. This flag must be
		/// accompanied in the metadata by a specialname flag.
		/// keyword: rtspecialname
		/// </summary>
		public const int RTSpecialName = 0x400;

		/// <summary>
		/// The field has an associated FieldMarshal record specifying how the field must be marshaled
		/// when consumed by unmanaged code. The ILAsm construct marshal('native_type') defines the
		/// marshaling information emitted to the FieldMarshal table but does not set the flag directly.
		/// Rather, the flag is set behind the scenes by the metadata emission API when the marshaling
		/// information is emitted.
		/// keyword: marshal
		/// </summary>
		public const int HasFieldMarshal = 0x1000;

		/// <summary>
		/// The field has an associated Constant record. The flag is set by the metadata emission API
		/// when the respective Constant record is emitted.
		/// keyword: None
		/// </summary>
		public const int HasDefault = 0x8000;

		/// <summary>
		/// The field is mapped to data and has an associated FieldRVA record. The flag is set by the
		/// metadata emission API when the respective FieldRVA record is emitted.
		/// keyword: None
		/// </summary>
		public const int HasFieldRVA = 0x100;

		#endregion
	}
}
