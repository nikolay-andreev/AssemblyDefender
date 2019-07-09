using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Represents a lexical scope within a method.
	/// </summary>
	[Guid("68005D0F-B8E0-3B01-84D5-A11A94154942")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedScope
	{
		/// <summary>
		/// Gets the method that contains this scope.
		/// </summary>
		/// <param name="pRetVal">A pointer to the returned ISymUnmanagedMethod interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetMethod(
			[Out] out ISymUnmanagedMethod pRetVal);

		/// <summary>
		/// Gets the parent scope of this scope.
		/// </summary>
		/// <param name="pRetVal">A pointer to the returned ISymUnmanagedScope interface.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetParent(
			[Out] out ISymUnmanagedScope pRetVal);

		/// <summary>
		/// Gets the children of this scope.
		/// </summary>
		/// <param name="cChildren">A ULONG32 that indicates the size of the children array.</param>
		/// <param name="pcChildren">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the children.</param>
		/// <param name="children">The returned array of children.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetChildren(
			[In] int cChildren,
			[Out] out int pcChildren,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedScope[] children);

		/// <summary>
		/// Gets the start offset for this scope.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that contains the starting offset.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetStartOffset(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the end offset for this scope.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the end offset.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetEndOffset(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets a count of the local variables defined within this scope.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the count of local variables.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetLocalCount(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the local variables defined within this scope.
		/// </summary>
		/// <param name="cLocals">A ULONG32 that indicates the size of the locals array.</param>
		/// <param name="pcLocals">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the local variables.</param>
		/// <param name="locals">The array that receives the local variables.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetLocals(
			[In] int cLocals,
			[Out] out int pcLocals,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedVariable[] locals);

		/// <summary>
		/// Gets the namespaces that are being used within this scope.
		/// </summary>
		/// <param name="cNameSpaces">The size of the namespaces array.</param>
		/// <param name="pcNameSpaces">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the namespaces.</param>
		/// <param name="namespaces">The array that receives the namespaces.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetNamespaces(
			[In] int cNameSpaces,
			[Out] out int pcNameSpaces,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);
	}
}
