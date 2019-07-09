using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The Subsystem of a PE header.
	/// </summary>
	public enum SubsystemType : ushort
	{
		/// <summary>
		/// An unknown subsystem
		/// </summary>
		UNKNOWN = 0,

		/// <summary>
		/// Device drivers and native Windows processes
		/// </summary>
		NATIVE = 1,

		/// <summary>
		/// The Windows graphical user interface (GUI) subsystem
		/// </summary>
		WINDOWS_GUI = 2,

		/// <summary>
		/// The Windows character subsystem
		/// </summary>
		WINDOWS_CUI = 3,

		/// <summary>
		/// The Posix character subsystem
		/// </summary>
		POSIX_CUI = 7,

		/// <summary>
		/// Windows CE
		/// </summary>
		WINDOWS_CE_GUI = 9,

		/// <summary>
		/// An Extensible Firmware Interface (EFI) application
		/// </summary>
		EFI_APPLICATION = 10,

		/// <summary>
		/// An EFI driver with boot services
		/// </summary>
		EFI_BOOT_SERVICE_DRIVER = 11,

		/// <summary>
		/// An EFI driver with run-time services
		/// </summary>
		EFI_RUNTIME_DRIVER = 12,

		/// <summary>
		/// An EFI ROM image
		/// </summary>
		EFI_ROM = 13,

		/// <summary>
		/// XBOX
		/// </summary>
		XBOX = 14,
	}
}
