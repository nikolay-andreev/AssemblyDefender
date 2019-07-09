using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in AssemblyProcessor table.
	/// </summary>
	public struct AssemblyProcessorRow : IEquatable<AssemblyProcessorRow>
	{
		public uint Processor;

		public bool Equals(AssemblyProcessorRow other)
		{
			if (Processor != other.Processor)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Processor;
		}
	}
}
