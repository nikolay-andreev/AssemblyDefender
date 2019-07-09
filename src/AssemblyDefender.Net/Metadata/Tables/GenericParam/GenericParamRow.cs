using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in GenericParam table.
	/// </summary>
	public struct GenericParamRow : IEquatable<GenericParamRow>
	{
		/// <summary>
		/// Zero-based ordinal of the generic parameter in the generic type's parameter list.
		/// 2-byte unsigned integer.
		/// </summary>
		public int Number;

		/// <summary>
		/// The binary flags indicating certain kinds of constraints imposed on this generic parameter.
		/// 2-byte bit field.
		/// </summary>
		public ushort Flags;

		/// <summary>
		/// A token of the generic type or method definition to which this generic parameter belongs.
		/// Coded token of type TypeOrMethodDef.
		/// </summary>
		public int Owner;

		/// <summary>
		/// The name of the generic parameter. This entry may be zero (unnamed parameter).
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		public bool Equals(GenericParamRow other)
		{
			if (Number != other.Number)
				return false;

			if (Flags != other.Flags)
				return false;

			if (Owner != other.Owner)
				return false;

			if (Name != other.Name)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Number ^
				(int)Flags ^
				Owner ^
				Name;
		}
	}
}
