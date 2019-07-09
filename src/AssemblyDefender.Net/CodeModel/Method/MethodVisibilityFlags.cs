using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum MethodVisibilityFlags : int
	{
		/// <summary>
		/// This is the default accessibility. A private scope method is exempt from the requirement
		/// of having a unique triad of owner, name, and signature and hence must always be referenced
		/// by a MethodDef token and never by a MemberRef token. The privatescope methods are
		/// accessible (callable) from anywhere within current module.
		/// </summary>
		PrivateScope = 0,

		/// <summary>
		/// The method is accessible from its owner class and from classes nested in the method’s owner.
		/// </summary>
		Private = 0x1,

		/// <summary>
		/// The method is accessible from types belonging to the owner’s family—that is,
		/// the owner itself and all its descendants—defined in the current assembly.
		/// </summary>
		FamAndAssem = 0x2,

		/// <summary>
		/// The method is accessible from types defined in the current assembly.
		/// </summary>
		Assembly = 0x3,

		/// <summary>
		/// The method is accessible from the owner’s family.
		/// </summary>
		Family = 0x4,

		/// <summary>
		/// The method is accessible from the owner’s family and from all types defined in the current assembly.
		/// </summary>
		FamOrAssem = 0x5,

		/// <summary>
		/// The method is accessible from any type.
		/// </summary>
		Public = 0x6,
	}
}
