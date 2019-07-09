using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Represents a method within the symbol store. This interface provides access to only the symbol-related
	/// attributes of a method, instead of the type-related attributes.
	/// </summary>
	[Guid("B62B923C-B500-3158-A543-24F307A8B7E1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedMethod
	{
		/// <summary>
		/// Returns the metadata token for this method.
		/// </summary>
		/// <param name="pToken">A pointer to a mdMethodDef that receives the size, in characters, of the buffer
		/// required to contain the metadata.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetToken(
			[Out] out int pToken);

		/// <summary>
		/// Gets the count of sequence points within this method.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the sequence points.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSequencePointCount(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the root lexical scope within this method. This scope encloses the entire method.
		/// </summary>
		/// <param name="pRetVal">A pointer that is set to the returned ISymUnmanagedScope interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetRootScope(
			[Out] out ISymUnmanagedScope pRetVal);

		/// <summary>
		/// Gets the most enclosing lexical scope within this method that encloses the given offset.
		/// This can be used to start local variable searches.
		/// </summary>
		/// <param name="offset">A ULONG that contains the offset.</param>
		/// <param name="pRetVal">A pointer that is set to the returned ISymUnmanagedScope interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetScopeFromOffset(
			[In] int offset,
			[Out] out ISymUnmanagedScope pRetVal);

		/// <summary>
		/// Returns the offset within this method that corresponds to a given position within a document.
		/// </summary>
		/// <param name="document">A pointer to the document for which the offset is requested.</param>
		/// <param name="line">The document line for which the offset is requested.</param>
		/// <param name="column">The document column for which the offset is requested.</param>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the offsets.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetOffset(
			[In] ISymUnmanagedDocument document,
			[In] int line,
			[In] int column,
			[Out] out int pRetVal);

		/// <summary>
		/// Given a position in a document, returns an array of start and end offset pairs that correspond to the
		/// ranges of Microsoft intermediate language (MSIL) that the position covers within this method.
		/// The array is an array of integers and has the format [start, end, start, end]. The number of
		/// range pairs is the length of the array divided by 2.
		/// </summary>
		/// <param name="document">The document for which the offset is requested.</param>
		/// <param name="line">The document line corresponding to the ranges.</param>
		/// <param name="column">The document column corresponding to the ranges.</param>
		/// <param name="cRanges">The size of the ranges array.</param>
		/// <param name="pcRanges">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the ranges.</param>
		/// <param name="ranges">A pointer to the buffer that receives the ranges.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetRanges(
			[In] ISymUnmanagedDocument document,
			[In] int line,
			[In] int column,
			[In] int cRanges,
			[Out] out int pcRanges,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 3)] int[] ranges);

		/// <summary>
		/// Gets the parameters for this method. The parameters are returned in the order in which they are defined
		/// within the method's signature.
		/// </summary>
		/// <param name="cParams">The size of the params array.</param>
		/// <param name="pcParams">A pointer to a ULONG32 that receives the size of the buffer that is required to
		/// contain the parameters.</param>
		/// <param name="methodParams">A pointer to the buffer that receives the parameters.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetParameters(
			[In] int cParams,
			[Out] out int pcParams,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedVariable[] methodParams);

		/// <summary>
		/// Gets the namespace within which this method is defined.
		/// </summary>
		/// <param name="pRetVal">A pointer that is set to the returned ISymUnmanagedNamespace interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetNamespace(
			[Out] out ISymUnmanagedNamespace pRetVal);

		/// <summary>
		/// Gets the start and end document positions for the source of this method. The first array position
		/// is the start, and the second array position is the end.
		/// </summary>
		/// <param name="docs">The starting and ending source documents.</param>
		/// <param name="lines">The starting and ending lines in the corresponding source documents.</param>
		/// <param name="columns">The starting and ending columns in the corresponding source documents.</param>
		/// <param name="pRetVal">true if positions were defined; otherwise, false.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSourceStartEnd(
			[In] ISymUnmanagedDocument[] docs,
			[In] int[] lines,
			[In] int[] columns,
			[Out] out bool pRetVal);

		/// <summary>
		/// Gets all the sequence points within this method.
		/// </summary>
		/// <param name="cPoints">A ULONG32 that receives the size of the offsets, documents, lines, columns,
		/// endLines, and endColumns arrays.</param>
		/// <param name="pcPoints">A pointer to a ULONG32 that receives the length of the buffer required to
		/// contain the sequence points.</param>
		/// <param name="offsets">An array in which to store the Microsoft intermediate language (MSIL) offsets
		/// from the beginning of the method for the sequence points.</param>
		/// <param name="documents">An array in which to store the documents in which the sequence points are located.</param>
		/// <param name="lines">An array in which to store the lines in the documents at which the
		/// sequence points are located.</param>
		/// <param name="columns">An array in which to store the columns in the documents at which the
		/// sequence points are located.</param>
		/// <param name="endLines">The array of lines in the documents at which the sequence points end.</param>
		/// <param name="endColumns">The array of columns in the documents at which the sequence points end.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSequencePoints(
			[In] int cPoints,
			[Out] out int pcPoints,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] int[] offsets,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedDocument[] documents,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] int[] lines,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] int[] columns,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] int[] endLines,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] int[] endColumns);
	}
}
