using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in ENCLog table.
	/// </summary>
	public struct ENCLogRow : IEquatable<ENCLogRow>
	{
		/// <summary>
		/// Token, or like a token, but with (ixTbl|0x80) instead of token type.
		/// </summary>
		public uint Token;

		/// <summary>
		/// Function code describing the nature of ENC change.
		/// </summary>
		public uint FuncCode;

		public bool Equals(ENCLogRow other)
		{
			if (Token != other.Token)
				return false;

			if (FuncCode != other.FuncCode)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Token ^
				(int)FuncCode;
		}
	}
}
