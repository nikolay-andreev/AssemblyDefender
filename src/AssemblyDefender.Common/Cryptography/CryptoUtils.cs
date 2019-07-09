using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AssemblyDefender.Common.Cryptography
{
	public static class CryptoUtils
	{
		private static RNGCryptoServiceProvider _rng;

		public static RandomNumberGenerator RandomNumberGenerator
		{
			get
			{
				if (_rng == null)
				{
					_rng = new RNGCryptoServiceProvider();
				}

				return _rng;
			}
		}

		public static byte[] GetRandomBytes(int size)
		{
			byte[] data = new byte[size];
			RandomNumberGenerator.GetBytes(data);

			return data;
		}

		public static byte[] GetRandomNonZeroBytes(int size)
		{
			byte[] data = new byte[size];
			RandomNumberGenerator.GetNonZeroBytes(data);

			return data;
		}

		public static byte[] Encrypt(this SymmetricAlgorithm algorithm, byte[] input)
		{
			return Encrypt(algorithm, input, 0, input.Length);
		}

		public static byte[] Encrypt(this SymmetricAlgorithm algorithm, byte[] input, int offset, int count)
		{
			using (var transform = algorithm.CreateEncryptor())
			{
				return Transform(transform, input, offset, count);
			}
		}

		public static byte[] Decrypt(this SymmetricAlgorithm algorithm, byte[] input)
		{
			return Decrypt(algorithm, input, 0, input.Length);
		}

		public static byte[] Decrypt(this SymmetricAlgorithm algorithm, byte[] input, int offset, int count)
		{
			using (var transform = algorithm.CreateDecryptor())
			{
				return Transform(transform, input, offset, count);
			}
		}

		public static byte[] Transform(this ICryptoTransform transform, byte[] buffer)
		{
			return Transform(transform, buffer, 0, buffer.Length);
		}

		public static byte[] Transform(this ICryptoTransform transform, byte[] buffer, int offset, int count)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
				{
					cryptoStream.Write(buffer, offset, count);
					cryptoStream.FlushFinalBlock();
				}

				return memoryStream.ToArray();
			}
		}

		public static byte[] ToCapiPrivateKeyBlob(this RSA rsa)
		{
			RSAParameters p = rsa.ExportParameters(true);
			int keyLength = p.Modulus.Length; // in bytes
			byte[] blob = new byte[20 + (keyLength << 2) + (keyLength >> 1)];

			blob[0] = 0x07;	// Type - PRIVATEKEYBLOB (0x07)
			blob[1] = 0x02;	// Version - Always CUR_BLOB_VERSION (0x02)
			// [2], [3]		// RESERVED - Always 0
			blob[5] = 0x24;	// ALGID - Always 00 24 00 00 (for CALG_RSA_SIGN)
			blob[8] = 0x52;	// Magic - RSA2 (ASCII in hex)
			blob[9] = 0x53;
			blob[10] = 0x41;
			blob[11] = 0x32;

			int lenVal = keyLength << 3;
			byte[] bitlen = new byte[]
			{
				(byte) (lenVal & 0xff),
				(byte) ((lenVal >> 8) & 0xff),
				(byte) ((lenVal >> 16) & 0xff),
				(byte) ((lenVal >> 24) & 0xff)
			};
			blob[12] = bitlen[0];	// bitlen
			blob[13] = bitlen[1];
			blob[14] = bitlen[2];
			blob[15] = bitlen[3];

			// public exponent (DWORD)
			int pos = 16;
			int n = p.Exponent.Length;
			while (n > 0)
				blob[pos++] = p.Exponent[--n];
			// modulus
			pos = 20;
			byte[] part = p.Modulus;
			int len = part.Length;
			Array.Reverse(part, 0, len);
			Buffer.BlockCopy(part, 0, blob, pos, len);
			pos += len;
			// private key
			part = p.P;
			len = part.Length;
			Array.Reverse(part, 0, len);
			Buffer.BlockCopy(part, 0, blob, pos, len);
			pos += len;

			part = p.Q;
			len = part.Length;
			Array.Reverse(part, 0, len);
			Buffer.BlockCopy(part, 0, blob, pos, len);
			pos += len;

			part = p.DP;
			len = part.Length;
			Array.Reverse(part, 0, len);
			Buffer.BlockCopy(part, 0, blob, pos, len);
			pos += len;

			part = p.DQ;
			len = part.Length;
			Array.Reverse(part, 0, len);
			Buffer.BlockCopy(part, 0, blob, pos, len);
			pos += len;

			part = p.InverseQ;
			len = part.Length;
			Array.Reverse(part, 0, len);
			Buffer.BlockCopy(part, 0, blob, pos, len);
			pos += len;

			part = p.D;
			len = part.Length;
			Array.Reverse(part, 0, len);
			Buffer.BlockCopy(part, 0, blob, pos, len);

			return blob;
		}
	}
}
