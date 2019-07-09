using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Common
{
	public static class RandomUtils
	{
		/// <summary>
		/// Fills the elements of a specified array of bytes with random numbers
		/// </summary>
		/// <param name="random">Random number generator.</param>
		/// <param name="size">Size of the buffer.</param>
		/// <returns>A buffer filled with random bytes.</returns>
		public static byte[] NextBytes(this Random random, int size)
		{
			byte[] buffer = new byte[size];
			random.NextBytes(buffer);

			return buffer;
		}

		/// <summary>
		/// Generates a random string with the given length.
		/// </summary>
		/// <param name="random">Random number generator.</param>
		/// <param name="size">Size of the string.</param>
		/// <param name="includeDigits">If true, include digits in generated string.</param>
		/// <returns>Random string.</returns>
		public static string NextString(this Random random, int size, bool includeDigits = false)
		{
			char[] chars = new char[size];
			char[] charSet = includeDigits ? StringGenerator.AsciiLetterOrDigit : StringGenerator.AsciiLetter;
			int charSetLength = charSet.Length;

			for (int i = 0; i < size; i++)
			{
				chars[i] = charSet[random.Next(0, charSetLength)];
			}

			return new string(chars);
		}

		public static string NextUniqueString(this Random random, HashList<string> hash, int size, bool includeDigits = false)
		{
			char[] chars = new char[size];
			char[] charSet = includeDigits ? StringGenerator.AsciiLetterOrDigit : StringGenerator.AsciiLetter;
			int charSetLength = charSet.Length;
			int count = 0;
			string s;
			do
			{
				if (count > 20)
				{
					count = 0;
					size++;
					chars = new char[size];
				}

				for (int i = 0; i < size; i++)
				{
					chars[i] = charSet[random.Next(0, charSetLength)];
				}

				s = new string(chars);
				count++;
			}
			while (hash.Contains(s));

			return s;
		}

		/// <summary>
		/// Returns a random boolean value.
		/// </summary>
		/// <param name="random">Random number generator.</param>
		/// <returns>Random boolean value.</returns>
		public static bool NextBool(this Random random)
		{
			return (random.NextDouble() > 0.5);
		}
	}
}
