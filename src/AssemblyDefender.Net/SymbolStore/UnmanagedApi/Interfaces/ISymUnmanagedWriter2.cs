using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Represents a symbol writer, and provides methods to define documents, sequence points, lexical scopes,
	/// and variables. This interface extends the ISymUnmanagedWriter interface.
	/// </summary>
	[Guid("0B97726E-9E6D-4f05-9A26-424022093CAA")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedWriter2 : ISymUnmanagedWriter
	{
		#region ISymUnmanagedWriter

		/// <summary>
		/// Defines a source document. GUIDs are provided for known languages, vendors, and document types.
		/// </summary>
		/// <param name="url">A pointer to a WCHAR that defines the uniform resource locator (URL) that identifies
		/// the document.</param>
		/// <param name="language">A pointer to a GUID that defines the document language.</param>
		/// <param name="languageVendor">A pointer to a GUID that defines the identity of the vendor for the
		/// document language.</param>
		/// <param name="documentType">A pointer to a GUID that defines the type of the document.</param>
		/// <param name="pRetVal">A pointer to the returned ISymUnmanagedWriter interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int DefineDocument(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string url,
			[In] Guid language,
			[In] Guid languageVendor,
			[In] Guid documentType,
			[Out] out ISymUnmanagedDocumentWriter pRetVal);

		/// <summary>
		/// Specifies the user-defined method that is the entry point for this module. For example,
		/// this entry point could be the user's main method instead of compiler-generated stubs before main.
		/// </summary>
		/// <param name="entryMethod">The metadata token for the method that is the user entry point.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int SetUserEntryPoint(
			[In] int entryMethod);

		/// <summary>
		/// Opens a method into which symbol information is emitted. The given method becomes the current method
		/// for calls to define sequence points, parameters, and lexical scopes. There is an implicit lexical
		/// scope around the entire method. Reopening a method that was previously closed erases any previously
		/// defined symbols for that method. There can be only one open method at a time.
		/// </summary>
		/// <param name="method">The metadata token for the method to be opened.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int OpenMethod(
			[In] int method);

		/// <summary>
		/// Closes the current method. Once a method is closed, no more symbols can be defined within it.
		/// </summary>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int CloseMethod();

		/// <summary>
		/// Opens a new lexical scope in the current method. The scope becomes the new current scope and is
		/// pushed onto a stack of scopes. Scopes must form a hierarchy. Siblings are not allowed to overlap.
		/// </summary>
		/// <param name="startOffset">The offset of the first instruction in the lexical scope, in bytes,
		/// from the beginning of the method.</param>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the scope identifier.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int OpenScope(
			[In] int startOffset,
			[Out] out int pRetVal);

		/// <summary>
		/// Closes the current lexical scope.
		/// </summary>
		/// <param name="endOffset">The offset from the beginning of the method of the point at the end of the
		/// last instruction in the lexical scope, in bytes.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int CloseScope(
			[In] int endOffset);

		/// <summary>
		/// Defines the offset range for the specified lexical scope. The scope becomes the new current scope and
		/// is pushed onto a stack of scopes. Scopes must form a hierarchy. Siblings are not allowed to overlap.
		/// </summary>
		/// <param name="scopeID">The scope identifier for the scope.</param>
		/// <param name="startOffset">The offset, in bytes, of the first instruction in the lexical scope from the
		/// beginning of the method.</param>
		/// <param name="endOffset">The offset, in bytes, of the last instruction in the lexical scope from the
		/// beginning of the method.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int SetScopeRange(
			[In] int scopeID,
			[In] int startOffset,
			[In] int endOffset);

		/// <summary>
		/// Defines a single variable in the current lexical scope. This method can be called multiple times for a
		/// variable of the same name that has multiple homes throughout a scope. In this case, however,
		/// the values of the startOffset and endOffset parameters must not overlap.
		/// </summary>
		/// <param name="name">A pointer to a WCHAR that defines the local variable name.</param>
		/// <param name="attributes">The local variable attributes.</param>
		/// <param name="cSig">A ULONG32 that indicates the size, in bytes, of the signature buffer.</param>
		/// <param name="signature">The local variable signature.</param>
		/// <param name="addrKind">The address type.</param>
		/// <param name="addr1">The first address for the parameter specification.</param>
		/// <param name="addr2">The second address for the parameter specification.</param>
		/// <param name="addr3">The third address for the parameter specification.</param>
		/// <param name="startOffset">The start offset for the variable. This parameter is optional.
		/// If it is 0, this parameter is ignored and the variable is defined throughout the entire scope.
		/// If it is a nonzero value, the variable falls within the offsets of the current scope.</param>
		/// <param name="endOffset">The end offset for the variable. This parameter is optional.
		/// If it is 0, this parameter is ignored and the variable is defined throughout the entire scope.
		/// If it is a nonzero value, the variable falls within the offsets of the current scope.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int DefineLocalVariable(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] int attributes,
			[In] int cSig,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 2)] byte[] signature,
			[In] int addrKind,
			[In] int addr1,
			[In] int addr2,
			[In] int addr3,
			[In] int startOffset,
			[In] int endOffset);

		/// <summary>
		/// Defines a single parameter in the current method. The parameter type is taken from the parameter's
		/// position (sequence) within the method's signature.
		/// If parameters are defined in the metadata for a given method, you do not have to define them again
		/// by using this method. The symbol readers must check the normal metadata for the parameters before
		/// checking the symbol store.
		/// </summary>
		/// <param name="name">The parameter name.</param>
		/// <param name="attributes">The parameter attributes.</param>
		/// <param name="sequence">The parameter signature.</param>
		/// <param name="addrKind">The address type.</param>
		/// <param name="addr1">The first address for the parameter specification.</param>
		/// <param name="addr2">The second address for the parameter specification.</param>
		/// <param name="addr3">The third address for the parameter specification.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int DefineParameter(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] int attributes,
			[In] int sequence,
			[In] int addrKind,
			[In] int addr1,
			[In] int addr2,
			[In] int addr3);

		/// <summary>
		/// Defines a single variable that is not within a method. This method is used for certain fields in
		/// classes, bit fields, and so on.
		/// </summary>
		/// <param name="parent">The metadata type or method token.</param>
		/// <param name="name">The field name.</param>
		/// <param name="attributes">The field attributes.</param>
		/// <param name="cSig">A ULONG32 that is the size, in characters, of the buffer required to
		/// contain the field signature.</param>
		/// <param name="signature">The array of field signatures.</param>
		/// <param name="addrKind">The address type</param>
		/// <param name="addr1">The first address for the field specification.</param>
		/// <param name="addr2">The second address for the field specification.</param>
		/// <param name="addr3">The third address for the field specification.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int DefineField(
			[In] int parent,
			[In] string name,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] int attributes,
			[In] int cSig,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 3)] byte[] signature,
			[In] int addrKind,
			[In] int addr1,
			[In] int addr2,
			[In] int addr3);

		/// <summary>
		/// Defines a single global variable.
		/// </summary>
		/// <param name="name">A pointer to a WCHAR that defines the global variable name.</param>
		/// <param name="attributes">The global variable attributes.</param>
		/// <param name="cSig">A ULONG32 that indicates the size, in characters, of the signature buffer.</param>
		/// <param name="signature">The global variable signature.</param>
		/// <param name="addrKind">The address type.</param>
		/// <param name="addr1">The first address for the parameter specification.</param>
		/// <param name="addr2">The second address for the parameter specification.</param>
		/// <param name="addr3">The third address for the parameter specification.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int DefineGlobalVariable(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] int attributes,
			[In] int cSig,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 2)] byte[] signature,
			[In] int addrKind,
			[In] int addr1,
			[In] int addr2,
			[In] int addr3);

		/// <summary>
		/// Closes the symbol writer after committing the symbols to the symbol store.
		/// </summary>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int Close();

		/// <summary>
		/// Defines a custom attribute based upon its name. These attributes are held in the symbol store,
		/// unlike metadata custom attributes.
		/// </summary>
		/// <param name="parent">The metadata token for which the attribute is being defined.</param>
		/// <param name="name">A pointer to a WCHAR that contains the attribute name.</param>
		/// <param name="cData">A ULONG32 that indicates the size of the data array.</param>
		/// <param name="data">The attribute value.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int SetSymAttribute(
			[In] int parent,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] int cData,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data);

		/// <summary>
		/// Opens a new namespace. Call this method before defining methods or variables that occupy a namespace.
		/// Namespaces can be nested.
		/// </summary>
		/// <param name="name">A pointer to the name of the new namespace.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int OpenNamespace(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name);

		/// <summary>
		/// Closes the most recently opened namespace.
		/// </summary>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int CloseNamespace();

		/// <summary>
		/// Specifies that the given fully qualified namespace name is being used within the currently open
		/// lexical scope. The namespace will be used within all scopes that inherit from the currently open scope.
		/// Closing the current scope will also stop the use of the namespace.
		/// </summary>
		/// <param name="fullName">A pointer to the fully qualified name of the namespace.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int UsingNamespace(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string fullName);

		/// <summary>
		/// Specifies the true start and end of a method within a source file. Use this method to specify the
		/// extent of a method independently of the sequence points that exist within the method.
		/// </summary>
		/// <param name="startDoc">A pointer to the document containing the starting position.</param>
		/// <param name="startLine">The starting line number.</param>
		/// <param name="startColumn">The starting column.</param>
		/// <param name="endDoc">A pointer to the document containing the ending position.</param>
		/// <param name="endLine">The ending line number.</param>
		/// <param name="endColumn">The ending column number.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int SetMethodSourceRange(
			[In] ISymUnmanagedDocumentWriter startDoc,
			[In] int startLine,
			[In] int startColumn,
			[In] ISymUnmanagedDocumentWriter endDoc,
			[In] int endLine,
			[In] int endColumn);

		/// <summary>
		/// Sets the metadata emitter interface with which this writer will be associated, and sets the output
		/// file name to which the debugging symbols will be written.
		/// This method can be called only once, and it must be called before any other writer methods.
		/// Some writers may require a file name. However, you can always pass a file name to this method without
		/// any negative effect on writers that do not use the file name.
		/// </summary>
		/// <param name="emitter">A pointer to the metadata emitter interface.</param>
		/// <param name="filename">The file name to which the debugging symbols are written. If a file name is
		/// specified for a writer that does not use file names, this parameter is ignored.</param>
		/// <param name="pIStream">If specified, the symbol writer will emit the symbols into the given IStream
		/// rather than to the file specified in the filename parameter. The pIStream parameter is optional.</param>
		/// <param name="fFullBuild">true if this is a full rebuild; false if this is an incremental compilation.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int Initialize(
			[In, MarshalAs(InteropUnmanagedType.IUnknown)] object emitter,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string filename,
			[In] IStream pIStream,
			[In] bool fFullBuild);

		/// <summary>
		/// Returns the information necessary for a compiler to write the debug directory entry in the portable
		/// executable (PE) file header. The symbol writer fills out all fields except for TimeDateStamp and
		/// PointerToRawData. (The compiler is responsible for setting these two fields appropriately.)
		/// A compiler should call this method, emit the data blob to the PE file, set the PointerToRawData field
		/// in the IMAGE_DEBUG_DIRECTORY to point to the emitted data, and write the IMAGE_DEBUG_DIRECTORY to the
		/// PE file. The compiler should also set the TimeDateStamp field to equal the TimeDateStamp of the
		/// PE file being generated.
		/// </summary>
		/// <param name="pIDD">A pointer to an IMAGE_DEBUG_DIRECTORY that the symbol writer will fill out.</param>
		/// <param name="cData">A DWORD that contains the size of the debug data.</param>
		/// <param name="pcData">A pointer to a DWORD that receives the size of the buffer required to contain
		/// the debug data.</param>
		/// <param name="data">A pointer to a buffer that is large enough to hold the debug data for the symbol store.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int GetDebugInfo(
			[In, Out] ref IMAGE_DEBUG_DIRECTORY pIDD,
			[In] int cData,
			[Out] out int pcData,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data);

		/// <summary>
		/// Defines a group of sequence points within the current method. Each starting line and starting
		/// column define the start of a statement within a method. Each ending line and ending column define
		/// the end of a statement within a method. The arrays should be sorted in increasing order of offsets.
		/// The offset is always measured from the start of the method, in bytes.
		/// </summary>
		/// <param name="document">The document object for which the sequence points are being defined.</param>
		/// <param name="spCount">A ULONG32 that that indicates the size of each of the offsets, lines, columns,
		/// endLines, and endColumns buffers.</param>
		/// <param name="offsets">The offset of the sequence points measured from the beginning of the method.</param>
		/// <param name="lines">The starting line numbers of the sequence points.</param>
		/// <param name="columns">The starting column numbers of the sequence points.</param>
		/// <param name="endLines">The ending line numbers of the sequence points. This parameter is optional.</param>
		/// <param name="endColumns">The ending column numbers of the sequence points. This parameter is optional.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int DefineSequencePoints(
			[In] ISymUnmanagedDocumentWriter document,
			[In] int spCount,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] int[] offsets,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] int[] lines,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] int[] columns,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] int[] endLines,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] int[] endColumns);

		/// <summary>
		/// Notifies the symbol writer that a metadata token has been remapped as the metadata was emitted.
		/// If the symbol writer has stored the old token within the symbol store, it must either update the
		/// stored token with the new value, or it must save the map for the corresponding symbol reader to
		/// remap during the read phase.
		/// </summary>
		/// <param name="oldToken">The metadata token that was remapped.</param>
		/// <param name="newToken">The new metadata token to which oldToken was remapped.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int RemapToken(
			[In] int oldToken,
			[In] int newToken);

		/// <summary>
		/// Sets the metadata emitter interface with which this writer will be associated, and sets the output file
		/// name to which the debugging symbols will be written. This method also lets you set the final location
		/// of the program database (PDB) file.
		/// </summary>
		/// <param name="emitter">A pointer to the metadata emitter interface.</param>
		/// <param name="tempfilename">A pointer to a WCHAR that contains the file name to which the debugging
		/// symbols are written. If a file name is specified for a writer that does not use file names,
		/// this parameter is ignored.</param>
		/// <param name="pIStream">If specified, the symbol writer emits the symbols into the given IStream
		/// rather than to the file specified in the filename parameter. The pIStream parameter is optional.</param>
		/// <param name="fFullBuild">true if this is a full rebuild; false if this is an incremental compilation. </param>
		/// <param name="finalfilename">A pointer to a WCHAR that is the path string to the final location
		/// of the PDB file.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int Initialize2(
			[In, MarshalAs(InteropUnmanagedType.IUnknown)] object emitter,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string tempfilename,
			[In] IStream pIStream,
			[In] bool fFullBuild,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string finalfilename);

		/// <summary>
		/// Defines a name for a constant value.
		/// </summary>
		/// <param name="name">A pointer to a WCHAR that defines the constant name.</param>
		/// <param name="value">The value of the constant.</param>
		/// <param name="cSig">The size of the signature array.</param>
		/// <param name="signature">The type signature for the constant.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int DefineConstant(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] IntPtr value,
			[In] int cSig,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 2)] byte[] signature);

		/// <summary>
		/// Closes the symbol writer without committing the symbols to the symbol store. After this call,
		/// the symbol writer becomes invalid for further updates. To commit the symbols and close the
		/// symbol writer, use the ISymUnmanagedWriter::Close method instead.
		/// </summary>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		new int Abort();

		#endregion

		/// <summary>
		/// Defines a single variable in the current lexical scope. This method can be called multiple times
		/// for a variable of the same name that has multiple homes throughout a scope. In this case, however,
		/// the values of the startOffset and endOffset parameters must not overlap.
		/// </summary>
		/// <param name="name">The local variable name.</param>
		/// <param name="attributes">The local variable attributes.</param>
		/// <param name="sigToken">The metadata token of the signature.</param>
		/// <param name="addrKind">The address type.</param>
		/// <param name="addr1">The first address for the parameter specification.</param>
		/// <param name="addr2">The second address for the parameter specification.</param>
		/// <param name="addr3">The third address for the parameter specification.</param>
		/// <param name="startOffset">The start offset for the variable. This parameter is optional.
		/// If it is 0, this parameter is ignored and the variable is defined throughout the entire scope.
		/// If it is a nonzero value, the variable falls within the offsets of the current scope.</param>
		/// <param name="endOffset">The end offset for the variable. This parameter is optional.
		/// If it is 0, this parameter is ignored and the variable is defined throughout the entire scope.
		/// If it is a nonzero value, the variable falls within the offsets of the current scope.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int DefineLocalVariable2(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] int attributes,
			[In] int sigToken,
			[In] int addrKind,
			[In] int addr1,
			[In] int addr2,
			[In] int addr3,
			[In] int startOffset,
			[In] int endOffset);

		/// <summary>
		/// Defines a single global variable.
		/// </summary>
		/// <param name="name">The global variable name.</param>
		/// <param name="attributes">The global variable attributes.</param>
		/// <param name="sigToken">The metadata token of the signature.</param>
		/// <param name="addrKind">The address type.</param>
		/// <param name="addr1">The first address for the parameter specification.</param>
		/// <param name="addr2">The second address for the parameter specification.</param>
		/// <param name="addr3">The third address for the parameter specification.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int DefineGlobalVariable2(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] int attributes,
			[In] int sigToken,
			[In] int addrKind,
			[In] int addr1,
			[In] int addr2,
			[In] int addr3);

		/// <summary>
		/// Defines a name for a constant value.
		/// </summary>
		/// <param name="name">The constant name.</param>
		/// <param name="value">The value of the constant.</param>
		/// <param name="sigToken">The metadata token of the constant.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int DefineConstant2(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string name,
			[In] IntPtr value,
			[In] int sigToken);
	}
}
