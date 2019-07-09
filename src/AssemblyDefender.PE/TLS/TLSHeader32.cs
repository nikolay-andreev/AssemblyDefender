using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The 32-bit TLS directory.
	/// The StartAddressOfRawData, EndAddressOfRawData, AddressOfIndex, and AddressOfCallBacks fields hold VAs rather than
	/// RVAs, so you need to define the base relocations for them in the .reloc section.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct TLSHeader32
	{
		/// <summary>
		/// The starting address of the TLS template. The template is a block of data that is used to initialize
		/// TLS data. The system copies all of this data each time a thread is created, so it must not be corrupted.
		/// Note that this address is not an RVA; it is an address for which there should be a base relocation in the
		/// .reloc section.
		/// </summary>
		public uint StartAddressOfRawData;

		/// <summary>
		/// The address of the last byte of the TLS, except for the zero fill. As with the Raw Data Start VA field,
		/// this is a VA, not an RVA.
		/// </summary>
		public uint EndAddressOfRawData;

		/// <summary>
		/// The location to receive the TLS index, which the loader assigns. This location is in an ordinary
		/// data section, so it can be given a symbolic name that is accessible to the program.
		/// </summary>
		public uint AddressOfIndex;

		/// <summary>
		/// The pointer to an array of TLS callback functions. The array is null-terminated, so if no callback
		/// function is supported, this field points to 4 bytes set to zero.
		/// </summary>
		public uint AddressOfCallBacks;

		/// <summary>
		/// The size in bytes of the template, beyond the initialized data delimited by the Raw Data Start VA and
		/// Raw Data End VA fields. The total template size should be the same as the total size of TLS data in the
		/// image file. The zero fill is the amount of data that comes after the initialized nonzero data.
		/// </summary>
		public int SizeOfZeroFill;

		/// <summary>
		/// Reserved for possible future use by TLS flags.
		/// </summary>
		public uint Characteristics;
	}
}
