using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Common
{
	public class StringGenerator
	{
		#region Fields

		public static readonly char[] AsciiLetter;
		public static readonly char[] AsciiLetterOrDigit;
		private int _stringLength;
		private int _charSetLength;
		private int[] _pointers;
		private char[] _chars;
		private char[] _charSet;

		#endregion

		#region Ctors

		static StringGenerator()
		{
			var random = new Random();

			AsciiLetter = new char[]
			{
				'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
				'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
			};

			AsciiLetterOrDigit = new char[]
			{
				'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
				'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
				'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4', '5',
			};

			AsciiLetter.Shuffle(random);
			AsciiLetterOrDigit.Shuffle(random);
		}

		public StringGenerator(bool includeDigits = false, int stringLength = 1)
			: this(includeDigits ? AsciiLetterOrDigit : AsciiLetter, stringLength)
		{
		}

		public StringGenerator(char[] charSet, int stringLength = 1)
		{
			if (charSet == null || charSet.Length == 0)
				throw new ArgumentNullException("charSet");

			_charSet = charSet;
			_charSetLength = charSet.Length;
			_stringLength = stringLength;
		}

		#endregion

		#region Methods

		public int StringLength
		{
			get { return _stringLength; }
			set
			{
				if (value == _stringLength)
					return;

				_stringLength = 1;
				_chars = null;
				_pointers = null;
			}
		}

		public string Generate()
		{
			if (_chars == null)
			{
				_chars = new char[_stringLength];
				_pointers = new int[_stringLength];
			}

			// Generate
			for (int i = 0; i < _stringLength; i++)
			{
				_chars[i] = _charSet[_pointers[i]];
			}

			// Create string and add to cache
			string s = new string(_chars);

			// Move to next
			for (int i = _stringLength - 1; i >= 0; i--)
			{
				int pointer = _pointers[i] + 1;
				if (pointer < _charSetLength)
				{
					_pointers[i] = pointer;
					break;
				}

				if (i == 0)
				{
					// Increase string length
					_stringLength++;
					_chars = new char[_stringLength];
					_pointers = new int[_stringLength];
					break;
				}

				_pointers[i] = 0;
			}

			return s;
		}

		public void Reset()
		{
			_chars = null;
			_pointers = null;
		}

		#endregion
	}
}
