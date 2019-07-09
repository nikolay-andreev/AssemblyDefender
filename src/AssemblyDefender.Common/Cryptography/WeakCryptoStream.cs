using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.Cryptography
{
	/// <summary>
	/// Crypto stream supporting read and write simultaneously. Weak encryption level.
	/// </summary>
	public class WeakCryptoStream : Stream
	{
		#region Fields

		private bool _closeInner;
		private long _position;
		private int _key;
		private Stream _stream;

		#endregion

		#region Ctors

		public WeakCryptoStream(Stream stream)
			: this(stream, StrongCryptoUtils.DefaultKey)
		{
		}

		public WeakCryptoStream(Stream stream, int key, bool closeInner = true)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			_key = key;
			_stream = stream;
			_closeInner = closeInner;

			if (CanSeek)
			{
				_position = _stream.Position;
			}
		}

		#endregion

		#region Properties

		public override bool CanRead
		{
			get { return _stream.CanRead; }
		}

		public override bool CanWrite
		{
			get { return _stream.CanWrite; }
		}

		public override bool CanSeek
		{
			get { return _stream.CanSeek; }
		}

		public override bool CanTimeout
		{
			get { return _stream.CanTimeout; }
		}

		public override long Length
		{
			get { return _stream.Length; }
		}

		public override long Position
		{
			get { return _position; }
			set
			{
				_stream.Position = value;
				_position = _stream.Position;
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

		public override int Read(byte[] buffer, int offset, int count)
		{
			int readCount = _stream.Read(buffer, offset, count);

			for (int i = offset; i < readCount; i++)
			{
				buffer[i] ^= (byte)(_key >> (((int)_position % 4) << 3));
				_position++;
			}

			return readCount;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			byte[] newBuff = new byte[0x1000];

			while (count > 0)
			{
				int copySize = (int)(count > 0x1000 ? 0x1000 : count);

				for (int i = 0, j = offset; i < copySize; i++, j++)
				{
					newBuff[i] = (byte)(buffer[j] ^ (byte)(_key >> (((int)_position % 4) << 3)));
					_position++;
				}

				_stream.Write(newBuff, 0, copySize);

				offset += copySize;
				count -= copySize;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			_position = _stream.Seek(offset, origin);
			return _position;
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_stream != null)
				{
					if (_closeInner)
					{
						_stream.Close();
					}

					_stream = null;
				}
			}
		}

		#endregion
	}
}
