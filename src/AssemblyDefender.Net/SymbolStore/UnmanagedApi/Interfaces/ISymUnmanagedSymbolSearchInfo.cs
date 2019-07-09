using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Provides methods that get information about the search path. Obtain this interface by calling
	/// QueryInterface on an object that implements the ISymUnmanagedReader interface.
	/// </summary>
	[Guid("F8B3534A-A46B-4980-B520-BEC4ACEABA8F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedSymbolSearchInfo
	{
		/// <summary>
		/// Gets the search path length.
		/// </summary>
		/// <param name="pcchPath">A pointer to a ULONG32 that receives the size, in characters,
		/// of the buffer required to contain the search path length.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSearchPathLength(
			[Out] out int pcchPath);

		/// <summary>
		/// Gets the search path.
		/// </summary>
		/// <param name="cchPath"></param>
		/// <param name="pcchPath"></param>
		/// <param name="szPath"></param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSearchPath(
			[In] int cchPath,
			[Out] out int pcchPath,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] char[] szPath);

		/// <summary>
		/// Gets the HRESULT.
		/// </summary>
		/// <param name="phr">A pointer to the HRESULT.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetHRESULT(
			[Out] out int phr);
	}
}
