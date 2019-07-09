using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public static class PEUtils
	{
		[DllImport("Imagehlp.dll", EntryPoint = "MapFileAndCheckSum", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern uint MapFileAndCheckSum(string filePath, out uint headerSum, out uint checkSum);

		public static bool GetPEChecksum(string filePath, out uint existingSum, out uint checkSum)
		{
			return HRESULT.S_OK == MapFileAndCheckSum(filePath, out existingSum, out checkSum);
		}

		public static unsafe void WritePEChecksum(string filePath, uint checkSum)
		{
			using (var accessor = new StreamAccessor(new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite)))
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
				accessor.ReadBytes(PEConstants.COFFHeaderSize);

				// PE
				ushort peMagic = accessor.ReadUInt16();
				if (peMagic == PEConstants.PEMagic32)
				{
					accessor.ReadBytes(62);
				}
				else if (peMagic == 0x20b)
				{
					accessor.ReadBytes(62);
				}
				else
				{
					throw new BadImageFormatException(SR.PEHeaderSignatureNotValid);
				}

				accessor.Write((uint)checkSum);
			}
		}

		public static DataDirectory ReadDataDirectory(this IBinaryAccessor accessor)
		{
			return new DataDirectory(accessor.ReadUInt32(), accessor.ReadInt32());
		}

		public static void Write(this IBinaryAccessor accessor, DataDirectory dd)
		{
			accessor.Write((uint)dd.RVA);
			accessor.Write((int)dd.Size);
		}

		public static void Write(this Blob blob, ref int position, DataDirectory dd)
		{
			blob.Write(ref position, (uint)dd.RVA);
			blob.Write(ref position, (int)dd.Size);
		}
	}
}
