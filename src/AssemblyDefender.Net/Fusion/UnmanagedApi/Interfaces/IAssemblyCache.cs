using System;
using System.Runtime.InteropServices;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	/// <summary>
	/// Represents the global assembly cache for use by the fusion technology.
	/// </summary>
	[Guid("E707DCDE-D1CD-11D2-BAB9-00C04F8ECEAE")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface IAssemblyCache
	{
		/// <summary>
		/// Uninstalls the specified assembly from the global assembly cache.
		/// </summary>
		/// <param name="dwFlags">No flags defined. Must be zero.</param>
		/// <param name="pszAssemblyName">The name of the assembly to uninstall.</param>
		/// <param name="pRefData">A FUSION_INSTALL_REFERENCE structure that contains the installation data for the assembly.</param>
		/// <param name="pulDisposition">One of the disposition values defined in IASSEMBLYCACHE_UNINSTALL_DISPOSITION.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int UninstallAssembly(
			[In] uint dwFlags,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string pszAssemblyName,
			[In, MarshalAs(InteropUnmanagedType.LPArray)] FUSION_INSTALL_REFERENCE[] pRefData,
			[Out] out uint pulDisposition);

		/// <summary>
		/// Gets the requested data about the specified assembly.
		/// </summary>
		/// <param name="dwFlags">QUERYASMINFO_FLAG values.</param>
		/// <param name="pszAssemblyName">The name of the assembly for which data will be retrieved.</param>
		/// <param name="pAsmInfo">An ASSEMBLY_INFO structure that contains data about the assembly.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int QueryAssemblyInfo(
			[In] uint dwFlags,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string pszAssemblyName,
			[In, Out] ref ASSEMBLY_INFO pAsmInfo);

		/// <summary>
		/// Gets a reference to a new IAssemblyCacheItem.
		/// </summary>
		/// <param name="dwFlags">IASSEMBLYCACHE_INSTALL_FLAG flags.</param>
		/// <param name="pvReserved">Reserved for future extensibility. pvReserved must be a null reference.</param>
		/// <param name="ppAsmItem">The returned IAssemblyCacheItem pointer.</param>
		/// <param name="pszAssemblyName">Uncanonicalized, comma-separated name=value pairs (optianal).</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int CreateAssemblyCacheItem(
			[In] uint dwFlags,
			[In] IntPtr pvReserved,
			[Out] out IAssemblyCacheItem ppAsmItem,
			[In, Optional, MarshalAs(InteropUnmanagedType.LPWStr)] string pszAssemblyName);

		/// <summary>
		/// Reserved for internal use by the fusion technology.
		/// </summary>
		/// <param name="ppAsmScavenger">The returned IUnknown pointer.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int CreateAssemblyScavenger(
			[Out, MarshalAs(InteropUnmanagedType.IUnknown)] out object ppAsmScavenger);

		/// <summary>
		/// Installs the specified assembly in the global assembly cache.
		/// </summary>
		/// <param name="dwFlags">IASSEMBLYCACHE_INSTALL_FLAG values</param>
		/// <param name="pszManifestFilePath">The path to the manifest for the assembly to install.</param>
		/// <param name="pRefData">A FUSION_INSTALL_REFERENCE structure that contains data for the installation.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int InstallAssembly(
			[In] uint dwFlags,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string pszManifestFilePath,
			[In, MarshalAs(InteropUnmanagedType.LPArray)] FUSION_INSTALL_REFERENCE[] pRefData);
	}
}
