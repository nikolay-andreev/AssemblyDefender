using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	internal class ReflectionSignatureLexer
	{
		#region Fields

		private static char[] EscapeChars = new char[] { '\\', '=', ',', '[', ']', '+', '*' };
		private int _length;
		private int _pos;
		private string _inputText;

		#endregion

		#region Ctors

		internal ReflectionSignatureLexer(string inputText)
		{
			_inputText = inputText ?? string.Empty;
			_length = _inputText.Length;
		}

		#endregion

		#region Properties

		internal bool IsAtEndOfInput
		{
			get { return _pos == _length; }
		}

		#endregion

		#region Methods

		internal int Read()
		{
			if (_pos == _length)
				return -1;

			return _inputText[_pos++];
		}

		internal int Read(char[] buffer, int index, int count)
		{
			int num = _length - _pos;
			if (num > 0)
			{
				if (num > count)
				{
					num = count;
				}
				_inputText.CopyTo(_pos, buffer, index, num);
				_pos += num;
			}

			return num;
		}

		internal string ReadIdentity()
		{
			return ReadTo(EscapeChars, '\\');
		}

		internal string ReadTo(char[] endChars, char escapeChar)
		{
			int startPos = _pos;
			bool escape = false;
			while (_pos < _length)
			{
				char ch = _inputText[_pos];
				if (!escape && ch == escapeChar)
				{
					_pos++;
					escape = true;
					continue;
				}

				for (int i = 0; i < endChars.Length; i++)
				{
					if (ch == endChars[i])
					{
						if (!escape)
						{
							return _inputText.Substring(startPos, _pos - startPos);
						}

						break;
					}
				}

				_pos++;
				escape = false;
			}

			if (escape)
				return string.Empty;

			return _inputText.Substring(startPos, _pos - startPos);
		}

		internal string ReadToEnd()
		{
			string str;
			if (_pos == 0)
			{
				str = _inputText;
			}
			else
			{
				str = _inputText.Substring(_pos, _length - _pos);
			}

			_pos = _length;

			return str;
		}

		internal void ReadWhiteSpaces()
		{
			int startPos = _pos;
			while (_pos < _length)
			{
				if (!char.IsWhiteSpace(_inputText[_pos]))
					break;

				_pos++;
			}
		}

		internal void Move()
		{
			_pos++;
		}

		internal void Move(int offset)
		{
			offset += _pos;

			if (offset < 0 || _length <= offset)
				_pos = _length;
			else
				_pos += offset;
		}

		internal int Peek()
		{
			if (_pos == _length)
				return -1;

			return _inputText[_pos];
		}

		internal int Peek(int offset)
		{
			offset += _pos;

			if (offset < 0 || _length <= offset)
				return -1;

			return _inputText[offset];
		}

		internal int Peek(int offset, bool skipWhiteSpace)
		{
			offset += _pos;

			if (offset < 0 || _length <= offset)
				return -1;

			while (offset < _length)
			{
				if (char.IsWhiteSpace(_inputText[offset]))
				{
					offset++;
					continue;
				}

				return _inputText[offset];
			}

			return -1;
		}

		#endregion
	}
}
