using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	[Flags]
	public enum ASSEMBLYINFO_FLAG
	{
		/// <summary>
		/// Indicates that the assembly is installed. The current version of the .NET Framework always sets
		/// dwAssemblyFlags to this value.
		/// </summary>
		INSTALLED = 0x00000001,

		/// <summary>
		/// Indicates that the assembly is a payload resident. The current version of the .NET Framework never sets
		/// dwAssemblyFlags to this value.
		/// </summary>
		PAYLOADRESIDENT = 0x00000002,
	}
}
