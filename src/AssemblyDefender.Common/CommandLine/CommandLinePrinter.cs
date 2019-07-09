using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.CommandLine
{
	public static class CommandLinePrinter
	{
		public static void Print(ArgumentUsage[] usages)
		{
			var lines = PrintLines(usages, Console.WindowWidth);

			foreach (string line in lines)
			{
				Console.WriteLine(line);
			}
		}

		public static IEnumerable<string> PrintLines(ArgumentUsage[] usages, int maxLineLength)
		{
			int namesMaxLength = usages.Max(argUsage => argUsage.Name.Length);

			var lines = new List<string>();

			for (int i = 0; i < usages.Length; i++)
			{
				var usage = usages[i];
				string line = usage.Name;
				line = line.PadRight(namesMaxLength + 2);

				foreach (string wrappedLine in WordWrap(usage.Description, maxLineLength - namesMaxLength - 3))
				{
					line += wrappedLine;
					lines.Add(line);
					line = new string(' ', namesMaxLength + 2);
				}
			}

			return lines;
		}

		/// <summary>
		/// Word wrap text for a specified maximum line length.
		/// </summary>
		/// <param name="text">Text to word wrap.</param>
		/// <param name="maxLineLength">Maximum length of a line.</param>
		/// <returns>Collection of lines for the word wrapped text.</returns>
		private static IEnumerable<string> WordWrap(string text, int maxLineLength)
		{
			var lines = new List<string>();
			string currentLine = string.Empty;

			foreach (string word in text.Split(' '))
			{
				// Whenever adding the word would push us over the maximum
				// width, add the current line to the lines collection and
				// begin a new line. The new line starts with space padding
				// it to be left aligned with the previous line of text from
				// this column.
				if (currentLine.Length + word.Length > maxLineLength)
				{
					lines.Add(currentLine);
					currentLine = string.Empty;
				}

				currentLine += word;

				// Add spaces between words except for when we are at exactly the
				// maximum width.
				if (currentLine.Length != maxLineLength)
				{
					currentLine += " ";
				}
			}

			// Add the remainder of the current line except for when it is
			// empty, which is true in the case when we had just started a
			// new line.
			if (currentLine.Trim() != string.Empty)
			{
				lines.Add(currentLine);
			}

			return lines;
		}
	}
}
