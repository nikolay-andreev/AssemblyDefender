using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The MS-DOS header and stub are present in image files only. Placed at the beginning of an
	/// image file, they represent a valid application that runs under MS-DOS.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public unsafe struct DOSHeader
	{
		/// <summary>
		/// 0x4d, 0x5a ('MZ'). This is the "magic number" of an EXE file. Value: 0x5a4d
		/// </summary>
		public ushort Signature;

		/// <summary>
		/// The number of bytes in the last block of the program that are actually used. If this value is zero,
		/// that means the entire last block is used (i.e. the effective value is 512).
		/// </summary>
		public ushort BytesInLastBlock;

		/// <summary>
		/// Number of blocks in the file that are part of the EXE file. If BytesInLastBlock is non-zero,
		/// only that much of the last block is used.
		/// </summary>
		public ushort BlocksInFile;

		/// <summary>
		/// Number of relocation entries stored after the header. May be zero.
		/// </summary>
		public ushort NumRelocs;

		/// <summary>
		/// Number of paragraphs in the header. The program's data begins just after the header, and this field
		/// can be used to calculate the appropriate file offset. The header includes the relocation entries.
		/// Note that some OSs and/or programs may fail if the header is not a multiple of 512 bytes.
		/// </summary>
		public ushort HeaderParagraphs;

		/// <summary>
		/// Number of paragraphs of additional memory that the program will need. This is the equivalent of the BSS size
		/// in a Unix program. The program can't be loaded if there isn't at least this much memory available to it.
		/// </summary>
		public ushort MinExtraParagraphs;

		/// <summary>
		/// Maximum number of paragraphs of additional memory. Normally, the OS reserves all the remaining
		/// conventional memory for your program, but you can limit it with this field.
		/// </summary>
		public ushort MaxExtraParagraphs;

		/// <summary>
		/// Relative value of the stack segment. This value is added to the segment the program was loaded at,
		/// and the result is used to initialize the SS register.
		/// </summary>
		public ushort SS;

		/// <summary>
		/// Initial value of the SP register.
		/// </summary>
		public ushort SP;

		/// <summary>
		/// Word checksum. If set properly, the 16-bit sum of all words in the file should be zero.
		/// Usually, this isn't filled in.
		/// </summary>
		public ushort Checksum;

		/// <summary>
		/// Initial value of the IP register.
		/// </summary>
		public ushort IP;

		/// <summary>
		/// Initial value of the CS register, relative to the segment the program was loaded at.
		/// </summary>
		public ushort CS;

		/// <summary>
		/// Offset of the first relocation item in the file.
		/// </summary>
		public ushort RelocTableOffset;

		/// <summary>
		/// Overlay number. Normally zero, meaning that it's the main program.
		/// </summary>
		public ushort OverlayNumber;

		/// <summary>
		/// Reserved words.
		/// </summary>
		public fixed short Reserved[4];

		/// <summary>
		/// OEM identifier (for OEMInfo).
		/// </summary>
		public ushort OEMID;

		/// <summary>
		/// OEM information (OEMID specific).
		/// </summary>
		public ushort OEMInfo;

		/// <summary>
		/// Reserved words.
		/// </summary>
		public fixed short Reserved2[10];

		/// <summary>
		/// File pointer to the PE signature, which allows the operating system to properly execute the image file.
		/// </summary>
		public uint Lfanew;
	}
}
