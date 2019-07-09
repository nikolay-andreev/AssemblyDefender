using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Represents calling convention used in method.
	/// </summary>
	public enum MethodCallingConvention
	{
		/// <summary>
		/// Default (normal) method with a fixed-length argument list. ILAsm has no keyword for this
		/// calling convention.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Unmanaged calling convention.
		/// </summary>
		C = 0x1,

		/// <summary>
		/// Unmanaged calling convention.
		/// </summary>
		StdCall = 0x2,

		/// <summary>
		/// Unmanaged calling convention.
		/// </summary>
		ThisCall = 0x3,

		/// <summary>
		/// Unmanaged calling convention.
		/// </summary>
		FastCall = 0x4,

		/// <summary>
		/// Method with a variable-length argument list. The ILAsm keyword is vararg.
		/// </summary>
		VarArgs = 0x5,
	}
}
