using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	/// <summary>
	/// Represents in-memory buffer much like MemoryStream except without position.
	/// </summary>
	public class Blob
	{
		#region Fields

		protected int _origin;
		protected int _length;
		protected int _capacity;
		protected bool _writable;
		protected bool _expandable;
		protected byte[] _buffer;

		#endregion

		#region Ctors

		public Blob()
			: this(0)
		{
		}

		public Blob(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity");

			if (capacity == 0)
				capacity = 0x100;

			_buffer = new byte[capacity];
			_capacity = capacity;
			_writable = true;
			_expandable = true;
		}

		public Blob(byte[] buffer)
			: this(buffer, true)
		{
		}

		public Blob(byte[] buffer, bool writable)
		{
			if (buffer != null)
			{
				_buffer = buffer;
				_length = _capacity = buffer.Length;
			}
			else
			{
				_capacity = 0x100;
				_buffer = new byte[_capacity];
			}

			_writable = writable;
			_expandable = true;
		}

		public Blob(byte[] buffer, int offset, int count)
			: this(buffer, offset, count, true)
		{
		}

		public Blob(byte[] buffer, int offset, int count, bool writable)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			int buffLen = buffer.Length;
			if (buffLen - offset < count)
				throw new ArgumentOutOfRangeException("offset");

			_buffer = buffer;
			_origin = offset;
			_length = _capacity = offset + count;
			_writable = writable;
			_expandable = false;
		}

		protected Blob(Blob blob)
		{
			_origin = blob._origin;
			_length = blob._length;
			_capacity = blob._capacity;
			_writable = blob._writable;
			_expandable = blob._expandable;
			_buffer = blob._buffer;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the length of the blob in bytes.
		/// </summary>
		public int Length
		{
			get { return _length - _origin; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("value");

				int length = value + _origin;

				if (length < _length)
					throw new ArgumentOutOfRangeException("value");

				if (!_writable)
					throw new NotSupportedException(SR.BlobWriteNotSupported);

				if (length > _capacity)
				{
					EnsureCapacity(length);
				}

				_length = length;
			}
		}

		/// <summary>
		/// Gets or sets the number of bytes allocated for this blob.
		/// </summary>
		public int Capacity
		{
			get { return _capacity - _origin; }
			set
			{
				if (value != _capacity)
				{
					if (!_expandable)
						throw new IOException(SR.BlobNotExpandable);

					if (value < _length)
						throw new ArgumentOutOfRangeException("value");

					if (value > 0)
					{
						byte[] dst = new byte[value];
						if (_length > 0)
						{
							Buffer.BlockCopy(_buffer, 0, dst, 0, _length);
						}

						_buffer = dst;
					}
					else
					{
						_buffer = BufferUtils.EmptyArray;
					}

					_capacity = value;
				}
			}
		}

		public int Origin
		{
			get { return _origin; }
		}

		/// <summary>
		/// Gets a value indicating whether the current blob supports writing.
		/// </summary>
		public bool Writable
		{
			get { return _writable; }
		}

		/// <summary>
		/// Gets a value indicating whether the current blob can be expanded.
		/// </summary>
		public bool Expandable
		{
			get { return _expandable; }
		}

		#endregion

		#region Methods

		public void SaveToFile(string filePath)
		{
			using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
			{
				stream.Write(_buffer, 0, Length);
			}
		}

		public int IndexOf(int index, byte value)
		{
			int startIndex = _origin + index;
			for (int i = startIndex; i < _length; i++)
			{
				if (_buffer[i] == value)
					return i - _origin;
			}

			return -1;
		}

		/// <summary>
		/// Gets internal buffer which can be bigger than the data stored.
		/// </summary>
		public byte[] GetBuffer()
		{
			return _buffer;
		}

		/// <summary>
		/// Gets a copy of buffer data.
		/// </summary>
		public byte[] ToArray()
		{
			int count = _length - _origin;
			byte[] buffer = new byte[count];
			Buffer.BlockCopy(_buffer, 0, buffer, 0, count);
			return buffer;
		}

		public void Align(int align, byte alignByte)
		{
			int length = _length - _origin;
			int alignCount = length.Align(align) - length;
			if (alignCount > 0)
			{
				int pos = length;
				Write(ref pos, alignByte, alignCount);
			}
		}

		/// <summary>
		/// Allocate new data within exising data.
		/// </summary>
		/// <param name="offset">The byte position in the blob.</param>
		/// <param name="count">The size in bytes of new allocated data.</param>
		public void Allocate(int offset, int count)
		{
			if (!_writable)
				throw new NotSupportedException(SR.BlobWriteNotSupported);

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			int startIndex = offset + _origin;

			EnsureSafeToWrite(startIndex, count);
		}

		/// <summary>
		/// Shrink exising data.
		/// </summary>
		/// <param name="offset">The byte position in the blob.</param>
		/// <param name="count">The size in bytes.</param>
		public void Shrink(int offset, int count)
		{
			if (!_writable)
				throw new NotSupportedException(SR.BlobWriteNotSupported);

			if (!_expandable)
				throw new IOException(SR.BlobNotExpandable);

			if (count < 0)
				throw new ArgumentOutOfRangeException("length");

			if (offset >= _length)
				throw new ArgumentOutOfRangeException("offset");

			int sizeToCopy = _length - offset - count;
			if (sizeToCopy < 0)
				throw new ArgumentOutOfRangeException("count");

			if (sizeToCopy > 0)
			{
				Buffer.BlockCopy(_buffer, offset + count, _buffer, offset, sizeToCopy);
			}

			Array.Clear(_buffer, _length - count, count);
			_length -= count;
		}

		private void EnsureCapacity(int value)
		{
			if (value <= _capacity)
				return;

			if (value < 0x100)
				value = 0x100;

			if (value < (_capacity * 2))
				value = _capacity * 2;

			Capacity = value;
		}

		#endregion

		#region Reading

		/// <summary>
		/// Reads a byte from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>The byte cast to a Int32, or -1 if the end of the stream has been reached.</returns>
		public int Read(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex >= _length)
				return -1;

			position++;
			return _buffer[startIndex];
		}

		/// <summary>
		/// Reads a block of bytes from the blob and writes the data to buffer.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <param name="buffer">When this method returns, contains the specified byte array with the values
		/// between offset and (offset + count - 1) replaced by the characters read from the current stream.</param>
		/// <param name="offset">The byte offset in buffer at which to begin reading.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <returns>The total number of bytes written into the buffer. This can be less than the number of
		/// bytes requested if that number of bytes are not currently available, or zero if the end of the
		/// blob is reached before any bytes are read.</returns>
		public int Read(ref int position, byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			if ((buffer.Length - offset) < count)
				throw new ArgumentOutOfRangeException("count");

			int startIndex = position + _origin;

			int numOfBytesToRead = _length - startIndex;
			if (numOfBytesToRead > count)
				numOfBytesToRead = count;

			if (numOfBytesToRead <= 0)
				return 0;

			if (numOfBytesToRead <= 8)
			{
				int num2 = numOfBytesToRead;
				while (--num2 >= 0)
				{
					buffer[offset + num2] = _buffer[startIndex + num2];
				}
			}
			else
			{
				Buffer.BlockCopy(_buffer, startIndex, buffer, offset, numOfBytesToRead);
			}

			position += numOfBytesToRead;

			return numOfBytesToRead;
		}

		/// <summary>
		/// Reads a signed byte from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>A signed byte read from the blob.</returns>
		public sbyte ReadSByte(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 1 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position++;
			return (sbyte)_buffer[startIndex];
		}

		/// <summary>
		/// Reads a byte from the current blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>The next byte read from the blob.</returns>
		public byte ReadByte(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 1 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position++;
			return _buffer[startIndex];
		}

		/// <summary>
		/// Reads a byte array from the current blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>The byte array read from the blob.</returns>
		public byte[] ReadBytes(ref int position, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			byte[] buffer = new byte[count];
			int readCount = Read(ref position, buffer, 0, count);
			if (readCount != count)
				throw new IOException(SR.BlobReadOutOfBound);

			return buffer;
		}

		/// <summary>
		/// Reads a Boolean value from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		public bool ReadBoolean(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 1 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position++;

			return (_buffer[startIndex] != 0);
		}

		/// <summary>
		/// Reads a Boolean value from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		public bool? ReadNullableBoolean(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 1 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position++;

			byte b = _buffer[startIndex];

			if (b == 0)
				return false;
			else if (b == 1)
				return true;
			else
				return null;
		}

		/// <summary>
		/// Reads character from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>A character read from the blob.</returns>
		public char ReadChar(ref int position)
		{
			return (char)ReadInt16(ref position);
		}

		/// <summary>
		/// Reads a 2-byte signed integer from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>A 2-byte signed integer read from the current blob.</returns>
		public short ReadInt16(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 2 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += 2;

			return
				(short)
				(
					(_buffer[startIndex]) |
					(_buffer[startIndex + 1] << 0x8)
				);
		}

		/// <summary>
		/// Reads a 4-byte signed integer from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>A 4-byte signed integer read from the current blob.</returns>
		public int ReadInt32(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 4 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += 4;

			return
				(
					(_buffer[startIndex]) |
					(_buffer[startIndex + 1] << 0x8) |
					(_buffer[startIndex + 2] << 0x10) |
					(_buffer[startIndex + 3] << 0x18)
				);
		}

		/// <summary>
		/// Reads an 8-byte signed integer from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>An 8-byte signed integer read from the blob.</returns>
		public long ReadInt64(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 8 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += 8;

			int i1 =
				(
					(_buffer[startIndex]) |
					(_buffer[startIndex + 1] << 0x8) |
					(_buffer[startIndex + 2] << 0x10) |
					(_buffer[startIndex + 3] << 0x18)
				);

			int i2 =
				(
					(_buffer[startIndex + 4] |
					(_buffer[startIndex + 5] << 0x8)) |
					(_buffer[startIndex + 6] << 0x10) |
					(_buffer[startIndex + 7] << 0x18)
				);

			return (uint)i1 | ((long)i2 << 32);
		}

		/// <summary>
		/// Reads a 2-byte unsigned integer from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>A 2-byte unsigned integer read from the current blob.</returns>
		public ushort ReadUInt16(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 2 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += 2;

			return
				(ushort)
				(
					(_buffer[startIndex]) |
					(_buffer[startIndex + 1] << 0x8)
				);
		}

		/// <summary>
		/// Reads a 4-byte unsigned integer from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>A 4-byte unsigned integer read from the current blob.</returns>
		public uint ReadUInt32(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 4 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += 4;

			return
				(uint)
				(
					(_buffer[startIndex]) |
					(_buffer[startIndex + 1] << 0x8) |
					(_buffer[startIndex + 2] << 0x10) |
					(_buffer[startIndex + 3] << 0x18)
				);
		}

		/// <summary>
		/// Reads an 8-byte unsigned integer from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>An 8-byte unsigned integer read from the blob.</returns>
		public ulong ReadUInt64(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 8 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += 8;

			uint i1 =
				(uint)
				(
					(_buffer[startIndex]) |
					(_buffer[startIndex + 1] << 0x8) |
					(_buffer[startIndex + 2] << 0x10) |
					(_buffer[startIndex + 3] << 0x18)
				);

			uint i2 =
				(uint)
				(
					(_buffer[startIndex + 4] |
					(_buffer[startIndex + 5] << 0x8)) |
					(_buffer[startIndex + 6] << 0x10) |
					(_buffer[startIndex + 7] << 0x18)
				);

			return (uint)i1 | ((ulong)i2 << 32);
		}

		/// <summary>
		/// Reads a 4-byte floating point value from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>A 4-byte floating point value read from the current stream.</returns>
		public unsafe float ReadSingle(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 4 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += 4;

			int val =
				(
					(_buffer[startIndex]) |
					(_buffer[startIndex + 1] << 0x8) |
					(_buffer[startIndex + 2] << 0x10) |
					(_buffer[startIndex + 3] << 0x18)
				);

			return *(float*)&val;
		}

		/// <summary>
		/// Reads an 8-byte floating point value from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>An 8-byte floating point value read from the blob..</returns>
		public unsafe double ReadDouble(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 8 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += 8;

			int i1 =
				(
					(_buffer[startIndex]) |
					(_buffer[startIndex + 1] << 0x8) |
					(_buffer[startIndex + 2] << 0x10) |
					(_buffer[startIndex + 3] << 0x18)
				);

			int i2 =
				(
					(_buffer[startIndex + 4] |
					(_buffer[startIndex + 5] << 0x8)) |
					(_buffer[startIndex + 6] << 0x10) |
					(_buffer[startIndex + 7] << 0x18)
				);

			long val = (uint)i1 | ((long)i2 << 32);

			return *(double*)&val;
		}

		/// <summary>
		/// Reads a decimal value from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>A decimal value read from the blob.</returns>
		public decimal ReadDecimal(ref int position)
		{
			int startIndex = position + _origin;
			if (startIndex + 16 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += 16;

			return ConvertUtils.ToDecimal(_buffer, startIndex);
		}

		/// <summary>
		/// Reads a string from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadString(ref int position, int length)
		{
			return ReadString(ref position, length, Encoding.UTF8, false);
		}

		/// <summary>
		/// Reads a string from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadString(ref int position, int length, Encoding encoding)
		{
			return ReadString(ref position, length, encoding, encoding is UnicodeEncoding);
		}

		private string ReadString(ref int position, int length, Encoding encoding, bool is2BytesPerChar)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			if (length < 0)
				throw new ArgumentOutOfRangeException("length");

			if (length == 0)
				return string.Empty;

			if (is2BytesPerChar)
				length *= 2;

			int startIndex = position + _origin;
			if (startIndex + length > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += length;
			return encoding.GetString(_buffer, startIndex, length);
		}

		/// <summary>
		/// Reads a string from the blob. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		public string ReadLengthPrefixedString(ref int position)
		{
			return ReadLengthPrefixedString(ref position, Encoding.UTF8);
		}

		/// <summary>
		/// Reads a string from the blob. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadLengthPrefixedString(ref int position, Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			int length = Read7BitEncodedInt(ref position);
			if (length < -1)
				throw new IOException(SR.BlobReadOutOfBound);

			if (length == -1)
				return null;

			if (length == 0)
				return string.Empty;

			int startIndex = position + _origin;
			if (startIndex + length > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			position += length;
			return encoding.GetString(_buffer, startIndex, length);
		}

		/// <summary>
		/// Reads a null-terminated string from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		public string ReadNullTerminatedString(ref int position)
		{
			return ReadNullTerminatedString(ref position, Encoding.UTF8);
		}

		/// <summary>
		/// Reads a null-terminated string from the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadNullTerminatedString(ref int position, Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			int startIndex = position + _origin;
			if (startIndex >= _length)
				throw new ArgumentOutOfRangeException("startIndex");

			int index = startIndex;
			while (index < _length)
			{
				if (_buffer[index] == 0)
					break;

				index++;
			}

			int count = index - startIndex;
			position += count + 1;

			return encoding.GetString(_buffer, startIndex, count);
		}

		/// <summary>
		/// Reads a System.Guid value from the blob.
		/// </summary>
		public Guid ReadGuid(ref int position)
		{
			return new Guid(ReadBytes(ref position, 16));
		}

		/// <summary>
		/// Reads a System.DateTime value from the blob.
		/// </summary>
		public DateTime ReadDateTime(ref int position)
		{
			return ConvertUtils.ToDateTime(ReadInt32(ref position));
		}

		/// <summary>
		/// Reads in a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin reading.</param>
		/// <returns>A 32-bit integer in compressed format.</returns>
		public int Read7BitEncodedInt(ref int position)
		{
			int ret = 0;
			int shift = 0;
			byte b;
			int index = position + _origin;

			do
			{
				if (index >= _length)
					throw new IOException(SR.BlobReadOutOfBound);

				b = _buffer[index++];
				ret = ret | ((b & 0x7f) << shift);
				shift += 7;
			}
			while ((b & 0x80) != 0);

			position = index - _origin;

			return ret;
		}

		#endregion

		#region Writing

		/// <summary>
		/// Writes a signed byte to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The signed byte to write. </param>
		public void Write(ref int position, sbyte value)
		{
			Write(ref position, (byte)value);
		}

		/// <summary>
		/// Writes a byte to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The byte to write.</param>
		public void Write(ref int position, byte value)
		{
			if (!_writable)
				throw new NotSupportedException(SR.BlobWriteNotSupported);

			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 1);

			_buffer[startIndex] = value;

			position++;
		}

		/// <summary>
		/// Writes bytes to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The byte to write.</param>
		/// <param name="count">Number of bytes to write</param>
		public void Write(ref int position, byte value, int count)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, count);

			if (count <= 0x100)
			{
				for (int i = 0; i < count; i++)
				{
					_buffer[startIndex + i] = value;
				}
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
					Buffer.BlockCopy(buffer, 0, _buffer, startIndex, copySize);
					startIndex += copySize;
					count -= copySize;
				}
			}

			position += count;
		}

		/// <summary>
		/// Writes a block of bytes to the blob using data read from buffer.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing from.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		public void Write(ref int position, byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			Write(ref position, buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Writes a block of bytes to the blob using data read from buffer.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing from.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		public void Write(ref int position, byte[] buffer, int offset, int count)
		{
			if (!_writable)
				throw new NotSupportedException(SR.BlobWriteNotSupported);

			if (buffer == null)
				throw new ArgumentNullException("buffer");

			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			if ((buffer.Length - offset) < count)
				throw new ArgumentOutOfRangeException("count");

			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, count);

			if (count <= 8)
			{
				int num2 = count;
				while (--num2 >= 0)
				{
					_buffer[startIndex + num2] = buffer[offset + num2];
				}
			}
			else
			{
				Buffer.BlockCopy(buffer, offset, _buffer, startIndex, count);
			}

			position += count;
		}

		/// <summary>
		/// Writes a one-byte Boolean value to the blob, with 0 representing false and 1 representing true.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The Boolean value to write (0 or 1). </param>
		public void Write(ref int position, bool value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 1);

			_buffer[startIndex] = value ? (byte)1 : (byte)0;

			position++;
		}

		/// <summary>
		/// Writes a one-byte Boolean value to the blob, with 0 representing false and 1 representing true.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The Boolean value to write (0 or 1). </param>
		public void Write(ref int position, bool? value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 1);

			_buffer[startIndex] = value.HasValue ? (value.Value ? (byte)1 : (byte)0) : (byte)2;

			position++;
		}

		/// <summary>
		/// Writes a character array to the blob and advances the current position of the stream in accordance
		/// with the Encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="ch">The non-surrogate, Unicode character to write.</param>
		public void Write(ref int position, char ch)
		{
			Write(ref position, (short)ch);
		}

		/// <summary>
		/// Writes a two-byte signed integer to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The two-byte signed integer to write.</param>
		public void Write(ref int position, short value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 2);

			_buffer[startIndex] = (byte)value;
			_buffer[startIndex + 1] = (byte)(value >> 0x8);

			position += 2;
		}

		/// <summary>
		/// Writes a four-byte signed integer to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The four-byte signed integer to write.</param>
		public void Write(ref int position, int value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 4);

			_buffer[startIndex] = (byte)value;
			_buffer[startIndex + 1] = (byte)(value >> 0x8);
			_buffer[startIndex + 2] = (byte)(value >> 0x10);
			_buffer[startIndex + 3] = (byte)(value >> 0x18);

			position += 4;
		}

		/// <summary>
		/// Writes an eight-byte signed integer to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The eight-byte signed integer to write.</param>
		public void Write(ref int position, long value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 8);

			_buffer[startIndex] = (byte)value;
			_buffer[startIndex + 1] = (byte)(value >> 0x8);
			_buffer[startIndex + 2] = (byte)(value >> 0x10);
			_buffer[startIndex + 3] = (byte)(value >> 0x18);
			_buffer[startIndex + 4] = (byte)(value >> 0x20);
			_buffer[startIndex + 5] = (byte)(value >> 0x28);
			_buffer[startIndex + 6] = (byte)(value >> 0x30);
			_buffer[startIndex + 7] = (byte)(value >> 0x38);

			position += 8;
		}

		/// <summary>
		/// Writes a two-byte unsigned integer to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		public void Write(ref int position, ushort value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 2);

			_buffer[startIndex] = (byte)value;
			_buffer[startIndex + 1] = (byte)(value >> 0x8);

			position += 2;
		}

		/// <summary>
		/// Writes a four-byte unsigned integer to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		public void Write(ref int position, uint value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 4);

			_buffer[startIndex] = (byte)value;
			_buffer[startIndex + 1] = (byte)(value >> 0x8);
			_buffer[startIndex + 2] = (byte)(value >> 0x10);
			_buffer[startIndex + 3] = (byte)(value >> 0x18);

			position += 4;
		}

		/// <summary>
		/// Writes an eight-byte unsigned integer to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		public void Write(ref int position, ulong value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 8);

			_buffer[startIndex] = (byte)value;
			_buffer[startIndex + 1] = (byte)(value >> 0x8);
			_buffer[startIndex + 2] = (byte)(value >> 0x10);
			_buffer[startIndex + 3] = (byte)(value >> 0x18);
			_buffer[startIndex + 4] = (byte)(value >> 0x20);
			_buffer[startIndex + 5] = (byte)(value >> 0x28);
			_buffer[startIndex + 6] = (byte)(value >> 0x30);
			_buffer[startIndex + 7] = (byte)(value >> 0x38);

			position += 8;
		}

		/// <summary>
		/// Writes a four-byte floating-point value to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The four-byte floating-point value to write.</param>
		public unsafe void Write(ref int position, float value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 4);

			uint num = *((uint*)&value);
			_buffer[startIndex] = (byte)num;
			_buffer[startIndex + 1] = (byte)(num >> 0x8);
			_buffer[startIndex + 2] = (byte)(num >> 0x10);
			_buffer[startIndex + 3] = (byte)(num >> 0x18);

			position += 4;
		}

		/// <summary>
		/// Writes an eight-byte floating-point value to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The eight-byte floating-point value to write.</param>
		public unsafe void Write(ref int position, double value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 8);

			ulong num = *((ulong*)&value);
			_buffer[startIndex] = (byte)num;
			_buffer[startIndex + 1] = (byte)(num >> 0x8);
			_buffer[startIndex + 2] = (byte)(num >> 0x10);
			_buffer[startIndex + 3] = (byte)(num >> 0x18);
			_buffer[startIndex + 4] = (byte)(num >> 0x20);
			_buffer[startIndex + 5] = (byte)(num >> 0x28);
			_buffer[startIndex + 6] = (byte)(num >> 0x30);
			_buffer[startIndex + 7] = (byte)(num >> 0x38);

			position += 8;
		}

		/// <summary>
		/// Writes a decimal value to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The decimal value to write.</param>
		public void Write(ref int position, decimal value)
		{
			int startIndex = position + _origin;

			EnsureSafeToWrite(startIndex, 16);

			BinaryUtils.FillBytes(value, _buffer, startIndex);

			position += 16;
		}

		/// <summary>
		/// Writes a string to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void Write(ref int position, string value)
		{
			Write(ref position, value, Encoding.UTF8);
		}

		/// <summary>
		/// Writes a string to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void Write(ref int position, string value, Encoding encoding)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			byte[] buffer = encoding.GetBytes(value);
			Write(ref position, buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Writes a length-prefixed string to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteLengthPrefixedString(ref int position, string value)
		{
			WriteLengthPrefixedString(ref position, value, Encoding.UTF8);
		}

		/// <summary>
		/// Writes a length-prefixed string to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteLengthPrefixedString(ref int position, string value, Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			if (value == null)
			{
				Write7BitEncodedInt(ref position, -1);
			}
			else
			{
				byte[] buffer = encoding.GetBytes(value);
				Write7BitEncodedInt(ref position, buffer.Length);
				Write(ref position, buffer, 0, buffer.Length);
			}
		}

		/// <summary>
		/// Writes a null-terminated string to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteNullTerminatedString(ref int position, string value)
		{
			WriteNullTerminatedString(ref position, value, Encoding.UTF8);
		}

		/// <summary>
		/// Writes a null-terminated string to the blob.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteNullTerminatedString(ref int position, string value, Encoding encoding)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			byte[] buffer = encoding.GetBytes(value);
			Write(ref position, buffer, 0, buffer.Length);
			Write(ref position, (byte)0);
		}

		/// <summary>
		/// Writes a System.Guid value to the blob.
		/// </summary>
		public void Write(ref int position, Guid value)
		{
			Write(ref position, (byte[])value.ToByteArray());
		}

		/// <summary>
		/// Writes a System.DateTime value to the blob.
		/// </summary>
		public void Write(ref int position, DateTime value)
		{
			Write(ref position, (int)value.To_time_t());
		}

		/// <summary>
		/// Writes a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="position">The byte position in the blob at which to begin writing.</param>
		/// <param name="value">The 32-bit integer to be written.</param>
		public void Write7BitEncodedInt(ref int position, int value)
		{
			if (!_writable)
				throw new NotSupportedException(SR.BlobWriteNotSupported);

			int index = position + _origin;

			uint num = (uint)value;
			while (num >= 0x80)
			{
				EnsureSafeToWrite(index, 1);
				_buffer[index++] = (byte)(num | 0x80);

				num = num >> 7;
			}

			EnsureSafeToWrite(index, 1);
			_buffer[index++] = (byte)num;

			position = index - _origin;
		}

		protected void EnsureSafeToWrite(int index, int count)
		{
			if (!_writable)
				throw new NotSupportedException(SR.BlobWriteNotSupported);

			int length = index + count;
			if (length > _length)
			{
				if (length > _capacity)
				{
					EnsureCapacity(length);
				}

				_length = length;
			}
		}

		#endregion
	}
}
