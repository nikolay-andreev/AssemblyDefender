using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public static class ILMethodFlags
	{
		public const int InitLocals = 0x0010; // call default constructor on all local vars
		public const int MoreSects = 0x0008; // there is another attribute after this one

		// Indicates the format for the COR_ILMETHOD header
		public const int FormatShift = 3;
		public const int FormatMask = ((1 << FormatShift) - 1);
		public const int TinyFormat = 0x0002; // use this code if the code size is even
		public const int SmallFormat = 0x0000;
		public const int FatFormat = 0x0003;
		public const int TinyFormat1 = 0x0006; // use this code if the code size is odd
	}
}
