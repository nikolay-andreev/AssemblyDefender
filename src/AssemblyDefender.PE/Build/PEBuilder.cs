using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// Build PE (Portable Executable) image.
	/// </summary>
	public class PEBuilder
	{
		#region Fields

		private bool _is32Bits;
		private MachineType _machine;
		private ImageCharacteristics _characteristics;
		private uint _addressOfEntryPoint;
		private ulong _imageBase;
		private uint _sectionAlignment;
		private uint _fileAlignment;
		private SubsystemType _subsystem;
		private DllCharacteristics _dllCharacteristics;
		private ulong _sizeOfStackReserve;
		private ulong _sizeOfStackCommit;
		private ulong _sizeOfHeapReserve;
		private ulong _sizeOfHeapCommit;
		private byte _majorLinkerVersion;
		private byte _minorLinkerVersion;
		private ushort _majorOperatingSystemVersion;
		private ushort _minorOperatingSystemVersion;
		private ushort _majorImageVersion;
		private ushort _minorImageVersion;
		private ushort _majorSubsystemVersion;
		private ushort _minorSubsystemVersion;
		private uint _baseOfCode;
		private uint _baseOfData;
		private uint _sizeOfCode;
		private uint _sizeOfInitializedData;
		private uint _sizeOfUninitializedData;
		private uint _sizeOfImage;
		private uint _sizeOfHeaders;
		private DateTime _timeDateStamp;
		private DataDirectory[] _directories;
		private BuildTaskCollection _tasks;
		private BuildSectionCollection _sections;
		private BuildFixupCollection _fixups;

		#endregion

		#region Ctors

		public PEBuilder()
		{
			_is32Bits = true;
			_machine = MachineType.I386;
			_characteristics = ImageCharacteristics.EXECUTABLE_IMAGE | ImageCharacteristics.MACHINE_32BIT;
			_imageBase = 0x400000;
			_sectionAlignment = 0x2000;
			_fileAlignment = 0x200;
			_subsystem = SubsystemType.WINDOWS_CUI;
			_dllCharacteristics =
				DllCharacteristics.DYNAMIC_BASE |
				DllCharacteristics.NX_COMPAT |
				DllCharacteristics.NO_SEH;
			_sizeOfStackReserve = 0x100000;
			_sizeOfStackCommit = 0x1000;
			_sizeOfHeapReserve = 0x100000;
			_sizeOfHeapCommit = 0x1000;
			_majorLinkerVersion = 8;
			_minorLinkerVersion = 0;
			_majorOperatingSystemVersion = 4;
			_minorOperatingSystemVersion = 0;
			_majorSubsystemVersion = 4;
			_minorSubsystemVersion = 0;
			_timeDateStamp = DateTime.Now;
			_directories = new DataDirectory[PEConstants.NumberOfRvaAndSizes];

			_tasks = new BuildTaskCollection(this);
			_sections = new BuildSectionCollection(this);
			_fixups = new BuildFixupCollection(this);
		}

		#endregion

		#region Properties

		public bool Is32Bits
		{
			get { return _is32Bits; }
			set { _is32Bits = value; }
		}

		/// <summary>
		/// Gets a value that identifies the type of target machine.
		/// </summary>
		public MachineType Machine
		{
			get { return _machine; }
			set
			{
				_machine = value;

				switch (_machine)
				{
					case MachineType.I386:
						Is32Bits = true;
						break;

					case MachineType.IA64:
					case MachineType.AMD64:
						Is32Bits = false;
						break;
				}
			}
		}

		/// <summary>
		/// The flags that indicate the attributes of the file. For specific flag values.
		/// </summary>
		public ImageCharacteristics Characteristics
		{
			get { return _characteristics; }
			set { _characteristics = value; }
		}

		/// <summary>
		/// RVA of the entry point function. For unmanaged DLLs, this can be 0. For
		/// managed PE files, this value always points to the common language runtime
		/// invocation stub.
		/// </summary>
		public uint AddressOfEntryPoint
		{
			get { return _addressOfEntryPoint; }
			set { _addressOfEntryPoint = value; }
		}

		/// <summary>
		/// Image's preferred starting virtual address; must be aligned on the
		/// 64KB boundary (0x10000). In ILAsm, this field can be specified explicitly
		/// by the directive .imagebase {integer value} and/or the command-line
		/// option /BASE={integer value}. The command-line option takes
		/// precedence over the directive.
		/// </summary>
		public ulong ImageBase
		{
			get { return _imageBase; }
			set { _imageBase = value; }
		}

		/// <summary>
		/// Alignment of sections when loaded in memory. This setting must be
		/// greater than or equal to the value of the FileAlignment field. The default
		/// is the memory page size.
		/// </summary>
		public uint SectionAlignment
		{
			get { return _sectionAlignment; }
			set { _sectionAlignment = value; }
		}

		/// <summary>
		/// Alignment of sections in the disk image file. The value should be a
		/// power of 2, from 512 to 64,000 0x200 (to 0x10000). If SectionAlignment
		/// is set to less than the memory page size, FileAlignment must match
		/// SectionAlignment. In ILAsm, this field can be specified explicitly by
		/// the directive .file alignment {integer value} and/or the
		/// command-line option /ALIGNMENT={integer value}.
		/// The command-line option takes precedence over the directive.
		/// </summary>
		public uint FileAlignment
		{
			get { return _fileAlignment; }
			set { _fileAlignment = value; }
		}

		/// <summary>
		/// User interface subsystem required to run this image file.
		/// </summary>
		public SubsystemType Subsystem
		{
			get { return _subsystem; }
			set { _subsystem = value; }
		}

		/// <summary>
		/// In managed files of v1.0, always set to 0. In managed files of v1.1 and later, always set
		/// to 0x400: no unmanaged Windows structural exception handling.
		/// </summary>
		public DllCharacteristics DllCharacteristics
		{
			get { return _dllCharacteristics; }
			set { _dllCharacteristics = value; }
		}

		/// <summary>
		/// Size of virtual memory to reserve for the initial thread’s stack. Only the
		/// SizeOfStackCommit field is committed; the rest is available in one-page increments.
		/// The default is 1MB for 32-bit images and 4MB for 64-bit images. In ILAsm, this field
		/// can be specified explicitly by the directive .stackreserve {integer value} and/or
		/// the command-line option /STACK={integer value}. The command-line option takes
		/// precedence over the directive.
		/// </summary>
		public ulong SizeOfStackReserve
		{
			get { return _sizeOfStackReserve; }
			set { _sizeOfStackReserve = value; }
		}

		/// <summary>
		/// Size of virtual memory initially committed for the initial thread’s stack. The default is
		/// one page (4KB) for 32-bit images and 16KB for 64-bit images.
		/// </summary>
		public ulong SizeOfStackCommit
		{
			get { return _sizeOfStackCommit; }
			set { _sizeOfStackCommit = value; }
		}

		/// <summary>
		/// Size of virtual memory to reserve for the initial process heap. Only the
		/// SizeOfHeapCommit field is committed; the rest is available in one-page increments.
		/// The default is 1MB for both 32-bit and 64bit images.
		/// </summary>
		public ulong SizeOfHeapReserve
		{
			get { return _sizeOfHeapReserve; }
			set { _sizeOfHeapReserve = value; }
		}

		/// <summary>
		/// Size of virtual memory initially committed for the process heap. The default is 4KB
		/// (one operating system memory page) for 32-bit images and 2KB for 64-bit images.
		/// </summary>
		public ulong SizeOfHeapCommit
		{
			get { return _sizeOfHeapCommit; }
			set { _sizeOfHeapCommit = value; }
		}

		/// <summary>
		/// Linker major version number. The VC++ linker sets this field to 8; the pure-IL file
		/// generator employed by other compilers does the same. In earlier versions, this field
		/// was set to 7 and 6, respectively.
		/// </summary>
		public byte MajorLinkerVersion
		{
			get { return _majorLinkerVersion; }
			set { _majorLinkerVersion = value; }
		}

		/// <summary>
		/// Linker minor version number.
		/// </summary>
		public byte MinorLinkerVersion
		{
			get { return _minorLinkerVersion; }
			set { _minorLinkerVersion = value; }
		}

		/// <summary>
		/// Major version number of the required operating system.
		/// </summary>
		public ushort MajorOperatingSystemVersion
		{
			get { return _majorOperatingSystemVersion; }
			set { _majorOperatingSystemVersion = value; }
		}

		/// <summary>
		/// Minor version number of the operating system.
		/// </summary>
		public ushort MinorOperatingSystemVersion
		{
			get { return _minorOperatingSystemVersion; }
			set { _minorOperatingSystemVersion = value; }
		}

		/// <summary>
		/// Major version number of the application.
		/// </summary>
		public ushort MajorImageVersion
		{
			get { return _majorImageVersion; }
			set { _majorImageVersion = value; }
		}

		/// <summary>
		/// Minor version number of the application.
		/// </summary>
		public ushort MinorImageVersion
		{
			get { return _minorImageVersion; }
			set { _minorImageVersion = value; }
		}

		/// <summary>
		/// Major version number of the subsystem.
		/// </summary>
		public ushort MajorSubsystemVersion
		{
			get { return _majorSubsystemVersion; }
			set { _majorSubsystemVersion = value; }
		}

		/// <summary>
		/// Minor version number of the subsystem.
		/// </summary>
		public ushort MinorSubsystemVersion
		{
			get { return _minorSubsystemVersion; }
			set { _minorSubsystemVersion = value; }
		}

		/// <summary>
		/// RVA of the beginning of the file’s code section(s).
		/// </summary>
		public uint BaseOfCode
		{
			get { return _baseOfCode; }
		}

		/// <summary>
		/// RVA of the beginning of the file’s data section(s).
		/// This entry doesn’t exist in the 64-bit Optional header.
		/// </summary>
		public uint BaseOfData
		{
			get { return _baseOfData; }
		}

		/// <summary>
		/// Size of the code section (.text) or the sum of all code sections if multiple code
		/// sections exist. The IL assembler always emits a single code section.
		/// </summary>
		public uint SizeOfCode
		{
			get { return _sizeOfCode; }
		}

		/// <summary>
		/// Size of the initialized data section (held in field SizeOfRawData of the respective
		/// section header) or the sum of all such sections. The initialized data is defined
		/// as specific values, stored in the disk image file.
		/// </summary>
		public uint SizeOfInitializedData
		{
			get { return _sizeOfInitializedData; }
		}

		/// <summary>
		/// Size of the uninitialized data section (.bss) or the sum of all such sections. This data is
		/// not part of the disk file and does not have specific values, but the OS loader commits
		/// memory space for this data when the file is loaded.
		/// </summary>
		public uint SizeOfUninitializedData
		{
			get { return _sizeOfUninitializedData; }
		}

		/// <summary>
		/// Size of the image file (in bytes), including all headers. This field
		/// must be set to a multiple of the SectionAlignment value.
		/// </summary>
		public uint SizeOfImage
		{
			get { return _sizeOfImage; }
		}

		/// <summary>
		/// Sum of the sizes of the MS-DOS header and stub, the COFF header, the PE header,
		/// and the section headers, rounded up to a multiple of the FileAlignment value.
		/// </summary>
		public uint SizeOfHeaders
		{
			get { return _sizeOfHeaders; }
		}

		public DateTime TimeDateStamp
		{
			get { return _timeDateStamp; }
			set { _timeDateStamp = value; }
		}

		public DataDirectory[] Directories
		{
			get { return _directories; }
		}

		public BuildTaskCollection Tasks
		{
			get { return _tasks; }
		}

		public BuildSectionCollection Sections
		{
			get { return _sections; }
		}

		public BuildFixupCollection Fixups
		{
			get { return _fixups; }
		}

		#endregion

		#region Methods

		public void Build()
		{
			// Build tasks
			foreach (var task in _tasks)
			{
				task.Build();
			}

			// Remove empty sections
			for (int i = _sections.Count - 1; i >= 0; i--)
			{
				if (_sections[i].Blobs.Count == 0)
				{
					_sections.RemoveAt(i);
				}
			}

			// Init
			if (_is32Bits)
			{
				_sizeOfHeaders = (uint)
					(
						PEConstants.DosHeaderSize +
						PEConstants.DosX86Stub.Length +
						4 + // NTSignature
						PEConstants.COFFHeaderSize +
						PEConstants.PEHeaderSize +
						PEConstants.NumberOfRvaAndSizes * 8 +
						PEConstants.SectionHeaderSize * _sections.Count
					);
			}
			else
			{
				_sizeOfHeaders = (uint)
					(
						PEConstants.DosHeaderSize +
						PEConstants.DosX86Stub.Length +
						4 + // NTSignature
						PEConstants.COFFHeaderSize +
						PEConstants.PEHeader64Size +
						PEConstants.NumberOfRvaAndSizes * 8 +
						PEConstants.SectionHeaderSize * _sections.Count
					);
			}

			_sizeOfHeaders = _sizeOfHeaders.Align(_fileAlignment);
			_sizeOfImage = _sizeOfHeaders;
			_baseOfCode = 0;
			_baseOfData = 0;
			_sizeOfCode = 0;
			_sizeOfInitializedData = 0;
			_sizeOfUninitializedData = 0;

			if (_is32Bits)
				_characteristics |= ImageCharacteristics.MACHINE_32BIT;
			else
				_characteristics &= ~ImageCharacteristics.MACHINE_32BIT;

			// Build sections
			uint virtualAddress = _sectionAlignment;
			uint pointerToRawData = _sizeOfHeaders;

			foreach (BuildSection section in _sections)
			{
				section._rva = virtualAddress;
				section._pointerToRawData = pointerToRawData;

				uint blobLength = 0;
				foreach (var blob in section.Blobs)
				{
					if (blob.OffsetAlignment > 0)
					{
						blobLength = blobLength.Align(blob.OffsetAlignment);
					}

					blob._rva = virtualAddress + blobLength;
					blob._pointerToRawData = pointerToRawData + blobLength;
					blobLength += (uint)blob.Length;
				}

				section._virtualSize = blobLength;
				section._sizeOfRawData = blobLength.Align(_fileAlignment);

				virtualAddress += section.VirtualSize.Align(_sectionAlignment);
				pointerToRawData += section.SizeOfRawData;

				if ((section.Characteristics & SectionCharacteristics.ContainsCode) != 0)
				{
					_sizeOfCode += section.SizeOfRawData;
					if (_baseOfCode == 0)
					{
						_baseOfCode = section.RVA;
					}
				}

				if ((section.Characteristics & SectionCharacteristics.ContainsInitializedData) != 0)
				{
					_sizeOfInitializedData += section.SizeOfRawData;
					if (_baseOfData == 0)
					{
						_baseOfData = section.RVA;
					}
				}

				if ((section.Characteristics & SectionCharacteristics.ContainsUninitializedData) != 0)
				{
					_sizeOfUninitializedData += section.SizeOfRawData;
				}
			}

			_sizeOfImage = virtualAddress;

			// Apply fixups
			foreach (var fixup in _fixups)
			{
				fixup.ApplyFixup();
			}
		}

		public void Save(IBinaryAccessor accessor)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			// DOS
			accessor.Write((ushort)PEConstants.DosSignature);
			accessor.Write((ushort)0x90); // BytesInLastBlock
			accessor.Write((ushort)3); // BlocksInFile
			accessor.Write((ushort)0); // NumRelocs
			accessor.Write((ushort)4); // HeaderParagraphs
			accessor.Write((ushort)0); // MinExtraParagraphs
			accessor.Write((ushort)0xFFFF); // MaxExtraParagraphs
			accessor.Write((ushort)0); // SS
			accessor.Write((ushort)0xB8); // SP
			accessor.Write((ushort)0); // Checksum
			accessor.Write((ushort)0); // IP
			accessor.Write((ushort)0); // CS
			accessor.Write((ushort)0x40); // RelocTableOffset
			accessor.Write((ushort)0); // OverlayNumber
			accessor.Write(0, 32); // Reserved

			if ((_characteristics & ImageCharacteristics.EXECUTABLE_IMAGE) != 0)
				accessor.Write((uint)(PEConstants.DosHeaderSize + PEConstants.DosX86Stub.Length)); // Lfanew
			else
				accessor.Write((uint)0); // Image file is not valid

			accessor.Write(PEConstants.DosX86Stub);

			// NT Signature
			accessor.Write((uint)PEConstants.NTSignature);

			// COFF
			accessor.Write((ushort)Machine);
			accessor.Write((ushort)_sections.Count); // NumberOfSections
			accessor.Write((uint)TimeDateStamp.To_time_t());
			accessor.Write((uint)0); // PointerToSymbolTable
			accessor.Write((uint)0); // NumberOfSymbols
			accessor.Write((ushort)(Is32Bits ? 0xE0 : 0xF0)); // SizeOfOptionalHeader
			accessor.Write((ushort)Characteristics);

			// PE Standard
			if (Is32Bits)
				accessor.Write((ushort)PEConstants.PEMagic32);
			else
				accessor.Write((ushort)PEConstants.PEMagic64);

			accessor.Write((byte)MajorLinkerVersion);
			accessor.Write((byte)MinorLinkerVersion);
			accessor.Write((uint)SizeOfCode);
			accessor.Write((uint)SizeOfInitializedData);
			accessor.Write((uint)SizeOfUninitializedData);
			accessor.Write((uint)AddressOfEntryPoint);
			accessor.Write((uint)BaseOfCode);

			if (Is32Bits)
				accessor.Write((uint)BaseOfData);

			// NT Specific
			if (Is32Bits)
				accessor.Write((uint)ImageBase);
			else
				accessor.Write((ulong)ImageBase);

			accessor.Write((uint)SectionAlignment);
			accessor.Write((uint)FileAlignment);
			accessor.Write((ushort)MajorOperatingSystemVersion);
			accessor.Write((ushort)MinorOperatingSystemVersion);
			accessor.Write((ushort)MajorImageVersion);
			accessor.Write((ushort)MinorImageVersion);
			accessor.Write((ushort)MajorSubsystemVersion);
			accessor.Write((ushort)MinorSubsystemVersion);
			accessor.Write((uint)0); // Win32VersionValue
			accessor.Write((uint)SizeOfImage);
			accessor.Write((uint)SizeOfHeaders);
			accessor.Write((uint)0); // CheckSum
			accessor.Write((ushort)Subsystem);
			accessor.Write((ushort)DllCharacteristics);

			if (Is32Bits)
				accessor.Write((uint)SizeOfStackReserve);
			else
				accessor.Write((ulong)SizeOfStackReserve);

			if (Is32Bits)
				accessor.Write((uint)SizeOfStackCommit);
			else
				accessor.Write((ulong)SizeOfStackCommit);

			if (Is32Bits)
				accessor.Write((uint)SizeOfHeapReserve);
			else
				accessor.Write((ulong)SizeOfHeapReserve);

			if (Is32Bits)
				accessor.Write((uint)SizeOfHeapCommit);
			else
				accessor.Write((ulong)SizeOfHeapCommit);

			accessor.Write((uint)0); // LoaderFlags (reserved)
			accessor.Write((uint)PEConstants.NumberOfRvaAndSizes);

			// Directories
			for (int i = 0; i < PEConstants.NumberOfRvaAndSizes; i++)
			{
				accessor.Write(_directories[i]);
			}

			// Write section headers
			foreach (var section in _sections)
			{
				string sectionName = section.Name.PadRight(8, '\0');
				accessor.Write(sectionName, Encoding.ASCII);
				accessor.Write((uint)section.VirtualSize);
				accessor.Write((uint)section.RVA);
				accessor.Write((uint)section.SizeOfRawData);
				accessor.Write((uint)section.PointerToRawData);
				accessor.Write((uint)0); // PointerToRelocations
				accessor.Write((uint)0); // PointerToLinenumbers
				accessor.Write((ushort)0); // NumberOfRelocations
				accessor.Write((ushort)0); // NumberOfLinenumbers
				accessor.Write((uint)section.Characteristics);
			}

			accessor.Write(0, (int)(SizeOfHeaders - accessor.Position));

			// Write section data
			foreach (var section in _sections)
			{
				int blobLength = 0;
				foreach (var blob in section.Blobs)
				{
					if (blob.OffsetAlignment > 0)
					{
						int alignCount = blobLength.Align(blob.OffsetAlignment) - blobLength;
						if (alignCount > 0)
						{
							accessor.Write(new byte[alignCount]);
							blobLength += alignCount;
						}
					}

					accessor.Write(blob.GetBuffer(), 0, blob.Length);
					blobLength += blob.Length;
				}

				accessor.Write(0, (int)(section.SizeOfRawData - blobLength));
			}
		}

		public byte[] Save()
		{
			var blob = new Blob();
			using (var accessor = new BlobAccessor(blob))
			{
				Save(accessor);
			}

			return blob.ToArray();
		}

		public void SaveToFile(string filePath)
		{
			using (var accessor = new StreamAccessor(new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
			{
				Save(accessor);
			}
		}

		public BuildSection GetSection(string name)
		{
			var section = _sections.FirstOrDefault(s => s.Name == name);
			if (section == null)
			{
				section = new BuildSection();
				section.Name = name;

				switch (name)
				{
					// Executable code (free format)
					case PESectionNames.Text:
						section.Priority = 1000;
						section.Characteristics =
							SectionCharacteristics.ContainsCode |
							SectionCharacteristics.MemExecute |
							SectionCharacteristics.MemoryRead;
						break;

					// Initialized data (free format)
					case PESectionNames.Data:
					case PESectionNames.SData:
						section.Priority = 2000;
						section.Characteristics =
							SectionCharacteristics.ContainsInitializedData |
							SectionCharacteristics.MemoryRead |
							SectionCharacteristics.MemoryWrite;
						break;

					// Export tables
					case PESectionNames.EData:
						section.Priority = 3000;
						section.Characteristics =
							SectionCharacteristics.ContainsInitializedData |
							SectionCharacteristics.MemoryRead;
						break;

					// Import tables
					case PESectionNames.IData:
						section.Priority = 4000;
						section.Characteristics =
							SectionCharacteristics.ContainsInitializedData |
							SectionCharacteristics.MemoryRead |
							SectionCharacteristics.MemoryWrite;
						break;

					// Exception information
					case PESectionNames.PData:
						section.Priority = 5000;
						section.Characteristics =
							SectionCharacteristics.ContainsInitializedData |
							SectionCharacteristics.MemoryRead;
						break;

					// Read-only initialized data
					case PESectionNames.RData:
						section.Priority = 6000;
						section.Characteristics =
							SectionCharacteristics.ContainsInitializedData |
							SectionCharacteristics.MemoryRead;
						break;

					// Thread-local storage
					case PESectionNames.Tls:
						section.Priority = 7000;
						section.Characteristics =
							SectionCharacteristics.ContainsInitializedData |
							SectionCharacteristics.MemoryRead |
							SectionCharacteristics.MemoryWrite;
						break;

					// Resource directory
					case PESectionNames.Rsrc:
						section.Priority = 8000;
						section.Characteristics =
							SectionCharacteristics.ContainsInitializedData |
							SectionCharacteristics.MemoryRead;
						break;

					// Image relocations
					case PESectionNames.Reloc:
						section.Priority = 9000;
						section.Characteristics =
							SectionCharacteristics.ContainsInitializedData |
							SectionCharacteristics.MemDiscardable |
							SectionCharacteristics.MemoryRead;
						break;

					default:
						throw new NotImplementedException(string.Format("Section name '{0}' is not supported.", name));
				}

				Sections.Add(section, section.Priority);
			}

			return section;
		}

		#endregion

		#region Nested types

		public class SetDataDirectoryFromBlobRVAFixup : BuildFixup
		{
			#region Fields

			private int _dataDirectoryID;
			private BuildBlob _blob;
			private int _offset;
			private int _size;

			#endregion

			#region Ctors

			public SetDataDirectoryFromBlobRVAFixup()
			{
			}

			public SetDataDirectoryFromBlobRVAFixup(int dataDirectoryID, BuildBlob blob)
				: this(dataDirectoryID, blob, 0, blob.Length)
			{
			}

			public SetDataDirectoryFromBlobRVAFixup(int dataDirectoryID, BuildBlob blob, int offset)
				: this(dataDirectoryID, blob, offset, blob.Length)
			{
			}

			public SetDataDirectoryFromBlobRVAFixup(int dataDirectoryID, BuildBlob blob, int offset, int size)
			{
				_dataDirectoryID = dataDirectoryID;
				_blob = blob;
				_offset = offset;
				_size = size;
			}

			#endregion

			#region Properties

			public int DataDirectoryID
			{
				get { return _dataDirectoryID; }
				set { _dataDirectoryID = value; }
			}

			public BuildBlob Blob
			{
				get { return _blob; }
				set { _blob = value; }
			}

			public int Offset
			{
				get { return _offset; }
				set { _offset = value; }
			}

			public int Size
			{
				get { return _size; }
				set { _size = value; }
			}

			#endregion

			#region Methods

			public override void ApplyFixup()
			{
				uint rva = _blob.RVA + (uint)_offset;
				PE.Directories[_dataDirectoryID] = new DataDirectory(rva, _size);
			}

			#endregion
		}

		#endregion
	}
}
