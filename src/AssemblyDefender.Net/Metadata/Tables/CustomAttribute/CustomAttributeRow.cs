using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in CustomAttribute table.
	/// </summary>
	public struct CustomAttributeRow : IEquatable<CustomAttributeRow>
	{
		/// <summary>
		/// This entry references the metadata item to which the attribute is assigned.
		/// Coded token of type HasCustomAttribute.
		/// </summary>
		public int Parent;

		/// <summary>
		/// This entry defines the type of the custom attribute itself.
		/// Coded token of type CustomAttributeType.
		/// </summary>
		public int Type;

		/// <summary>
		/// This entry is the binary representation of the custom attribute's parameters.
		/// Offset in the #Blob stream.
		/// </summary>
		public int Value;

		public bool Equals(CustomAttributeRow other)
		{
			if (Parent != other.Parent)
				return false;

			if (Type != other.Type)
				return false;

			if (Value != other.Value)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Parent ^
				Type ^
				Value;
		}
	}
}
