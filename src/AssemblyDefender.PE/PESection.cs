using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class PESection
	{
		#region Fields

		private string _name;
		private uint _virtualSize;
		private uint _virtualAddress;
		private uint _sizeOfRawData;
		private uint _pointerToRawData;
		private SectionCharacteristics _characteristics;

		#endregion

		#region Ctors

		private PESection()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the section.
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// The total size of the section when loaded into memory. If this value is greater than
		/// SizeOfRawData, the section is zero-padded. This field is valid only for executable images
		/// and should be set to zero for object files.
		/// </summary>
		public uint VirtualSize
		{
			get { return _virtualSize; }
		}

		/// <summary>
		/// For executable images, the address of the first byte of the section relative to the image base
		/// when the section is loaded into memory. For object files, this field is the address of the
		/// first byte before relocation is applied; for simplicity, compilers should set this to zero.
		/// Otherwise, it is an arbitrary value that is subtracted from offsets during relocation.
		/// </summary>
		public uint VirtualAddress
		{
			get { return _virtualAddress; }
		}

		/// <summary>
		/// The size of the section (for object files) or the size of the initialized data on disk
		/// (for image files). For executable images, this must be a multiple of FileAlignment from the
		/// optional header. If this is less than VirtualSize, the remainder of the section is zero-filled.
		/// Because the SizeOfRawData field is rounded but the VirtualSize field is not, it is possible
		/// for SizeOfRawData to be greater than VirtualSize as well. When a section contains only
		/// uninitialized data, this field should be zero.
		/// </summary>
		public uint SizeOfRawData
		{
			get { return _sizeOfRawData; }
		}

		/// <summary>
		/// The file pointer to the first page of the section within the COFF file. For executable images,
		/// this must be a multiple of FileAlignment from the optional header. For object files, the value
		/// should be aligned on a 4 byte boundary for best performance. When a section contains only
		/// uninitialized data, this field should be zero.
		/// </summary>
		public uint PointerToRawData
		{
			get { return _pointerToRawData; }
		}

		/// <summary>
		/// The flags that describe the characteristics of the section.
		/// </summary>
		public SectionCharacteristics Characteristics
		{
			get { return _characteristics; }
		}

		#endregion

		#region Methods

		public bool Contains(uint rva)
		{
			if (rva >= _virtualAddress &&
				rva < _virtualAddress + _sizeOfRawData)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a pointer to data inside a section.
		/// </summary>
		public long GetPointerToData(uint rva)
		{
			return (rva - _virtualAddress);
		}

		/// <summary>
		/// Gets a pointer to data in entire image.
		/// </summary>
		public long GetPointerToRawData(uint rva)
		{
			return (long)(_pointerToRawData + (rva - _virtualAddress));
		}

		#endregion

		#region Static

		internal static unsafe PESection Read(IBinaryAccessor accessor)
		{
			var section = new PESection();

			SectionHeader header;
			fixed (byte* pBuff = accessor.ReadBytes(PEConstants.SectionHeaderSize))
			{
				header = *(SectionHeader*)pBuff;
			}

			section._name = Marshal.PtrToStringAnsi(new IntPtr(header.Name));
			section._virtualSize = header.VirtualSize;
			section._virtualAddress = header.VirtualAddress;
			section._sizeOfRawData = header.SizeOfRawData;
			section._pointerToRawData = header.PointerToRawData;
			section._characteristics = header.Characteristics;

			return section;
		}

		#endregion
	}
}
