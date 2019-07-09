using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in ModuleRef table.
	/// </summary>
	public struct ModuleRefRow : IEquatable<ModuleRefRow>
	{
		/// <summary>
		/// The referenced module name.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		public bool Equals(ModuleRefRow other)
		{
			if (Name != other.Name)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Name;
		}
	}
}
