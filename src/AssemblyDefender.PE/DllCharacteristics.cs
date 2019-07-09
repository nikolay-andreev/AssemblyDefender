using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The following values are defined for the DllCharacteristics field of the optional header.
	/// </summary>
	public enum DllCharacteristics : ushort
	{
		/// <summary>
		/// DLL can be relocated at load time.
		/// </summary>
		DYNAMIC_BASE = 0x0040,

		/// <summary>
		/// Code Integrity checks are enforced.
		/// </summary>
		FORCE_INTEGRITY = 0x0080,

		/// <summary>
		/// Image is NX compatible.
		/// </summary>
		NX_COMPAT = 0x0100,

		/// <summary>
		/// Isolation aware, but do not isolate the image.
		/// </summary>
		NO_ISOLATION = 0x0200,

		/// <summary>
		/// Does not use structured exception (SE) handling. No SE handler may be called in this image.
		/// </summary>
		NO_SEH = 0x0400,

		/// <summary>
		/// Do not bind the image.
		/// </summary>
		NO_BIND = 0x0800,

		/// <summary>
		/// A WDM driver.
		/// </summary>
		WDM_DRIVER = 0x2000,

		/// <summary>
		/// Terminal Server aware.
		/// </summary>
		TERMINAL_SERVER_AWARE = 0x8000,
	}
}
