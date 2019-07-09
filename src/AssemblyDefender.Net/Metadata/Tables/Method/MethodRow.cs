using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in Method table.
	/// </summary>
	public struct MethodRow : IEquatable<MethodRow>
	{
		/// <summary>
		/// The RVA of the method body in the module. The method body consists of the header,
		/// IL code, and managed exception handling descriptors. The RVA must point to a read-only
		/// section of the PE file.
		/// 4-byte unsigned integer.
		/// </summary>
		public uint RVA;

		/// <summary>
		/// Implementation binary flags indicating the specifics of the method implementation.
		/// 2-byte unsigned integer.
		/// </summary>
		public ushort ImplFlags;

		/// <summary>
		/// Binary flags indicating the method's accessibility and other characteristics.
		/// 2-byte unsigned integer.
		/// </summary>
		public ushort Flags;

		/// <summary>
		/// The name of the method (not including the name of the class to which the method belongs).
		/// This entry must index a string of nonzero length no longer than 1,023 bytes in UTF-8 encoding.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// The method signature. This entry must index a blob of nonzero size.
		/// Offset in the #Blob stream.
		/// </summary>
		public int Signature;

		/// <summary>
		/// The record index of the start of the parameter list belonging to this method. The end of the
		/// parameter list is defined by the start of the next method's parameter list or by the end of the
		/// Param table (the same pattern as in method and field lists belonging to a TypeDef).
		/// RID in the Param table.
		/// </summary>
		public int ParamList;

		public bool Equals(MethodRow other)
		{
			if (RVA != other.RVA)
				return false;

			if (ImplFlags != other.ImplFlags)
				return false;

			if (Flags != other.Flags)
				return false;

			if (Name != other.Name)
				return false;

			if (Signature != other.Signature)
				return false;

			if (ParamList != other.ParamList)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)RVA ^
				(int)ImplFlags ^
				(int)Flags ^
				Name ^
				Signature ^
				ParamList;
		}
	}
}
