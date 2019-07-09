using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Represents a symbol binder for unmanaged code, and extends the ISymUnmanagedBinder interface.
	/// </summary>
	[Guid("ACCEE350-89AF-4CCB-8B40-1C2C4C6F9434")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedBinder2 : ISymUnmanagedBinder
	{
		#region ISymUnmanagedBinder

		/// <summary>
		/// Given a metadata interface and a file name, returns the correct ISymUnmanagedReader structure that will
		/// read the debugging symbols associated with the module.
		/// This method will open the program database (PDB) file only if it is next to the executable file.
		/// This change has been made for security purposes. If you need a more extensive search for the PDB file,
		/// use the ISymUnmanagedBinder2::GetReaderForFile2 method.
		/// </summary>
		/// <param name="importer">A pointer to the metadata import interface.</param>
		/// <param name="fileName">A pointer to the file name.</param>
		/// <param name="searchPath">A pointer to the search path.</param>
		/// <param name="pRetVal">A pointer that is set to the returned ISymUnmanagedReader interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetReaderForFile(
			[In, MarshalAs(InteropUnmanagedType.IUnknown)] object importer,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string fileName,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string searchPath,
			[Out] out ISymUnmanagedReader pRetVal);

		/// <summary>
		/// Given a metadata interface and a stream that contains the symbol store, returns the correct
		/// ISymUnmanagedReader structure that will read the debugging symbols from the given symbol store.
		/// </summary>
		/// <param name="importer">A pointer to the metadata import interface.</param>
		/// <param name="pstream">A pointer to the stream that contains the symbol store.</param>
		/// <param name="pRetVal">A pointer that is set to the returned ISymUnmanagedReader interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetReaderFromStream(
			[In, MarshalAs(InteropUnmanagedType.IUnknown)] object importer,
			[In] IStream pstream,
			[Out] out ISymUnmanagedReader pRetVal);

		#endregion

		/// <summary>
		/// Given a metadata interface and a file name, returns the correct ISymUnmanagedReader interface that will
		/// read the debugging symbols associated with the module.
		/// This method provides a more extensive search for the program database (PDB) file than the
		/// ISymUnmanagedBinder::GetReaderForFile method.
		/// </summary>
		/// <param name="importer">A pointer to the metadata import interface.</param>
		/// <param name="fileName">A pointer to the file name.</param>
		/// <param name="searchPath">A pointer to the search path.</param>
		/// <param name="searchPolicy">A value of the CorSymSearchPolicyAttributes enumeration that specifies the
		/// policy to be used when doing a search for a symbol reader.</param>
		/// <param name="pRetVal">A pointer that is set to the returned ISymUnmanagedReader interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetReaderForFile2(
			[In, MarshalAs(InteropUnmanagedType.IUnknown)] object importer,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string fileName,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string searchPath,
			[In] int searchPolicy,
			[Out] out ISymUnmanagedReader pRetVal);
	}
}
