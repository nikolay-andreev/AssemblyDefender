using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum ResourceVisibilityFlags : int
	{		/// <summary>
		/// The resource is exported from the Assembly
		/// </summary>
		Public = 0x0001,

		/// <summary>
		/// The resource is private to the Assembly
		/// </summary>
		Private = 0x0002,
	}
}
