using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public static class BinaryUtils
	{
		#region Utils

		public static void FillBytes(decimal value, byte[] buffer, int offset = 0)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			int[] bits = decimal.GetBits(value);
			int lo = bits[0];
			int mid = bits[1];
			int hi = bits[2];
			int flags = bits[3];

			buffer[offset] = (byte)lo;
			buffer[offset + 1] = (byte)(lo >> 0x8);
			buffer[offset + 2] = (byte)(lo >> 0x10);
			buffer[offset + 3] = (byte)(lo >> 0x18);

			buffer[offset + 4] = (byte)mid;
			buffer[offset + 5] = (byte)(mid >> 0x8);
			buffer[offset + 6] = (byte)(mid >> 0x10);
			buffer[offset + 7] = (byte)(mid >> 0x18);

			buffer[offset + 8] = (byte)hi;
			buffer[offset + 9] = (byte)(hi >> 0x8);
			buffer[offset + 10] = (byte)(hi >> 0x10);
			buffer[offset + 11] = (byte)(hi >> 0x18);

			buffer[offset + 12] = (byte)flags;
			buffer[offset + 13] = (byte)(flags >> 0x8);
			buffer[offset + 14] = (byte)(flags >> 0x10);
			buffer[offset + 15] = (byte)(flags >> 0x18);
		}

		#endregion

		#region IBinaryAccessor

		/// <summary>
		/// Compare two accessors.
		/// </summary>
		/// <param name="accessor1">The accessor1.</param>
		/// <param name="accessor2">The accessor2.</param>
		/// <returns>Return true indicates that the contents of the files are the same. Return false if the files are different.</returns>
		public static bool CompareTo(this IBinaryAccessor accessor1, IBinaryAccessor accessor2)
		{
			return CompareTo(accessor1, accessor2, 0x2000);
		}

		/// <summary>
		/// Compare two accessors.
		/// </summary>
		/// <param name="accessor1">The accessor1.</param>
		/// <param name="accessor2">The accessor2.</param>
		/// <param name="posOfFirstDiffByte">Position of the first diff byte or -1 if files are equal.</param>
		/// <returns>Return true indicates that the contents of the files are the same. Return false if the files are different.</returns>
		public static bool CompareTo(this IBinaryAccessor accessor1, IBinaryAccessor accessor2, int bufferSize)
		{
			long posOfFirstDiffByte;
			return CompareTo(accessor1, accessor2, bufferSize, out posOfFirstDiffByte);
		}

		/// <summary>
		/// Compare two accessors.
		/// </summary>
		/// <param name="accessor1">The accessor1.</param>
		/// <param name="accessor2">The accessor2.</param>
		/// <param name="bufferSize">The size of buffer.</param>
		/// <param name="posOfFirstDiffByte">Position of the first diff byte or -1 if files are equal.</param>
		/// <returns>Return true indicates that the contents of the files are the same. Return false if the files are different.</returns>
		public static bool CompareTo(this IBinaryAccessor accessor1, IBinaryAccessor accessor2, int bufferSize, out long posOfFirstDiffByte)
		{
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException("bufferSize");

			posOfFirstDiffByte = -1;

			// If some of the files does not exists then assume they are different.
			if (accessor1 == null || accessor2 == null)
				return false;

			int position = 0;
			byte[] buffer1 = new byte[bufferSize];
			byte[] buffer2 = new byte[bufferSize];

			while (true)
			{
				int readCount1 = accessor1.Read(buffer1, 0, bufferSize);
				int readCount2 = accessor2.Read(buffer2, 0, bufferSize);

				int minReadCount = Math.Min(readCount1, readCount2);
				for (int i = 0; i < minReadCount; i++)
				{
					if (buffer1[i] != buffer2[i])
					{
						posOfFirstDiffByte = position;
						return false;
					}

					position++;
				}

				if (readCount1 != readCount2)
				{
					posOfFirstDiffByte = position;
					return false;
				}

				if (readCount1 < bufferSize)
					break;
			}

			return true;
		}

		public static bool IsEof(this IBinaryAccessor accessor)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			return accessor.Position >= accessor.Length;
		}

		public static long GetRemainingSize(this IBinaryAccessor accessor)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			return accessor.Length - accessor.Position;
		}

		public static void Align(this IBinaryAccessor accessor, int align)
		{
			Align(accessor, 0, align);
		}

		public static void Align(this IBinaryAccessor accessor, long startPos, int align)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			long size = accessor.Position - startPos;
			int diff = (int)(((size + align - 1) & ~(align - 1)) - size);
			accessor.Position = startPos + size + diff;
		}

		public static IBinaryAccessor Map(this IBinaryAccessor accessor)
		{
			return accessor.Map(0, accessor.Length);
		}

		/// <summary>
		/// Reads a Boolean value.
		/// </summary>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		public static bool? ReadNullableBoolean(this IBinaryAccessor accessor)
		{
			byte b = accessor.ReadByte();
			if (b == 0)
				return false;
			else if (b == 1)
				return true;
			else
				return null;
		}

		public static byte[] ReadBytes(this IBinaryAccessor accessor, int count)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			byte[] buffer = new byte[count];
			int readCount = accessor.Read(buffer, 0, count);
			if (readCount != count)
			{
				throw new IOException(SR.StreamReadOutOfBound);
			}

			return buffer;
		}

		public static byte[] ReadAllBytes(this IBinaryAccessor accessor)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			return accessor.ReadBytes((int)(accessor.Length - accessor.Position));
		}

		public static Guid ReadGuid(this IBinaryAccessor accessor)
		{
			return new Guid(accessor.ReadBytes(16));
		}

		public static DateTime ReadDateTime(this IBinaryAccessor accessor)
		{
			return ConvertUtils.ToDateTime(accessor.ReadInt32());
		}

		public static void Write(this IBinaryAccessor accessor, bool? value)
		{
			accessor.Write((byte)(value.HasValue ? (value.Value ? (byte)1 : (byte)0) : (byte)2));
		}

		/// <summary>
		/// Writes a byte array to the accessor and advances the current position of the accessor.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies all bytes from buffer to the current accessor.</param>
		public static void Write(this IBinaryAccessor accessor, byte[] buffer)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			if (buffer == null)
				throw new ArgumentNullException("buffer");

			accessor.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Writes data from accessor.
		/// </summary>
		public static void Write(this IBinaryAccessor accessor, IBinaryAccessor source, long count, int bufferSize = 0x1000, bool throwIfOutOfBound = false)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			if (source == null)
				throw new ArgumentNullException("sourceBinaryAccessor");

			byte[] buffer = new byte[bufferSize];

			while (count > 0)
			{
				int copySize = (int)(count > bufferSize ? bufferSize : count);
				int readCount = source.Read(buffer, 0, copySize);

				if (copySize != readCount)
				{
					if (throwIfOutOfBound)
					{
						throw new IOException(SR.BlobReadOutOfBound);
					}
				}

				accessor.Write(buffer, 0, readCount);

				count -= copySize;
			}
		}

		/// <summary>
		/// Writes a byte multiple times to the stream.
		/// </summary>
		public static void Write(this IBinaryAccessor accessor, byte value, int count)
		{
			if (count <= 0x100)
			{
				byte[] buffer = new byte[count];
				for (int i = 0; i < count; i++)
				{
					buffer[i] = value;
				}

				accessor.Write(buffer, 0, count);
			}
			else
			{
				byte[] buffer = new byte[0x100];
				for (int i = 0; i < 0x100; i++)
				{
					buffer[i] = value;
				}

				while (count > 0)
				{
					int copySize = (count > 0x100 ? 0x100 : count);
					accessor.Write(buffer, 0, copySize);
					count -= 0x100;
				}
			}
		}

		public static void Write(this IBinaryAccessor accessor, Guid value)
		{
			accessor.Write(value.ToByteArray());
		}

		public static void Write(this IBinaryAccessor accessor, DateTime value)
		{
			accessor.Write(ConvertUtils.To_time_t(value));
		}

		#endregion

		#region Stream

		/// <summary>
		/// Compare two streams.
		/// </summary>
		/// <param name="stream1">The stream1.</param>
		/// <param name="stream2">The stream2.</param>
		/// <returns>Return true indicates that the contents of the files are the same. Return false if the files are different.</returns>
		public static bool CompareTo(this Stream stream1, Stream stream2)
		{
			return CompareTo(stream1, stream2, 0x2000);
		}

		/// <summary>
		/// Compare two streams.
		/// </summary>
		/// <param name="stream1">The stream1.</param>
		/// <param name="stream2">The stream2.</param>
		/// <param name="posOfFirstDiffByte">Position of the first diff byte or -1 if files are equal.</param>
		/// <returns>Return true indicates that the contents of the files are the same. Return false if the files are different.</returns>
		public static bool CompareTo(this Stream stream1, Stream stream2, int bufferSize)
		{
			long posOfFirstDiffByte;
			return CompareTo(stream1, stream2, bufferSize, out posOfFirstDiffByte);
		}

		/// <summary>
		/// Compare two streams.
		/// </summary>
		/// <param name="stream1">The stream1.</param>
		/// <param name="stream2">The stream2.</param>
		/// <param name="bufferSize">The size of buffer.</param>
		/// <param name="posOfFirstDiffByte">Position of the first diff byte or -1 if files are equal.</param>
		/// <returns>Return true indicates that the contents of the files are the same. Return false if the files are different.</returns>
		public static bool CompareTo(this Stream stream1, Stream stream2, int bufferSize, out long posOfFirstDiffByte)
		{
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException("bufferSize");

			posOfFirstDiffByte = -1;

			// If some of the files does not exists then assume they are different.
			if (stream1 == null || stream2 == null)
				return false;

			int position = 0;
			byte[] buffer1 = new byte[bufferSize];
			byte[] buffer2 = new byte[bufferSize];

			while (true)
			{
				int readCount1 = stream1.Read(buffer1, 0, bufferSize);
				int readCount2 = stream2.Read(buffer2, 0, bufferSize);

				int minReadCount = Math.Min(readCount1, readCount2);
				for (int i = 0; i < minReadCount; i++)
				{
					if (buffer1[i] != buffer2[i])
					{
						posOfFirstDiffByte = position;
						return false;
					}

					position++;
				}

				if (readCount1 != readCount2)
				{
					posOfFirstDiffByte = position;
					return false;
				}

				if (readCount1 < bufferSize)
					break;
			}

			return true;
		}

		public static bool IsEof(this Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			return stream.Position >= stream.Length;
		}

		public static long GetRemainingSize(this Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			return stream.Length - stream.Position;
		}

		public static void Align(this Stream stream, int align)
		{
			Align(stream, 0, align);
		}

		public static void Align(this Stream stream, long startPos, int align)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			long size = stream.Position - startPos;
			int diff = (int)(((size + align - 1) & ~(align - 1)) - size);
			stream.Position = startPos + size + diff;
		}

		public static Stream AsNonClosable(this Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			var nonClosingStream = stream as NonClosingStream;
			if (nonClosingStream == null)
			{
				nonClosingStream = new NonClosingStream(stream);
			}

			return nonClosingStream;
		}

		public static Stream UnwrapNonClosable(this Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			var nonClosingStream = stream as NonClosingStream;
			if (nonClosingStream == null)
				return stream;

			return nonClosingStream.InnerStream;
		}

		public static byte[] ReadBytes(this Stream stream, int count)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			byte[] buffer = new byte[count];
			int readCount = stream.Read(buffer, 0, count);
			if (readCount != count)
			{
				throw new IOException(SR.StreamReadOutOfBound);
			}

			return buffer;
		}

		public static byte[] ReadAllBytes(this Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			try
			{
				byte[] buffer = new byte[stream.Length - stream.Position];
				stream.Read(buffer, 0, buffer.Length);

				return buffer;
			}
			catch (NotSupportedException)
			{
				return ReadAllBytes(stream, 0x400);
			}
		}

		public static byte[] ReadAllBytes(this Stream stream, int blockSize)
		{
			var buffer = new byte[blockSize];
			int bufferLength = blockSize;
			int pos = 0;
			int readCount;
			do
			{
				if (bufferLength < pos + blockSize)
				{
					var newBuffer = new byte[bufferLength * 2];
					Buffer.BlockCopy(buffer, 0, newBuffer, 0, bufferLength);
					buffer = newBuffer;
					bufferLength = newBuffer.Length;
				}

				readCount = stream.Read(buffer, pos, blockSize);
				pos += readCount;
			}
			while (readCount == blockSize);

			if (pos < bufferLength)
			{
				var newBuffer = new byte[pos];
				Buffer.BlockCopy(buffer, 0, newBuffer, 0, pos);
				buffer = newBuffer;
			}

			return buffer;
		}

		/// <summary>
		/// Writes a byte array to the stream and advances the current position of the stream.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies all bytes from buffer to the current stream.</param>
		public static void Write(this Stream stream, byte[] buffer)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (buffer == null)
				throw new ArgumentNullException("buffer");

			stream.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Writes data from stream.
		/// </summary>
		public static void Write(this Stream stream, Stream source, long count, int bufferSize = 0x1000, bool throwIfOutOfBound = false)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (source == null)
				throw new ArgumentNullException("sourceStream");

			byte[] buffer = new byte[bufferSize];

			while (count > 0)
			{
				int copySize = (int)(count > bufferSize ? bufferSize : count);
				int readCount = source.Read(buffer, 0, copySize);

				if (copySize != readCount)
				{
					if (throwIfOutOfBound)
					{
						throw new IOException(SR.StreamReadOutOfBound);
					}
				}

				stream.Write(buffer, 0, readCount);

				count -= copySize;
			}
		}

		#endregion

		#region BinaryReader

		/// <summary>
		/// Reads a Boolean value.
		/// </summary>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		public static bool? ReadNullableBoolean(this BinaryReader reader)
		{
			byte b = reader.ReadByte();
			if (b == 0)
				return false;
			else if (b == 1)
				return true;
			else
				return null;
		}

		/// <summary>
		/// Reads in a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 32-bit integer in compressed format.</returns>
		public static int Read7BitEncodedInt(this BinaryReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			int ret = 0;
			int shift = 0;
			int b;

			do
			{
				b = reader.ReadByte();
				ret = ret | ((b & 0x7f) << shift);
				shift += 7;
			}
			while ((b & 0x80) != 0);

			return ret;
		}

		/// <summary>
		/// Reads a string from the reader.
		/// </summary>
		/// <param name="position">The byte position in the reader at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <returns>The string being read.</returns>
		public static string ReadString(this BinaryReader reader, int length)
		{
			return ReadString(reader, length, Encoding.UTF8, false);
		}

		/// <summary>
		/// Reads a string from the reader.
		/// </summary>
		/// <param name="position">The byte position in the reader at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public static string ReadString(this BinaryReader reader, int length, Encoding encoding)
		{
			return ReadString(reader, length, encoding, encoding is UnicodeEncoding);
		}

		/// <summary>
		/// Reads a string from the reader. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the reader at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		public static string ReadLengthPrefixedString(this BinaryReader reader)
		{
			return ReadLengthPrefixedString(reader, Encoding.UTF8);
		}

		/// <summary>
		/// Reads a string from the reader. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the reader at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public static string ReadLengthPrefixedString(this BinaryReader reader, Encoding encoding)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			int length = reader.Read7BitEncodedInt();
			if (length < -1)
				throw new IOException(SR.StreamReadOutOfBound);

			if (length == -1)
				return null;

			if (length == 0)
				return string.Empty;

			byte[] buffer = reader.ReadBytes(length);
			return encoding.GetString(buffer, 0, length);
		}

		/// <summary>
		/// Reads a null-terminated string from the reader.
		/// </summary>
		/// <param name="position">The byte position in the reader at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		public static string ReadNullTerminatedString(this BinaryReader reader)
		{
			return ReadNullTerminatedString(reader, Encoding.UTF8);
		}

		/// <summary>
		/// Reads a null-terminated string from the reader.
		/// </summary>
		/// <param name="position">The byte position in the reader at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public static string ReadNullTerminatedString(this BinaryReader reader, Encoding encoding)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			int buffLen = 1024;
			byte[] buffer = new byte[buffLen];

			int count = 0;

			while (true)
			{
				int b = reader.ReadByte();
				if (b == 0 || b == -1)
					break;

				if (count == buffLen)
				{
					int newBuffLen = buffLen + 1024;
					byte[] newBuffer = new byte[newBuffLen];
					Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffLen);
					buffer = newBuffer;
					buffLen = newBuffLen;
				}

				buffer[count++] = (byte)b;
			}

			if (count == 0)
				return string.Empty;

			return encoding.GetString(buffer, 0, count);
		}

		/// <summary>
		/// Reads a string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="length">The length of the string to read.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <param name="is2BytesPerChar">True is one char is represented by 2 bytes; otherwise false.</param>
		/// <returns>The string being read.</returns>
		private static string ReadString(this BinaryReader reader, int length, Encoding encoding, bool is2BytesPerChar)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			if (length < 0)
				throw new ArgumentOutOfRangeException("length");

			if (length == 0)
				return string.Empty;

			if (is2BytesPerChar)
				length *= 2;

			byte[] buffer = reader.ReadBytes(length);
			return encoding.GetString(buffer, 0, length);
		}

		#endregion

		#region BinaryWriter

		/// <summary>
		/// Writes a one-byte Boolean value.
		/// </summary>
		public static void Write(this BinaryWriter writer, bool? value)
		{
			writer.Write((byte)(value.HasValue ? (value.Value ? (byte)1 : (byte)0) : (byte)2));
		}

		/// <summary>
		/// Writes a byte multiple times to the writer.
		/// </summary>
		public static void Write(this BinaryWriter writer, byte value, long count)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			if (count <= 0x100)
			{
				byte[] buffer = new byte[count];
				for (int i = 0; i < count; i++)
				{
					buffer[i] = value;
				}

				writer.Write(buffer, 0, (int)count);
			}
			else
			{
				byte[] buffer = new byte[0x100];
				for (int i = 0; i < 0x100; i++)
				{
					buffer[i] = value;
				}

				while (count > 0)
				{
					int copySize = (int)(count > 0x100 ? 0x100 : count);
					writer.Write(buffer, 0, copySize);
					count -= 0x100;
				}
			}
		}

		/// <summary>
		/// Writes a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="position">The byte position in the writer at which to begin writing.</param>
		/// <param name="value">The 32-bit integer to be written.</param>
		public static void Write7BitEncodedInt(this BinaryWriter writer, int value)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			uint num = (uint)value;
			while (num >= 0x80)
			{
				writer.Write((byte)(num | 0x80));
				num = num >> 7;
			}

			writer.Write((byte)num);
		}

		/// <summary>
		/// Writes a string to the writer.
		/// </summary>
		/// <param name="position">The byte position in the writer at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public static void Write(this BinaryWriter writer, string value)
		{
			Write(writer, value, Encoding.UTF8);
		}

		/// <summary>
		/// Writes a string to the writer.
		/// </summary>
		/// <param name="position">The byte position in the writer at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public static unsafe void Write(this BinaryWriter writer, string value, Encoding encoding)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			if (value == null)
				throw new ArgumentNullException("value");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			byte[] buffer = encoding.GetBytes(value);
			writer.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Writes a length-prefixed string to the writer.
		/// </summary>
		/// <param name="position">The byte position in the writer at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public static void WriteLengthPrefixedString(this BinaryWriter writer, string value)
		{
			WriteLengthPrefixedString(writer, value, Encoding.UTF8);
		}

		/// <summary>
		/// Writes a length-prefixed string to the writer.
		/// </summary>
		/// <param name="position">The byte position in the writer at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public static void WriteLengthPrefixedString(this BinaryWriter writer, string value, Encoding encoding)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			if (value == null)
			{
				Write7BitEncodedInt(writer, -1);
			}
			else
			{
				byte[] buffer = encoding.GetBytes(value);
				Write7BitEncodedInt(writer, buffer.Length);
				writer.Write(buffer, 0, buffer.Length);
			}
		}

		/// <summary>
		/// Writes a null-terminated string to the writer.
		/// </summary>
		/// <param name="position">The byte position in the writer at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public static void WriteNullTerminatedString(this BinaryWriter writer, string value)
		{
			WriteNullTerminatedString(writer, value, Encoding.UTF8);
		}

		/// <summary>
		/// Writes a null-terminated string to the writer.
		/// </summary>
		/// <param name="position">The byte position in the writer at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public static void WriteNullTerminatedString(this BinaryWriter writer, string value, Encoding encoding)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			if (value == null)
				throw new ArgumentNullException("value");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			Write(writer, value, encoding);
			writer.Write((byte)0);
		}

		#endregion
	}
}
