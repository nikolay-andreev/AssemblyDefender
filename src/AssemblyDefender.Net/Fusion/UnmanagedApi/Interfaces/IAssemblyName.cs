using System;
using System.Runtime.InteropServices;
using System.Text;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	/// <summary>
	/// Provides methods for describing and working with an assembly's unique identity.
	/// </summary>
	[Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface IAssemblyName
	{
		/// <summary>
		/// Sets the value of the property referenced by the specified property identifier.
		/// </summary>
		/// <param name="PropertyId">The unique identifier of the property whose value will be set. ASM_NAME_PROPERTY value.</param>
		/// <param name="pvProperty">The value to which to set the property referenced by PropertyId.</param>
		/// <param name="cbProperty">The size, in bytes, of pvProperty.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int SetProperty(
			[In] int PropertyId,
			[In] IntPtr pvProperty,
			[In] int cbProperty);

		/// <summary>
		/// Gets a pointer to the property referenced by the specified property identifier.
		/// </summary>
		/// <param name="PropertyId">The unique identifier for the requested property. ASM_NAME_PROPERTY value.</param>
		/// <param name="pvProperty">The returned property data.</param>
		/// <param name="pcbProperty">The size, in bytes, of pvProperty.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetProperty(
			[In] int PropertyId,
			[Out] IntPtr pvProperty,
			[In, Out] ref int pcbProperty);

		/// <summary>
		/// Allows this IAssemblyName object to release resources and perform other cleanup operations
		/// before its destructor is called.
		/// </summary>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int Finalize();

		/// <summary>
		/// Gets the human-readable name of the assembly referenced by this IAssemblyName object.
		/// </summary>
		/// <param name="szDisplayName">The string buffer that contains the name of the referenced assembly.</param>
		/// <param name="pccDisplayName">The size of szDisplayName in wide characters, including a
		/// null terminator character.</param>
		/// <param name="dwDisplayFlags">A bitwise combination of ASM_DISPLAY_FLAGS values that influence the
		/// features of szDisplayName. ASM_DISPLAY_FLAGS value.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetDisplayName(
			[Out, MarshalAs(InteropUnmanagedType.LPWStr)] StringBuilder szDisplayName,
			[In, Out] ref int pccDisplayName,
			[In] uint dwDisplayFlags);

		[PreserveSig]
		int BindToObject(
			ref Guid refIID,
			[MarshalAs(InteropUnmanagedType.IUnknown)] object pUnkSink,
			[MarshalAs(InteropUnmanagedType.IUnknown)] object pUnkContext,
			[MarshalAs(InteropUnmanagedType.LPWStr)] string szCodeBase,
			long llFlags,
			IntPtr pvReserved,
			uint cbReserved,
			out IntPtr ppv);

		/// <summary>
		/// Gets the simple, unencrypted name of the assembly referenced by this IAssemblyName object.
		/// </summary>
		/// <param name="lpcwBuffer">The size of pwzName in wide characters, including the null terminator character.</param>
		/// <param name="pwzName">A buffer to hold the name of the referenced assembly.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetName(
			[In, Out] ref int lpcwBuffer,
			[Out, MarshalAs(InteropUnmanagedType.LPWStr)] StringBuilder pwzName);

		/// <summary>
		/// Gets the version information for the assembly referenced by this IAssemblyName object.
		/// </summary>
		/// <param name="pdwVersionHi">The high 32 bits of the version.</param>
		/// <param name="pdwVersionLow">The low 32 bits of the version.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetVersion(
			[Out] out int pdwVersionHi,
			[Out] out int pdwVersionLow);

		/// <summary>
		/// Determines whether a specified IAssemblyName object is equal to this IAssemblyName,
		/// based on the specified comparison flags.
		/// </summary>
		/// <param name="pName">The IAssemblyName object to which to compare this IAssemblyName.</param>
		/// <param name="dwCmpFlags">A bitwise combination of ASM_CMP_FLAGS values that influence the comparison.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int IsEqual(
			[In] IAssemblyName pName,
			[In] uint dwCmpFlags);

		/// <summary>
		/// Creates a shallow copy of this IAssemblyName object.
		/// </summary>
		/// <param name="pName">The returned copy of this IAssemblyName object.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int Clone(
			[Out] out IAssemblyName pName);
	}
}
