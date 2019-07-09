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
	public class MemoryMappedFileStream : Stream
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

		public MemoryMappedFileStream(string filePath, bool writable)
			: this(filePath, 0L, 0L, writable)
		{
		}

		public MemoryMappedFileStream(string filePath, long offset, long length, bool writable)
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

		public MemoryMappedFileStream(MemoryMappedViewAccessor accessor, long offset, long length)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			_accessor = accessor;
			_buffer = _accessor.SafeMemoryMappedViewHandle;
			_origin = offset;
			_length = length;
			_disposeFile = false;
		}

		#endregion

		#region Properties

		public override long Length
		{
			get { return _length - _origin; }
		}

		public override long Position
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

		public override bool CanRead
		{
			get { return _accessor.CanRead; }
		}

		public override bool CanWrite
		{
			get { return _accessor.CanWrite; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		#endregion

		#region Methods

		public override int ReadByte()
		{
			long position = _position + _origin;
			if (position >= _length)
				return -1;

			var value = _accessor.ReadByte(position);
			_position++;

			return value;
		}

		public override unsafe int Read(byte[] buffer, int offset, int count)
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

		public override void WriteByte(byte value)
		{
			_accessor.Write(_position, value);
			_position++;
		}

		public override unsafe void Write(byte[] buffer, int offset, int count)
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

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (offset > 0x7fffffffL)
				throw new ArgumentOutOfRangeException("offset");

			switch (origin)
			{
				case SeekOrigin.Begin:
					{
						Position = offset;
					}
					break;

				case SeekOrigin.Current:
					{
						Position += offset;
					}
					break;

				case SeekOrigin.End:
					throw new NotSupportedException();

				default:
					throw new InvalidOperationException();
			}

			return (long)_position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && _disposeFile)
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
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion
	}
}
