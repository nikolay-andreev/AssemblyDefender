using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	public static class MetadataUtils
	{
		public static int GetCompressedIntegerByteSize(int value)
		{
			if (value < 0x80)
				return 1; // 1 byte
			else if (value < 0x4000)
				return 2; // 2 byte
			else
				return 4; // 4 byte
		}

		public static bool IsCodedTokenType(int type)
		{
			return type >= 64 && type <= 76;
		}

		public static bool IsTableType(int type)
		{
			return type >= 0 && type <= 44;
		}

		#region Blob

		public static int ReadCompressedInteger(this Blob blob, ref int position, out int length)
		{
			long startPos = position;
			int result = ReadCompressedInteger(blob, ref position);
			length = (int)(position - startPos);

			return result;
		}

		public static int ReadCompressedInteger(this Blob blob, ref int position)
		{
			int result = 0;
			byte b = blob.ReadByte(ref position);
			if ((b & 0x80) == 0)
			{
				// 1 byte
				result = b;
			}
			else if ((b & 0x40) == 0)
			{
				// 2 byte
				result = (b & ~0x80) << 8;
				result |= blob.ReadByte(ref position);
			}
			else
			{
				// 4 byte
				result = (b & ~0xc0) << 24;
				result |= blob.ReadByte(ref position) << 16;
				result |= blob.ReadByte(ref position) << 8;
				result |= blob.ReadByte(ref position);
			}

			return result;
		}

		public static void WriteCompressedInteger(this Blob blob, ref int position, int value)
		{
			if (value < 0x80)
			{
				// 1 byte
				blob.Write(ref position, (byte)value);
			}
			else if (value < 0x4000)
			{
				// 2 byte
				blob.Write(ref position,
					new byte[]
					{
						(byte)(0x80 | (value >> 8)),
						(byte)(value & 0xff),
					});
			}
			else
			{
				// 4 byte
				blob.Write(ref position,
					new byte[]
					{
						(byte)((value >> 24) | 0xc0),
						(byte)((value >> 16) & 0xff),
						(byte)((value >> 8) & 0xff),
						(byte)(value & 0xff),
					});
			}
		}

		public static int ReadCell(this Blob blob, ref int position, bool size4)
		{
			// Cell value has unsigned type.
			if (size4)
				return blob.ReadInt32(ref position);
			else
				return blob.ReadUInt16(ref position);
		}

		public static void WriteCell(this Blob blob, ref int position, bool size4, int value)
		{
			// Cell value has unsigned type.
			if (size4)
				blob.Write(ref position, (uint)value);
			else
				blob.Write(ref position, (ushort)value);
		}

		#endregion

		#region IBinaryAccessor

		public static int ReadCompressedInteger(this IBinaryAccessor accessor, out int length)
		{
			long pos = accessor.Position;
			int result = ReadCompressedInteger(accessor);
			length = (int)(accessor.Position - pos);

			return result;
		}

		public static int ReadCompressedInteger(this IBinaryAccessor accessor)
		{
			int result = 0;
			byte b = accessor.ReadByte();
			if ((b & 0x80) == 0)
			{
				// 1 byte
				result = b;
			}
			else if ((b & 0x40) == 0)
			{
				// 2 byte
				result = (b & ~0x80) << 8;
				result |= accessor.ReadByte();
			}
			else
			{
				// 4 byte
				result = (b & ~0xc0) << 24;
				result |= accessor.ReadByte() << 16;
				result |= accessor.ReadByte() << 8;
				result |= accessor.ReadByte();
			}

			return result;
		}

		public static int ReadCell(this IBinaryAccessor accessor, bool size4)
		{
			// Cell value has unsigned type.
			if (size4)
				return accessor.ReadInt32();
			else
				return accessor.ReadUInt16();
		}

		public static void WriteCompressedInteger(this IBinaryAccessor accessor, int value)
		{
			if (value < 0x80)
			{
				// 1 byte
				accessor.Write((byte)value);
			}
			else if (value < 0x4000)
			{
				// 2 byte
				accessor.Write(
					new byte[]
					{
						(byte)(0x80 | (value >> 8)),
						(byte)(value & 0xff),
					});
			}
			else
			{
				// 4 byte
				accessor.Write(
					new byte[]
					{
						(byte)((value >> 24) | 0xc0),
						(byte)((value >> 16) & 0xff),
						(byte)((value >> 8) & 0xff),
						(byte)(value & 0xff),
					});
			}
		}

		public static void WriteCell(this IBinaryAccessor accessor, bool size4, int value)
		{
			// Cell value has unsigned type.
			if (size4)
				accessor.Write((uint)value);
			else
				accessor.Write((ushort)value);
		}

		#endregion
	}
}
