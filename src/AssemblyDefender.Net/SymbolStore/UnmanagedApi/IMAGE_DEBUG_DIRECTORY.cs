using System;
using System.Runtime.InteropServices;
using System.Text;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Image files contain an optional "debug directory" indicating what form of debug information
	/// is present and where it is. This directory consists of an array of "debug directory entries"
	/// whose location and size are indicated in the image optional header.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGE_DEBUG_DIRECTORY
	{
		public uint Characteristics;
		public uint TimeDateStamp;
		public ushort MajorVersion;
		public ushort MinorVersion;
		public int Type;
		public int SizeOfData;
		public int AddressOfRawData;
		public int PointerToRawData;
	}
}
