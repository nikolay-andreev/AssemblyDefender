using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.PE
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct ResourceTableHeader
	{
		/// <summary>
		/// Reserved, must be 0.
		/// </summary>
		public uint Characteristics;

		/// <summary>
		/// The time and date that the export data was created.
		/// </summary>
		public int TimeDateStamp;

		/// <summary>
		/// The major version number. The major and minor version numbers can be set by the user.
		/// </summary>
		public ushort MajorVersion;

		/// <summary>
		/// The minor version number.
		/// </summary>
		public ushort MinorVersion;

		/// <summary>
		/// The number of directory entries immediately following the table that use strings to identify
		/// Type, Name, or Language entries (depending on the level of the table).
		/// </summary>
		public ushort NumberOfNamedEntries;

		/// <summary>
		/// The number of directory entries immediately following the Name entries that use numeric IDs for
		/// Type, Name, or Language entries.
		/// </summary>
		public ushort NumberOfIdEntries;
	}
}
