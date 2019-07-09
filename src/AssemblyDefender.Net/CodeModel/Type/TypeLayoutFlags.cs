using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum TypeLayoutFlags : int
	{
		/// <summary>
		/// The type fields are laid out automatically, at the loader’s discretion. This is the default.
		/// </summary>
		Auto = 0,

		/// <summary>
		/// The loader shall preserve the order of the instance fields.
		/// </summary>
		Sequential = 0x8,

		/// <summary>
		/// The type layout is specified explicitly, and the loader shall follow it.
		/// </summary>
		Explicit = 0x10,
	}
}
