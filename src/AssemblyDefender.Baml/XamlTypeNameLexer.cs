using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml.Parser
{
	internal class XamlTypeNameLexer
	{
		#region Fields

		private const char CloseBracket = ']';
		private const char CloseParen = ')';
		private const char Colon = ':';
		private const char Comma = ',';
		private const char OpenBracket = '[';
		private const char OpenParen = '(';
		private const char Space = ' ';
		private const char NullChar = '\0';
		private char _lastChar;
		private int _idx;
		private int _multiCharTokenLength;
		private int _multiCharTokenStartIdx;
		private string _inputText;
		private string _tokenText;
		private State _state;
		private XamlTypeNameLexerToken _token;
		private XamlTypeNameLexerToken _pushedBackSymbol;

		#endregion

		#region Ctors

		public XamlTypeNameLexer(string text)
		{
			_inputText = text;
			_state = State.START;
			_pushedBackSymbol = XamlTypeNameLexerToken.NONE;
		}

		#endregion

		#region Properties

		internal char ErrorCurrentChar
		{
			get { return _lastChar; }
		}

		internal string MultiCharTokenText
		{
			get { return _tokenText; }
		}

		internal bool IsAtEndOfInput
		{
			get { return _idx >= _inputText.Length; }
		}

		internal XamlTypeNameLexerToken Token
		{
			get { return _token; }
		}

		private char CurrentChar
		{
			get { return _inputText[_idx]; }
		}

		#endregion

		#region Methods

		internal void Read()
		{
			if (_pushedBackSymbol != XamlTypeNameLexerToken.NONE)
			{
				_token = _pushedBackSymbol;
				_pushedBackSymbol = XamlTypeNameLexerToken.NONE;
			}
			else
			{
				_token = XamlTypeNameLexerToken.NONE;
				_tokenText = string.Empty;
				_multiCharTokenStartIdx = -1;
				_multiCharTokenLength = 0;

				while (_token == XamlTypeNameLexerToken.NONE)
				{
					if (IsAtEndOfInput)
					{
						if (_state == State.INNAME)
						{
							_token = XamlTypeNameLexerToken.NAME;
							_state = State.START;
						}

						break;
					}

					switch (_state)
					{
						case State.START:
							State_Start();
							break;

						case State.INNAME:
							State_InName();
							break;
					}
				}

				if (_multiCharTokenLength > 0)
				{
					_tokenText = CollectMultiCharToken();
				}
			}
		}

		private void AddToMultiCharToken()
		{
			_multiCharTokenLength++;
		}

		private string CollectMultiCharToken()
		{
			if ((_multiCharTokenStartIdx == 0) && (_multiCharTokenLength == _inputText.Length))
			{
				return _inputText;
			}

			return _inputText.Substring(_multiCharTokenStartIdx, _multiCharTokenLength);
		}

		private void StartMultiCharToken()
		{
			_multiCharTokenStartIdx = _idx;
			_multiCharTokenLength = 1;
		}

		private void State_Start()
		{
			AdvanceOverWhitespace();

			if (IsAtEndOfInput)
			{
				_token = XamlTypeNameLexerToken.NONE;
			}
			else
			{
				switch (CurrentChar)
				{
					case '(':
						_token = XamlTypeNameLexerToken.ROUND_BRACKET_OPEN;
						break;

					case ')':
						_token = XamlTypeNameLexerToken.ROUND_BRACKET_CLOSE;
						break;

					case '[':
						_token = XamlTypeNameLexerToken.SQUARE_BRACKET_OPEN;
						break;

					case ']':
						_token = XamlTypeNameLexerToken.SQUARE_BRACKET_CLOSE;
						break;

					case ',':
						_token = XamlTypeNameLexerToken.COMMA;
						break;

					case ':':
						_token = XamlTypeNameLexerToken.COLON;
						break;

					default:
						if (XamlParseUtils.IsValidNameStartChar(CurrentChar))
						{
							StartMultiCharToken();
							_state = State.INNAME;
						}
						else
						{
							_token = XamlTypeNameLexerToken.ERROR;
						}
						break;
				}

				_lastChar = CurrentChar;
				Advance();
			}
		}

		private void State_InName()
		{
			if ((IsAtEndOfInput || XamlParseUtils.IsWhitespaceChar(CurrentChar)) || (CurrentChar == '['))
			{
				_token = XamlTypeNameLexerToken.NAME;
				_state = State.START;
			}
			else
			{
				switch (CurrentChar)
				{
					case '(':
						_pushedBackSymbol = XamlTypeNameLexerToken.ROUND_BRACKET_OPEN;
						_token = XamlTypeNameLexerToken.NAME;
						_state = State.START;
						break;

					case ')':
						_pushedBackSymbol = XamlTypeNameLexerToken.ROUND_BRACKET_CLOSE;
						_token = XamlTypeNameLexerToken.NAME;
						_state = State.START;
						break;

					case ',':
						_pushedBackSymbol = XamlTypeNameLexerToken.COMMA;
						_token = XamlTypeNameLexerToken.NAME;
						_state = State.START;
						break;

					case ':':
						_pushedBackSymbol = XamlTypeNameLexerToken.COLON;
						_token = XamlTypeNameLexerToken.NAME;
						_state = State.START;
						break;

					default:
						if (XamlParseUtils.IsValidQualifiedNameChar(CurrentChar))
						{
							AddToMultiCharToken();
						}
						else
						{
							_token = XamlTypeNameLexerToken.ERROR;
						}
						break;
				}

				_lastChar = CurrentChar;
				Advance();
			}
		}

		private bool Advance()
		{
			_idx++;
			if (IsAtEndOfInput)
			{
				_idx = _inputText.Length;
				return false;
			}

			return true;
		}

		private bool AdvanceOverWhitespace()
		{
			bool flag = true;
			while (!IsAtEndOfInput && XamlParseUtils.IsWhitespaceChar(CurrentChar))
			{
				flag = true;
				Advance();
			}

			return flag;
		}

		#endregion

		#region Nested types

		private enum State
		{
			START,
			INNAME,
		}

		#endregion
	}
}

