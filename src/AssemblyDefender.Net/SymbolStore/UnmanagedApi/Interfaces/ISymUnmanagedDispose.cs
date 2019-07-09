using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Disposes of unmanaged resources.
	/// </summary>
	[Guid("969708D2-05E5-4861-A3B0-96E473CDF63F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedDispose
	{
		/// <summary>
		/// Causes the underlying object to release all internal references and return failure on any
		/// subsequent method calls.
		/// </summary>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int Destroy();
	}
}
