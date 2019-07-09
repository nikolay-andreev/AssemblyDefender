using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public static class ILMethodSect
	{
		public const int Reserved = 0;
		public const int EHTable = 1;
		public const int OptILTable = 2;

		public const int KindMask = 0x3F; // The mask for decoding the type code
		public const int FatFormat = 0x40; // fat format
		public const int MoreSects = 0x80; // there is another attribute after this one
	}
}
