using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Common.Text
{
	/// <summary>
	/// Class provide a wrapper of <see cref="StringBuilder"/>.
	/// </summary>
	public class StringBuilderEx
	{
		#region Fields

		private StringBuilder _sb;
		private int _indent;
		private bool _applyIndent;
		private string _indentString;

		#endregion

		#region Ctors

		public StringBuilderEx()
			: this(new StringBuilder())
		{
		}

		public StringBuilderEx(StringBuilder sb)
		{
			_sb = sb;
			_indentString = "\t";
			_applyIndent = true;
		}

		#endregion

		#region Properties

		public int Indent
		{
			get { return _indent; }
			set { _indent = value; }
		}

		public string IndentString
		{
			get { return _indentString; }
			set { _indentString = value; }
		}

		public int Length
		{
			get { return _sb.Length; }
		}

		#endregion

		#region Methods

		public StringBuilderEx Append(object obj)
		{
			if (obj == null)
				return this;

			return Append(obj.ToString());
		}

		public StringBuilderEx Append(string text)
		{
			if (string.IsNullOrEmpty(text))
				return this;

			EnsureIndent();
			_sb.Append(text);

			return this;
		}

		public StringBuilderEx AppendFormat(object obj, params object[] args)
		{
			if (obj == null)
				return this;

			return AppendFormat(null, obj.ToString(), args);
		}

		public StringBuilderEx AppendFormat(string text, params object[] args)
		{
			return AppendFormat(null, text, args);
		}

		public StringBuilderEx AppendFormat(IFormatProvider provider, string text, params object[] args)
		{
			if (string.IsNullOrEmpty(text))
				return this;

			EnsureIndent();
			_sb.AppendFormat(provider, text, args);

			return this;
		}

		public StringBuilderEx AppendMultiline(object obj)
		{
			if (obj == null)
				return this;

			return AppendMultiline(obj.ToString());
		}

		public StringBuilderEx AppendMultiline(string text)
		{
			if (string.IsNullOrEmpty(text))
				return this;

			int index = 0;
			while (index < text.Length)
			{
				int newLineIndex = text.IndexOf('\n', index);
				if (newLineIndex < 0)
				{
					// no new lines
					EnsureIndent();
					_sb.Append(text.Substring(index));
					index = text.Length;
				}
				else
				{
					EnsureIndent();

					int length;
					if (newLineIndex > 0 && text[newLineIndex - 1] == '\r')
						length = newLineIndex - index - 1;
					else
						length = newLineIndex - index;

					_sb.Append(text.Substring(index, length));
					AppendLine();

					index = newLineIndex + 1;
				}
			}

			return this;
		}

		public StringBuilderEx AppendMultilineFormat(object obj, params object[] args)
		{
			if (obj == null)
				return this;

			return AppendMultilineFormat(null, obj.ToString(), args);
		}

		public StringBuilderEx AppendMultilineFormat(string text, params object[] args)
		{
			return AppendMultilineFormat(null, text, args);
		}

		public StringBuilderEx AppendMultilineFormat(IFormatProvider provider, string text, params object[] args)
		{
			text = string.Format(provider, text, args);
			return AppendMultiline(text);
		}

		public StringBuilderEx AppendLine()
		{
			_sb.AppendLine();
			_applyIndent = true;

			return this;
		}

		public StringBuilderEx AppendLines(int numberOfLines)
		{
			for (int i = 0; i < numberOfLines; i++)
			{
				AppendLine();
			}

			return this;
		}

		public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			_sb.CopyTo(sourceIndex, destination, destinationIndex, count);
		}

		public StringBuilderEx EnsureWhiteSpace()
		{
			if (Length == 0)
				return this;

			char[] chars = new char[1];
			_sb.CopyTo(Length - 1, chars, 0, 1);

			char ch = chars[0];
			if (char.IsWhiteSpace(ch))
				return this;

			_sb.Append(' ');

			return this;
		}

		public override string ToString()
		{
			return _sb.ToString();
		}

		private void EnsureIndent()
		{
			if (!_applyIndent)
				return;

			for (int i = 0; i < _indent; i++)
			{
				_sb.Append(_indentString);
			}

			_applyIndent = false;
		}

		#endregion
	}
}
