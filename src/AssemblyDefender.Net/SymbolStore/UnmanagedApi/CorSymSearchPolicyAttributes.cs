using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	public enum CorSymSearchPolicyAttributes : int
	{
		/// <summary>
		/// Query the registry for symbol search paths.
		/// </summary>
		AllowRegistryAccess = 0x1,

		/// <summary>
		/// Access a symbol server.
		/// </summary>
		AllowSymbolServerAccess = 0x2,

		/// <summary>
		/// Look at the path specified in Debug Directory.
		/// </summary>
		AllowOriginalPathAccess = 0x4,

		/// <summary>
		/// Look for PDB in the place where the exe is.
		/// </summary>
		AllowReferencePathAccess = 0x8
	}
}
