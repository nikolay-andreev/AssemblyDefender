using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace AssemblyDefender.Common.IO
{
	public class MemoryMappedFileAccessor : IBinaryAccessor
	{
		#region Fields

		private long _origin;
		private long _position;
		private long _length;
		private bool _disposeFile = true;
		private MemoryMappedFile _file;
		private MemoryMappedViewAccessor _accessor;
		private SafeMemoryMappedViewHandle _buffer;

		#endregion

		#region Ctors

		public MemoryMappedFileAccessor(string filePath, bool writable)
			: this(filePath, 0L, 0L, writable)
		{
		}

		public MemoryMappedFileAccessor(string filePath, long offset, long length, bool writable)
		{
			var access = writable ? MemoryMappedFileAccess.ReadWrite : MemoryMappedFileAccess.Read;

			using (var stream = new FileStream(filePath,
				FileMode.Open,
				writable ? FileAccess.ReadWrite : FileAccess.Read,
				FileShare.ReadWrite))
			{
				_length = stream.Length;

				if (length > 0)
					_length = length;

				_file = MemoryMappedFile.CreateFromFile(
					stream,
					null,
					_length,
					access,
					null,
					HandleInheritability.None,
					false);
			}

			_accessor = _file.CreateViewAccessor(offset, _length, access);
			_buffer = _accessor.SafeMemoryMappedViewHandle;
		}

		public MemoryMappedFileAccessor(MemoryMappedViewAccessor accessor, long offset, long length)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			_accessor = accessor;
			_buffer = _accessor.SafeMemoryMappedViewHandle;
			_origin = offset;
			_length = length;
			_disposeFile = false;
		}

		public MemoryMappedFileAccessor(MemoryMappedFileAccessor accessor)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			_origin = accessor._origin;
			_length = accessor._length;
			_file = accessor._file;
			_accessor = accessor._accessor;
			_buffer = accessor._buffer;
			_disposeFile = false;
		}

		public MemoryMappedFileAccessor(MemoryMappedFileAccessor accessor, long offset, long length)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			_origin = accessor._origin + offset;
			_length = _origin + length;
			_file = accessor._file;
			_accessor = accessor._accessor;
			_buffer = accessor._buffer;
			_disposeFile = false;
		}

		#endregion

		#region Properties

		public long Length
		{
			get { return _length - _origin; }
		}

		public long Position
		{
			get { return _position; }
			set
			{
				if (value < 0 || value > _length - _origin)
					throw new ArgumentOutOfRangeException("Position");

				_position = value;
			}
		}

		public bool DisposeFile
		{
			get { return _disposeFile; }
			set { _disposeFile = value; }
		}

		public bool CanRead
		{
			get { return _accessor.CanRead; }
		}

		public bool CanWrite
		{
			get { return _accessor.CanWrite; }
		}

		public bool CanSeek
		{
			get { return true; }
		}

		#endregion

		#region Methods

		public IBinaryAccessor Map(long offset, long length)
		{
			if (offset + length > Length)
				throw new ArgumentOutOfRangeException("length");

			return new MemoryMappedFileAccessor(this, offset, length);
		}

		public void Dispose()
		{
			if (_disposeFile)
			{
				if (_accessor != null)
				{
					_accessor.Dispose();
				}

				if (_buffer != null)
				{
					_buffer.Dispose();
				}

				if (_file != null)
				{
					_file.Dispose();
				}
			}

			_accessor = null;
			_buffer = null;
			_file = null;
		}

		#endregion

		#region Reading

		public int Read()
		{
			long position = _position + _origin;
			if (position >= _length)
				return -1;

			var value = _accessor.ReadByte(position);
			_position++;

			return value;
		}

		public unsafe int Read(byte[] buffer, int offset, int count)
		{
			long position = _position + _origin;

			long numOfBytesToRead = _length - position;
			if (numOfBytesToRead > count)
				numOfBytesToRead = count;

			if (numOfBytesToRead <= 0)
				return 0;

			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				_buffer.AcquirePointer(ref pointer);
				BufferUtils.Memcpy((byte*)(pointer + position), 0, buffer, offset, count);
			}
			finally
			{
				if (pointer != null)
				{
					_buffer.ReleasePointer();
				}
			}

			_position += count;

			return count;
		}

		public bool ReadBoolean()
		{
			long position = _position + _origin;
			if (position + 1 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadBoolean(position);
			_position++;

			return value;
		}

		public byte ReadByte()
		{
			long position = _position + _origin;
			if (position + 1 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadByte(position);
			_position++;

			return value;
		}

		public sbyte ReadSByte()
		{
			long position = _position + _origin;
			if (position + 1 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadSByte(position);
			_position++;

			return value;
		}

		public short ReadInt16()
		{
			long position = _position + _origin;
			if (position + 2 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadInt16(position);
			_position += 2;

			return value;
		}

		public int ReadInt32()
		{
			long position = _position + _origin;
			if (position + 4 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadInt32(position);
			_position += 4;

			return value;
		}

		public long ReadInt64()
		{
			long position = _position + _origin;
			if (position + 8 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadInt64(position);
			_position += 8;

			return value;
		}

		public ushort ReadUInt16()
		{
			long position = _position + _origin;
			if (position + 2 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadUInt16(position);
			_position += 2;

			return value;
		}

		public uint ReadUInt32()
		{
			long position = _position + _origin;
			if (position + 4 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadUInt32(position);
			_position += 4;

			return value;
		}

		public ulong ReadUInt64()
		{
			long position = _position + _origin;
			if (position + 8 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadUInt64(position);
			_position += 8;

			return value;
		}

		public float ReadSingle()
		{
			long position = _position + _origin;
			if (position + 4 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadSingle(position);
			_position += 4;

			return value;
		}

		public double ReadDouble()
		{
			long position = _position + _origin;
			if (position + 8 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadDouble(position);
			_position += 8;

			return value;
		}

		public decimal ReadDecimal()
		{
			long position = _position + _origin;
			if (position + 16 > _length)
				throw new IOException(SR.BlobReadOutOfBound);

			var value = _accessor.ReadDecimal(position);
			_position += 16;

			return value;
		}

		public char ReadChar()
		{
			return (char)ReadInt16();
		}

		public int Read7BitEncodedInt()
		{
			int ret = 0;
			int shift = 0;
			byte b;
			long position = _position + _origin;

			do
			{
				if (position >= _length)
					throw new IOException(SR.BlobReadOutOfBound);

				b = _accessor.ReadByte(position++);
				ret = ret | ((b & 0x7f) << shift);
				shift += 7;
			}
			while ((b & 0x80) != 0);

			_position = position - _origin;

			return ret;
		}

		public string ReadString(int length)
		{
			return ReadString(length, Encoding.UTF8, false);
		}

		public string ReadString(int length, Encoding encoding)
		{
			return ReadString(length, encoding, encoding is UnicodeEncoding);
		}

		public string ReadLengthPrefixedString()
		{
			return ReadLengthPrefixedString(Encoding.UTF8);
		}

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

		public string ReadNullTerminatedString()
		{
			return ReadNullTerminatedString(Encoding.UTF8);
		}

		public unsafe string ReadNullTerminatedString(Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			string value;
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				_buffer.AcquirePointer(ref pointer);

				long position = _position + _origin;
				byte* startPointer = (pointer + position);
				int length = 0;
				while (*(byte*)(startPointer + length) != 0)
				{
					length++;
				}

				if (position >= _length)
					throw new IOException(SR.BlobReadOutOfBound);

				value = new string((sbyte*)startPointer, 0, length, encoding);
				_position += length + 1;
			}
			finally
			{
				if (pointer != null)
				{
					_buffer.ReleasePointer();
				}
			}

			return value;
		}

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

		public unsafe void Write(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			long position = _position + _origin;
			if (position + count >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				_buffer.AcquirePointer(ref pointer);
				BufferUtils.Memcpy(buffer, offset, (byte*)(pointer + position), 0, count);
			}
			finally
			{
				if (pointer != null)
				{
					_buffer.ReleasePointer();
				}
			}

			_position += count;
		}

		public void Write(char value)
		{
			Write((short)value);
		}

		public void Write(bool value)
		{
			long position = _position + _origin;
			if (position + 1 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position++;
		}

		public void Write(sbyte value)
		{
			long position = _position + _origin;
			if (position + 1 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position++;
		}

		public void Write(byte value)
		{
			_accessor.Write(_position, value);
			_position++;
		}

		public void Write(short value)
		{
			long position = _position + _origin;
			if (position + 2 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position += 2;
		}

		public void Write(int value)
		{
			long position = _position + _origin;
			if (position + 4 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position += 4;
		}

		public void Write(long value)
		{
			long position = _position + _origin;
			if (position + 8 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position += 8;
		}

		public void Write(ushort value)
		{
			long position = _position + _origin;
			if (position + 2 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position += 2;
		}

		public void Write(uint value)
		{
			long position = _position + _origin;
			if (position + 4 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position += 4;
		}

		public void Write(ulong value)
		{
			long position = _position + _origin;
			if (position + 8 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position += 8;
		}

		public void Write(float value)
		{
			long position = _position + _origin;
			if (position + 4 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position += 4;
		}

		public void Write(double value)
		{
			long position = _position + _origin;
			if (position + 8 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position += 8;
		}

		public void Write(decimal value)
		{
			long position = _position + _origin;
			if (position + 16 >= _length)
				throw new IOException(SR.BlobWriteOutOfBound);

			_accessor.Write(position, value);
			_position += 16;
		}

		public void Write7BitEncodedInt(int value)
		{
			long position = _position + _origin;

			uint num = (uint)value;
			while (num >= 0x80)
			{
				_accessor.Write(position++, (byte)(num | 0x80));
				num = num >> 7;
			}

			_accessor.Write(position++, (byte)num);

			_position = position - _origin;
		}

		public void Write(string value)
		{
			Write(value, Encoding.UTF8);
		}

		public void Write(string value, Encoding encoding)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			byte[] buffer = encoding.GetBytes(value);
			Write(buffer, 0, buffer.Length);
		}

		public void WriteLengthPrefixedString(string value)
		{
			WriteLengthPrefixedString(value, Encoding.UTF8);
		}

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

		public void WriteNullTerminatedString(string value)
		{
			WriteNullTerminatedString(value, Encoding.UTF8);
		}

		public void WriteNullTerminatedString(string value, Encoding encoding)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			if (encoding == null)
				throw new ArgumentNullException("encoding");

			byte[] buffer = encoding.GetBytes(value);
			Write(buffer, 0, buffer.Length);
			Write((byte)0);
		}

		#endregion
	}
}
