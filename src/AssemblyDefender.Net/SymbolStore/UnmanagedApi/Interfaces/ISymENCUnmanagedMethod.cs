using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Provides information for the Edit and Continue feature.
	/// </summary>
	[Guid("85E891DA-A631-4C76-ACA2-A44A39C46B8C")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymENCUnmanagedMethod
	{
		/// <summary>
		/// Gets the file name for the line associated with an offset.
		/// </summary>
		/// <param name="dwOffset">A ULONG32 that contains the offset.</param>
		/// <param name="cchName">A ULONG32 that indicates the size of the szName buffer.</param>
		/// <param name="pcchName">A pointer to a ULONG32 that receives the size, in characters, of the buffer
		/// required to contain the file names.</param>
		/// <param name="szName">The buffer that contains the file names.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetFileNameFromOffset(
			[In] int dwOffset,
			[In] int cchName,
			[Out] out int pcchName,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] char[] szName);

		/// <summary>
		/// Gets the line information associated with an offset. If the offset parameter (dwOffset) is not a
		/// sequence point, this method gets the line information associated with the previous offset.
		/// </summary>
		/// <param name="dwOffset">A ULONG32 that contains the offset.</param>
		/// <param name="pline">A pointer to a ULONG32 that receives the line.</param>
		/// <param name="pcolumn">A pointer to a ULONG32 that receives the column.</param>
		/// <param name="pendLine">A pointer to a ULONG32 that receives the the end line.</param>
		/// <param name="pendColumn">A pointer to a ULONG32 that receives the end column.</param>
		/// <param name="pdwStartOffset">A pointer to a ULONG32 that receives the associated sequence point.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetLineFromOffset(
			[In] int dwOffset,
			[Out] out int pline,
			[Out] out int pcolumn,
			[Out] out int pendLine,
			[Out] out int pendColumn,
			[Out] out int pdwStartOffset);

		/// <summary>
		/// Gets the number of documents that this method has lines in.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the documents.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetDocumentsForMethodCount(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the documents that this method has lines in.
		/// </summary>
		/// <param name="cDocs">The length of the buffer pointed to by pcDocs.</param>
		/// <param name="pcDocs">A pointer to a ULONG32 that receives the size, in characters, of the buffer required
		/// to contain the documents.</param>
		/// <param name="documents">The buffer that contains the documents.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetDocumentsForMethod(
			[In] int cDocs,
			[Out] out int pcDocs,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedDocument[] documents);

		/// <summary>
		/// Gets the smallest start line and largest end line for the method in a specific document.
		/// </summary>
		/// <param name="document">A pointer to the document.</param>
		/// <param name="pstartLine">A pointer to a ULONG32 that receives the the start line.</param>
		/// <param name="pendLine">A pointer to a ULONG32 that receives the end line.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSourceExtentInDocument(
			[In] ISymUnmanagedDocument document,
			[Out] out int pstartLine,
			[Out] out int pendLine);
	}
}
