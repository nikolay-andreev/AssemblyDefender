using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public static class CompressionUtils
	{
		public static byte[] GZipCompress(byte[] data)
		{
			return GZipCompress(data, 0, data.Length);
		}

		public static byte[] GZipCompress(byte[] data, int index, int count)
		{
			var stream = new MemoryStream(count);
			using (var zip = new GZipStream(stream, CompressionMode.Compress))
			{
				zip.Write(data, index, count);
			}

			return stream.ToArray();
		}

		public static byte[] GZipDecompress(byte[] data)
		{
			return GZipDecompress(data, 0, data.Length);
		}

		public static byte[] GZipDecompress(byte[] data, int index, int count)
		{
			var stream = new MemoryStream(count);
			using (var zip = new GZipStream(new MemoryStream(data, index, count), CompressionMode.Decompress))
			{
				byte[] buffer = new byte[4096];
				int n;
				while ((n = zip.Read(buffer, 0, 4096)) != 0)
				{
					stream.Write(buffer, 0, n);
				}
			}

			return stream.ToArray();
		}
	}
}
