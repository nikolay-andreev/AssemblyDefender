using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AssemblyDefender.Common
{
	public static class BufferUtils
	{
		public static readonly byte[] EmptyArray = new byte[0];

		public static bool IsNullOrEmpty(byte[] buffer)
		{
			return buffer == null || buffer.Length == 0;
		}

		/// <summary>
		/// <para>Returns a string from a byte array represented as a hexidecimal number (eg: 0F351A).</para>
		/// </summary>
		/// <param name="bytes">
		/// <para>The byte array to convert to forat as a hexidecimal number.</para>
		/// </param>
		/// <returns>
		/// <para>The formatted representation of the bytes as a hexidcimal number.</para>
		/// </returns>
		public static string GetHexStringFromBytes(byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException("bytes");

			if (data.Length == 0)
				throw new ArgumentException("Value must be greater than 0 bytes", "bytes");

			var builder = new StringBuilder(data.Length * 2);
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i].ToString("X2", CultureInfo.InvariantCulture));
			}

			return builder.ToString();
		}

		/// <summary>
		/// <para>Combines two byte arrays into one.</para>
		/// </summary>
		/// <param name="buffer1"><para>The prefixed bytes.</para></param>
		/// <param name="buffer2"><para>The suffixed bytes.</para></param>
		/// <returns><para>The combined byte arrays.</para></returns>
		public static byte[] CombineBytes(byte[] buffer1, byte[] buffer2)
		{
			byte[] combinedBytes = new byte[buffer1.Length + buffer2.Length];
			Buffer.BlockCopy(buffer1, 0, combinedBytes, 0, buffer1.Length);
			Buffer.BlockCopy(buffer2, 0, combinedBytes, buffer1.Length, buffer2.Length);

			return combinedBytes;
		}

		/// <summary>
		/// Reverse all bytes in the given array
		/// </summary>
		/// <param name="src">The source array</param>
		/// <returns>Reversed byte array</returns>
		public static byte[] ReverseBytes(byte[] src)
		{
			byte[] result = new byte[src.Length];

			for (int i = 0; i < src.Length; i++)
			{
				result[i] = src[(src.Length - 1) - i];
			}

			return result;
		}

		/// <summary>
		/// Swap indexes of two bytes in the byte area
		/// </summary>
		/// <param name="src">Source byte area</param>
		/// <param name="index1">First index</param>
		/// <param name="index2">Second index</param>
		/// <returns></returns>
		public static byte[] SwapBytes(byte[] src, int index1, int index2)
		{
			byte[] result = new byte[src.Length];

			src.CopyTo(result, 0);

			result[index1] = src[index2];
			result[index2] = src[index1];

			return result;
		}

		/// <summary>
		/// Make XOR operation in the bytes in the area
		/// </summary>
		/// <param name="src">Source byte area</param>
		/// <param name="start">Start index</param>
		/// <param name="end">End index</param>
		/// <param name="xorVal">Value to XOR with</param>
		/// <returns>Returns transformed byte area</returns>
		public static byte[] XorBytes(byte[] src, int start, int end, byte xorVal)
		{
			byte[] result = new byte[src.Length];
			src.CopyTo(result, 0);

			for (int i = start; i <= end; i++)
			{
				result[i] ^= xorVal;
			}

			return result;
		}

		/// <summary>
		/// <para>Fills <paramref name="bytes"/> zeros.</para>
		/// </summary>
		/// <param name="bytes">
		/// <para>The byte array to fill.</para>
		/// </param>
		public static void ZeroOutBytes(byte[] bytes)
		{
			if (bytes == null)
				return;

			Array.Clear(bytes, 0, bytes.Length);
		}

		public static byte[] Append(byte[] data, byte[] appendData)
		{
			byte[] newData = new byte[data.Length + appendData.Length];
			Array.Copy(data, 0, newData, 0, data.Length);
			Array.Copy(appendData, 0, newData, data.Length, appendData.Length);

			return newData;
		}

		public static byte[] ToArray(Stream stream)
		{
			if (stream == null)
				return null;

			long offset = stream.Position;
			stream.Position = 0;

			byte[] buffer = new byte[stream.Length];
			stream.Read(buffer, 0, (int)stream.Length);

			stream.Position = offset;

			return buffer;
		}

		public static unsafe void MemCopy(byte* src, byte* dest, int len)
		{
			if (len >= 0x10)
			{
				do
				{
					*((int*)dest) = *((int*)src);
					*((int*)(dest + 4)) = *((int*)(src + 4));
					*((int*)(dest + 8)) = *((int*)(src + 8));
					*((int*)(dest + 12)) = *((int*)(src + 12));
					dest += 0x10;
					src += 0x10;
				}
				while ((len -= 0x10) >= 0x10);
			}
			if (len > 0)
			{
				if ((len & 8) != 0)
				{
					*((int*)dest) = *((int*)src);
					*((int*)(dest + 4)) = *((int*)(src + 4));
					dest += 8;
					src += 8;
				}
				if ((len & 4) != 0)
				{
					*((int*)dest) = *((int*)src);
					dest += 4;
					src += 4;
				}
				if ((len & 2) != 0)
				{
					*((short*)dest) = *((short*)src);
					dest += 2;
					src += 2;
				}
				if ((len & 1) != 0)
				{
					dest[0] = src[0];
				}
			}
		}

		public static unsafe void ZeroMemory(byte* src, int len)
		{
			while (len > 0)
			{
				src[--len] = 0;
			}
		}

		public static byte[] NullIfEmpty(this byte[] value)
		{
			return (value != null && value.Length == 0) ? null : value;
		}

		public static int Get7BitEncodedIntSize(int value)
		{
			int size = 1;

			uint num = (uint)value;
			while (num >= 0x80)
			{
				size++;
				num = num >> 7;
			}

			return size;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static unsafe void Memcpy(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
		{
			if (len == 0)
				return;

			fixed (byte* numRef = dest)
			{
				MemcpyImpl(src + srcIndex, numRef + destIndex, len);
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static unsafe void Memcpy(byte[] src, int srcIndex, byte* pDest, int destIndex, int len)
		{
			if (len == 0)
				return;

			fixed (byte* numRef = src)
			{
				MemcpyImpl(numRef + srcIndex, pDest + destIndex, len);
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static unsafe void MemcpyImpl(byte* src, byte* dest, int len)
		{
			if (len >= 0x10)
			{
				do
				{
					*((int*)dest) = *((int*)src);
					*((int*)(dest + 4)) = *((int*)(src + 4));
					*((int*)(dest + 8)) = *((int*)(src + 8));
					*((int*)(dest + 12)) = *((int*)(src + 12));
					dest += 0x10;
					src += 0x10;
				}
				while ((len -= 0x10) >= 0x10);
			}
			if (len > 0)
			{
				if ((len & 8) != 0)
				{
					*((int*)dest) = *((int*)src);
					*((int*)(dest + 4)) = *((int*)(src + 4));
					dest += 8;
					src += 8;
				}
				if ((len & 4) != 0)
				{
					*((int*)dest) = *((int*)src);
					dest += 4;
					src += 4;
				}
				if ((len & 2) != 0)
				{
					*((short*)dest) = *((short*)src);
					dest += 2;
					src += 2;
				}
				if ((len & 1) != 0)
				{
					dest[0] = src[0];
				}
			}
		}

		#region Reading

		/// <summary>
		/// Reads a Boolean value from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		public static bool ReadBoolean(byte[] buffer, ref int position)
		{
			return buffer[position++] != 0;
		}

		/// <summary>
		/// Reads a 2-byte signed integer from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>A 2-byte signed integer read from the current buffer.</returns>
		public static short ReadInt16(byte[] buffer, ref int position)
		{
			int startIndex = position;
			position += 2;

			return
				(short)
				(
					(buffer[startIndex]) |
					(buffer[startIndex + 1] << 0x8)
				);
		}

		/// <summary>
		/// Reads a 4-byte signed integer from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>A 4-byte signed integer read from the current buffer.</returns>
		public static int ReadInt32(byte[] buffer, ref int position)
		{
			int startIndex = position;
			position += 4;

			return
				(
					(buffer[startIndex]) |
					(buffer[startIndex + 1] << 0x8) |
					(buffer[startIndex + 2] << 0x10) |
					(buffer[startIndex + 3] << 0x18)
				);
		}

		/// <summary>
		/// Reads an 8-byte signed integer from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>An 8-byte signed integer read from the buffer.</returns>
		public static long ReadInt64(byte[] buffer, ref int position)
		{
			int startIndex = position;
			position += 8;

			int i1 =
				(
					(buffer[startIndex]) |
					(buffer[startIndex + 1] << 0x8) |
					(buffer[startIndex + 2] << 0x10) |
					(buffer[startIndex + 3] << 0x18)
				);

			int i2 =
				(
					(buffer[startIndex + 4] |
					(buffer[startIndex + 5] << 0x8)) |
					(buffer[startIndex + 6] << 0x10) |
					(buffer[startIndex + 7] << 0x18)
				);

			return (uint)i1 | ((long)i2 << 32);
		}

		/// <summary>
		/// Reads a 2-byte unsigned integer from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>A 2-byte unsigned integer read from the current buffer.</returns>
		public static ushort ReadUInt16(byte[] buffer, ref int position)
		{
			int startIndex = position;
			position += 2;

			return
				(ushort)
				(
					(buffer[startIndex]) |
					(buffer[startIndex + 1] << 0x8)
				);
		}

		/// <summary>
		/// Reads a 4-byte unsigned integer from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>A 4-byte unsigned integer read from the current buffer.</returns>
		public static uint ReadUInt32(byte[] buffer, ref int position)
		{
			int startIndex = position;
			position += 4;

			return
				(uint)
				(
					(buffer[startIndex]) |
					(buffer[startIndex + 1] << 0x8) |
					(buffer[startIndex + 2] << 0x10) |
					(buffer[startIndex + 3] << 0x18)
				);
		}

		/// <summary>
		/// Reads an 8-byte unsigned integer from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>An 8-byte unsigned integer read from the buffer.</returns>
		public static ulong ReadUInt64(byte[] buffer, ref int position)
		{
			int startIndex = position;
			position += 8;

			uint i1 =
				(uint)
				(
					(buffer[startIndex]) |
					(buffer[startIndex + 1] << 0x8) |
					(buffer[startIndex + 2] << 0x10) |
					(buffer[startIndex + 3] << 0x18)
				);

			uint i2 =
				(uint)
				(
					(buffer[startIndex + 4] |
					(buffer[startIndex + 5] << 0x8)) |
					(buffer[startIndex + 6] << 0x10) |
					(buffer[startIndex + 7] << 0x18)
				);

			return (uint)i1 | ((ulong)i2 << 32);
		}

		/// <summary>
		/// Reads a 4-byte floating point value from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>A 4-byte floating point value read from the current stream.</returns>
		public static unsafe float ReadSingle(byte[] buffer, ref int position)
		{
			int startIndex = position;
			position += 4;

			int val =
				(
					(buffer[startIndex]) |
					(buffer[startIndex + 1] << 0x8) |
					(buffer[startIndex + 2] << 0x10) |
					(buffer[startIndex + 3] << 0x18)
				);

			return *(float*)&val;
		}

		/// <summary>
		/// Reads an 8-byte floating point value from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>An 8-byte floating point value read from the buffer..</returns>
		public static unsafe double ReadDouble(byte[] buffer, ref int position)
		{
			int startIndex = position;
			position += 8;

			int i1 =
				(
					(buffer[startIndex]) |
					(buffer[startIndex + 1] << 0x8) |
					(buffer[startIndex + 2] << 0x10) |
					(buffer[startIndex + 3] << 0x18)
				);

			int i2 =
				(
					(buffer[startIndex + 4] |
					(buffer[startIndex + 5] << 0x8)) |
					(buffer[startIndex + 6] << 0x10) |
					(buffer[startIndex + 7] << 0x18)
				);

			long val = (uint)i1 | ((long)i2 << 32);
			return *(double*)&val;
		}

		/// <summary>
		/// Reads a decimal value from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer from which to read.</param>
		/// <param name="position">The byte position in the buffer at which to begin reading.</param>
		/// <returns>A decimal value read from the buffer.</returns>
		public static decimal ReadDecimal(byte[] buffer, ref int position)
		{
			int startIndex = position;
			position += 16;

			int lo =
				(
					(buffer[startIndex]) |
					(buffer[startIndex + 1] << 0x8) |
					(buffer[startIndex + 2] << 0x10) |
					(buffer[startIndex + 3] << 0x18)
				);

			int mid =
				(
					(buffer[startIndex + 4]) |
					(buffer[startIndex + 5] << 0x8) |
					(buffer[startIndex + 6] << 0x10) |
					(buffer[startIndex + 7] << 0x18)
				);

			int hi =
				(
					(buffer[startIndex + 8]) |
					(buffer[startIndex + 9] << 0x8) |
					(buffer[startIndex + 10] << 0x10) |
					(buffer[startIndex + 11] << 0x18)
				);

			int flags =
				(
					(buffer[startIndex + 12]) |
					(buffer[startIndex + 13] << 0x8) |
					(buffer[startIndex + 14] << 0x10) |
					(buffer[startIndex + 15] << 0x18)
				);

			return new decimal(new int[] { lo, mid, hi, flags });
		}

		public static byte[] ReadBytes(byte[] buffer, ref int pos, int count)
		{
			byte[] output = new byte[count];
			Buffer.BlockCopy(buffer, pos, output, 0, count);
			pos += count;

			return output;
		}

		/// <summary>
		/// Reads in a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 32-bit integer in compressed format.</returns>
		public static int Read7BitEncodedInt(byte[] buffer, ref int position)
		{
			int length = buffer.Length;

			byte num3;
			int num = 0;
			int num2 = 0;
			do
			{
				num3 = buffer[position++];
				num |= (num3 & 0x7f) << num2;
				num2 += 7;
			}
			while ((num3 & 0x80) != 0);

			return num;
		}

		/// <summary>
		/// Reads a string from the blob. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		public static string ReadLengthPrefixedString(byte[] buffer, ref int position)
		{
			return ReadLengthPrefixedString(buffer, ref position, Encoding.UTF8);
		}

		/// <summary>
		/// Reads a string from the blob. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public static string ReadLengthPrefixedString(byte[] buffer, ref int position, Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			int length = Read7BitEncodedInt(buffer, ref position);
			if (length < -1)
				throw new IOException(SR.BlobReadOutOfBound);

			if (length == -1)
				return null;

			if (length == 0)
				return string.Empty;

			string s = encoding.GetString(buffer, position, length);
			position += length;

			return s;
		}

		#endregion

		#region Writing

		/// <summary>
		/// Writes a one-byte Boolean value to the buffer, with 0 representing false and 1 representing true.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The Boolean value to write (0 or 1). </param>
		public static void Write(byte[] buffer, ref int position, bool value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			buffer[position++] = (byte)(value ? 1 : 0);
		}

		/// <summary>
		/// Writes a signed byte to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The signed byte to write. </param>
		public static void Write(byte[] buffer, ref int position, sbyte value)
		{
			Write(buffer, ref position, (byte)value);
		}

		/// <summary>
		/// Writes a byte to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The byte to write.</param>
		public static void Write(byte[] buffer, ref int position, byte value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			buffer[position++] = value;
		}

		/// <summary>
		/// Writes a byte multiple times to the buffer.
		/// </summary>
		public static void Write(byte[] buffer, ref int position, byte value, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			for (int i = 0; i < count; i++)
			{
				buffer[position++] = value;
			}
		}

		/// <summary>
		/// Writes a two-byte signed integer to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The two-byte signed integer to write.</param>
		public static void Write(byte[] buffer, ref int position, short value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			buffer[position++] = (byte)value;
			buffer[position++] = (byte)(value >> 0x8);
		}

		/// <summary>
		/// Writes a four-byte signed integer to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The four-byte signed integer to write.</param>
		public static void Write(byte[] buffer, ref int position, int value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			buffer[position++] = (byte)value;
			buffer[position++] = (byte)(value >> 0x8);
			buffer[position++] = (byte)(value >> 0x10);
			buffer[position++] = (byte)(value >> 0x18);
		}

		/// <summary>
		/// Writes an eight-byte signed integer to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The eight-byte signed integer to write.</param>
		public static void Write(byte[] buffer, ref int position, long value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			buffer[position++] = (byte)value;
			buffer[position++] = (byte)(value >> 0x8);
			buffer[position++] = (byte)(value >> 0x10);
			buffer[position++] = (byte)(value >> 0x18);
			buffer[position++] = (byte)(value >> 0x20);
			buffer[position++] = (byte)(value >> 0x28);
			buffer[position++] = (byte)(value >> 0x30);
			buffer[position++] = (byte)(value >> 0x38);
		}

		/// <summary>
		/// Writes a two-byte unsigned integer to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		public static void Write(byte[] buffer, ref int position, ushort value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			buffer[position++] = (byte)value;
			buffer[position++] = (byte)(value >> 0x8);
		}

		/// <summary>
		/// Writes a four-byte unsigned integer to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		public static void Write(byte[] buffer, ref int position, uint value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			buffer[position++] = (byte)value;
			buffer[position++] = (byte)(value >> 0x8);
			buffer[position++] = (byte)(value >> 0x10);
			buffer[position++] = (byte)(value >> 0x18);
		}

		/// <summary>
		/// Writes an eight-byte unsigned integer to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		public static void Write(byte[] buffer, ref int position, ulong value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			buffer[position++] = (byte)value;
			buffer[position++] = (byte)(value >> 0x8);
			buffer[position++] = (byte)(value >> 0x10);
			buffer[position++] = (byte)(value >> 0x18);
			buffer[position++] = (byte)(value >> 0x20);
			buffer[position++] = (byte)(value >> 0x28);
			buffer[position++] = (byte)(value >> 0x30);
			buffer[position++] = (byte)(value >> 0x38);
		}

		/// <summary>
		/// Writes a four-byte floating-point value to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The four-byte floating-point value to write.</param>
		public static unsafe void Write(byte[] buffer, ref int position, float value)
		{
			Write(buffer, ref position, *((int*)&value));
		}

		/// <summary>
		/// Writes an eight-byte floating-point value to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The eight-byte floating-point value to write.</param>
		public static unsafe void Write(byte[] buffer, ref int position, double value)
		{
			Write(buffer, ref position, *((long*)&value));
		}

		/// <summary>
		/// Writes a decimal value to the buffer.
		/// </summary>
		/// <param name="buffer">The buffer at which to write.</param>
		/// <param name="position">The byte position in the buffer at which to begin writing.</param>
		/// <param name="value">The decimal value to write.</param>
		public static void Write(byte[] buffer, ref int position, decimal value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			int[] bits = decimal.GetBits(value);
			int lo = bits[0];
			int mid = bits[1];
			int hi = bits[2];
			int flags = bits[3];

			buffer[position++] = (byte)lo;
			buffer[position++] = (byte)(lo >> 0x8);
			buffer[position++] = (byte)(lo >> 0x10);
			buffer[position++] = (byte)(lo >> 0x18);

			buffer[position++] = (byte)mid;
			buffer[position++] = (byte)(mid >> 0x8);
			buffer[position++] = (byte)(mid >> 0x10);
			buffer[position++] = (byte)(mid >> 0x18);

			buffer[position++] = (byte)hi;
			buffer[position++] = (byte)(hi >> 0x8);
			buffer[position++] = (byte)(hi >> 0x10);
			buffer[position++] = (byte)(hi >> 0x18);

			buffer[position++] = (byte)flags;
			buffer[position++] = (byte)(flags >> 0x8);
			buffer[position++] = (byte)(flags >> 0x10);
			buffer[position++] = (byte)(flags >> 0x18);
		}

		/// <summary>
		/// Writes a block of bytes to the blob using data read from buffer.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="source">The buffer to write data from.</param>
		public static void Write(byte[] buffer, ref int position, byte[] source)
		{
			Write(buffer, ref position, source, 0, source.Length);
		}

		/// <summary>
		/// Writes a block of bytes to the blob using data read from buffer.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="source">The buffer to write data from.</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing from.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		public static void Write(byte[] buffer, ref int position, byte[] source, int offset, int count)
		{
			Buffer.BlockCopy(source, offset, buffer, position, count);
			position += count;
		}

		/// <summary>
		/// Writes a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="position">The byte position in the writer at which to begin writing.</param>
		/// <param name="value">The 32-bit integer to be written.</param>
		public static void Write7BitEncodedInt(byte[] buffer, ref int position, int value)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			uint num = (uint)value;
			while (num >= 0x80)
			{
				buffer[position++] = (byte)(num | 0x80);
				num = num >> 7;
			}
		}

		#endregion
	}
}
