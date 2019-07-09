using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.Text
{
	/// <summary>
	/// Class provide a wrapper of <see cref="TextReader"/>.
	/// </summary>
	public class TextReaderEx
	{
		#region Fields

		private TextReader _reader;
		private char[] _buffer;
		private int _charPos;
		private int _charLen;
		private int _offset;
		private int _line;
		private int _column;
		private bool _closeInnerWhenDisposed;

		#endregion

		#region Ctors

		public TextReaderEx(TextReader reader)
			: this(reader, false)
		{
		}

		public TextReaderEx(TextReader reader, bool closeInnerWhenDisposed)
		{
			_reader = reader;
			_closeInnerWhenDisposed = closeInnerWhenDisposed;
			_line = 1;
			_column = 1;
		}

		#endregion

		#region Properties

		public bool EOF
		{
			get { return _buffer == null && _reader.Peek() == -1; }
		}

		public int Offset
		{
			get { return _offset; }
		}

		public int Line
		{
			get { return _line; }
		}

		public int Column
		{
			get { return _column; }
		}

		#endregion

		#region Methods

		public int Read()
		{
			int result;

			if (_buffer == null)
			{
				// Buffer is empty, so read directly
				result = _reader.Read();
			}
			else
			{
				// Read from buffer
				result = _buffer[_charPos++];
				if (_charPos == _charLen)
				{
					_buffer = null;
					_charPos = 0;
					_charLen = 0;
				}
			}

			_offset++;

			if (result == '\n')
			{
				_line++;
				_column = 1;
			}
			else
			{
				_column++;
			}

			return result;
		}

		public int Read(char[] buffer, int index, int count)
		{
			int result;

			if (_buffer == null)
			{
				// Buffer is empty, so read directly
				result = _reader.Read(buffer, index, count);
			}
			else
			{
				int len = _charLen - _charPos;
				if (len > count)
				{
					// Data in the buffer is sufficient
					Buffer.BlockCopy(_buffer, _charPos * 2, buffer, index * 2, count * 2);
					_charPos += count;
					result = count;

					if (_charPos == _charLen)
					{
						_buffer = null;
						_charPos = 0;
						_charLen = 0;
					}
				}
				else
				{
					// Remaining data is less than requested. Read from buffer and directly
					Buffer.BlockCopy(_buffer, _charPos * 2, buffer, index * 2, len * 2);
					_buffer = null;
					_charPos = 0;
					_charLen = 0;

					result = len;
					result += _reader.Read(buffer, index + len, count - len);
				}
			}

			_offset += result;

			for (int i = 0; i < result; i++)
			{
				if (buffer[i] == '\n')
				{
					_line++;
					_column = 1;
				}
				else
				{
					_column++;
				}
			}

			return result;
		}

		public int Peek(int offset)
		{
			if (_buffer == null)
			{
				// Buffer is empty
				// Check if EOF is reached
				if (_reader.Peek() == -1)
					return -1;

				// Create buffer
				_charLen = offset + 0x400;
				_buffer = new char[_charLen];
				_charPos = 0;

				int readLen = _reader.Read(_buffer, 0, _charLen);
				if (_charLen > readLen)
				{
					// Read length is less than allocated. Shrink buffer.
					_charLen = readLen;
					char[] newBuffer = new char[_charLen];
					Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _charLen * 2);
					_buffer = newBuffer;
				}

				if (_charLen > offset)
					return _buffer[offset];
				else
					return -1;
			}
			else
			{
				int len = _charLen - _charPos;
				if (len > offset)
				{
					// Data in the buffer is sufficient
					return _buffer[_charPos + offset];
				}
				else
				{
					// Remaining data is less than requested. Expand buffer.
					// Check if EOF is reached.
					if (_reader.Peek() == -1)
						return -1;

					_charLen = offset + 0x400;
					char[] newBuffer = new char[_charLen];
					Buffer.BlockCopy(_buffer, _charPos * 2, newBuffer, 0, len * 2);
					_buffer = newBuffer;
					_charPos = 0;

					int count = _charLen - len;
					int readLen = _reader.Read(_buffer, len, count);
					if (count > readLen)
					{
						// Read length is less than allocated. Shrink buffer.
						_charLen = len + readLen;
						newBuffer = new char[_charLen];
						Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _charLen * 2);
						_buffer = newBuffer;
					}

					if (_charLen > offset)
						return _buffer[offset];
					else
						return -1;
				}
			}
		}

		public TextPoint GetPoint()
		{
			var point = new TextPoint();
			point.Offset = _offset;
			point.Line = _line;
			point.Column = _column;

			return point;
		}

		public void GetPoint(out int offset, out int line, out int column)
		{
			offset = _offset;
			line = _line;
			column = _column;
		}

		public void Dispose()
		{
			if (_reader != null)
			{
				if (_closeInnerWhenDisposed)
				{
					_reader.Dispose();
				}

				_reader = null;
			}
		}

		#endregion

		#region Static

		public static TextReaderEx FromText(string text)
		{
			// It's faster to store text in internal buffer rather than use the reader.
			// Create a dummy reader.
			var reader = new TextReaderEx(new StringReader(string.Empty), false);
			reader._buffer = text.ToCharArray();
			reader._charLen = text.Length;

			return reader;
		}

		public static TextReaderEx FromFile(string path)
		{
			var inner = new StreamReader(path);
			return new TextReaderEx(inner, true);
		}

		public static TextReaderEx FromFile(string path, bool detectEncodingFromByteOrderMarks)
		{
			var inner = new StreamReader(path, detectEncodingFromByteOrderMarks);
			return new TextReaderEx(inner, true);
		}

		public static TextReaderEx FromStream(Stream stream)
		{
			var inner = new StreamReader(stream);
			return new TextReaderEx(inner, true);
		}

		public static TextReaderEx FromStream(Stream stream, bool detectEncodingFromByteOrderMarks)
		{
			var inner = new StreamReader(stream, detectEncodingFromByteOrderMarks);
			return new TextReaderEx(inner, true);
		}

		#endregion
	}
}
