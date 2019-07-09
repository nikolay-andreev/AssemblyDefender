using System;
using System.Runtime.InteropServices;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	/// <summary>
	/// Represents an enumerator for an array of IAssemblyName objects.
	/// </summary>
	[Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface IAssemblyEnum
	{
		/// <summary>
		/// Gets a pointer to the next IAssemblyName contained in this IAssemblyEnum object.
		/// </summary>
		/// <param name="pvReserved">Reserved for future extensibility. pvReserved must be a null reference.</param>
		/// <param name="ppName">The returned IAssemblyName pointer.</param>
		/// <param name="dwFlags">Reserved for future extensibility. dwFlags must be 0 (zero).</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetNextAssembly(
			[In] IntPtr pvReserved,
			[Out] out IAssemblyName ppName,
			[In] uint dwFlags);

		/// <summary>
		/// Resets this IAssemblyEnum object to its starting position.
		/// </summary>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int Reset();

		/// <summary>
		/// Creates a shallow copy of this IAssemblyEnum object.
		/// </summary>
		/// <param name="ppEnum">A pointer to the copy.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int Clone(
			[Out] out IAssemblyEnum ppEnum);
	}
}
