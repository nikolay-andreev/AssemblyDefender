using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Provides source server data for a module. Obtain this interface by calling QueryInterface on an object
	/// that implements the ISymUnmanagedReader interface.
	/// </summary>
	[Guid("997DD0CC-A76F-4C82-8D79-EA87559D27AD")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedSourceServerModule
	{
		/// <summary>
		/// Returns the source server data for the module. The caller must free resources by using CoTaskMemFree.
		/// </summary>
		/// <param name="pDataByteCount">A pointer to a ULONG32 that receives the size, in bytes, of the
		/// source server data.</param>
		/// <param name="ppData">A pointer to the returned pDataByteCount value.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSourceServerData(
			[Out] out int pDataByteCount,
			[Out] out IntPtr ppData);
	}
}
