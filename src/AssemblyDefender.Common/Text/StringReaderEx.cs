using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.Text
{
	public class StringReaderEx
	{
		#region Fields

		private int _length;
		private int _pos;
		private string _s;

		#endregion

		#region Ctors

		public StringReaderEx(string s)
		{
			_s = s ?? string.Empty;
			_length = _s.Length;
		}

		#endregion

		#region Properties

		public bool EOF
		{
			get { return _pos == _length; }
		}

		#endregion

		#region Methods

		public int Read()
		{
			if (_pos == _length)
				return -1;

			return _s[_pos++];
		}

		public int Read(char[] buffer, int index, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			if (index < 0)
				throw new ArgumentOutOfRangeException("index");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			if ((buffer.Length - index) < count)
				throw new ArgumentOutOfRangeException("index");

			int num = _length - _pos;
			if (num > 0)
			{
				if (num > count)
				{
					num = count;
				}
				_s.CopyTo(_pos, buffer, index, num);
				_pos += num;
			}

			return num;
		}

		public string ReadLine()
		{
			if (_pos == _length)
				return string.Empty;

			int index = _pos;
			while (index < _length)
			{
				char ch = _s[index];
				switch (ch)
				{
					case '\r':
					case '\n':
						{
							string str = _s.Substring(_pos, index - _pos);
							_pos = index + 1;
							if (ch == '\r' && _pos < _length && _s[_pos] == '\n')
							{
								_pos++;
							}
							return str;
						}
				}
				index++;
			}

			string str2 = _s.Substring(_pos, index - _pos);
			_pos = index;
			return str2;
		}

		public string ReadTo(char ch)
		{
			int startPos = _pos;
			while (_pos < _length)
			{
				if (_s[_pos] == ch)
					break;

				_pos++;
			}

			return _s.Substring(startPos, _pos - startPos);
		}

		public string ReadToEnd()
		{
			string str;
			if (_pos == 0)
			{
				str = _s;
			}
			else
			{
				str = _s.Substring(_pos, _length - _pos);
			}

			_pos = _length;

			return str;
		}

		public string ReadWhiteSpaces()
		{
			int startPos = _pos;
			while (_pos < _length)
			{
				if (!char.IsWhiteSpace(_s[_pos]))
					break;

				_pos++;
			}

			return _s.Substring(startPos, _pos - startPos);
		}

		public int Peek(int offset)
		{
			offset += _pos;

			if (offset < 0 || _length <= offset)
				return -1;

			return _s[offset];
		}

		#endregion
	}
}
