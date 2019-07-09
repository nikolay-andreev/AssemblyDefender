using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public struct BuildBlobPoint
	{
		public int Offset;
		public BuildBlob Blob;

		public uint RVA
		{
			get { return Blob.RVA + (uint)Offset; }
		}

		public uint PointerToRawData
		{
			get { return Blob.PointerToRawData + (uint)Offset; }
		}
	}
}
