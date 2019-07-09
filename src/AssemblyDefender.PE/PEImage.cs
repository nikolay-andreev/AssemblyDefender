using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// Represents portable executable (PE) image.
	/// </summary>
	public class PEImage : IDisposable
	{
		#region Fields

		private bool _is32Bits;
		private MachineType _machine;
		private uint _addressOfEntryPoint;
		private ulong _imageBase;
		private uint _sectionAlignment;
		private uint _fileAlignment;
		private SubsystemType _subsystem;
		private ImageCharacteristics _characteristics;
		private DllCharacteristics _dllCharacteristics;
		private ulong _sizeOfStackReserve;
		private DataDirectory[] _directories = new DataDirectory[PEConstants.NumberOfRvaAndSizes];
		private PESection[] _sections;
		private IBinarySource _source;
		private bool _disposeSource;

		#endregion

		#region Ctors

		public PEImage(IBinarySource source, bool disposeSource = true)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			_source = source;
			_disposeSource = disposeSource;
			Read();
		}

		#endregion

		#region Properties

		public string Location
		{
			get { return _source.Location; }
		}

		public bool DisposeSource
		{
			get { return _disposeSource; }
			set { _disposeSource = value; }
		}

		public bool Is32Bits
		{
			get { return _is32Bits; }
		}

		/// <summary>
		/// Gets a value that identifies the type of target machine.
		/// </summary>
		public MachineType Machine
		{
			get { return _machine; }
		}

		/// <summary>
		/// RVA of the entry point function. For unmanaged DLLs, this can be 0. For
		/// managed PE files, this value always points to the common language runtime
		/// invocation stub.
		/// </summary>
		public uint AddressOfEntryPoint
		{
			get { return _addressOfEntryPoint; }
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
		}

		/// <summary>
		/// Alignment of sections when loaded in memory. This setting must be
		/// greater than or equal to the value of the FileAlignment field. The default
		/// is the memory page size.
		/// </summary>
		public uint SectionAlignment
		{
			get { return _sectionAlignment; }
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
		}

		/// <summary>
		/// User interface subsystem required to run this image file.
		/// </summary>
		public SubsystemType Subsystem
		{
			get { return _subsystem; }
		}

		/// <summary>
		/// The flags that indicate the attributes of the file. For specific flag values.
		/// </summary>
		public ImageCharacteristics Characteristics
		{
			get { return _characteristics; }
		}

		/// <summary>
		/// In managed files of v1.0, always set to 0. In managed files of v1.1 and later, always set
		/// to 0x400: no unmanaged Windows structural exception handling.
		/// </summary>
		public DllCharacteristics DllCharacteristics
		{
			get { return _dllCharacteristics; }
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
		}

		public DataDirectory[] Directories
		{
			get { return _directories; }
		}

		public PESection[] Sections
		{
			get { return _sections; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Open an accessor.
		/// </summary>
		public IBinaryAccessor OpenImage()
		{
			return _source.Open();
		}

		public IBinaryAccessor OpenImage(long position)
		{
			var accessor = _source.Open();

			if (position != 0)
				accessor.Position = position;

			return accessor;
		}

		public IBinaryAccessor OpenImageToSectionData(uint rva)
		{
			IBinaryAccessor accessor;
			PESection section;
			if (!TryOpenImageToSectionData(rva, out accessor, out section))
			{
				throw new BadImageFormatException(string.Format(SR.ResolveImageSectionFailure, rva));
			}

			return accessor;
		}

		public IBinaryAccessor OpenImageToSectionData(uint rva, out PESection section)
		{
			IBinaryAccessor accessor;
			if (!TryOpenImageToSectionData(rva, out accessor, out section))
			{
				throw new BadImageFormatException(string.Format(SR.ResolveImageSectionFailure, rva));
			}

			return accessor;
		}

		public bool TryOpenImageToSectionData(uint rva, out IBinaryAccessor accessor)
		{
			PESection section;
			return TryOpenImageToSectionData(rva, out accessor, out section);
		}

		public virtual bool TryOpenImageToSectionData(uint rva, out IBinaryAccessor accessor, out PESection section)
		{
			section = FindSection(rva);
			if (section == null)
			{
				accessor = null;
				return false;
			}

			long position = section.GetPointerToRawData(rva);

			accessor = OpenImage();
			accessor.Position = position;

			return true;
		}

		public PESection FindSection(uint rva)
		{
			foreach (PESection section in _sections)
			{
				if (section.Contains(rva))
				{
					return section;
				}
			}

			return null;
		}

		public bool ResolvePositionToSectionData(uint rva, out long offset)
		{
			PESection section;
			return ResolvePositionToSectionData(rva, out offset, out section);
		}

		public bool ResolvePositionToSectionData(uint rva, out long offset, out PESection section)
		{
			section = FindSection(rva);
			if (section == null)
			{
				offset = 0;
				return false;
			}

			offset = section.GetPointerToRawData(rva);
			return true;
		}

		public long ResolvePositionToSectionData(uint rva)
		{
			PESection section;
			return ResolvePositionToSectionData(rva, out section);
		}

		public long ResolvePositionToSectionData(uint rva, out PESection section)
		{
			section = FindSection(rva);
			if (section == null)
			{
				throw new BadImageFormatException(string.Format(SR.ResolveImageSectionFailure, rva));
			}

			return section.GetPointerToRawData(rva);
		}

		public void Dispose()
		{
			if (_source != null)
			{
				if (_disposeSource)
				{
					_source.Dispose();
				}

				_source = null;
			}
		}

		protected void Read()
		{
			using (var accessor = OpenImage())
			{
				Read(accessor);
			}
		}

		protected unsafe void Read(IBinaryAccessor accessor)
		{
			// DOS
			DOSHeader dosHeader;
			fixed (byte* pBuff = accessor.ReadBytes(PEConstants.DosHeaderSize))
			{
				dosHeader = *(DOSHeader*)pBuff;
			}

			if (dosHeader.Signature != PEConstants.DosSignature)
			{
				throw new BadImageFormatException(SR.DOSHeaderSignatureNotValid);
			}

			accessor.Position = dosHeader.Lfanew;

			// NT Signature
			if (accessor.ReadUInt32() != PEConstants.NTSignature)
			{
				throw new BadImageFormatException(SR.PESignatureNotValid);
			}

			// COFF
			COFFHeader coffHeader;
			fixed (byte* pBuff = accessor.ReadBytes(PEConstants.COFFHeaderSize))
			{
				coffHeader = *(COFFHeader*)pBuff;
			}

			_characteristics = coffHeader.Characteristics;
			_machine = coffHeader.Machine;

			// PE
			ushort peMagic = accessor.ReadUInt16();
			accessor.Position -= 2;
			if (peMagic == PEConstants.PEMagic32)
			{
				_is32Bits = true;

				PEHeader peHeader;
				fixed (byte* pBuff = accessor.ReadBytes(PEConstants.PEHeaderSize))
				{
					peHeader = *(PEHeader*)pBuff;
				}

				_addressOfEntryPoint = peHeader.AddressOfEntryPoint;
				_imageBase = peHeader.ImageBase;
				_sectionAlignment = peHeader.SectionAlignment;
				_fileAlignment = peHeader.FileAlignment;
				_subsystem = peHeader.Subsystem;
				_dllCharacteristics = peHeader.DllCharacteristics;
				_sizeOfStackReserve = peHeader.SizeOfStackReserve;
			}
			else if (peMagic == 0x20b)
			{
				_is32Bits = false;

				PEHeader64 peHeader;
				fixed (byte* pBuff = accessor.ReadBytes(PEConstants.PEHeader64Size))
				{
					peHeader = *(PEHeader64*)pBuff;
				}

				_addressOfEntryPoint = peHeader.AddressOfEntryPoint;
				_imageBase = peHeader.ImageBase;
				_sectionAlignment = peHeader.SectionAlignment;
				_fileAlignment = peHeader.FileAlignment;
				_subsystem = peHeader.Subsystem;
				_dllCharacteristics = peHeader.DllCharacteristics;
				_sizeOfStackReserve = peHeader.SizeOfStackReserve;
			}
			else
			{
				throw new BadImageFormatException(SR.PEHeaderSignatureNotValid);
			}

			// Directories
			for (int i = 0; i < PEConstants.NumberOfRvaAndSizes; i++)
			{
				fixed (byte* pBuff = accessor.ReadBytes(8))
				{
					_directories[i] = *(DataDirectory*)pBuff;
				}
			}

			// Sections
			_sections = new PESection[coffHeader.NumberOfSections];
			for (int i = 0; i < coffHeader.NumberOfSections; i++)
			{
				_sections[i] = PESection.Read(accessor);
			}
		}

		#endregion

		#region Static

		public static bool IsValidFile(string filePath)
		{
			return IsValid(new StreamAccessor(filePath, false));
		}

		public static bool IsValid(IBinaryAccessor accessor)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			if (accessor.Length - accessor.Position < 2)
				return false;

			ushort signature = accessor.ReadUInt16();
			accessor.Position -= 2;

			if (signature != PEConstants.DosSignature)
				return false;

			return true;
		}

		public static PEImage Load(byte[] buffer, string location)
		{
			return Load(buffer, 0, buffer.Length, location);
		}

		public static PEImage Load(byte[] buffer, int offset, int count, string location)
		{
			var blob = new Blob(buffer, offset, count, false);
			var source = new BlobBinarySource(blob, location);
			return new PEImage(source);
		}

		public static PEImage LoadFile(string filePath, FileLoadMode loadMode = FileLoadMode.OnDemand)
		{
			IBinarySource source;
			switch (loadMode)
			{
				case FileLoadMode.OnDemand:
					source = new FileBinarySource(filePath, false);
					break;

				case FileLoadMode.InMemory:
					source = new BlobBinarySource(File.ReadAllBytes(filePath), filePath);
					break;

				case FileLoadMode.MemoryMappedFile:
					source = new MemoryMappedFileBinarySource(filePath, 0L, 0L, false);
					break;

				default:
					throw new NotImplementedException();
			}

			return new PEImage(source);
		}

		#endregion
	}
}
