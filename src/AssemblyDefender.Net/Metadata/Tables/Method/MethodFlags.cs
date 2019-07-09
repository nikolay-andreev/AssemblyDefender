using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Specifies flags for method attributes. These flags are defined in the corhdr.h file.
	/// </summary>
	public static class MethodFlags
	{
		#region Accessibility flags (mask 0x0007)

		public const int VisibilityMask = 0x0007;

		/// <summary>
		/// This is the default accessibility. A private scope method is exempt from the requirement
		/// of having a unique triad of owner, name, and signature and hence must always be referenced
		/// by a MethodDef token and never by a MemberRef token. The privatescope methods are
		/// accessible (callable) from anywhere within current module.
		/// </summary>
		public const int PrivateScope = 0;

		/// <summary>
		/// The method is accessible from its owner class and from classes nested in the method’s owner.
		/// </summary>
		public const int Private = 0x1;

		/// <summary>
		/// The method is accessible from types belonging to the owner’s family—that is,
		/// the owner itself and all its descendants—defined in the current assembly.
		/// </summary>
		public const int FamAndAssem = 0x2;

		/// <summary>
		/// The method is accessible from types defined in the current assembly.
		/// </summary>
		public const int Assembly = 0x3;

		/// <summary>
		/// The method is accessible from the owner’s family.
		/// </summary>
		public const int Family = 0x4;

		/// <summary>
		/// The method is accessible from the owner’s family and from all types defined in the current assembly.
		/// </summary>
		public const int FamOrAssem = 0x5;

		/// <summary>
		/// The method is accessible from any type.
		/// </summary>
		public const int Public = 0x6;

		#endregion

		#region Contract flags (mask 0x00F0)

		public const int ContractMask = 0x00F0;

		/// <summary>
		/// The method is static, shared by all instances of the type.
		/// </summary>
		public const int Static = 0x10;

		/// <summary>
		/// The method cannot be overridden. This flag must be paired with the virtual flag,
		/// otherwise, it is meaningless and is ignored.
		/// </summary>
		public const int Final = 0x20;

		/// <summary>
		/// The method is virtual. This flag cannot be paired with the static flag.
		/// </summary>
		public const int Virtual = 0x40;

		/// <summary>
		/// The method hides all methods of the parent classes that have a matching signature
		/// and  name (as opposed to having a matching name only). This flag is ignored by the
		/// common language  runtime and is provided for the use of compilers only.
		/// The IL assembler recognizes this flag but  does not use it.
		/// </summary>
		public const int HideBySig = 0x80;

		#endregion

		#region Virtual method table (v-table) control flags (mask 0x0300)

		public const int VirtualMethodMask = 0x0300;

		/// <summary>
		/// A new slot is created in the class’s v-table for this virtual method
		/// so that it does not override the virtual method of the same name and signature this
		/// class inherited from its base class. This flag can be used only in conjunction with
		/// the virtual flag.
		/// </summary>
		public const int NewSlot = 0x100;

		/// <summary>
		/// This virtual method can be overridden only if it is accessible from the overriding class.
		/// This flag can be used only in conjunction with the virtual flag.
		/// </summary>
		public const int Strict = 0x200;

		#endregion

		#region Implementation flags (mask 0x2C08)

		public const int ImplementationMask = 0x2C08;

		/// <summary>
		/// The method is abstract, no implementation is provided. This method must be overridden
		/// by the nonabstract descendants of the class owning the abstract method. Any class owning
		/// an abstract method must have its own abstract flag set. The RVA entry of an abstract method
		/// record must be 0.
		/// </summary>
		public const int Abstract = 0x400;

		/// <summary>
		/// The method is special in some way, as described by the name.
		/// </summary>
		public const int SpecialName = 0x800;

		/// <summary>
		/// The method has an unmanaged implementation and is called through the platform invocation
		/// mechanism P/Invoke. 'pinvoke_spec' in parentheses defines the implementation map,
		/// which is a record in the ImplMap metadata table specifying the unmanaged DLL exporting the
		/// method and the method’s unmanaged calling convention. If the DLL name in 'pinvoke_spec' is provided,
		/// the method’s RVA must be 0, because the method is implemented externally.
		/// If the DLL name is not specified or the 'pinvoke_spec' itself is not provided—that is,
		/// the parentheses are empty — the defined method is a local P/Invoke, implemented in unmanaged
		/// native code embedded in the current PE file, in this case, its RVA must not be 0 and must
		/// point to the location, in the current PE file, of the native method’s body.
		/// </summary>
		public const int PInvokeImpl = 0x2000;

		/// <summary>
		/// The managed method is exposed as an unmanaged export.
		/// This flag is not currently used by the common language runtime.
		/// </summary>
		public const int UnmanagedExp = 0x8;

		#endregion

		#region Reserved flags (cannot be set explicitly, mask 0xD000)

		public const int ReservedMask = 0xD000;

		/// <summary>
		/// The method has a special name reserved for the internal use of the runtime.
		/// Four method names are reserved: .ctor for instance constructors, .cctor for class constructors,
		/// _VtblGap* for v-table placeholders, and _Deleted* for methods marked for deletion but not
		/// actually removed from metadata. The keyword rtspecialname is ignored by the IL assembler
		/// and is displayed by the IL disassembler for informational purposes only.
		/// This flag must be accompanied by a specialname flag.
		/// </summary>
		public const int RTSpecialName = 0x1000;

		/// <summary>
		/// The method either has an associated DeclSecurity metadata record that holds security details
		/// concerning access to the method or has the associated custom attribute
		/// System.Security.SuppressUnmanagedCodeSecurityAttribute.
		/// </summary>
		public const int HasSecurity = 0x4000;

		/// <summary>
		/// This method calls another method containing security code, so it requires an additional
		/// stack slot for a security object. This flag is formally under the Reserved mask,
		/// so it cannot be set explicitly. Setting this flag requires emitting the pseudocustom attribute
		/// System.Security.DynamicSecurityMethodAttribute. When the IL assembler
		/// encounters the keyword reqsecobj, it does exactly that: emits the pseudocustom
		/// attribute and thus sets this "reserved" flag.
		/// </summary>
		public const int RequireSecObject = 0x8000;

		#endregion
	}
}
