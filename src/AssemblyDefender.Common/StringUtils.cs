using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AssemblyDefender.Common
{
	public static class StringUtils
	{
		public static readonly char[] WhitespaceChars = new char[]
		{
			' ', '\t', '\n', '\v', '\f', '\r',
			'\x0085', '\x00a0', '\u2028', '\u2029'
		};

		/// <summary>
		/// An implementation of the Contains member of string that takes in a
		/// string comparison. The traditional .NET string Contains member uses
		/// StringComparison.Ordinal.
		/// </summary>
		/// <param name="s">The string.</param>
		/// <param name="value">The string value to search for.</param>
		/// <param name="comparison">The string comparison type.</param>
		/// <returns>Returns true when the substring is found.</returns>
		public static bool Contains(this string s, string value, StringComparison comparison)
		{
			return s.IndexOf(value, comparison) >= 0;
		}

		public static string RemoveChars(this string value)
		{
			return RemoveChars(value, WhitespaceChars);
		}

		public static string RemoveChars(this string value, params char[] trimChars)
		{
			var builder = new StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				bool trim = false;
				for (int j = 0; j < trimChars.Length; j++)
				{
					if (trimChars[j] == c)
					{
						trim = true;
						break;
					}
				}

				if (!trim)
				{
					builder.Append(c);
				}
			}

			return builder.ToString();
		}

		public static string AppendLine(this string value)
		{
			return value + Environment.NewLine;
		}

		public static string Copy(this string value, int count)
		{
			var builder = new StringBuilder();

			for (int i = 0; i < count; i++)
				builder.Append(value);

			return builder.ToString();
		}

		public static string[] Split(this string value, string separator, bool removeEmptyEntries = false)
		{
			return value.Split(
				new string[] { separator },
				removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
		}

		public static string[] Split(this string value, char separator, bool removeEmptyEntries = false)
		{
			return value.Split(
				new char[] { separator },
				removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
		}

		public static string[] SplitLines(this string value)
		{
			var lines = new List<string>();

			int index = 0;
			int startIndex = 0;
			while (true)
			{
				if (index >= value.Length)
				{
					if (startIndex < index)
					{
						lines.Add(value.Substring(startIndex, index - startIndex));
					}

					break;
				}

				char ch = value[index];
				if (ch == '\r')
				{
					index++;
					if (index < value.Length && value[index] == '\n')
					{
						lines.Add(value.Substring(startIndex, index - startIndex - 1));
						index++;
						startIndex = index;
					}
					else
					{
						lines.Add(value.Substring(startIndex, index - startIndex - 1));
						startIndex = index;
					}

					continue;
				}

				if (ch == '\n')
				{
					lines.Add(value.Substring(startIndex, index - startIndex - 1));
					index++;
					startIndex = index;
					continue;
				}

				index++;
			}

			return lines.ToArray();
		}

		public static string[] SplitLines(this string value, bool keepNewLine)
		{
			if (!keepNewLine)
				return SplitLines(value);

			var lines = new List<string>();

			int index = 0;
			int startIndex = 0;
			while (true)
			{
				if (index >= value.Length)
				{
					if (startIndex < index)
					{
						lines.Add(value.Substring(startIndex, index - startIndex));
					}

					break;
				}

				char ch = value[index];
				if (ch == '\r')
				{
					index++;
					if (index < value.Length && value[index] == '\n')
					{
						index++;
						lines.Add(value.Substring(startIndex, index - startIndex));
						startIndex = index;
					}
					else
					{
						lines.Add(value.Substring(startIndex, index - startIndex));
						startIndex = index;
					}

					continue;
				}

				if (ch == '\n')
				{
					index++;
					lines.Add(value.Substring(startIndex, index - startIndex));
					startIndex = index;
					continue;
				}

				index++;
			}

			return lines.ToArray();
		}

		public static string Escape(this string value, char[] charsToEscape, char escapeChar)
		{
			var builder = new StringBuilder();
			int length = value.Length;
			int pos = 0;
			while (pos < length)
			{
				char ch = value[pos++];
				for (int i = 0; i < charsToEscape.Length; i++)
				{
					if (ch == charsToEscape[i])
					{
						builder.Append(escapeChar);
						break;
					}
				}

				builder.Append(ch);
			}

			return builder.ToString();
		}

		public static string Surround(this string value, string surroudString)
		{
			return surroudString + value + surroudString;
		}

		public static bool IsQuateSurrounded(this string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;

			if (value.Length < 2)
				return false;

			if (value[0] != '\'' || value[value.Length - 1] != '\'')
				return false;

			return true;
		}

		public static string QuateSurround(this string value)
		{
			return '\'' + value + '\'';
		}

		public static string QuateUnsurround(this string value)
		{
			if (IsQuateSurrounded(value))
			{
				value = value.Substring(1, value.Length - 2);
			}

			return value;
		}

		public static bool IsDoubleQuateSurrounded(this string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;

			if (value.Length < 2)
				return false;

			if (value[0] != '"' || value[value.Length - 1] != '"')
				return false;

			return true;
		}

		public static string DoubleQuateSurround(this string value)
		{
			return '"' + value + '"';
		}

		public static string DoubleQuateUnsurround(this string value)
		{
			if (IsDoubleQuateSurrounded(value))
			{
				value = value.Substring(1, value.Length - 2);
			}

			return value;
		}

		/// <summary>
		/// Returns a hash code based on the string content. Strings that differ only in case will always have the same hash code.
		/// </summary>
		/// <param name="value">The string to hash.</param>
		public static int CaseInsensitiveStringHash(this string value)
		{
			int hashCode = 0;
			for (int i = 0, n = value.Length; i < n; i++)
			{
				char ch = value[i];
				ch = Char.ToLower(ch, CultureInfo.InvariantCulture);
				hashCode = hashCode * 17 + ch;
			}
			return hashCode;
		}

		/// <summary>
		/// Returns a hash code based on the string content. Strings that differ only in case will always have the same hash code.
		/// </summary>
		/// <param name="value">The string to hash.</param>
		public static int CaseSensitiveStringHash(this string value)
		{
			int hashCode = 0;
			for (int i = 0, n = value.Length; i < n; i++)
			{
				char ch = value[i];
				hashCode = hashCode * 17 + ch;
			}
			return hashCode;
		}

		public static string NullIfEmpty(this string value)
		{
			if (value == null)
				return null;

			if (value.Length == 0)
				return null;

			return value;
		}

		public static string NullIfEmptyTrimmed(this string value)
		{
			if (value == null)
				return null;

			if (value.Trim().Length == 0)
				return null;

			return value;
		}

		public static string EmptyIfNull(this string value)
		{
			return value ?? string.Empty;
		}
	}
}
