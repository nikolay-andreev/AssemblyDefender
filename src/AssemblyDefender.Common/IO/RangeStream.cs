using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	/// <summary>
	/// Stream which define a range to inner stream.
	/// </summary>
	public class RangeStream : Stream
	{
		#region Fields

		private Stream _stream;
		private long _origin;
		private long _length;

		#endregion

		#region Ctors

		public RangeStream(Stream stream, long offset, int size)
		{
			_stream = stream;
			_stream.Position = offset;
			_origin = offset;
			_length = offset + (size > 0 ? size : _stream.Length);
		}

		#endregion

		#region Properties

		public Stream InnerStream
		{
			get { return _stream; }
		}

		public override bool CanRead
		{
			get { return _stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _stream.CanWrite; }
		}

		public override bool CanTimeout
		{
			get { return _stream.CanTimeout; }
		}

		public override long Length
		{
			get
			{
				return (long)(_length - _origin);
			}
		}

		public override long Position
		{
			get
			{
				return _stream.Position - _origin;
			}
			set
			{
				_stream.Position = _origin + (int)value;
			}
		}

		public override int ReadTimeout
		{
			get { return _stream.ReadTimeout; }
			set { _stream.ReadTimeout = value; }
		}

		public override int WriteTimeout
		{
			get { return _stream.WriteTimeout; }
			set { _stream.WriteTimeout = value; }
		}

		#endregion

		#region Methods

		public override int ReadByte()
		{
			if (_stream.Position >= _length)
			{
				return -1;
			}

			return _stream.ReadByte();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count > 0)
			{
				long available = _length - _stream.Position;
				if (count > available)
				{
					if (available > int.MaxValue)
					{
						throw new InvalidOperationException();
					}

					count = (int)available;
				}
			}

			return _stream.Read(buffer, offset, count);
		}

		public override void WriteByte(byte value)
		{
			_stream.WriteByte(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _stream.Seek(_origin + offset, origin);
		}

		public override void SetLength(long value)
		{
			long newLen = _origin + value;
			_stream.SetLength(newLen);
			_length = newLen;
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		#endregion
	}
}
