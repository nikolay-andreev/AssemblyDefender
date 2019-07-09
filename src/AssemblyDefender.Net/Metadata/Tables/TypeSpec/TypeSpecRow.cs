using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in TypeSpec table.
	/// </summary>
	public struct TypeSpecRow : IEquatable<TypeSpecRow>
	{
		/// <summary>
		/// Represent the signature of the constructed type.
		/// Offset in the #Blob stream.
		/// </summary>
		public int Signature;

		public bool Equals(TypeSpecRow other)
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
