using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in MemberRef table.
	/// </summary>
	public struct MemberRefRow : IEquatable<MemberRefRow>
	{
		/// <summary>
		/// This entry references the TypeRef or ModuleRef table. Method references, residing in the
		/// same table, can have their Class entries referencing the Method and TypeSpec tables as well
		/// Coded token of type MemberRefParent.
		/// </summary>
		public int Class;

		/// <summary>
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// Offset in the #Blob stream.
		/// </summary>
		public int Signature;

		public bool Equals(MemberRefRow other)
		{
			if (Class != other.Class)
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
				Class ^
				Name ^
				Signature;
		}
	}
}
