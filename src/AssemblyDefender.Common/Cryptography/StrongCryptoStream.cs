using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.Cryptography
{
	/// <summary>
	/// Crypto stream support only one operation at a time. Read or Write but not both. Strong encryption level.
	/// </summary>
	public class StrongCryptoStream : Stream
	{
		#region Fields

		private const int DefaultBlockSize = 4096;
		private int _blockSize;
		private byte[] _buffer;
		private int _key;
		private int _bufferIndex;
		private int _bufferLength;
		private Stream _stream;
		private StrongCryptoStreamMode _mode;

		#endregion

		#region Ctors

		public StrongCryptoStream(Stream stream)
			: this(stream, StrongCryptoUtils.DefaultKey)
		{
		}

		public StrongCryptoStream(
			Stream stream, int key,
			StrongCryptoStreamMode mode = StrongCryptoStreamMode.Read,
			int blockSize = DefaultBlockSize)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			_stream = stream;
			_key = key;
			_mode = mode;
			_blockSize = (blockSize > 0 ? blockSize : DefaultBlockSize);
			_buffer = new byte[_blockSize];

			switch (_mode)
			{
				case StrongCryptoStreamMode.Read:
					{
						if (!_stream.CanRead)
						{
							throw new ArgumentException(SR.StreamReadNotSupported);
						}
					}
					break;

				case StrongCryptoStreamMode.Write:
					{
						if (!_stream.CanWrite)
						{
							throw new ArgumentException(SR.StreamWriteNotSupported);
						}
					}
					break;

				default:
					throw new NotImplementedException();
			}
		}

		#endregion

		#region Properties

		public override bool CanRead
		{
			get { return _mode == StrongCryptoStreamMode.Read; }
		}

		public override bool CanWrite
		{
			get { return _mode == StrongCryptoStreamMode.Write; }
		}

		public override bool CanSeek
		{
			get { return _mode == StrongCryptoStreamMode.Read && _stream.CanSeek; }
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
			get { return _stream.Position - _bufferLength + _bufferIndex; }
			set
			{
				if (!CanSeek)
				{
					throw new IOException(SR.StreamSeekNotSupported);
				}

				if (value < 0)
				{
					throw new IOException(SR.StreamReadOutOfBound);
				}

				long position = (value / _blockSize) * _blockSize;
				_bufferIndex = (int)(value - position);
				_bufferLength = 0;
				_stream.Position = position;
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

		public StrongCryptoStreamMode Mode
		{
			get { return _mode; }
		}

		public int BlockSize
		{
			get { return _blockSize; }
		}

		#endregion

		#region Methods

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (!CanRead)
			{
				throw new IOException(SR.StreamReadNotSupported);
			}

			if (_bufferLength < _blockSize)
			{
				if (_bufferLength == 0)
				{
					int readLen = _stream.Read(_buffer, 0, _blockSize);
					if (readLen == 0)
						return 0;

					_bufferLength = readLen;
					StrongCryptoUtils.Decrypt(_buffer, _key, 0, _bufferLength);
				}

				if (_bufferIndex >= _bufferLength)
					return 0;
			}

			int readCount = 0;
			while (count > 0)
			{
				if (_bufferIndex == _bufferLength)
				{
					_bufferIndex = 0;

					_bufferLength = _stream.Read(_buffer, 0, _blockSize);
					if (_bufferLength == 0)
						break;

					StrongCryptoUtils.Decrypt(_buffer, _key, 0, _bufferLength);
				}

				int size = _bufferLength - _bufferIndex;
				if (size > count)
					size = count;

				Buffer.BlockCopy(_buffer, _bufferIndex, buffer, offset, size);

				count -= size;
				offset += size;
				readCount += size;
				_bufferIndex += size;
			}

			return readCount;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!CanWrite)
			{
				throw new IOException(SR.StreamWriteNotSupported);
			}

			while (count > 0)
			{
				if (_bufferIndex == _blockSize)
				{
					StrongCryptoUtils.Encrypt(_buffer, _key, 0, _bufferIndex);
					_stream.Write(_buffer, 0, _bufferIndex);
					_bufferIndex = 0;
				}

				int size = _blockSize - _bufferIndex;
				if (size > count)
					size = count;

				Buffer.BlockCopy(buffer, offset, _buffer, _bufferIndex, size);

				count -= size;
				offset += size;
				_bufferIndex += size;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
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
					throw new ArgumentException("InvalidSeekOrigin");
			}

			return Position;
		}

		public override void SetLength(long value)
		{
			if (!CanWrite)
			{
				throw new IOException(SR.StreamWriteNotSupported);
			}

			_stream.SetLength(value);
		}

		public override void Flush()
		{
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					switch (_mode)
					{
						case StrongCryptoStreamMode.Write:
							{
								if (_bufferIndex > 0)
								{
									StrongCryptoUtils.Encrypt(_buffer, _key, 0, _bufferIndex);
									_stream.Write(_buffer, 0, _bufferIndex);
									_bufferIndex = 0;
								}
							}
							break;
					}

					_stream.Close();
				}

				_buffer = null;
				_stream = null;
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion
	}
}
