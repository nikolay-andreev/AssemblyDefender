using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The import information begins with the import directory table, which describes the remainder of the
	/// import information. The import directory table contains address information that is used to resolve
	/// fixup references to the entry points within a DLL image. The import directory table consists of an array
	/// of import directory entries, one entry for each DLL to which the image refers. The last directory entry
	/// is empty (filled with null values), which indicates the end of the directory table.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct ImportTableHeader
	{
		/// <summary>
		/// The RVA of the import lookup table (unbound IAT). This table contains a name or ordinal for each import.
		/// (The name "Characteristics" is used in Winnt.h, but no longer describes this field.)
		/// </summary>
		public uint ImportLookupTableRVA;

		/// <summary>
		/// The stamp that is set to zero until the image is bound. After the image is bound, this field is set
		/// to the time/data stamp of the DLL.
		/// </summary>
		public int TimeDateStamp;

		/// <summary>
		/// The index of the first forwarder reference. -1 if no forwarders
		/// </summary>
		public int ForwarderChain;

		/// <summary>
		/// The address of an ASCII string that contains the name of the DLL. This address is relative to the image base.
		/// </summary>
		public uint Name;

		/// <summary>
		/// The RVA of the import address table. The contents of this table are identical to the contents of the
		/// import lookup table until the image is bound.
		/// </summary>
		public uint ImportAddressTableRVA;
	}
}
