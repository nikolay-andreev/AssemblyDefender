using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common.Text
{
	/// <summary>
	/// Defines a range in text.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct BufferSpan
	{
		public static BufferSpan Empty = new BufferSpan();

		/// <summary>
		/// Starting offset whitin the buffer.
		/// </summary>
		public int StartOffset;

		/// <summary>
		/// Ending offset whitin the buffer.
		/// </summary>
		public int EndOffset;

		public BufferSpan(int startOffset, int endOffset)
		{
			this.StartOffset = startOffset;
			this.EndOffset = endOffset;
		}

		public bool IsEmpty()
		{
			return StartOffset == EndOffset;
		}
	}
}
