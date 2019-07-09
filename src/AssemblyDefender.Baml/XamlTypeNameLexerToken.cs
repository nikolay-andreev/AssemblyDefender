using System;

namespace AssemblyDefender.Baml.Parser
{
	internal enum XamlTypeNameLexerToken
	{
		NONE,
		ERROR,
		ROUND_BRACKET_OPEN,
		ROUND_BRACKET_CLOSE,
		SQUARE_BRACKET_OPEN,
		SQUARE_BRACKET_CLOSE,
		COLON,
		COMMA,
		NAME
	}
}

