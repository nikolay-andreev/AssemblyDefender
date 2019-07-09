using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Represents a symbol reader that provides access to documents, methods, and variables within a symbol store.
	/// This interface extends the ISymUnmanagedReader interface.
	/// </summary>
	[Guid("A09E53B2-2A57-4CCA-8F63-B84F7C35D4AA")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedReader2 : ISymUnmanagedReader
	{
		#region ISymUnmanagedReader

		/// <summary>
		/// Finds a document. The document language, vendor, and type are optional.
		/// </summary>
		/// <param name="url">The URL that identifies the document.</param>
		/// <param name="language">The document language. This parameter is optional.</param>
		/// <param name="languageVendor">The identity of the vendor for the document language. This parameter is optional.</param>
		/// <param name="documentType">The type of the document. This parameter is optional.</param>
		/// <param name="pRetVal">A pointer to the returned interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetDocument(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string url,
			[In] Guid language,
			[In] Guid languageVendor,
			[In] Guid documentType,
			[Out] out ISymUnmanagedDocument pRetVal);

		/// <summary>
		/// Returns an array of all the documents defined in the symbol store.
		/// </summary>
		/// <param name="cDocs">The size of the pDocs array.</param>
		/// <param name="pcDocs">A pointer to a variable that receives the array length.</param>
		/// <param name="pDocs">A pointer to a variable that receives the document array.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetDocuments(
			[In] int cDocs,
			[Out] out int pcDocs,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedDocument[] pDocs);

		/// <summary>
		/// Returns the method that was specified as the user entry point for the module, if any.
		/// For example, this method could be the user's main method rather than compiler-generated stubs
		/// before the main method.
		/// </summary>
		/// <param name="pToken">A pointer to a variable that receives the entry point.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetUserEntryPoint(
			[Out] out int pToken);

		/// <summary>
		/// Gets a symbol reader method, given a method token.
		/// </summary>
		/// <param name="token">The method token.</param>
		/// <param name="pRetVal">A pointer to the returned interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetMethod(
			[In] int token,
			[Out] out ISymUnmanagedMethod pRetVal);

		/// <summary>
		/// Gets a symbol reader method, given a method token and an edit-and-copy version number.
		/// Version numbers start at 1 and are incremented each time the method is changed as a result of an
		/// edit-and-copy operation.
		/// </summary>
		/// <param name="token">The method token.</param>
		/// <param name="version">The method version.</param>
		/// <param name="pRetVal">A pointer to the returned interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetMethodByVersion(
			[In] int token,
			[In] int version,
			[Out] out ISymUnmanagedMethod pRetVal);

		/// <summary>
		/// Returns a non-local variable, given its parent and name.
		/// </summary>
		/// <param name="parent">The parent of the variable.</param>
		/// <param name="cVars">The size of the pVars array.</param>
		/// <param name="pcVars">A pointer to the variable that receives the number of variables returned in pVars.</param>
		/// <param name="pVars">A pointer to the variable that receives the variables.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetVariables(
			[In] int parent,
			[In] int cVars,
			[Out] out int pcVars,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 1)] ISymUnmanagedVariable[] pVars);

		/// <summary>
		/// Returns all global variables.
		/// </summary>
		/// <param name="cVars">The length of the buffer pointed to by pcVars.</param>
		/// <param name="pcVars">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the variables.</param>
		/// <param name="pVars">A buffer that contains the variables.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetGlobalVariables(
			[In] int cVars,
			[Out] out int pcVars,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedVariable[] pVars);

		/// <summary>
		/// Returns the method that contains the breakpoint at the given position in a document.
		/// </summary>
		/// <param name="document">The specified document.</param>
		/// <param name="line">The line of the specified document.</param>
		/// <param name="column">The column of the specified document.</param>
		/// <param name="pRetVal">A pointer to the address of a ISymUnmanagedMethod Interface object that represents
		/// the method containing the breakpoint.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetMethodFromDocumentPosition(
			[In] ISymUnmanagedDocument document,
			[In] int line,
			[In] int column,
			[Out] out ISymUnmanagedMethod pRetVal);

		/// <summary>
		/// Gets a custom attribute based upon its name. Unlike metadata custom attributes, these custom attributes
		/// are held in the symbol store.
		/// </summary>
		/// <param name="parent">The metadata token for the object for which the attribute is requested.</param>
		/// <param name="name">A pointer to the variable that indicates the attribute to retrieve.</param>
		/// <param name="cBuffer">The size of the buffer array.</param>
		/// <param name="pcBuffer">A pointer to the variable that receives the length of the attribute data.</param>
		/// <param name="buffer">A pointer to the variable that receives the attribute data.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetSymAttribute(
			[In] int parent,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] int cBuffer,
			[Out] out int pcBuffer,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer);

		/// <summary>
		/// Gets the namespaces defined at global scope within this symbol store.
		/// </summary>
		/// <param name="cNameSpaces">The size of the namespaces array.</param>
		/// <param name="pcNameSpaces">A pointer to a variable that receives the length of the namespace list.</param>
		/// <param name="namespaces">A pointer to a variable that receives the namespace list.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetNamespaces(
			[In] int cNameSpaces,
			[Out] out int pcNameSpaces,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);

		/// <summary>
		/// Initializes the symbol reader with the metadata importer interface that this reader will be associated with,
		/// along with the file name of the module.
		/// </summary>
		/// <param name="importer">The metadata importer interface with which this reader will be associated.</param>
		/// <param name="filename">The file name of the module. You can use the pIStream parameter instead.</param>
		/// <param name="searchPath">The path to search. This parameter is optional.</param>
		/// <param name="pIStream">The file stream, used as an alternative to the filename parameter.</param>
		/// <returns>HRESULT</returns>
		/// <remarks>This method can be called only once, and must be called before any other reader methods.</remarks>
		[PreserveSig]
		new int Initialize(
			[In, MarshalAs(InteropUnmanagedType.IUnknown)] object importer,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string filename,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string searchPath,
			[In] IStream pIStream);

		/// <summary>
		/// Updates the existing symbol store with a delta symbol store. This method is used in edit-and-continue
		/// scenarios to update the symbol store to match deltas to the original portable executable (PE) file.
		/// </summary>
		/// <param name="filename">The name of the file that contains the symbol store.</param>
		/// <param name="pIStream">The file stream, used as an alternative to the filename parameter.</param>
		/// <returns>HRESULT</returns>
		/// <remarks>You need specify only one of the filename or pIStream parameters, not both. If filename is
		/// specified, the symbol store will be updated with the symbols in that file. If pIStream is specified,
		/// the store will be updated with the data from the IStream.</remarks>
		[PreserveSig]
		new int UpdateSymbolStore(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string filename,
			[In] IStream pIStream);

		/// <summary>
		/// Replaces the existing symbol store with a delta symbol store. This method is similar to the
		/// UpdateSymbolStore method, except that the given delta acts as a complete replacement rather than an update.
		/// </summary>
		/// <param name="filename">The name of the file containing the symbol store.</param>
		/// <param name="pIStream">The file stream, used as an alternative to the filename parameter.</param>
		/// <returns>HRESULT</returns>
		/// <remarks>You need specify only one of the filename or pIStream parameters, not both. If filename is
		/// specified, the symbol store will be updated with the symbols in that file. If pIStream is specified,
		/// the store will be updated with the data from the IStream.</remarks>
		[PreserveSig]
		new int ReplaceSymbolStore(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string filename,
			[In] IStream pIStream);

		/// <summary>
		/// Provides the on-disk file name of the symbol store.
		/// </summary>
		/// <param name="cchName">The size of the szName buffer.</param>
		/// <param name="pcchName">A pointer to the variable that receives the length of the name returned in szName,
		/// including the null termination.</param>
		/// <param name="szName">A pointer to the variable that receives the file name of the symbol store.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetSymbolStoreFileName(
			[In] int cchName,
			[Out] out int pcchName,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);

		/// <summary>
		/// Returns an array of methods, each of which contains the breakpoint at the given position in a document.
		/// </summary>
		/// <param name="document">The specified document.</param>
		/// <param name="line">The line of the specified document.</param>
		/// <param name="column">The column of the specified document.</param>
		/// <param name="cMethod">The size of the pRetVal array.</param>
		/// <param name="pcMethod">A pointer to a variable that receives the number of elements returned in the
		/// pRetVal array.</param>
		/// <param name="pRetVal">An array of pointers, each of which points to an ISymUnmanagedMethod object that
		/// represents a method containing the breakpoint.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetMethodsFromDocumentPosition(
			[In] ISymUnmanagedDocument document,
			[In] int line,
			[In] int column,
			[In] int cMethod,
			[Out] out int pcMethod,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 3)] ISymUnmanagedMethod[] pRetVal);

		/// <summary>
		/// Gets the specified version of the specified document. The document version starts at 1 and is incremented
		/// each time the document is updated using the UpdateSymbolStore method. If the pbCurrent parameter is true,
		/// this is the latest version of the document.
		/// </summary>
		/// <param name="pDoc">The specified document.</param>
		/// <param name="version">A pointer to a variable that receives the version of the specified document.</param>
		/// <param name="pbCurrent">A pointer to a variable that receives true if this is the latest version of the
		/// document, or false if it isn't the latest version.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetDocumentVersion(
			[In] ISymUnmanagedDocument pDoc,
			[Out] out int version,
			[Out] out bool pbCurrent);

		/// <summary>
		/// Gets the method version. The method version starts at 1 and is incremented each time the method is recompiled.
		/// Recompilation can happen without changes to the method.
		/// </summary>
		/// <param name="pMethod">The method for which to get the version.</param>
		/// <param name="version">A pointer to a variable that receives the method version.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetMethodVersion(
			[In] ISymUnmanagedMethod pMethod,
			[Out] out int version);

		#endregion

		/// <summary>
		/// Gets a symbol reader method, given a method token and an edit-and-continue version number.
		/// Version numbers start at 1 and are incremented each time the method is changed as a result of an
		/// edit-and-continue operation.
		/// </summary>
		/// <param name="token">The method metadata token.</param>
		/// <param name="version">The method version.</param>
		/// <param name="pRetVal">A pointer to the returned ISymUnmanagedMethod interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetMethodByVersionPreRemap(
			[In] int token,
			[In] int version,
			[Out] out ISymUnmanagedMethod pRetVal);

		/// <summary>
		///
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="cBuffer"></param>
		/// <param name="pcBuffer"></param>
		/// <param name="buffer"></param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSymAttributePreRemap(
			[In] int parent,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] int cBuffer,
			[Out] out int pcBuffer,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer);

		/// <summary>
		/// Gets every method that has line information in the provided document.
		/// </summary>
		/// <param name="document">A pointer to the document.</param>
		/// <param name="cMethod">A ULONG32 that indicates the size of the pRetVal array.</param>
		/// <param name="pcMethod">A pointer to a ULONG32 that receives the size of the buffer required to contain the methods.</param>
		/// <param name="pRetVal">A pointer to the buffer that receives the methods.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetMethodsInDocument(
			[In] ISymUnmanagedDocument document,
			[In] int cMethod,
			[Out] out int pcMethod,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 1)] ISymUnmanagedMethod[] pRetVal);
	}
}
