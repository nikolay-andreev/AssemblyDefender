using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AssemblyDefender.Common.Cryptography
{
	public static class StrongCryptoUtils
	{
		public static readonly int DefaultKey = -934657345;

		public static int ComputeHash(Stream stream)
		{
			const int blockSize = 0x2000;

			int hash = -2128831035;
			int index = 1;
			byte[] buffer = new byte[blockSize];

			while (true)
			{
				int readSize = stream.Read(buffer, 0, blockSize);
				if (readSize == 0)
					break;

				for (int i = 0; i <= readSize; i++)
				{
					hash += buffer[i] * index++;
				}
			}

			return hash;
		}

		public static int ComputeHash(Stream stream, long count)
		{
			const int blockSize = 0x2000;

			int hash = -2128831035;
			int index = 1;
			byte[] buffer = new byte[blockSize];

			while (count > 0)
			{
				int size = blockSize;
				if (size > count)
					size = (int)count;

				int readSize = stream.Read(buffer, 0, size);
				if (readSize == 0)
					break;

				for (int i = 0; i <= readSize; i++)
				{
					hash += buffer[i] * index++;
				}

				count -= size;
			}

			return hash;
		}

		public static int ComputeHash(byte[] buffer)
		{
			return ComputeHash(buffer, 0, buffer.Length);
		}

		public static int ComputeHash(byte[] buffer, int offset, int count)
		{
			int hash = -2128831035;
			int index = 1;
			int last = offset + count - 1;
			for (int i = offset; i <= last; i++)
			{
				hash += buffer[i] * index++;
			}

			return hash;
		}

		public static int ComputeHash(string s)
		{
			return ComputeHash(s, 0, s.Length);
		}

		public static int ComputeHash(string s, int offset, int count)
		{
			int hash = -2128831035;
			int index = 1;
			int last = offset + count - 1;
			for (int i = offset; i <= last; i++)
			{
				hash += s[i] * index++;
			}

			return hash;
		}

		public static void Encrypt(byte[] buffer)
		{
			Encrypt(buffer, DefaultKey, 0, buffer.Length);
		}

		public static void Encrypt(byte[] buffer, int offset, int count)
		{
			Encrypt(buffer, DefaultKey, offset, count);
		}

		public static void Encrypt(byte[] buffer, int key)
		{
			Encrypt(buffer, key, 0, buffer.Length);
		}

		public static void Encrypt(byte[] buffer, int key, int offset, int count)
		{
			// Init salt.
			int salt = -2128831035;

			for (int i = offset, num = 0; num < count; i++, num++)
			{
				byte b = buffer[i];

				int offset4 = (num % 4);

				if (offset4 == 0)
					key ^= salt; // Salt key.

				byte b2 = (byte)(key >> (offset4 << 3));
				if (b2 == 0)
				{
					int j = 1;
					do
					{
						b2 = (byte)(key >> (((num + j++) % 4) << 3));
					}
					while (b2 == 0);
				}

				b += b2;

				salt = (salt ^ b) * 16777619;

				buffer[i] = b;
			}
		}

		public static void Decrypt(byte[] buffer)
		{
			Decrypt(buffer, DefaultKey, 0, buffer.Length);
		}

		public static void Decrypt(byte[] buffer, int offset, int count)
		{
			Decrypt(buffer, DefaultKey, offset, count);
		}

		public static void Decrypt(byte[] buffer, int key)
		{
			Decrypt(buffer, key, 0, buffer.Length);
		}

		public static void Decrypt(byte[] buffer, int key, int offset, int count)
		{
			// Init salt.
			int salt = -2128831035;

			for (int i = offset, num = 0; num < count; i++, num++)
			{
				byte b = buffer[i];

				int offset4 = (num % 4);

				if (offset4 == 0)
					key ^= salt; // Salt key.

				salt = (salt ^ b) * 16777619;

				byte b2 = (byte)(key >> (offset4 << 3));
				if (b2 == 0)
				{
					int j = 1;
					do
					{
						b2 = (byte)(key >> (((num + j++) % 4) << 3));
					}
					while (b2 == 0);
				}

				b -= b2;

				buffer[i] = b;
			}
		}
	}
}
