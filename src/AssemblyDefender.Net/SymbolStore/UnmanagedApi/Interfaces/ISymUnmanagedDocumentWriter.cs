using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Provides methods for writing to a document referenced by a symbol store.
	/// </summary>
	[Guid("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedDocumentWriter
	{
		/// <summary>
		/// Sets embedded source for a document that is being written.
		/// </summary>
		/// <param name="sourceSize">A ULONG32 that contains the size of the source buffer.</param>
		/// <param name="source">The buffer that stores the embedded source.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int SetSource(
			[In] int sourceSize,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] byte[] source);

		/// <summary>
		/// Sets checksum information.
		/// </summary>
		/// <param name="algorithmId">The GUID that represents the algorithm identifier.</param>
		/// <param name="checkSumSize">A ULONG32 that indicates the size, in bytes, of the checkSum buffer.</param>
		/// <param name="checkSum">The buffer that stores the checksum information.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int SetCheckSum(
			[In] Guid algorithmId,
			[In] int checkSumSize,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] byte[] checkSum);
	}
}
