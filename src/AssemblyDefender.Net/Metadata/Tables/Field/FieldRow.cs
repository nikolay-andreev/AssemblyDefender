using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in Field table.
	/// </summary>
	public struct FieldRow : IEquatable<FieldRow>
	{
		/// <summary>
		/// Binary flags indicating the field's characteristics.
		/// 2-byte unsigned integer.
		/// </summary>
		public ushort Flags;

		/// <summary>
		/// The field's name.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// The field's signature.
		/// Offset in the #Blob stream.
		/// </summary>
		public int Signature;

		public bool Equals(FieldRow other)
		{
			if (Flags != other.Flags)
				return false;

			if (Name != other.Name)
				return false;

			if (Signature != other.Signature)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Flags ^
				Name ^
				Signature;
		}
	}
}
