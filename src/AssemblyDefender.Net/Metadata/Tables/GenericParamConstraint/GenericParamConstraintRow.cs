using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in GenericParamConstraint table.
	/// </summary>
	public struct GenericParamConstraintRow : IEquatable<GenericParamConstraintRow>
	{
		/// <summary>
		/// The index of the GenericParam record describing the generic parameter to which this
		/// constraint is attributed.
		/// RID in the GenericParam table.
		/// </summary>
		public int Owner;

		/// <summary>
		/// A token of the constraining type, which can reside in the TypeDef, TypeRef, or TypeSpec table.
		/// Coded token of type TypeDefOrRef.
		/// </summary>
		public int Constraint;

		public bool Equals(GenericParamConstraintRow other)
		{
			if (Owner != other.Owner)
				return false;

			if (Constraint != other.Constraint)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Owner ^
				Constraint;
		}
	}
}
