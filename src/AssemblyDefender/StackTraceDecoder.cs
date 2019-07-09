using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AssemblyDefender
{
	internal class StackTraceDecoder
	{
		#region Fields

		private int _index;
		private int _length;
		private string _text;
		private BuildLog _log;

		#endregion

		#region Ctors

		internal StackTraceDecoder(BuildLog log, string text)
		{
			if (log == null)
				throw new ArgumentNullException("log");

			_log = log;
			_text = text;
			_length = (_text != null) ? _text.Length : 0;
		}

		#endregion

		#region Properties

		internal string Text
		{
			get { return _text; }
		}

		#endregion

		#region Methods

		internal void Decode()
		{
			if (string.IsNullOrEmpty(_text))
				return;

			bool isName = IsNameChar(_text[0]);

			var sb = new StringBuilder();

			while (_index < _length)
			{
				sb.Append(isName ? ReadNameToken(ref isName) : ReadToken(ref isName));
			}

			_text = sb.ToString();
		}

		private string ReadToken(ref bool isName)
		{
			int startIndex = _index;

			while (_index < _length && !IsNameChar(_text[_index]))
			{
				_index++;
			}

			isName = true;

			int count = _index - startIndex;
			if (count == 0)
				return string.Empty;

			return _text.Substring(startIndex, count);
		}

		private string ReadNameToken(ref bool isName)
		{
			int startIndex = _index;

			while (_index < _length && IsNameChar(_text[_index]))
			{
				_index++;
			}

			isName = false;

			int count = _index - startIndex;
			if (count == 0)
				return string.Empty;

			string token = _text.Substring(startIndex, count);

			// Check for namespace
			if (token == _log.MainTypeNamespace)
			{
				if (_index + 1 < _length && _text[_index] == '.' && IsNameChar(_text[_index + 1]))
				{
					isName = true;
					_index++;
					return string.Empty;
				}
			}

			string newName;
			if (_log.TryGetOldName(token, out newName))
				return newName;

			return token;
		}

		private bool IsNameChar(char c)
		{
			if (char.IsLetterOrDigit(c))
				return true;

			if (c == '_')
				return true;

			return false;
		}

		#endregion
	}
}
