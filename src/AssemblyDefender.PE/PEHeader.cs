using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The 64bits PE header, which immediately follows the COFF header, provides information for the OS loader.
	/// Although this header is referred to as the optional header, it is optional only in the sense that
	/// object files usually don’t contain it. For PE files, this header is mandatory.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct PEHeader
	{
		#region Standard

		/// <summary>
		/// The PE signature that usually (but not necessarily) immediately follows the MS-DOS stub
		/// is a 4-byte item, identifying the file as a PE format image file. The signature contains the characters
		/// P and E, followed by 2 null bytes.
		/// </summary>
		public ushort Magic;

		/// <summary>
		/// Linker major version number. The VC++ linker sets this field to 8; the pure-IL file
		/// generator employed by other compilers does the same. In earlier versions, this field
		/// was set to 7 and 6, respectively.
		/// </summary>
		public byte MajorLinkerVersion;

		/// <summary>
		/// Linker minor version number.
		/// </summary>
		public byte MinorLinkerVersion;

		/// <summary>
		/// Size of the code section (.text) or the sum of all code sections if multiple code
		/// sections exist. The IL assembler always emits a single code section.
		/// </summary>
		public uint SizeOfCode;

		/// <summary>
		/// Size of the initialized data section (held in field SizeOfRawData of the respective
		/// section header) or the sum of all such sections. The initialized data is defined
		/// as specific values, stored in the disk image file.
		/// </summary>
		public uint SizeOfInitializedData;

		/// <summary>
		/// Size of the uninitialized data section (.bss) or the sum of all such sections. This data is
		/// not part of the disk file and does not have specific values, but the OS loader commits
		/// memory space for this data when the file is loaded.
		/// </summary>
		public uint SizeOfUninitializedData;

		/// <summary>
		/// RVA of the entry point function. For unmanaged DLLs, this can be 0. For
		/// managed PE files, this value always points to the common language runtime
		/// invocation stub.
		/// </summary>
		public uint AddressOfEntryPoint;

		/// <summary>
		/// RVA of the beginning of the file’s code section(s).
		/// </summary>
		public uint BaseOfCode;

		/// <summary>
		/// RVA of the beginning of the file’s data section(s).
		/// This entry doesn’t exist in the 64-bit Optional header.
		/// </summary>
		public uint BaseOfData;

		#endregion

		#region NT Specific

		/// <summary>
		/// Image's preferred starting virtual address; must be aligned on the
		/// 64KB boundary (0x10000). In ILAsm, this field can be specified explicitly
		/// by the directive .imagebase {integer value} and/or the command-line
		/// option /BASE={integer value}. The command-line option takes
		/// precedence over the directive.
		/// </summary>
		public uint ImageBase;

		/// <summary>
		/// Alignment of sections when loaded in memory. This setting must be
		/// greater than or equal to the value of the FileAlignment field. The default
		/// is the memory page size.
		/// </summary>
		public uint SectionAlignment;

		/// <summary>
		/// Alignment of sections in the disk image file. The value should be a
		/// power of 2, from 512 to 64,000 0x200 (to 0x10000). If SectionAlignment
		/// is set to less than the memory page size, FileAlignment must match
		/// SectionAlignment. In ILAsm, this field can be specified explicitly by
		/// the directive .file alignment {integer value} and/or the
		/// command-line option /ALIGNMENT={integer value}.
		/// The command-line option takes precedence over the directive.
		/// </summary>
		public uint FileAlignment;

		/// <summary>
		/// Major version number of the required operating system.
		/// </summary>
		public ushort MajorOperatingSystemVersion;

		/// <summary>
		/// Minor version number of the operating system.
		/// </summary>
		public ushort MinorOperatingSystemVersion;

		/// <summary>
		/// Major version number of the application.
		/// </summary>
		public ushort MajorImageVersion;

		/// <summary>
		/// Minor version number of the application.
		/// </summary>
		public ushort MinorImageVersion;

		/// <summary>
		/// Major version number of the subsystem.
		/// </summary>
		public ushort MajorSubsystemVersion;

		/// <summary>
		/// Minor version number of the subsystem.
		/// </summary>
		public ushort MinorSubsystemVersion;

		/// <summary>
		/// Reserved.
		/// </summary>
		public uint Win32VersionValue;

		/// <summary>
		/// Size of the image file (in bytes), including all headers. This field
		/// must be set to a multiple of the SectionAlignment value.
		/// </summary>
		public uint SizeOfImage;

		/// <summary>
		/// Sum of the sizes of the MS-DOS header and stub, the COFF header, the PE header,
		/// and the section headers, rounded up to a multiple of the FileAlignment value.
		/// </summary>
		public uint SizeOfHeaders;

		/// <summary>
		/// Checksum of the disk image file.
		/// </summary>
		public uint CheckSum;

		/// <summary>
		/// User interface subsystem required to run this image file.
		/// </summary>
		public SubsystemType Subsystem;

		/// <summary>
		/// In managed files of v1.0, always set to 0. In managed files of v1.1 and later, always set
		/// to 0x400: no unmanaged Windows structural exception handling.
		/// </summary>
		public DllCharacteristics DllCharacteristics;

		/// <summary>
		/// Size of virtual memory to reserve for the initial thread’s stack. Only the
		/// SizeOfStackCommit field is committed; the rest is available in one-page increments.
		/// The default is 1MB for 32-bit images and 4MB for 64-bit images. In ILAsm, this field
		/// can be specified explicitly by the directive .stackreserve {integer value} and/or
		/// the command-line option /STACK={integer value}. The command-line option takes
		/// precedence over the directive.
		/// </summary>
		public uint SizeOfStackReserve;

		/// <summary>
		/// Size of virtual memory initially committed for the initial thread’s stack. The default is
		/// one page (4KB) for 32-bit images and 16KB for 64-bit images.
		/// </summary>
		public uint SizeOfStackCommit;

		/// <summary>
		/// Size of virtual memory to reserve for the initial process heap. Only the
		/// SizeOfHeapCommit field is committed; the rest is available in one-page increments.
		/// The default is 1MB for both 32-bit and 64bit images.
		/// </summary>
		public uint SizeOfHeapReserve;

		/// <summary>
		/// Size of virtual memory initially committed for the process heap. The default is 4KB
		/// (one operating system memory page) for 32-bit images and 2KB for 64-bit images.
		/// </summary>
		public uint SizeOfHeapCommit;

		/// <summary>
		/// Obsolete, set to 0.
		/// </summary>
		public uint LoaderFlags;

		/// <summary>
		/// Number of entries in the DataDirectory array; at least 16. Although it is theoretically
		/// possible to emit more than 16 data directories, all existing managed compilers
		/// emit exactly 16 data directories, with the 16th (last) data directory never used (reserved).
		/// </summary>
		public uint NumberOfRvaAndSizes;

		#endregion
	}
}
