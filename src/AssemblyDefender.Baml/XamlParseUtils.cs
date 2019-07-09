using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace AssemblyDefender.Baml.Parser
{
	internal static class XamlParseUtils
	{
		internal static readonly char[] WhitespaceChars = new char[] { ' ', '\t', '\n', '\r', '\f' };

		internal static bool IsWhitespaceChar(char ch)
		{
			return
				ch == WhitespaceChars[0] ||
				ch == WhitespaceChars[1] ||
				ch == WhitespaceChars[2] ||
				ch == WhitespaceChars[3] ||
				ch == WhitespaceChars[4];
		}

		internal static bool IsNameValid(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;

			if (!IsValidNameStartChar(name[0]))
				return false;

			for (int i = 1; i < name.Length; i++)
			{
				if (!IsValidQualifiedNameChar(name[i]))
					return false;
			}

			return true;
		}

		internal static bool IsNameValid_WithPlus(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;

			if (!IsValidNameStartChar(name[0]))
				return false;

			for (int i = 1; i < name.Length; i++)
			{
				if (!IsValidQualifiedNameCharPlus(name[i]))
					return false;
			}

			return true;
		}

		internal static bool IsValidNameStartChar(char ch)
		{
			return
				ch == '_' ||
				char.IsLetter(ch);
		}

		internal static bool IsValidNameChar(char ch)
		{
			if (!IsValidNameStartChar(ch) && !char.IsDigit(ch))
			{
				var unicodeCategory = char.GetUnicodeCategory(ch);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark && unicodeCategory != UnicodeCategory.SpacingCombiningMark)
					return false;
			}

			return true;
		}

		internal static bool IsValidQualifiedNameChar(char ch)
		{
			return
				ch == '.' ||
				IsValidNameChar(ch);
		}

		internal static bool IsValidQualifiedNameCharPlus(char ch)
		{
			return
				ch == '+' ||
				IsValidQualifiedNameChar(ch);
		}
	}
}
