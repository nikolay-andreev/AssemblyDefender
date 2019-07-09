using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in NestedClass table.
	/// </summary>
	public struct NestedClassRow : IEquatable<NestedClassRow>
	{
		/// <summary>
		/// An index of the nested type (the nestee).
		/// RID in the TypeDef table.
		/// </summary>
		public int NestedClass;

		/// <summary>
		/// An index of the type in which the current type is nested (the encloser, or nester).
		/// RID in the TypeDef table.
		/// </summary>
		public int EnclosingClass;

		public bool Equals(NestedClassRow other)
		{
			if (NestedClass != other.NestedClass)
				return false;

			if (EnclosingClass != other.EnclosingClass)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				NestedClass ^
				EnclosingClass;
		}
	}
}
