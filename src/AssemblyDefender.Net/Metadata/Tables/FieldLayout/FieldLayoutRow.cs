using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in FieldLayout table.
	/// </summary>
	public struct FieldLayoutRow : IEquatable<FieldLayoutRow>
	{
		/// <summary>
		/// The relative offset of the field in the class layout (not an RVA) or the field's ordinal
		/// in case of sequential layout. The offset is relative to the start of the class instance's data.
		/// 4-byte unsigned integer.
		/// </summary>
		public int OffSet;

		/// <summary>
		/// The index of the field for which the offset is specified.
		/// RID to the Field table.
		/// </summary>
		public int Field;

		public bool Equals(FieldLayoutRow other)
		{
			if (OffSet != other.OffSet)
				return false;

			if (Field != other.Field)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				OffSet ^
				Field;
		}
	}
}
