using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Represents a document referenced by a symbol store. A document is defined by a uniform resource locator (URL)
	/// and a document type GUID. You can locate the document regardless of how it is stored by using the URL and
	/// document type GUID. You can store the document source in the symbol store and retrieve it through this interface.
	/// </summary>
	[Guid("40DE4037-7C81-3E1E-B022-AE1ABFF2CA08")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedDocument
	{
		/// <summary>
		/// Returns the uniform resource locator (URL) for this document.
		/// </summary>
		/// <param name="cchUrl">The size, in characters, of the szURL buffer.</param>
		/// <param name="pcchUrl">A pointer to a variable that receives the size of the URL, including the null termination.</param>
		/// <param name="szUrl">The buffer containing the URL.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetURL(
			[In] int cchUrl,
			[Out] out int pcchUrl,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] char[] szUrl);

		/// <summary>
		/// Gets the document type of this document.
		/// </summary>
		/// <param name="pRetVal">Pointer to a variable that receives the document type.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetDocumentType(
			[Out] out Guid pRetVal);

		/// <summary>
		/// Gets the language identifier of this document.
		/// </summary>
		/// <param name="pRetVal">A pointer to a variable that receives the language identifier.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetLanguage(
			[Out] out Guid pRetVal);

		/// <summary>
		/// Gets the language vendor of this document.
		/// </summary>
		/// <param name="pRetVal">A pointer to a variable that receives the language vendor.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetLanguageVendor(
			[Out] out Guid pRetVal);

		/// <summary>
		/// Gets the checksum algorithm identifier, or returns a GUID of all zeros if there is no checksum.
		/// </summary>
		/// <param name="pRetVal">A pointer to a variable that receives the checksum algorithm identifier.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetCheckSumAlgorithmId(
			[Out] out Guid pRetVal);

		/// <summary>
		/// Gets the checksum.
		/// </summary>
		/// <param name="cData">The length of the buffer provided by the data parameter.</param>
		/// <param name="pcData">The size and length of the checksum, in bytes.</param>
		/// <param name="data">The buffer that receives the checksum.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetCheckSum(
			[In] int cData,
			[Out] out int pcData,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] byte[] data);

		/// <summary>
		/// Returns the closest line that is a sequence point, given a line in this document that may or may not be a
		/// sequence point.
		/// </summary>
		/// <param name="line">A line in this document.</param>
		/// <param name="pRetVal">A pointer to a variable that receives the line.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int FindClosestLine(
			[In] int line,
			[Out] out int pRetVal);

		/// <summary>
		/// Returns true if the document has source embedded in the debugging symbols; otherwise, returns false.
		/// </summary>
		/// <param name="pRetVal">A pointer to a variable that indicates whether the document has source embedded in the
		/// debugging symbols.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int HasEmbeddedSource(
			[Out] out bool pRetVal);

		/// <summary>
		/// Gets the length, in bytes, of the embedded source.
		/// </summary>
		/// <param name="pRetVal">A pointer to a variable that indicates the length, in bytes, of the embedded source.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSourceLength(
			[Out] out int pRetVal);

		/// <summary>
		/// Returns the specified range of the embedded source into the given buffer. The buffer must be large enough
		/// to hold the source.
		/// </summary>
		/// <param name="startLine">The starting line in the current document.</param>
		/// <param name="startColumn">The starting column in the current document.</param>
		/// <param name="endLine">The final line in the current document.</param>
		/// <param name="endColumn">The final column in the current document.</param>
		/// <param name="cSourceBytes">The size of the source, in bytes.</param>
		/// <param name="pcSourceBytes">A pointer to a variable that receives the source size.</param>
		/// <param name="source">The size and length of the specified range of the source document, in bytes.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSourceRange(
			[In] int startLine,
			[In] int startColumn,
			[In] int endLine,
			[In] int endColumn,
			[In] int cSourceBytes,
			[Out] out int pcSourceBytes,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 4)] byte[] source);
	}
}
