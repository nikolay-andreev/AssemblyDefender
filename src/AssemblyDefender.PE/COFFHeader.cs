using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// A standard COFF header is located immediately after the PE signature. The COFF header provides
	/// the most general characteristics of a PE/COFF file, applicable to both object and executable files.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct COFFHeader
	{
		/// <summary>
		/// The number that identifies the type of target machine.
		/// </summary>
		public MachineType Machine;

		/// <summary>
		/// The number of sections. This indicates the size of the section table, which immediately follows the headers.
		/// </summary>
		public ushort NumberOfSections;

		/// <summary>
		/// The low 32 bits of the number of seconds since 00:00 January 1, 1970 (a C run-time time_t value),
		/// that indicates when the file was created.
		/// </summary>
		public uint TimeDateStamp;

		/// <summary>
		/// The file offset of the COFF symbol table, or zero if no COFF symbol table is present.
		/// This value should be zero for an image because COFF debugging information is deprecated.
		/// </summary>
		public uint PointerToSymbolTable;

		/// <summary>
		/// The number of entries in the symbol table. This data can be used to locate the string table,
		/// which immediately follows the symbol table. This value should be zero for an image because
		/// COFF debugging information is deprecated.
		/// </summary>
		public uint NumberOfSymbols;

		/// <summary>
		/// The size of the optional header, which is required for executable files but not for object files.
		/// This value should be zero for an object file.
		/// </summary>
		public ushort SizeOfOptionalHeader;

		/// <summary>
		/// The flags that indicate the attributes of the file. For specific flag values.
		/// </summary>
		public ImageCharacteristics Characteristics;
	}
}
