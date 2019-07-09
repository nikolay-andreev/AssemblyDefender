using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common.Text
{
	/// <summary>
	/// Defines a text address in terms of a character and a line.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct TextPoint
	{
		public static TextPoint Null = new TextPoint(-1, -1, -1);

		public int Offset;
		public int Line;
		public int Column;

		public TextPoint(int offset, int line, int column)
		{
			this.Offset = offset;
			this.Line = line;
			this.Column = column;
		}

		public void Clear()
		{
			this.Offset = 0;
			this.Line = 0;
			this.Column = 0;
		}

		public bool IsNull()
		{
			return this.Offset < 0;
		}

		public bool IsEqual(TextPoint address)
		{
			return this.Offset == address.Offset;
		}

		public override string ToString()
		{
			return string.Format("Ln {1} Col {2} At {0}", this.Offset, this.Line, this.Column);
		}
	}
}
