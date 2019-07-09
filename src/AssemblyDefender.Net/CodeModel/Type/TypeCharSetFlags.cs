using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum TypeCharSetFlags : int
	{
		/// <summary>
		/// When interoperating with native methods, the managed strings are by default marshaled to
		/// and from ANSI strings. Managed strings are instances of the System.String class defined
		/// in the .NET Framework class library. Marshaling is a general term for data conversion on the
		/// managed and unmanaged code boundaries.
		/// </summary>
		Ansi = 0,

		/// <summary>
		/// By default, managed strings are marshaled to and from Unicode (UTF-16).
		/// </summary>
		Unicode = 0x10000,

		/// <summary>
		/// The default string marshaling is defined by the underlying platform.
		/// </summary>
		Autochar = 0x20000,
	}
}
