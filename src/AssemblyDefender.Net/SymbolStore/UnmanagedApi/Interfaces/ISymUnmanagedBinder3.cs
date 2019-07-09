using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Extends the symbol binder interface. Obtain this interface by calling QueryInterface on an object that
	/// implements the ISymUnmanagedBinder interface.
	/// </summary>
	[Guid("28AD3D43-B601-4D26-8A1B-25F9165AF9D7")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedBinder3 : ISymUnmanagedBinder, ISymUnmanagedBinder2
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

		#region ISymUnmanagedBinder2

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
		new int GetReaderForFile2(
			[In, MarshalAs(InteropUnmanagedType.IUnknown)] object importer,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string fileName,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string searchPath,
			[In] int searchPolicy,
			[Out] out ISymUnmanagedReader pRetVal);

		#endregion

		/// <summary>
		/// Allows the user to implement or supply via callback either an IID_IDiaReadExeAtRVACallback or
		/// IID_IDiaReadExeAtOffsetCallback to obtain the debug directory information from memory.
		/// </summary>
		/// <param name="importer">A pointer to the metadata import interface.</param>
		/// <param name="fileName">A pointer to the file name.</param>
		/// <param name="searchPath">A pointer to the search path.</param>
		/// <param name="searchPolicy">A value of the CorSymSearchPolicyAttributes enumeration that specifies the
		/// policy to be used when doing a search for a symbol reader.</param>
		/// <param name="callback">A pointer to the callback function.</param>
		/// <param name="pRetVal">A pointer that is set to the returned ISymUnmanagedReader interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetReaderFromCallback(
			[In, MarshalAs(InteropUnmanagedType.IUnknown)] object importer,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string fileName,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string searchPath,
			[In] int searchPolicy,
			[In, MarshalAs(InteropUnmanagedType.IUnknown)] object callback,
			[Out] out ISymUnmanagedReader pRetVal);
	}
}
