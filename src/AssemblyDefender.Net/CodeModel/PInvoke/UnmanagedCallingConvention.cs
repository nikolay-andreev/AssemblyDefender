using System;
using System.Collections.Generic;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Specifies the calling convention required to call methods implemented in unmanaged code.
	/// </summary>
	public enum UnmanagedCallingConvention
	{
		/// <summary>
		/// Calling convention is not specified.
		/// </summary>
		None,

		/// <summary>
		/// This member is not actually a calling convention, but instead uses the default
		/// platform calling convention. For example, on Windows the default is
		/// System.Runtime.InteropServices.CallingConvention.StdCall and on Windows CE.NET it is
		/// System.Runtime.InteropServices.CallingConvention.Cdecl.
		/// </summary>
		Winapi = ImplMapFlags.CallConvWinApi,

		/// <summary>
		/// The caller cleans the stack. This enables calling functions with varargs,
		/// which makes it appropriate to use for methods that accept a variable number
		/// of parameters, such as Printf.
		/// </summary>
		Cdecl = ImplMapFlags.CallConvCdecl,

		/// <summary>
		/// The callee cleans the stack. This is the default convention for calling unmanaged functions
		/// with platform invoke.
		/// </summary>
		StdCall = ImplMapFlags.CallConvStdCall,

		/// <summary>
		/// The first parameter is the this pointer and is stored in register ECX. Other parameters are pushed
		/// on the stack. This calling convention is used to call methods on classes exported from an unmanaged DLL.
		/// </summary>
		ThisCall = ImplMapFlags.CallConvThisCall,

		/// <summary>
		/// This calling convention is not supported.
		/// </summary>
		FastCall = ImplMapFlags.CallConvFastCall,
	}
}
