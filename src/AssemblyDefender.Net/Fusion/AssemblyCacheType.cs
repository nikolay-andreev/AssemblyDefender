using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Fusion.UnmanagedApi;

namespace AssemblyDefender.Net.Fusion
{
	public enum AssemblyCacheType
	{
		/// <summary>
		/// Enumerates the cache of precompiled assemblies by using Ngen.exe.
		/// </summary>
		ZAP = ASM_CACHE_FLAGS.ZAP,

		/// <summary>
		/// Enumerates the global assembly cache.
		/// </summary>
		GAC = ASM_CACHE_FLAGS.GAC,

		/// <summary>
		/// Enumerates the assemblies that have been downloaded on demand or that have been shadow-copied.
		/// </summary>
		DOWNLOAD = ASM_CACHE_FLAGS.DOWNLOAD,
	}
}
