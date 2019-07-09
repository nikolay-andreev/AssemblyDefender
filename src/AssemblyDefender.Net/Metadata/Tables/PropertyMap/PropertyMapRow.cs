using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in PropertyMap table.
	/// </summary>
	public struct PropertyMapRow : IEquatable<PropertyMapRow>
	{
		/// <summary>
		/// The type declaring the properties.
		/// RID in the TypeDef table.
		/// </summary>
		public int Parent;

		/// <summary>
		/// The beginning of the properties declared by the type referenced by the Parent entry.
		/// RID in the Property table.
		/// </summary>
		public int PropertyList;

		public bool Equals(PropertyMapRow other)
		{
			if (Parent != other.Parent)
				return false;

			if (PropertyList != other.PropertyList)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Parent ^
				PropertyList;
		}
	}
}
