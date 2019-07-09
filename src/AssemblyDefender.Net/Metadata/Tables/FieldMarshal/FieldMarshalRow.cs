using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in FieldMarshal table.
	/// </summary>
	public struct FieldMarshalRow : IEquatable<FieldMarshalRow>
	{
		/// <summary>
		/// Coded token to HasFieldMarshal (an index into Field or Param)
		/// </summary>
		public int Parent;

		/// <summary>
		/// Offset in the #Blob heap.
		/// </summary>
		public int NativeType;

		public bool Equals(FieldMarshalRow other)
		{
			if (Parent != other.Parent)
				return false;

			if (NativeType != other.NativeType)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Parent ^
				NativeType;
		}
	}
}
