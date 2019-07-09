using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public class StreamAccessor : IBinaryAccessor
	{
		#region Fields

		private bool _disposeStream = true;
		private byte[] _buffer;
		private Stream _stream;

		#endregion

		#region Ctors

		public StreamAccessor(string filePath)
			: this(filePath, false)
		{
		}

		public StreamAccessor(string filePath, bool writable)
		{
			if (writable)
				_stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			else
				_stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			_buffer = new byte[0x10];
		}

		public StreamAccessor(Stream stream)
			: this(stream, true)
		{
		}

		public StreamAccessor(Stream stream, bool disposeStream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			_stream = stream;
			_disposeStream = disposeStream;
			_buffer = new byte[0x10];
		}

		#endregion

		#region Properties

		public long Length
		{
			get { return _stream.Length; }
		}

		public long Position
		{
			get { return _stream.Position; }
			set { _stream.Position = value; }
		}

		public bool DisposeStream
		{
			get { return _disposeStream; }
			set { _disposeStream = value; }
		}

		public bool CanRead
		{
			get { return _stream.CanRead; }
		}

		public bool CanWrite
		{
			get { return _stream.CanWrite; }
		}

		public bool CanSeek
		{
			get { return _stream.CanSeek; }
		}

		public Stream BaseStream
		{
			get { return _stream; }
		}

		#endregion

		#region Methods

		public IBinaryAccessor Map(long offset, long length)
		{
			if (offset + length > _stream.Length)
				throw new ArgumentOutOfRangeException("length");

			_stream.Position = offset;
			byte[] buffer = _stream.ReadBytes((int)length);
			return new BlobAccessor(buffer);
		}

		public void Dispose()
		{
			if (_stream != null)
			{
				if (_disposeStream)
				{
					_stream.Close();
				}

				_stream = null;
			}
		}

		private void FillBuffer(int count)
		{
			int readCount = _stream.Read(_buffer, 0, count);
			if (readCount != count)
			{
				throw new IOException(SR.StreamReadOutOfBound);
			}
		}

		#endregion

		#region Reading

		public int Read()
		{
			return _stream.ReadByte();
		}

		public int Read(byte[] buffer, int offset, int count)
		{
			return _stream.Read(buffer, offset, count);
		}

		/// <summary>
		/// Reads a Boolean value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		public bool ReadBoolean()
		{
			int b = _stream.ReadByte();
			if (b == -1)
			{
				throw new IOException(SR.StreamReadOutOfBound);
			}

			return b != 0;
		}

		/// <summary>
		/// Reads a byte from the current stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>The next byte read from the stream.</returns>
		public byte ReadByte()
		{
			int b = _stream.ReadByte();
			if (b == -1)
			{
				throw new IOException(SR.StreamReadOutOfBound);
			}

			return (byte)b;
		}

		/// <summary>
		/// Reads a signed byte from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A signed byte read from the stream.</returns>
		public sbyte ReadSByte()
		{
			int b = _stream.ReadByte();
			if (b == -1)
			{
				throw new IOException(SR.StreamReadOutOfBound);
			}

			return (sbyte)b;
		}

		/// <summary>
		/// Reads a 2-byte signed integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 2-byte signed integer read from the current stream.</returns>
		public short ReadInt16()
		{
			FillBuffer(2);

			return
				(short)
				(
					(_buffer[0]) |
					(_buffer[1] << 0x8)
				);
		}

		/// <summary>
		/// Reads a 4-byte signed integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 4-byte signed integer read from the current stream.</returns>
		public int ReadInt32()
		{
			FillBuffer(4);

			return
				(
					(_buffer[0]) |
					(_buffer[1] << 0x8) |
					(_buffer[2] << 0x10) |
					(_buffer[3] << 0x18)
				);
		}

		/// <summary>
		/// Reads an 8-byte signed integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>An 8-byte signed integer read from the stream.</returns>
		public long ReadInt64()
		{
			FillBuffer(8);

			int i1 =
				(
					(_buffer[0]) |
					(_buffer[1] << 0x8) |
					(_buffer[2] << 0x10) |
					(_buffer[3] << 0x18)
				);

			int i2 =
				(
					(_buffer[4] |
					(_buffer[5] << 0x8)) |
					(_buffer[6] << 0x10) |
					(_buffer[7] << 0x18)
				);

			return (uint)i1 | ((long)i2 << 32);
		}

		/// <summary>
		/// Reads a 2-byte unsigned integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 2-byte unsigned integer read from the current stream.</returns>
		public ushort ReadUInt16()
		{
			FillBuffer(2);

			return
				(ushort)
				(
					(_buffer[0]) |
					(_buffer[1] << 0x8)
				);
		}

		/// <summary>
		/// Reads a 4-byte unsigned integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 4-byte unsigned integer read from the current stream.</returns>
		public uint ReadUInt32()
		{
			FillBuffer(4);

			return
				(uint)
				(
					(_buffer[0]) |
					(_buffer[1] << 0x8) |
					(_buffer[2] << 0x10) |
					(_buffer[3] << 0x18)
				);
		}

		/// <summary>
		/// Reads an 8-byte unsigned integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>An 8-byte unsigned integer read from the stream.</returns>
		public ulong ReadUInt64()
		{
			FillBuffer(8);

			uint i1 =
				(uint)
				(
					(_buffer[0]) |
					(_buffer[1] << 0x8) |
					(_buffer[2] << 0x10) |
					(_buffer[3] << 0x18)
				);

			uint i2 =
				(uint)
				(
					(_buffer[4] |
					(_buffer[5] << 0x8)) |
					(_buffer[6] << 0x10) |
					(_buffer[7] << 0x18)
				);

			return (uint)i1 | ((ulong)i2 << 32);
		}

		/// <summary>
		/// Reads a 4-byte floating point value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 4-byte floating point value read from the current stream.</returns>
		public unsafe float ReadSingle()
		{
			int val = ReadInt32();
			return *(float*)&val;
		}

		/// <summary>
		/// Reads an 8-byte floating point value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>An 8-byte floating point value read from the stream..</returns>
		public unsafe double ReadDouble()
		{
			long val = ReadInt64();
			return *(double*)&val;
		}

		/// <summary>
		/// Reads a decimal value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A decimal value read from the stream.</returns>
		public unsafe decimal ReadDecimal()
		{
			FillBuffer(16);
			return ConvertUtils.ToDecimal(_buffer);
		}

		/// <summary>
		/// Reads character from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A character read from the stream.</returns>
		public char ReadChar()
		{
			return (char)ReadInt16();
		}

		/// <summary>
		/// Reads in a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 32-bit integer in compressed format.</returns>
		public int Read7BitEncodedInt()
		{
			int ret = 0;
			int shift = 0;
			int b;

			do
			{
				b = _stream.ReadByte();
				if (b == -1)
				{
					throw new IOException(SR.StreamReadOutOfBound);
				}

				ret = ret | ((b & 0x7f) << shift);
				shift += 7;
			}
			while ((b & 0x80) != 0);

			return ret;
		}

		/// <summary>
		/// Reads a string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadString(int length)
		{
			return ReadString(length, Encoding.UTF8, false);
		}

		/// <summary>
		/// Reads a string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadString(int length, Encoding encoding)
		{
			return ReadString(length, encoding, encoding is UnicodeEncoding);
		}

		/// <summary>
		/// Reads a string from the stream. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		public string ReadLengthPrefixedString()
		{
			return ReadLengthPrefixedString(Encoding.UTF8);
		}

		/// <summary>
		/// Reads a string from the stream. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadLengthPrefixedString(Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			int length = Read7BitEncodedInt();
			if (length < -1)
				throw new IOException(SR.StreamReadOutOfBound);

			if (length == -1)
				return null;

			if (length == 0)
				return string.Empty;

			byte[] buffer = this.ReadBytes(length);
			return encoding.GetString(buffer, 0, length);
		}

		/// <summary>
		/// Reads a null-terminated string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		public string ReadNullTerminatedString()
		{
			return ReadNullTerminatedString(Encoding.UTF8);
		}

		/// <summary>
		/// Reads a null-terminated string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadNullTerminatedString(Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			int buffLen = 1024;
			byte[] buffer = new byte[buffLen];

			int count = 0;

			while (true)
			{
				int b = _stream.ReadByte();
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
		private string ReadString(int length, Encoding encoding, bool is2BytesPerChar)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			if (length < 0)
				throw new ArgumentOutOfRangeException("length");

			if (length == 0)
				return string.Empty;

			if (is2BytesPerChar)
				length *= 2;

			byte[] buffer = this.ReadBytes(length);
			return encoding.GetString(buffer, 0, length);
		}

		#endregion

		#region Writing

		public void Write(byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			Write(buffer, 0, buffer.Length);
		}

		public void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}

		/// <summary>
		/// Writes a character array to the stream and advances the current position of the stream in accordance
		/// with the Encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The non-surrogate, Unicode character to write.</param>
		public void Write(char value)
		{
			Write((short)value);
		}

		/// <summary>
		/// Writes a one-byte Boolean value to the stream, with 0 representing false and 1 representing true.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The Boolean value to write (0 or 1). </param>
		public void Write(bool value)
		{
			_stream.WriteByte((byte)(value ? 1 : 0));
		}

		/// <summary>
		/// Writes a signed byte to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The signed byte to write. </param>
		public void Write(sbyte value)
		{
			_stream.WriteByte((byte)value);
		}

		/// <summary>
		/// Writes a byte to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The byte to write.</param>
		public void Write(byte value)
		{
			_stream.WriteByte(value);
		}

		/// <summary>
		/// Writes a two-byte signed integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The two-byte signed integer to write.</param>
		public void Write(short value)
		{
			_buffer[0] = (byte)value;
			_buffer[1] = (byte)(value >> 8);
			_stream.Write(_buffer, 0, 2);
		}

		/// <summary>
		/// Writes a four-byte signed integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The four-byte signed integer to write.</param>
		public void Write(int value)
		{
			_buffer[0] = (byte)value;
			_buffer[1] = (byte)(value >> 8);
			_buffer[2] = (byte)(value >> 0x10);
			_buffer[3] = (byte)(value >> 0x18);
			_stream.Write(_buffer, 0, 4);
		}

		/// <summary>
		/// Writes an eight-byte signed integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The eight-byte signed integer to write.</param>
		public void Write(long value)
		{
			_buffer[0] = (byte)value;
			_buffer[1] = (byte)(value >> 8);
			_buffer[2] = (byte)(value >> 0x10);
			_buffer[3] = (byte)(value >> 0x18);
			_buffer[4] = (byte)(value >> 0x20);
			_buffer[5] = (byte)(value >> 40);
			_buffer[6] = (byte)(value >> 0x30);
			_buffer[7] = (byte)(value >> 0x38);
			_stream.Write(_buffer, 0, 8);
		}

		/// <summary>
		/// Writes a two-byte unsigned integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		public void Write(ushort value)
		{
			_buffer[0] = (byte)value;
			_buffer[1] = (byte)(value >> 8);
			_stream.Write(_buffer, 0, 2);
		}

		/// <summary>
		/// Writes a four-byte unsigned integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		public void Write(uint value)
		{
			_buffer[0] = (byte)value;
			_buffer[1] = (byte)(value >> 8);
			_buffer[2] = (byte)(value >> 0x10);
			_buffer[3] = (byte)(value >> 0x18);
			_stream.Write(_buffer, 0, 4);
		}

		/// <summary>
		/// Writes an eight-byte unsigned integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		public void Write(ulong value)
		{
			_buffer[0] = (byte)value;
			_buffer[1] = (byte)(value >> 8);
			_buffer[2] = (byte)(value >> 0x10);
			_buffer[3] = (byte)(value >> 0x18);
			_buffer[4] = (byte)(value >> 0x20);
			_buffer[5] = (byte)(value >> 40);
			_buffer[6] = (byte)(value >> 0x30);
			_buffer[7] = (byte)(value >> 0x38);
			_stream.Write(_buffer, 0, 8);
		}

		/// <summary>
		/// Writes a four-byte floating-point value to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The four-byte floating-point value to write.</param>
		public unsafe void Write(float value)
		{
			uint num = *((uint*)&value);
			_buffer[0] = (byte)num;
			_buffer[1] = (byte)(num >> 8);
			_buffer[2] = (byte)(num >> 0x10);
			_buffer[3] = (byte)(num >> 0x18);
			_stream.Write(_buffer, 0, 4);
		}

		/// <summary>
		/// Writes an eight-byte floating-point value to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The eight-byte floating-point value to write.</param>
		public unsafe void Write(double value)
		{
			ulong num = *((ulong*)&value);
			_buffer[0] = (byte)num;
			_buffer[1] = (byte)(num >> 8);
			_buffer[2] = (byte)(num >> 0x10);
			_buffer[3] = (byte)(num >> 0x18);
			_buffer[4] = (byte)(num >> 0x20);
			_buffer[5] = (byte)(num >> 40);
			_buffer[6] = (byte)(num >> 0x30);
			_buffer[7] = (byte)(num >> 0x38);
			_stream.Write(_buffer, 0, 8);
		}

		/// <summary>
		/// Writes a decimal value to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The decimal value to write.</param>
		public void Write(decimal value)
		{
			BinaryUtils.FillBytes(value, _buffer, 0);
			_stream.Write(_buffer, 0, 16);
		}

		/// <summary>
		/// Writes a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The 32-bit integer to be written.</param>
		public void Write7BitEncodedInt(int value)
		{
			uint num = (uint)value;
			while (num >= 0x80)
			{
				_stream.WriteByte((byte)(num | 0x80));
				num = num >> 7;
			}

			_stream.WriteByte((byte)num);
		}

		/// <summary>
		/// Writes a string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		public void Write(string value)
		{
			Write(value, Encoding.UTF8);
		}

		/// <summary>
		/// Writes a string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void Write(string value, Encoding encoding)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			byte[] buffer = encoding.GetBytes(value);
			Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Writes a length-prefixed string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteLengthPrefixedString(string value)
		{
			WriteLengthPrefixedString(value, Encoding.UTF8);
		}

		/// <summary>
		/// Writes a length-prefixed string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteLengthPrefixedString(string value, Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			if (value == null)
			{
				Write7BitEncodedInt(-1);
			}
			else
			{
				byte[] buffer = encoding.GetBytes(value);
				Write7BitEncodedInt(buffer.Length);
				Write(buffer, 0, buffer.Length);
			}
		}

		/// <summary>
		/// Writes a null-terminated string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteNullTerminatedString(string value)
		{
			WriteNullTerminatedString(value, Encoding.UTF8);
		}

		/// <summary>
		/// Writes a null-terminated string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteNullTerminatedString(string value, Encoding encoding)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			byte[] buffer = encoding.GetBytes(value);
			Write(buffer, 0, buffer.Length);
			_stream.WriteByte((byte)0);
		}

		#endregion
	}
}
