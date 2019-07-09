using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public class BuildSection
	{
		#region Fields

		internal int Priority;
		private string _name;
		private SectionCharacteristics _characteristics;
		internal uint _virtualSize;
		internal uint _rva;
		internal uint _sizeOfRawData;
		internal uint _pointerToRawData;
		private PEBuilder _pe;
		private BuildBlobCollection _blobs;

		#endregion

		#region Ctors

		public BuildSection()
		{
			_blobs = new BuildBlobCollection(this);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the section.
		/// </summary>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// The flags that describe the characteristics of the section.
		/// </summary>
		public SectionCharacteristics Characteristics
		{
			get { return _characteristics; }
			set { _characteristics = value; }
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
		public uint RVA
		{
			get { return _rva; }
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

		public PEBuilder PE
		{
			get { return _pe; }
			internal set { _pe = value; }
		}

		public BuildBlobCollection Blobs
		{
			get { return _blobs; }
		}

		#endregion
	}
}
