using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Specifies PInvoke attributes.
	/// </summary>
	public static class ImplMapFlags
	{
		public const int None = 0;

		/// <summary>
		/// The exported method’s name must be matched literally.
		/// </summary>
		public const int NoMangle = 0x0001;

		#region CharSet

		public const int CharSetMask = 0x0006;

		/// <summary>
		/// The method parameters of type string must be marshaled as ANSI
		/// zero-terminated strings unless explicitly specified otherwise.
		/// </summary>
		public const int Ansi = 0x0002;

		/// <summary>
		/// The method parameters of type string must be marshaled as Unicode strings.
		/// </summary>
		public const int Unicode = 0x0004;

		/// <summary>
		/// The method parameters of type string must be marshaled as ANSI or
		/// Unicode strings, depending on the underlying platform.
		/// </summary>
		public const int AutoChar = 0x0006;

		#endregion

		/// <summary>
		/// Allow "best fit" guessing when converting the strings.
		/// </summary>
		public const int BestFitOn = 0x0010;

		/// <summary>
		/// Disallow "best fit" guessing.
		/// </summary>
		public const int BestFitOff = 0x0020;

		/// <summary>
		/// The native method supports the last error querying by the Win32 API GetLastError.
		/// </summary>
		public const int LastError = 0x0040;

		#region CallConv

		public const int CallConvMask = 0x0700;

		/// <summary>
		/// The native method uses the calling convention standard for the underlying platform.
		/// </summary>
		public const int CallConvWinApi = 0x0100;

		/// <summary>
		/// The native method uses the C/C++-style calling convention; the call stack is cleaned up by the caller.
		/// </summary>
		public const int CallConvCdecl = 0x0200;

		/// <summary>
		/// The native method uses the standard Win32 API calling convention;
		/// the call stack is cleaned up by the callee.
		/// </summary>
		public const int CallConvStdCall = 0x0300;

		/// <summary>
		/// The native method uses the C++ member method (non-vararg) calling convention.
		/// The call stack is cleaned up by the callee, and the instance pointer (this) is pushed on the stack last.
		/// </summary>
		public const int CallConvThisCall = 0x0400;

		/// <summary>
		/// The native method uses the fastcall calling convention. This is much like stdcall where
		/// the first two parameters are passed in registers if possible.
		/// </summary>
		public const int CallConvFastCall = 0x0500;

		#endregion

		/// <summary>
		/// Throw an exception when an unmappable character is encountered in a string.
		/// </summary>
		public const int CharMapErrorOn = 0x1000;

		/// <summary>
		/// Don't throw an exception when an unmappable character is encountered.
		/// </summary>
		public const int CharMapErrorOff = 0x2000;
	}
}
