using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The Heap field of a tables stream header indicates the offset sizes to be used within the heaps.
	/// </summary>
	public static class HeapOffsetFlags
	{
		/// <summary>
		/// 2-byte heap offset.
		/// </summary>
		public const byte None = 0;

		/// <summary>
		/// String heap offset is 4-byte unsigned integer.
		/// </summary>
		public const byte StringHeap4 = 0x01;

		/// <summary>
		/// Guid heap offset is 4-byte unsigned integer.
		/// </summary>
		public const byte GuidHeap4 = 0x02;

		/// <summary>
		/// Blob heap offset is 4-byte unsigned integer.
		/// </summary>
		public const byte BlobHeap4 = 0x04;

		/// <summary>
		/// The stream contains only changes made during an edit-and-continue session
		/// </summary>
		public const byte OnlyEditAndContinue = 0x20;

		/// <summary>
		/// The metadata might contain items marked as deleted.
		/// </summary>
		public const byte MightContainDeleted = 0x80;
	}
}
