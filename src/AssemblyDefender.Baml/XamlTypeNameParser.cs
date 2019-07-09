using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml.Parser
{
	/// <summary>
	/// Prefix:Name(Generic)[Subscript]
	/// </summary>
	internal class XamlTypeNameParser
	{
		#region Fields

		private XamlTypeNameLexer _lexer;
		private Stack<XamlTypeName> _stack;

		#endregion

		#region Ctors

		private XamlTypeNameParser(string inputText)
		{
			_lexer = new XamlTypeNameLexer(inputText);
			_stack = new Stack<XamlTypeName>();
			_stack.Push(new XamlTypeName());
		}

		#endregion

		#region Methods

		private XamlTypeName GetName()
		{
			if (_stack.Count != 1)
				return null;

			var frame = _stack.Peek();
			if (frame.TypeArguments.Count != 1)
				return null;

			return frame.TypeArguments[0];
		}

		private IList<XamlTypeName> GetNameList()
		{
			if (_stack.Count == 0)
				return null;

			return _stack.Peek().TypeArguments;
		}

		private bool Parse()
		{
			_lexer.Read();

			if (!P_XamlTypeName())
				return false;

			if (_lexer.Token != XamlTypeNameLexerToken.NONE)
				return false;

			return true;
		}

		private bool ParseList()
		{
			_lexer.Read();

			if (!P_XamlTypeNameList())
				return false;

			if (_lexer.Token != XamlTypeNameLexerToken.NONE)
				return false;

			return true;
		}

		private void Callout_EndOfType()
		{
			var item = _stack.Pop();
			var frame = _stack.Peek();
			frame.TypeArguments.Add(item);
		}

		private bool P_XamlTypeName()
		{
			if (_lexer.Token != XamlTypeNameLexerToken.NAME)
				return false;

			P_SimpleTypeName();

			if (_lexer.Token == XamlTypeNameLexerToken.ROUND_BRACKET_OPEN)
			{
				if (!P_TypeParameters())
					return false;
			}

			if (_lexer.Token == XamlTypeNameLexerToken.SQUARE_BRACKET_OPEN)
			{
				if (!P_RepeatingSubscript())
					return false;
			}

			Callout_EndOfType();

			return true;
		}

		private bool P_XamlTypeNameList()
		{
			if (!P_XamlTypeName())
				return false;

			while (_lexer.Token == XamlTypeNameLexerToken.COMMA)
			{
				if (!P_NameListExt())
					return false;
			}

			return true;
		}

		private bool P_NameListExt()
		{
			_lexer.Read();

			if (!P_XamlTypeName())
				return false;

			return true;
		}

		private bool P_RepeatingSubscript()
		{
			_lexer.Read();

			int subscript = 1;

			while (_lexer.Token == XamlTypeNameLexerToken.COMMA)
			{
				subscript++;
				_lexer.Read();
			}

			if (_lexer.Token != XamlTypeNameLexerToken.SQUARE_BRACKET_CLOSE)
				return false;

			_lexer.Read();

			var item = _stack.Peek();
			item.Subscript = subscript;

			return true;
		}

		private bool P_SimpleTypeName()
		{
			string prefix = null;
			string multiCharTokenText = _lexer.MultiCharTokenText;

			_lexer.Read();

			if (_lexer.Token == XamlTypeNameLexerToken.COLON)
			{
				prefix = multiCharTokenText;

				_lexer.Read();

				if (_lexer.Token != XamlTypeNameLexerToken.NAME)
					return false;

				multiCharTokenText = _lexer.MultiCharTokenText;

				_lexer.Read();
			}

			_stack.Push(new XamlTypeName(multiCharTokenText, prefix));

			return true;
		}

		private bool P_TypeParameters()
		{
			_lexer.Read();

			if (!P_XamlTypeNameList())
				return false;

			if (_lexer.Token != XamlTypeNameLexerToken.ROUND_BRACKET_CLOSE)
				return false;

			_lexer.Read();

			return true;
		}

		#endregion

		#region Static

		internal static XamlTypeName Parse(string text)
		{
			var typeName = ParseIfTrivalName(text);
			if (typeName != null)
				return typeName;

			var parser = new XamlTypeNameParser(text);
			if (!parser.Parse())
				return null;

			return parser.GetName();
		}

		internal static IList<XamlTypeName> ParseList(string text)
		{
			var parser = new XamlTypeNameParser(text);
			if (!parser.ParseList())
				return null;

			return parser.GetNameList();
		}

		private static XamlTypeName ParseIfTrivalName(string text)
		{
			int index = text.IndexOf('(');
			int index2 = text.IndexOf('[');
			if (index != -1 || index2 != -1)
				return null;

			string prefix;
			string name;
			if (!ParseQualifiedName(text, out prefix, out name))
				return null;

			return new XamlTypeName(name, prefix);
		}

		private static bool ParseQualifiedName(string text, out string prefix, out string name)
		{
			int startIndex = 0;
			int index = text.IndexOf(':');

			prefix = string.Empty;
			name = string.Empty;

			if (index != -1)
			{
				prefix = text.Substring(startIndex, index).Trim();
				if (prefix.Length == 0 || !XamlParseUtils.IsNameValid(prefix))
					return false;

				startIndex = index + 1;
			}

			name = (startIndex == 0) ? text : text.Substring(startIndex).Trim();
			if (name.Length == 0)
				return false;

			if (!XamlParseUtils.IsNameValid_WithPlus(name))
				return false;

			return true;
		}

		#endregion
	}
}

