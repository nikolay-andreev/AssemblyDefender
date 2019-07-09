using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	public enum IASSEMBLYCACHE_UNINSTALL_DISPOSITION
	{
		/// <summary>
		/// The assembly files have been removed from the GAC.
		/// </summary>
		UNINSTALLED = 1,

		/// <summary>
		/// An application is using the assembly.
		/// This value is returned on Microsoft Windows 95 and Microsoft Windows 98.
		/// </summary>
		STILL_IN_USE = 2,

		/// <summary>
		/// The assembly does not exist in the GAC.
		/// </summary>
		ALREADY_UNINSTALLED = 3,

		/// <summary>
		/// Not used.
		/// </summary>
		DELETE_PENDING = 4,

		/// <summary>
		/// The assembly has not been removed from the GAC because another application reference exists.
		/// </summary>
		HAS_INSTALL_REFERENCES = 5,

		/// <summary>
		/// The reference that is specified in pRefData is not found in the GAC.
		/// </summary>
		REFERENCE_NOT_FOUND = 6,
	}
}
