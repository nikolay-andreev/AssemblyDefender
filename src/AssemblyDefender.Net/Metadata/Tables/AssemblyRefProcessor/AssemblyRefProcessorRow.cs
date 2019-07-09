using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in AssemblyRefProcessor table.
	/// </summary>
	public struct AssemblyRefProcessorRow : IEquatable<AssemblyRefProcessorRow>
	{
		/// <summary>
		/// 4-byte constant.
		/// </summary>
		public uint Processor;

		/// <summary>
		/// An index into the AssemblyRef table
		/// </summary>
		public int AssemblyRef;

		public bool Equals(AssemblyRefProcessorRow other)
		{
			if (Processor != other.Processor)
				return false;

			if (AssemblyRef != other.AssemblyRef)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Processor ^
				AssemblyRef;
		}
	}
}
