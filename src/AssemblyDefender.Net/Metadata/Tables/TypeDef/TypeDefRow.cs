using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in TypeDef table.
	/// </summary>
	public struct TypeDefRow : IEquatable<TypeDefRow>
	{
		/// <summary>
		/// Binary flags indicating special features of the type.
		/// </summary>
		public int Flags;

		/// <summary>
		/// The name of the type. This entry must not be empty.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// The namespace of the type, part of the full name to the left of the rightmost dot.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Namespace;

		/// <summary>
		/// A token of the type's parent - that is, of the type from which this type is derived.
		/// This entry must be set to 0 for all interfaces and for one class, the type hierarchy
		/// root class System.Object. For all other types, this entry should carry a valid reference
		/// to the TypeDef, TypeRef, or TypeSpec table. The TypeSpec table can be referenced only
		/// if the parent type is an instantiation of a generic type.
		/// Coded token of type TypeDefOrRef.
		/// </summary>
		public int Extends;

		/// <summary>
		/// An index to the Field table, marking the start of the field records belonging to this type.
		/// RID in the Field table.
		/// </summary>
		public int FieldList;

		/// <summary>
		/// An index to the Method table, marking the start of the method records belonging to this type.
		/// RID in the Method table.
		/// </summary>
		public int MethodList;

		public bool Equals(TypeDefRow other)
		{
			if (Flags != other.Flags)
				return false;

			if (Name != other.Name)
				return false;

			if (Namespace != other.Namespace)
				return false;

			if (Extends != other.Extends)
				return false;

			if (FieldList != other.FieldList)
				return false;

			if (MethodList != other.MethodList)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Flags ^
				Name ^
				Namespace ^
				Extends ^
				FieldList ^
				MethodList;
		}
	}
}
