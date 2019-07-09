using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.Text
{
	/// <summary>
	/// Class provide a wrapper of <see cref="TextWriter"/>.
	/// </summary>
	public class TextWriterEx : IDisposable
	{
		#region Fields

		private TextWriter _writer;
		private int _length;
		private int _indent;
		private bool _applyIndent;
		private string _indentString;
		private string _lastWriteString;
		private bool _closeInnerWhenDisposed;

		#endregion

		#region Ctors

		public TextWriterEx()
			: this(new StringWriter(), false)
		{
		}

		public TextWriterEx(TextWriter writer)
			: this(writer, false)
		{
		}

		public TextWriterEx(TextWriter writer, bool closeInnerWhenDisposed)
		{
			_writer = writer;
			_closeInnerWhenDisposed = closeInnerWhenDisposed;
			_indentString = "\t";
			_applyIndent = true;
		}

		#endregion

		#region Properties

		public int Length
		{
			get { return _length; }
		}

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

		#endregion

		#region Methods

		public TextWriterEx Append(object obj)
		{
			if (obj == null)
				return this;

			return Append(obj.ToString());
		}

		public TextWriterEx Append(string text)
		{
			if (string.IsNullOrEmpty(text))
				return this;

			EnsureIndent();
			Write(text);

			return this;
		}

		public TextWriterEx AppendFormat(object obj, params object[] args)
		{
			if (obj == null)
				return this;

			return AppendFormat(null, obj.ToString(), args);
		}

		public TextWriterEx AppendFormat(string text, params object[] args)
		{
			return AppendFormat(null, text, args);
		}

		public TextWriterEx AppendFormat(IFormatProvider provider, string text, params object[] args)
		{
			if (string.IsNullOrEmpty(text))
				return this;

			EnsureIndent();
			text = string.Format(provider, text, args);
			Write(text);

			return this;
		}

		public TextWriterEx AppendMultiline(object obj)
		{
			if (obj == null)
				return this;

			return AppendMultiline(obj.ToString());
		}

		public TextWriterEx AppendMultiline(string text)
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
					Write(text.Substring(index));
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

					Write(text.Substring(index, length));
					AppendLine();

					index = newLineIndex + 1;
				}
			}

			return this;
		}

		public TextWriterEx AppendMultilineFormat(object obj, params object[] args)
		{
			if (obj == null)
				return this;

			return AppendMultilineFormat(null, obj.ToString(), args);
		}

		public TextWriterEx AppendMultilineFormat(string text, params object[] args)
		{
			return AppendMultilineFormat(null, text, args);
		}

		public TextWriterEx AppendMultilineFormat(IFormatProvider provider, string text, params object[] args)
		{
			text = string.Format(provider, text, args);
			return AppendMultiline(text);
		}

		public TextWriterEx AppendLine()
		{
			Write(Environment.NewLine);
			_applyIndent = true;

			return this;
		}

		public TextWriterEx AppendLines(int numberOfLines)
		{
			for (int i = 0; i < numberOfLines; i++)
			{
				AppendLine();
			}

			return this;
		}

		public override string ToString()
		{
			return _writer.ToString();
		}

		private void Write(string value)
		{
			_writer.Write(value);
			_lastWriteString = value;
			_length += value.Length;
		}

		public TextWriterEx EnsureWhiteSpace()
		{
			if (_length == 0)
				return this;

			char ch = _lastWriteString[_lastWriteString.Length - 1];
			if (char.IsWhiteSpace(ch))
				return this;

			Write(" ");

			return this;
		}

		public TextWriterEx EnsureNewLine()
		{
			if (_length == 0)
				return this;

			char ch = _lastWriteString[_lastWriteString.Length - 1];
			if (ch == '\n')
				return this;

			Write(Environment.NewLine);

			return this;
		}

		private void EnsureIndent()
		{
			if (!_applyIndent)
				return;

			if (_indent > 0 && !string.IsNullOrEmpty(_indentString))
			{
				for (int i = 0; i < _indent; i++)
				{
					Write(_indentString);
				}
			}

			_applyIndent = false;
		}

		public void Dispose()
		{
			if (_writer != null)
			{
				if (_closeInnerWhenDisposed)
				{
					_writer.Dispose();
				}

				_writer = null;
			}
		}

		#endregion

		#region Static

		public static TextWriterEx FromText(StringBuilder sb)
		{
			var inner = new StringWriter(sb);
			return new TextWriterEx(inner, true);
		}

		public static TextWriterEx FromFile(string path)
		{
			var inner = new StreamWriter(path);
			return new TextWriterEx(inner, true);
		}

		public static TextWriterEx FromStream(Stream stream)
		{
			var inner = new StreamWriter(stream);
			return new TextWriterEx(inner, true);
		}

		#endregion
	}
}
