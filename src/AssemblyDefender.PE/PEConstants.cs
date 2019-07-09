using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public static class PEConstants
	{
		public const int NumberOfRvaAndSizes = 16;
		public const ushort DosSignature = 0x5A4D; // MZ
		public const uint NTSignature = 0x00004550;  // PE00
		public const ushort PEMagic32 = 0x10b;
		public const ushort PEMagic64 = 0x20b;

		#region Header Sizes

		public static readonly int DosHeaderSize;
		public static readonly int COFFHeaderSize;
		public static readonly int PEHeaderSize;
		public static readonly int PEHeader64Size;
		public static readonly int SectionHeaderSize;

		#endregion

		#region Stubs

		/// <summary>
		/// This is the stub program that says it can't be run in DOS mode
		/// it is x86 specific, but so is dos so I suppose that is OK
		/// </summary>
		public static readonly byte[] DosX86Stub = new byte[]
		{
			0x0e, 0x1f, 0xba, 0x0e, 0x00, 0xb4, 0x09, 0xcd, 0x21, 0xb8, 0x01, 0x4c, 0xcd, 0x21, 0x54, 0x68,
			0x69, 0x73, 0x20, 0x70, 0x72, 0x6f, 0x67, 0x72, 0x61, 0x6d, 0x20, 0x63, 0x61, 0x6e, 0x6e, 0x6f,
			0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6e, 0x20, 0x69, 0x6e, 0x20, 0x44, 0x4f, 0x53, 0x20,
			0x6d, 0x6f, 0x64, 0x65, 0x2e, 0x0d, 0x0d, 0x0a, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
		};

		#endregion

		#region Ctor

		static unsafe PEConstants()
		{
			DosHeaderSize = sizeof(DOSHeader);
			COFFHeaderSize = sizeof(COFFHeader);
			PEHeaderSize = sizeof(PEHeader);
			PEHeader64Size = sizeof(PEHeader64);
			SectionHeaderSize = sizeof(SectionHeader);
		}

		#endregion
	}
}
