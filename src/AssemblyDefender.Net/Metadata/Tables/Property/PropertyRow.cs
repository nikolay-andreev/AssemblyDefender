using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in Property table.
	/// </summary>
	public struct PropertyRow : IEquatable<PropertyRow>
	{
		/// <summary>
		/// Binary flags of the property characteristics.
		/// 2-byte unsigned integer.
		/// </summary>
		public ushort Flags;

		/// <summary>
		/// The name of the property, which must be a simple name no longer than 1,023 bytes in UTF-8 encoding.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// The property signature. It's about the only occurrence in the metadata when an entry named Type
		/// has nothing to do with type (TypeDef, TypeRef, or TypeSpec). Why this entry couldn't
		/// be called Signature (which it is) remains mystery to me.
		/// Offset in the #Blob stream.
		/// </summary>
		public int Type;

		public bool Equals(PropertyRow other)
		{
			if (Flags != other.Flags)
				return false;

			if (Name != other.Name)
				return false;

			if (Type != other.Type)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Flags ^
				Name ^
				Type;
		}
	}
}
