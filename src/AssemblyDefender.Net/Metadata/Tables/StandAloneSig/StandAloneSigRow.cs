using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in StandAloneSig table.
	/// </summary>
	public struct StandAloneSigRow : IEquatable<StandAloneSigRow>
	{
		/// <summary>
		/// Offset in the #Blob stream.
		/// </summary>
		public int Signature;

		public bool Equals(StandAloneSigRow other)
		{
			if (Signature != other.Signature)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Signature;
		}
	}
}
