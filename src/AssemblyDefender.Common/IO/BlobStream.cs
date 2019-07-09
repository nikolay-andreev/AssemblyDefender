using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public class BlobStream : Stream
	{
		#region Fields

		private int _position;
		private Blob _blob;

		#endregion

		#region Ctors

		public BlobStream()
			: this(new Blob())
		{
		}

		public BlobStream(byte[] buffer)
			: this(buffer, true)
		{
		}

		public BlobStream(byte[] buffer, int offset, int count)
			: this(new Blob(buffer, offset, count), 0)
		{
		}

		public BlobStream(byte[] buffer, bool writable)
			: this(new Blob(buffer, writable))
		{
		}

		public BlobStream(Blob blob)
			: this(blob, 0)
		{
		}

		public BlobStream(Blob blob, int position)
		{
			if (blob == null)
				throw new ArgumentNullException("blob");

			_blob = blob;
			_position = position;
		}

		#endregion

		#region Properties

		public Blob Blob
		{
			get { return _blob; }
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return _blob.Writable; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public virtual int Capacity
		{
			get { return _blob.Capacity; }
			set { _blob.Capacity = value; }
		}

		public override long Length
		{
			get { return _blob.Length; }
		}

		public override long Position
		{
			get { return _position; }
			set
			{
				if (value < 0L)
					throw new ArgumentOutOfRangeException("Position");

				if (value > 0x7fffffffL)
					throw new ArgumentOutOfRangeException("Position");

				_position = (int)value;
			}
		}

		#endregion

		#region Methods

		public override int ReadByte()
		{
			return _blob.ReadByte(ref _position);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _blob.Read(ref _position, buffer, offset, count);
		}

		public override void WriteByte(byte value)
		{
			_blob.Write(ref _position, value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_blob.Write(ref _position, buffer, offset, count);
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
					{
						Position = Length + offset;
					}
					break;

				default:
					throw new InvalidOperationException();
			}

			return (long)_position;
		}

		public override void SetLength(long value)
		{
			if (value < 0L || value > 0x7fffffff)
				throw new ArgumentOutOfRangeException("value");

			int len = (int)value;
			_blob.Length = len;

			if (_position > len)
				_position = len;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				_blob = null;
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion
	}
}
