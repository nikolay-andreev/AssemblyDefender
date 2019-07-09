using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in TypeRef table.
	/// </summary>
	public struct TypeRefRow : IEquatable<TypeRefRow>
	{
		/// <summary>
		/// An indicator of the location of the type definition.
		/// Coded token of type ResolutionScope.
		/// </summary>
		public int ResolutionScope;

		/// <summary>
		/// The name of the referenced type. This entry must not be empty.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// The namespace of the referenced type. This entry can be empty.
		/// The namespace and the name constitute the full name of the type.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Namespace;

		public bool Equals(TypeRefRow other)
		{
			if (ResolutionScope != other.ResolutionScope)
				return false;

			if (Name != other.Name)
				return false;

			if (Namespace != other.Namespace)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				ResolutionScope ^
				Name ^
				Namespace;
		}
	}
}
