using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in FieldRVA table.
	/// </summary>
	public struct FieldRVARow : IEquatable<FieldRVARow>
	{
		/// <summary>
		/// The relative virtual address of the data to which the field is mapped.
		/// 4-byte unsigned integer.
		/// </summary>
		public uint RVA;

		/// <summary>
		/// The index of the Field record being mapped.
		/// RID to the Field table.
		/// </summary>
		public int Field;

		public bool Equals(FieldRVARow other)
		{
			if (RVA != other.RVA)
				return false;

			if (Field != other.Field)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)RVA ^
				Field;
		}
	}
}
