using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	[Flags]
	public enum QUERYASMINFO_FLAG : int
	{
		/// <summary>
		/// Performs validation of the files in the GAC against the assembly manifest, including hash verification
		/// and strong name signature verification.
		/// </summary>
		VALIDATE = 1,

		/// <summary>
		/// Returns the size of all files in the assembly (disk footprint). If this is not specified, the
		/// ASSEMBLY_INFO::uliAssemblySizeInKB field is not modified.
		/// </summary>
		GETSIZE = 2,
	}
}
