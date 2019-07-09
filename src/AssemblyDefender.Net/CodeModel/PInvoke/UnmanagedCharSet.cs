using System;
using System.Collections.Generic;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Dictates which character set marshaled strings should use.
	/// </summary>
	public enum UnmanagedCharSet
	{
		/// <summary>
		/// This value is obsolete and has the same behavior as System.Runtime.InteropServices.CharSet.Ansi.
		/// </summary>
		None,

		/// <summary>
		/// Marshal strings as multiple-byte character strings.
		/// </summary>
		Ansi = ImplMapFlags.Ansi,

		/// <summary>
		/// Marshal strings as Unicode 2-byte characters.
		/// </summary>
		Unicode = ImplMapFlags.Unicode,

		/// <summary>
		/// Automatically marshal strings appropriately for the target operating system.
		/// The default is System.Runtime.InteropServices.CharSet.Unicode on Windows NT,
		/// Windows 2000, Windows XP, and the Windows Server 2003 family; the default
		/// is System.Runtime.InteropServices.CharSet.Ansi on Windows 98 and Windows Me.
		/// Although the common language runtime default is System.Runtime.InteropServices.CharSet.Auto,
		/// languages may override this default. For example, by default C# marks all
		/// methods and types as System.Runtime.InteropServices.CharSet.Ansi.
		/// </summary>
		Auto = ImplMapFlags.AutoChar,
	}
}
