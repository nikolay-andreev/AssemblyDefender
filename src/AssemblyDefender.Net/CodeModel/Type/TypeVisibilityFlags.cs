using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum TypeVisibilityFlags : int
	{
		/// <summary>
		/// The type is not visible outside the assembly. This is the default.
		/// </summary>
		Private = 0,

		/// <summary>
		/// The type is visible outside the assembly.
		/// </summary>
		Public = 0x1,

		/// <summary>
		/// The nested type has public visibility.
		/// </summary>
		NestedPublic = 0x2,

		/// <summary>
		/// The nested type has private visibility, it is not visible outside the enclosing class.
		/// </summary>
		NestedPrivate = 0x3,

		/// <summary>
		/// The nested type has family visibility—that is, it is visible to descendants
		/// of the enclosing class only.
		/// </summary>
		NestedFamily = 0x4,

		/// <summary>
		/// The nested type is visible within the assembly only.
		/// </summary>
		NestedAssembly = 0x5,

		/// <summary>
		/// The nested type is visible to the descendants of the enclosing class residing in the same assembly.
		/// </summary>
		NestedFamAndAssem = 0x6,

		/// <summary>
		/// The nested type is visible to the descendants of the enclosing class either within
		/// or outside the assembly and to every type within the assembly with no regard to "lineage".
		/// </summary>
		NestedFamOrAssem = 0x7,
	}
}
