using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	public static class FusionNative
	{
		#region FUSION_REFCOUNT

		/// <summary>
		/// The assembly is referenced by an application that was installed using the Microsoft Windows Installer. The szIdentifier field is set to MSI, and the szNonCanonicalData field is set to Windows Installer. This scheme is used for Windows side-by-side assemblies.
		/// </summary>
		public static readonly Guid FUSION_REFCOUNT_MSI_GUID = new Guid(0x25df0fc1, 0x7f97, 0x4070, 0xad, 0xd7, 0x4b, 0x13, 0xbb, 0xfd, 0x7c, 0xb8);

		/// <summary>
		/// The assembly is referenced by an application that appears in the Add/Remove Programs interface. The szIdentifier field provides the token that registers the application with the Add/Remove Programs interface.
		/// </summary>
		public static readonly Guid FUSION_REFCOUNT_UNINSTALL_SUBKEY_GUID = new Guid(0x8cedc215, 0xac4b, 0x488b, 0x93, 0xc0, 0xa5, 0x0a, 0x49, 0xcb, 0x2f, 0xb8);

		/// <summary>
		/// The assembly is referenced by an application that is represented by a file in the file system. The szIdentifier field provides the path to this file.
		/// </summary>
		public static readonly Guid FUSION_REFCOUNT_FILEPATH_GUID = new Guid(0xb02f9d65, 0xfb77, 0x4f7a, 0xaf, 0xa5, 0xb3, 0x91, 0x30, 0x9f, 0x11, 0xc9);

		/// <summary>
		/// The assembly is referenced by an application that is represented only by an opaque string. The szIdentifier field provides this opaque string. The global assembly cache does not check for the existence of opaque references when you remove this value.
		/// </summary>
		public static readonly Guid FUSION_REFCOUNT_OPAQUE_STRING_GUID = new Guid(0x2ec93463, 0xb0c3, 0x45e1, 0x83, 0x64, 0x32, 0x7e, 0x96, 0xae, 0xa8, 0x56);

		/// <summary>
		/// This value is reserved.
		/// </summary>
		public static readonly Guid FUSION_REFCOUNT_OSINSTALL_GUID = new Guid(0xd16d444c, 0x56d8, 0x11d5, 0x88, 0x2d, 0x00, 0x80, 0xc8, 0x47, 0xb1, 0x95);

		#endregion

		#region Global methods

		/// <summary>
		/// Clears the global assembly cache of downloaded assemblies.
		/// </summary>
		/// <returns>HRESULT</returns>
		[DllImport("fusion.dll", EntryPoint = "ClearDownloadCache", SetLastError = true, PreserveSig = true)]
		public static extern int ClearDownloadCache();

		/// <summary>
		/// Gets a pointer to a new IAssemblyCache instance that represents the global assembly cache.
		/// </summary>
		/// <param name="ppAsmCache">The returned IAssemblyCache pointer.</param>
		/// <param name="dwReserved">Reserved for future extensibility. dwReserved must be 0 (zero).</param>
		/// <returns>HRESULT</returns>
		[DllImport("fusion.dll", EntryPoint = "CreateAssemblyCache", SetLastError = true, PreserveSig = true)]
		public static extern int CreateAssemblyCache(
			out IAssemblyCache ppAsmCache,
			uint dwReserved);

		/// <summary>
		/// Gets a pointer to an IAssemblyEnum instance that can enumerate the objects in the
		/// assembly with the specified IAssemblyName.
		/// </summary>
		/// <param name="pEnum">Pointer to a memory location that contains the requested IAssemblyEnum pointer.</param>
		/// <param name="pUnkReserved">Reserved for future extensibility. pUnkReserved must be a null reference.</param>
		/// <param name="pName">The IAssemblyName of the requested assembly. This name is used to filter the
		/// enumeration. It can be null to enumerate all assemblies in the global assembly cache.</param>
		/// <param name="dwFlags">Flags for modifying the enumerator's behavior. This parameter contains exactly
		/// one bit from the ASM_CACHE_FLAGS enumeration.</param>
		/// <param name="pvReserved">Reserved for future extensibility. pvReserved must be a null reference.</param>
		/// <returns>HRESULT</returns>
		[DllImport("fusion.dll", EntryPoint = "CreateAssemblyEnum", SetLastError = true, PreserveSig = true)]
		public static extern int CreateAssemblyEnum(
			[Out] out IAssemblyEnum pEnum,
			[In] IntPtr pUnkReserved,
			[In] IAssemblyName pName,
			[In] uint dwFlags,
			[In] IntPtr pvReserved);

		/// <summary>
		/// Gets an interface pointer to an IAssemblyName instance that represents the unique identity of the
		/// assembly with the specified name.
		/// </summary>
		/// <param name="ppAssemblyNameObj">The returned IAssemblyName.</param>
		/// <param name="szAssemblyName">The name of the assembly for which to create the new IAssemblyName instance.</param>
		/// <param name="dwFlags">Flags to pass to the object constructor.</param>
		/// <param name="pvReserved">Reserved for future extensibility. pvReserved must be a null reference.</param>
		/// <returns>HRESULT</returns>
		[DllImport("fusion.dll", EntryPoint = "CreateAssemblyNameObject", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public static extern int CreateAssemblyNameObject(
			[Out] out IAssemblyName ppAssemblyNameObj,
			[In] string szAssemblyName,
			[In] uint dwFlags,
			[In] IntPtr pvReserved);

		/// <summary>
		/// Gets the path to the cached assembly, using the specified flags.
		/// </summary>
		/// <param name="dwCacheFlags">An ASM_CACHE_FLAGS value that indicates the source of the cached assembly.</param>
		/// <param name="pwzCachePath">The returned pointer to the path.</param>
		/// <param name="pcchPath">The requested maximum length of pwzCachePath, and upon return,
		/// the actual length of pwzCachePath.</param>
		/// <returns>HRESULT</returns>
		[DllImport("fusion.dll", EntryPoint = "GetCachePath", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public static extern int GetCachePath(
			[In] uint dwCacheFlags,
			[Out, MarshalAs(InteropUnmanagedType.LPWStr)] StringBuilder pwzCachePath,
			[In, Out] ref int pcchPath);

		/// <summary>
		/// Gets a value that indicates whether the specified assembly is managed.
		/// </summary>
		/// <param name="pwzAssemblyReference">The name of the assembly to check.</param>
		/// <param name="pbIsFrameworkAssembly">A Boolean value that indicates whether the assembly is managed.</param>
		/// <param name="pwzFrameworkAssemblyIdentity">An uncanonicalized string that contains the
		/// unique identity of the assembly.</param>
		/// <param name="pccSize">The size of pwzFrameworkAssemblyIdentity.</param>
		/// <returns></returns>
		[DllImport("fusion.dll", EntryPoint = "IsFrameworkAssembly", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public static extern int IsFrameworkAssembly(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string pwzAssemblyReference,
			[Out] out bool pbIsFrameworkAssembly,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string pwzFrameworkAssemblyIdentity,
			[In] int pccSize);

		#endregion
	}
}
