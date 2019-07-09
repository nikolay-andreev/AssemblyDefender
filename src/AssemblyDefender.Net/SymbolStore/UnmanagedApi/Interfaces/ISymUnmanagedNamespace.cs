using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Represents a namespace.
	/// </summary>
	[Guid("0DFF7289-54F8-11D3-BD28-0000F80849BD")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedNamespace
	{
		/// <summary>
		/// Gets the name of this namespace.
		/// </summary>
		/// <param name="cchName">A ULONG32 that indicates the size of the szName buffer.</param>
		/// <param name="pcchName">A pointer to a ULONG32 that receives the size, in characters, of the buffer
		/// required to contain the namespace name, including the null termination.</param>
		/// <param name="szName">A pointer to a buffer that contains the namespace name.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetName(
			[In] int cchName,
			[Out] out int pcchName,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);

		/// <summary>
		/// Gets the children of this namespace.
		/// </summary>
		/// <param name="cNameSpaces">A ULONG32 that indicates the size of the namespaces array.</param>
		/// <param name="pcNameSpaces">A pointer to a ULONG32 that receives the size, in characters, of the buffer
		/// required to contain the namespaces.</param>
		/// <param name="namespaces">A pointer to the buffer that contains the namespaces.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetNamespaces(
			[In] int cNameSpaces,
			[Out] out int pcNameSpaces,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);

		/// <summary>
		/// Returns all variables defined at global scope within this namespace.
		/// </summary>
		/// <param name="cVars">A ULONG32 that indicates the size of the pVars array.</param>
		/// <param name="pcVars">A pointer to a ULONG32 that receives the size of the buffer required to
		/// contain the namespaces.</param>
		/// <param name="pVars">A pointer to a buffer that contains the namespaces.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetVariables(
			[In] int cVars,
			[Out] out int pcVars,
			[Out, MarshalAs(InteropUnmanagedType.LPArray,
				ArraySubType = InteropUnmanagedType.Interface,
				SizeParamIndex = 0)] ISymUnmanagedVariable[] pVars);
	}
}
