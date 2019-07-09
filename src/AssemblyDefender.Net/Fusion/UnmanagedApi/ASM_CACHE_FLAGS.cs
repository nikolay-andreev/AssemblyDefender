using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	[Flags]
	public enum ASM_CACHE_FLAGS : int
	{
		/// <summary>
		/// Enumerates the cache of precompiled assemblies by using Ngen.exe.
		/// </summary>
		ZAP = 0x1,

		/// <summary>
		/// Enumerates the global assembly cache.
		/// </summary>
		GAC = 0x2,

		/// <summary>
		/// Enumerates the assemblies that have been downloaded on demand or that have been shadow-copied.
		/// </summary>
		DOWNLOAD = 0x4,
	}
}
