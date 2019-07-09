using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The machine type of a COFF header represents type of target machine.
	/// </summary>
	public enum MachineType : ushort
	{
		/// <summary>
		/// The contents of this field are assumed to be applicable to any machine type
		/// </summary>
		UNKNOWN = 0x0,

		/// <summary>
		/// Matsushita AM33
		/// </summary>
		AM33 = 0x1d3,

		/// <summary>
		/// x64
		/// </summary>
		AMD64 = 0x8664,

		/// <summary>
		/// ARM little endian
		/// </summary>
		ARM = 0x1c0,

		/// <summary>
		/// EFI byte code
		/// </summary>
		EBC = 0xebc,

		/// <summary>
		/// Intel 386 or later processors and compatible processors
		/// </summary>
		I386 = 0x14c,

		/// <summary>
		/// Intel Itanium processor family
		/// </summary>
		IA64 = 0x200,

		/// <summary>
		/// Mitsubishi M32R little endian
		/// </summary>
		M32R = 0x9041,

		/// <summary>
		/// MIPS16
		/// </summary>
		MIPS16 = 0x266,

		/// <summary>
		/// MIPS with FPU
		/// </summary>
		MIPSFPU = 0x366,

		/// <summary>
		/// MIPS16 with FPU
		/// </summary>
		MIPSFPU16 = 0x466,

		/// <summary>
		/// Power PC little endian
		/// </summary>
		POWERPC = 0x1f0,

		/// <summary>
		/// Power PC with floating point support
		/// </summary>
		POWERPCFP = 0x1f1,

		/// <summary>
		/// MIPS little endian
		/// </summary>
		R4000 = 0x166,

		/// <summary>
		/// Hitachi SH3
		/// </summary>
		SH3 = 0x1a2,

		/// <summary>
		/// Hitachi SH3 DSP
		/// </summary>
		SH3DSP = 0x1a3,

		/// <summary>
		/// Hitachi SH4
		/// </summary>
		SH4 = 0x1a6,

		/// <summary>
		/// Hitachi SH5
		/// </summary>
		SH5 = 0x1a8,

		/// <summary>
		/// Thumb
		/// </summary>
		THUMB = 0x1c2,

		/// <summary>
		/// MIPS little-endian WCE v2
		/// </summary>
		WCEMIPSV2 = 0x169,
	}
}
