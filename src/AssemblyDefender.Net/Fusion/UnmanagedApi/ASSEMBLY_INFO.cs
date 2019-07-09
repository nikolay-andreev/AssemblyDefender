using System;
using System.Runtime.InteropServices;
using System.Text;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	/// <summary>
	/// Contains information about an assembly that is registered in the global assembly cache.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ASSEMBLY_INFO
	{
		/// <summary>
		/// The size, in bytes, of the structure. This field is reserved for future extensibility.
		/// </summary>
		public int cbAssemblyInfo;

		/// <summary>
		/// Flags that indicate installation details about the assembly. ASSEMBLYINFO_FLAGS value.
		/// </summary>
		public int dwAssemblyFlags;

		/// <summary>
		/// The total size, in kilobytes, of the files that the assembly contains.
		/// </summary>
		public long uliAssemblySizeInKB;

		/// <summary>
		/// A pointer to a string buffer that holds the current path to the manifest file. The path must end
		/// with a null character.
		/// </summary>
		[MarshalAs(InteropUnmanagedType.LPWStr)]
		public string pszCurrentAssemblyPathBuf;

		/// <summary>
		/// The number of wide characters, including the null terminator, that pszCurrentAssemblyPathBuf contains.
		/// </summary>
		public int cchBuf;
	}
}
