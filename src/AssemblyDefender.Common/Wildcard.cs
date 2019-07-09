using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace AssemblyDefender.Common
{
	public class Wildcard
	{
		#region Fields

		private string _pattern;
		private bool _caseInsensitive;
		private WildcardElement _firstElement;

		#endregion

		#region Ctors

		public Wildcard()
		{
		}

		public Wildcard(string pattern)
			: this(pattern, false)
		{
		}

		public Wildcard(string pattern, bool caseInsensitive)
		{
			_pattern = pattern ?? string.Empty;
			_caseInsensitive = caseInsensitive;
		}

		#endregion

		#region Properties

		public string Pattern
		{
			get { return _pattern; }
			set
			{
				_pattern = value ?? string.Empty;
				_firstElement = null;
			}
		}

		public bool CaseInsensitive
		{
			get { return _caseInsensitive; }
			set
			{
				_caseInsensitive = value;
				_firstElement = null;
			}
		}

		#endregion

		#region Methods

		public bool IsMatch(string value)
		{
			if (_pattern.Length == 0)
				return string.IsNullOrEmpty(value);

			if (value == null)
				value = string.Empty;

			if (_caseInsensitive)
				value = value.ToLower();

			if (_firstElement == null)
				_firstElement = Build();

			int valueIndex = 0;
			if (!_firstElement.IsMatch(value, ref valueIndex))
				return false;

			return valueIndex == value.Length;
		}

		private WildcardElement Build()
		{
			string pattern = _pattern;

			if (_caseInsensitive)
				pattern = pattern.ToLower();

			int patternIndex = 0;
			return WildcardElement.Create(pattern, ref patternIndex);
		}

		#endregion

		#region Nested types

		private abstract class WildcardElement
		{
			public WildcardElement Next;

			public abstract bool StartsWith(string value, int valueIndex);

			public abstract bool IsMatch(string value, ref int valueIndex);

			internal static WildcardElement Create(string pattern, ref int patternIndex)
			{
				if (patternIndex >= pattern.Length)
					return null;

				switch (pattern[patternIndex])
				{
					case '*':
						return WildcardAsteriskElement.Create(pattern, ref patternIndex);

					case '?':
						return WildcardQuestionElement.Create(pattern, ref patternIndex);

					case '#':
						return WildcardSharpElement.Create(pattern, ref patternIndex);

					case '[':
						{
							int startIndex = patternIndex;
							WildcardElement element = WildcardRangeElement.Create(pattern, ref patternIndex);
							if (element == null)
							{
								patternIndex = startIndex;
								element = WildcardStringElement.Create(pattern, ref patternIndex);
							}

							return element;
						}

					default:
						return WildcardStringElement.Create(pattern, ref patternIndex);
				}
			}
		}

		/// <summary>
		/// Represents string.
		/// </summary>
		private unsafe class WildcardStringElement : WildcardElement
		{
			public string Pattern;

			public override bool StartsWith(string value, int valueIndex)
			{
				if (valueIndex >= value.Length)
					return false;

				return value[valueIndex] == Pattern[0];
			}

			public override bool IsMatch(string value, ref int valueIndex)
			{
				int patternIndex = 0;
				int patternLength = Pattern.Length;
				int valueLength = value.Length;
				while (patternIndex < patternLength)
				{
					if (valueIndex >= valueLength)
						return false;

					if (value[valueIndex++] != Pattern[patternIndex++])
						return false;
				}

				if (Next != null)
					return Next.IsMatch(value, ref valueIndex);

				return true;
			}

			internal new static WildcardStringElement Create(string pattern, ref int patternIndex)
			{
				int patternLength = pattern.Length;
				var builder = new StringBuilder(patternLength - patternLength);
				while (patternIndex < patternLength)
				{
					switch (pattern[patternIndex])
					{
						case '\\':
							{
								patternIndex++;

								if (patternIndex < patternLength)
								{
									builder.Append(pattern[patternIndex++]);
								}
							}
							continue;

						case '*':
						case '?':
						case '#':
						case '[':
							break;

						default:
							{
								builder.Append(pattern[patternIndex++]);
							}
							continue;
					}

					break;
				}

				var element = new WildcardStringElement();
				element.Pattern = builder.ToString();
				element.Next = WildcardElement.Create(pattern, ref patternIndex);

				return element;
			}
		}

		/// <summary>
		/// Represents zero or more of any character.
		/// </summary>
		private unsafe class WildcardAsteriskElement : WildcardElement
		{
			public override bool StartsWith(string value, int valueIndex)
			{
				return true;
			}

			public override bool IsMatch(string value, ref int valueIndex)
			{
				if (Next == null)
				{
					// This is the last element.
					valueIndex = value.Length;
					return true;
				}

				int valueLength = value.Length;
				while (valueIndex < valueLength)
				{
					if (Next.StartsWith(value, valueIndex))
						break;

					valueIndex++;
				}

				return Next.IsMatch(value, ref valueIndex);
			}

			internal new static WildcardAsteriskElement Create(string pattern, ref int patternIndex)
			{
				patternIndex++;

				var element = new WildcardAsteriskElement();
				element.Next = WildcardElement.Create(pattern, ref patternIndex);

				return element;
			}
		}

		/// <summary>
		/// Represents any single character.
		/// </summary>
		private unsafe class WildcardQuestionElement : WildcardElement
		{
			public override bool StartsWith(string value, int valueIndex)
			{
				return valueIndex < value.Length;
			}

			public override bool IsMatch(string value, ref int valueIndex)
			{
				if (valueIndex >= value.Length)
					return false;

				valueIndex++;

				if (Next != null)
					return Next.IsMatch(value, ref valueIndex);

				return true;
			}

			internal new static WildcardQuestionElement Create(string pattern, ref int patternIndex)
			{
				patternIndex++;

				var element = new WildcardQuestionElement();
				element.Next = WildcardElement.Create(pattern, ref patternIndex);

				return element;
			}
		}

		/// <summary>
		/// Represents any single digit.
		/// </summary>
		private unsafe class WildcardSharpElement : WildcardElement
		{
			public override bool StartsWith(string value, int valueIndex)
			{
				return valueIndex < value.Length && char.IsDigit(value[valueIndex]);
			}

			public override bool IsMatch(string value, ref int valueIndex)
			{
				if (valueIndex >= value.Length)
					return false;

				if (!char.IsDigit(value[valueIndex++]))
					return false;

				if (Next != null)
					return Next.IsMatch(value, ref valueIndex);

				return true;
			}

			internal new static WildcardSharpElement Create(string pattern, ref int patternIndex)
			{
				patternIndex++;

				var element = new WildcardSharpElement();
				element.Next = WildcardElement.Create(pattern, ref patternIndex);

				return element;
			}
		}

		/// <summary>
		/// Represents any one character in the set.
		/// </summary>
		/// <example>
		/// [a-f] - letter from a through f.
		/// [0-9] - digits from 0 to 9.
		/// [a-cst] - letter from a through c, s or t.
		/// [a-cx-z] - letter from a through c, x through z.
		/// [!f-t] - NOT letter from f through t.
		/// </example>
		private unsafe class WildcardRangeElement : WildcardElement
		{
			public bool Negate;
			public HashSet<char> Chars;

			public override bool StartsWith(string value, int valueIndex)
			{
				if (valueIndex >= value.Length)
					return false;

				return (Negate && !Chars.Contains(value[valueIndex])) || Chars.Contains(value[valueIndex]);
			}

			public override bool IsMatch(string value, ref int valueIndex)
			{
				int valueLength = value.Length;
				while (valueIndex < valueLength)
				{
					if ((Negate && Chars.Contains(value[valueIndex])) || !Chars.Contains(value[valueIndex]))
						break;

					valueIndex++;
				}

				if (Next != null)
					return Next.IsMatch(value, ref valueIndex);

				return true;
			}

			internal new static WildcardRangeElement Create(string pattern, ref int patternIndex)
			{
				patternIndex++;
				int patternLength = pattern.Length;

				bool negate = false;
				var chars = new HashSet<char>();

				if (patternIndex < patternLength && pattern[patternIndex] == '!')
				{
					patternIndex++;
					negate = true;
				}

				bool closed = false;
				while (patternIndex < patternLength)
				{
					char ch = pattern[patternIndex];
					switch (ch)
					{
						case ']':
							{
								patternIndex++;
								closed = true;
							}
							break;

						case '\\':
							{
								patternIndex++;

								if (patternIndex < patternLength)
								{
									ch = pattern[patternIndex++];
									if (!chars.Contains(ch))
										chars.Add(ch);
								}
							}
							continue;

						default:
							{
								if (char.IsLetter(ch))
								{
									patternIndex++;

									if (patternIndex + 1 < patternLength &&
										pattern[patternIndex] == '-' &&
										char.IsLetter(pattern[patternIndex + 1]))
									{
										patternIndex++; // -
										char fromChar = ch;
										char toChar = pattern[patternIndex++];
										for (char c = fromChar; c <= toChar; c++)
										{
											if (!chars.Contains(c))
												chars.Add(c);
										}
									}
									else
									{
										if (!chars.Contains(ch))
											chars.Add(ch);
									}
								}
								else if (char.IsDigit(ch))
								{
									patternIndex++;

									if (patternIndex + 1 < patternLength &&
										pattern[patternIndex] == '-' &&
										char.IsDigit(pattern[patternIndex + 1]))
									{
										patternIndex++; // -
										char fromChar = ch;
										char toChar = pattern[patternIndex++];
										for (char c = fromChar; c <= toChar; c++)
										{
											if (!chars.Contains(c))
												chars.Add(c);
										}
									}
									else
									{
										if (!chars.Contains(ch))
											chars.Add(ch);
									}
								}
								else
								{
									patternIndex++;

									if (!chars.Contains(ch))
										chars.Add(ch);
								}
							}
							continue;
					}

					break;
				}

				if (!closed || chars.Count == 0)
					return null;

				var element = new WildcardRangeElement();
				element.Negate = negate;
				element.Chars = chars;
				element.Next = WildcardElement.Create(pattern, ref patternIndex);

				return element;
			}
		}

		#endregion
	}
}
