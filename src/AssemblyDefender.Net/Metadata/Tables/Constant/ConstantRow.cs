using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in Constant table.
	/// </summary>
	public struct ConstantRow : IEquatable<ConstantRow>
	{
		/// <summary>
		/// The type of the constant
		/// </summary>
		public ConstantTableType Type;

		/// <summary>
		/// A reference to the owner of the constant - a record in the Field, Property, or Param table.
		/// Coded token of type HasConstant.
		/// </summary>
		public int Parent;

		/// <summary>
		/// A constant value blob.
		/// Offset in the #Blob stream.
		/// </summary>
		public int Value;

		public bool Equals(ConstantRow other)
		{
			if (Type != other.Type)
				return false;

			if (Parent != other.Parent)
				return false;

			if (Value != other.Value)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Type ^
				Parent ^
				Value;
		}
	}
}
