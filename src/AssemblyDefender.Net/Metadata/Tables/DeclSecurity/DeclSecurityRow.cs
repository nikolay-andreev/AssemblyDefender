using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in DeclSecurity table.
	/// </summary>
	public struct DeclSecurityRow : IEquatable<DeclSecurityRow>
	{
		/// <summary>
		/// The security action code.
		/// 2-byte unsigned integer.
		/// </summary>
		public SecurityAction Action;

		/// <summary>
		/// The index to the Assembly, TypeDef, or Method metadata table, indicating the
		/// metadata item with which the DeclSecurity record is associated.
		/// Coded token of type HasDeclSecurity.
		/// </summary>
		public int Parent;

		/// <summary>
		/// Encoded representation of the permission set associated with a specific security action
		/// and a specific metadata item.
		/// Offset in the #Blob stream.
		/// </summary>
		public int PermissionSet;

		public bool Equals(DeclSecurityRow other)
		{
			if (Action != other.Action)
				return false;

			if (Parent != other.Parent)
				return false;

			if (PermissionSet != other.PermissionSet)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Action ^
				Parent ^
				PermissionSet;
		}
	}
}
