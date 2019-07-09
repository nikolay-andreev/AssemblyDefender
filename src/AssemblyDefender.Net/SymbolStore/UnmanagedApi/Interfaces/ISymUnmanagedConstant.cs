using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Provides access to unmanaged constants.
	/// </summary>
	[Guid("48B25ED8-5BAD-41BC-9CEE-CD62FABC74E9")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface ISymUnmanagedConstant
	{
		/// <summary>
		/// Gets the name of the constant.
		/// </summary>
		/// <param name="cchName">The length of the buffer that the szName parameter points to.</param>
		/// <param name="pcchName">A pointer to a ULONG32 that receives the size, in characters, of the
		/// buffer required to contain the name, including the null termination.</param>
		/// <param name="szName">The buffer that stores the name.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetName(
			[In] int cchName,
			[Out] out int pcchName,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);

		/// <summary>
		/// Gets the value of the constant.
		/// </summary>
		/// <param name="pValue">A pointer to a variable that receives the value.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetValue(
			[Out] out IntPtr pValue);

		/// <summary>
		/// Gets the signature of the constant.
		/// </summary>
		/// <param name="cSig">The length of the buffer that the pcSig parameter points to.</param>
		/// <param name="pcSig">A pointer to a ULONG32 that receives the size, in characters, of the
		/// buffer required to contain the signature.</param>
		/// <param name="sig">The buffer that stores the signature.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int GetSignature(
			[In] int cSig,
			[Out] out int pcSig,
			[Out, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 0)] byte[] sig);
	}
}
