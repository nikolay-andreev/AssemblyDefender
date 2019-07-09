using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The type of table column.
	/// </summary>
	public static class MetadataColumnType
	{
		public const int Int16 = 96;
		public const int UInt16 = 97;
		public const int Int32 = 98;
		public const int UInt32 = 99;
		public const int Byte2 = 100; // 1 byte + 1 byte padding
		public const int String = 101;
		public const int Guid = 102;
		public const int Blob = 103;
	}
}
