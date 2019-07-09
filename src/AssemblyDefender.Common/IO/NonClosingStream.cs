using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public class NonClosingStream : Stream
	{
		#region Fields

		private bool _isClosed;
		private Stream _stream;

		#endregion

		#region Ctors

		public NonClosingStream(Stream innerStream)
		{
			if (innerStream == null)
				throw new ArgumentNullException("innerStream");

			_stream = innerStream;
		}

		#endregion

		#region Properties

		public override bool CanRead
		{
			get { return _isClosed ? false : _stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _isClosed ? false : _stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _isClosed ? false : _stream.CanWrite; }
		}

		public override bool CanTimeout
		{
			get { return _stream.CanTimeout; }
		}

		public override long Length
		{
			get
			{
				CheckClosed();
				return _stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				CheckClosed();
				return _stream.Position;
			}
			set
			{
				CheckClosed();
				_stream.Position = value;
			}
		}

		public override int ReadTimeout
		{
			get
			{
				CheckClosed();
				return _stream.ReadTimeout;
			}
			set
			{
				CheckClosed();
				_stream.ReadTimeout = value;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				CheckClosed();
				return _stream.WriteTimeout;
			}
			set
			{
				CheckClosed();
				_stream.WriteTimeout = value;
			}
		}

		public bool IsClosed
		{
			get { return _isClosed; }
		}

		public Stream InnerStream
		{
			get { return _stream; }
		}

		#endregion

		#region Methods

		public override void Flush()
		{
			CheckClosed();
			_stream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			CheckClosed();
			return _stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		public override void Close()
		{
			if (!_isClosed)
			{
				_stream.Flush();
			}

			_isClosed = true;
		}

		public override int ReadByte()
		{
			CheckClosed();
			return _stream.ReadByte();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			CheckClosed();
			return _stream.Read(buffer, offset, count);
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			CheckClosed();
			return _stream.BeginRead(buffer, offset, count, callback, state);
		}

		public override void WriteByte(byte value)
		{
			CheckClosed();
			_stream.WriteByte(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			CheckClosed();
			_stream.Write(buffer, offset, count);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			CheckClosed();
			return _stream.BeginWrite(buffer, offset, count, callback, state);
		}

		/// <summary>
		/// Throws an InvalidOperationException if the stream is closed.
		/// </summary>
		private void CheckClosed()
		{
			if (_isClosed)
			{
				throw new InvalidOperationException("Stream has been closed.");
			}
		}

		#endregion
	}
}
