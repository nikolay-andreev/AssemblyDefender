using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum FieldVisibilityFlags : int
	{
		/// <summary>
		/// This is the default accessibility. A private scope field is exempt from the requirement of
		/// having a unique triad of owner, name, and signature and hence must always be referenced by a
		/// FieldDef token and never by a MemberRef token (0x0A000000), because member references are resolved
		/// to the definitions by exactly this triad. The privatescope fields are accessible from anywhere
		/// within  current module.
		/// keyword: privatescope
		/// </summary>
		PrivateScope = 0,

		/// <summary>
		/// The field is accessible from its owner and from classes nested in the field’s owner.
		/// Global private fields are accessible from anywhere within current module.
		/// keyword: private
		/// </summary>
		Private = 0x1,

		/// <summary>
		/// The field is accessible from types belonging to the owner’s family defined in the
		/// current assembly. The term family here means the type itself and all its descendants.
		/// keyword: famandassem
		/// </summary>
		FamAndAssem = 0x2,

		/// <summary>
		/// The field is accessible from types defined in the current assembly.
		/// keyword: assembly
		/// </summary>
		Assembly = 0x3,

		/// <summary>
		/// The field is accessible from the owner’s family (defined in this or any other assembly).
		/// keyword: family
		/// </summary>
		Family = 0x4,

		/// <summary>
		/// The field is accessible from the owner’s family (defined in this or any other assembly)
		/// and from all types (of the owner’s family or not) defined in the current assembly.
		/// keyword: famorassem
		/// </summary>
		FamOrAssem = 0x5,

		/// <summary>
		/// The field is accessible from any type.
		/// keyword: public
		/// </summary>
		Public = 0x6,
	}
}
