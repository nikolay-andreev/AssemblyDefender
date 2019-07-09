using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in ENCMap table.
	/// </summary>
	public struct ENCMapRow : IEquatable<ENCMapRow>
	{
		/// <summary>
		/// Token, or like a token, but with (ixTbl|0x80) instead of token type.
		/// </summary>
		public uint Token;

		public bool Equals(ENCMapRow other)
		{
			if (Token != other.Token)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Token;
		}
	}
}
