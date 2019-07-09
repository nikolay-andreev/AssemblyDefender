using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Represents a variable, such as a parameter, a local variable, or a field.
	/// </summary>
	[Guid("9F60EEBE-2D9A-3F7C-BF58-80BC991C60BB")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedVariable
	{
		/// <summary>
		/// Gets the name of this variable.
		/// </summary>
		/// <param name="cchName">The length of the buffer that the pcchName parameter points to.</param>
		/// <param name="pcchName"> pointer to a ULONG32 that receives the size, in characters, of the buffer
		/// required to contain the name, including the null termination.</param>
		/// <param name="szName">The buffer that stores the name.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetName(
			[In] int cchName,
			[Out] out int pcchName,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);

		/// <summary>
		/// Gets the attribute flags for this variable.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the attributes. The returned value will be
		/// one of the values defined in the CorSymVarFlag enumeration.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetAttributes(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the signature of this variable.
		/// </summary>
		/// <param name="cSig">The length of the buffer pointed to by the sig parameter.</param>
		/// <param name="pcSig">A pointer to a ULONG32 that receives the size, in characters, of the buffer
		/// required to contain the signature.</param>
		/// <param name="sig">The buffer that stores the signature.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSignature(
			[In] int cSig,
			[Out] out int pcSig,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] byte[] sig);

		/// <summary>
		/// Gets the kind of address of this variable.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the value. The possible values are defined
		/// in the CorSymAddrKind enumeration.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetAddressKind(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the first address field for this variable. Its meaning depends on the kind of address.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the first address field.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetAddressField1(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the second address field for this variable. Its meaning depends on the kind of address.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the second address field.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetAddressField2(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the third address field for this variable. Its meaning depends on the kind of address.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the third address field.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetAddressField3(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the start offset of this variable within its parent. If this is a local variable within a scope,
		/// the start offset will fall within the offsets defined for the scope.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the start offset.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetStartOffset(
			[Out] out int pRetVal);

		/// <summary>
		/// Gets the end offset of this variable within its parent. If this is a local variable within a scope,
		/// the end offset will fall within the offsets defined for the scope.
		/// </summary>
		/// <param name="pRetVal">A pointer to a ULONG32 that receives the end offset.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetEndOffset(
			[Out] out int pRetVal);
	}
}
