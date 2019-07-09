using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Provides functions for the Edit and Continue feature.
	/// </summary>
	[Guid("E502D2DD-8671-4338-8F2A-FC08229628C4")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedENCUpdate
	{
		/// <summary>
		/// Allows a compiler to omit functions that have not been modified from the program database (PDB) stream,
		/// provided the line information meets the requirements. The correct line information can be determined with
		/// the old PDB line information and one delta for all lines in the function.
		/// </summary>
		/// <param name="pIStream">A pointer to an IStream that contains the line information.</param>
		/// <param name="pDeltaLines">A pointer to a SYMLINEDELTA structure that contains the lines that have changed.</param>
		/// <param name="cDeltaLines">A ULONG that represents the number of lines that have changed.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int UpdateSymbolStore2(
			[In] IStream pIStream,
			[In] ref SYMLINEDELTA pDeltaLines,
			[In] int cDeltaLines);

		/// <summary>
		/// Gets the number of local variables.
		/// </summary>
		/// <param name="mdMethodToken">The metadata token of methods.</param>
		/// <param name="pcLocals">A pointer to a ULONG32 that receives the size, in characters, of the buffer required
		/// to contain the number of local variables.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetLocalVariableCount(
			[In] int mdMethodToken,
			[Out] out int pcLocals);

		/// <summary>
		/// Gets the local variables.
		/// </summary>
		/// <param name="mdMethodToken">The metadata token of the method.</param>
		/// <param name="cLocals">A ULONG that indicates the size of the rgLocals parameter.</param>
		/// <param name="rgLocals">The returned array of ISymUnmanagedVariable instances.</param>
		/// <param name="pceltFetched">A pointer to a ULONG that receives the size of the rgLocals buffer required to
		/// contain the locals.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetLocalVariables(
			[In] int mdMethodToken,
			[In] int cLocals,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] ISymUnmanagedVariable[] rgLocals,
			[Out] out int pceltFetched);

		/// <summary>
		/// Allows method boundaries to be computed before the first call to the
		/// ISymUnmanagedENCUpdate::UpdateSymbolStore2 method.
		/// </summary>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int InitializeForEnc();

		/// <summary>
		/// Allows updating the line information for a method that has not been recompiled, but whose lines have
		/// moved independently. A delta for each statement is allowed.
		/// </summary>
		/// <param name="mdMethodToken">The metadata of the method token.</param>
		/// <param name="pDeltas">An array of INT32 values that indicates deltas for each sequence point in the method.</param>
		/// <param name="cDeltas">A ULONG containing the size of the pDeltas parameter.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int UpdateMethodLines(
			[In] int mdMethodToken,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 2)] int[] pDeltas,
			[In] int cDeltas);
	}
}
