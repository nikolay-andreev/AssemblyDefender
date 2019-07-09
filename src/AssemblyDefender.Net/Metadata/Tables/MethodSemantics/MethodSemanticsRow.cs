using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in MethodSemantics table.
	/// </summary>
	public struct MethodSemanticsRow : IEquatable<MethodSemanticsRow>
	{
		/// <summary>
		/// The kind of method association.
		/// 2-byte unsigned integer.
		/// </summary>
		public ushort Semantic;

		/// <summary>
		/// The index of the associated method.
		/// RID in the Method table.
		/// </summary>
		public int Method;

		/// <summary>
		/// A token indexing an event or a property the method is associated with.
		/// Coded token of type HasSemantics.
		/// </summary>
		public int Association;

		public bool Equals(MethodSemanticsRow other)
		{
			if (Semantic != other.Semantic)
				return false;

			if (Method != other.Method)
				return false;

			if (Association != other.Association)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Semantic ^
				Method ^
				Association;
		}
	}
}
