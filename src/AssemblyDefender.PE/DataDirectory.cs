using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The data directory table starts at offset 96 in a 32-bit PE header and at offset 112 in a 64-bit PE
	/// header. Each entry in the data directory table contains the RVA and size of a table or a string
	/// that this particular directory entry describes; this information is used by the operating system.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct DataDirectory
	{
		#region Fields

		public static readonly DataDirectory Null = new DataDirectory(0, 0);

		/// <summary>
		/// Relative virtual address (RVA): In an image file, the address of an item after it is loaded into memory,
		/// with the base address of the image file subtracted from it. The RVA of an item almost always differs
		/// from its position within the file on disk (file pointer).
		/// </summary>
		public uint RVA;

		public int Size;

		#endregion

		#region Ctors

		public DataDirectory(uint rva, int size)
		{
			this.RVA = rva;
			this.Size = size;
		}

		#endregion

		#region Properties

		public bool IsNull
		{
			get { return this == Null; }
		}

		#endregion

		#region Methods

		public bool Contains(uint rva)
		{
			if (rva >= this.RVA &&
				rva < this.RVA + this.Size)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return (int)(RVA.GetHashCode() ^ Size << 1);
		}

		public override bool Equals(object obj)
		{
			if (obj is DataDirectory)
			{
				var dd = (DataDirectory)obj;
				return (RVA == dd.RVA && Size == dd.Size);
			}

			return false;
		}

		public override string ToString()
		{
			return string.Format("RVA: 0x{0}, Size: 0x{1}", RVA.ToString("X"), Size.ToString("X"));
		}

		#endregion

		#region Static

		public static bool operator ==(DataDirectory a, DataDirectory b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(DataDirectory a, DataDirectory b)
		{
			return !a.Equals(b);
		}

		public static bool IsValidIndex(int dirIndex)
		{
			return dirIndex >= 0 && dirIndex < PEConstants.NumberOfRvaAndSizes;
		}

		#endregion
	}
}
