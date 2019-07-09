using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	internal static class CustomAttributeHelper
	{
		internal static int ReadArrayLength(IBinaryAccessor accessor)
		{
			int length = accessor.ReadInt32();
			if ((uint)length == 0xffffffff)
				length = 0;

			return length;
		}

		internal static string ReadBlobString(IBinaryAccessor accessor)
		{
			int len = 0;
			byte b = accessor.ReadByte();

			if (b == 0)
				return string.Empty;

			if (b == 0xff)
				return null;

			if ((b & 0x80) == 0)
			{
				// 1 byte
				len = b;
			}
			else if ((b & 0x40) == 0)
			{
				// 2 byte
				len = (b & ~0x80) << 8;
				len |= accessor.ReadByte();
			}
			else
			{
				// 4 byte
				len = (b & ~0xc0) << 24;
				len |= accessor.ReadByte() << 16;
				len |= accessor.ReadByte() << 8;
				len |= accessor.ReadByte();
			}

			return accessor.ReadString(len, Encoding.UTF8);
		}
	}
}
