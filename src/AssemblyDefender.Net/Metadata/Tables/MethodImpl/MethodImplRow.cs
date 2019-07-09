using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in MethodImpl table.
	/// </summary>
	public struct MethodImplRow : IEquatable<MethodImplRow>
	{
		/// <summary>
		/// The record index of the TypeDef implementing a method - in other words, replacing the
		/// method's implementation with that of another method.
		/// RID in the TypeDef table
		/// </summary>
		public int Class;

		/// <summary>
		/// A token indexing a record in the Method table that corresponds to the implementing
		/// method - that is, to the method whose implementation substitutes for another
		/// method's implementation. A coded token of this type can point to the MemberRef table as well,
		/// but this is illegal in the existing releases of the common language runtime.
		/// The method indexed by MethodBody must be virtual. In the existing releases of the runtime,
		/// the method indexed by MethodBody must belong to the class indexed by the Class entry.
		/// Coded token of type MethodDefOrRef.
		/// </summary>
		public int MethodBody;

		/// <summary>
		/// A token indexing a record in the Method table or the MemberRef table that corresponds
		/// to the implemented method - that is, to the method whose implementation is being replaced
		/// by another method's implementation. The method indexed by MethodDecl must be virtual.
		/// Coded token of type MethodDefOrRef.
		/// </summary>
		public int MethodDeclaration;

		public bool Equals(MethodImplRow other)
		{
			if (Class != other.Class)
				return false;

			if (MethodBody != other.MethodBody)
				return false;

			if (MethodDeclaration != other.MethodDeclaration)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Class ^
				MethodBody ^
				MethodDeclaration;
		}
	}
}
