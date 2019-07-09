using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Provides methods that get symbol search information. Obtain this interface by calling QueryInterface on an
	/// object that implements the ISymUnmanagedReader interface.
	/// </summary>
	[Guid("20D9645D-03CD-4E34-9C11-9848A5B084F1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedReaderSymbolSearchInfo
	{
		/// <summary>
		/// Gets a count of symbol search information.
		/// </summary>
		/// <param name="pcSearchInfo">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the search information.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSymbolSearchInfoCount(
			[Out] out int pcSearchInfo);

		/// <summary>
		/// Gets symbol search information.
		/// </summary>
		/// <param name="cSearchInfo">A ULONG32 that indicates the size of rgpSearchInfo.</param>
		/// <param name="pcSearchInfo">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the search information.</param>
		/// <param name="rgpSearchInfo">A pointer that is set to the returned ISymUnmanagedSymbolSearchInfo interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSymbolSearchInfo(
			[In] int cSearchInfo,
			[Out] out int pcSearchInfo,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedSymbolSearchInfo[] rgpSearchInfo);
	}
}
