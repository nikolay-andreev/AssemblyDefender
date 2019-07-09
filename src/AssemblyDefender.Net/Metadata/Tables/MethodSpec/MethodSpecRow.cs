using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in MethodSpec table.
	/// </summary>
	public struct MethodSpecRow : IEquatable<MethodSpecRow>
	{
		/// <summary>
		/// A token of the generic method definition being instantiated (references the Method or MemberRef table).
		/// Coded token of type MethodDefOrRef.
		/// </summary>
		public int Method;

		/// <summary>
		/// The instantiation signature, cannot be 0.
		/// Offset in the #Blob stream.
		/// </summary>
		public int Instantiation;

		public bool Equals(MethodSpecRow other)
		{
			if (Method != other.Method)
				return false;

			if (Instantiation != other.Instantiation)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Method ^
				Instantiation;
		}
	}
}
