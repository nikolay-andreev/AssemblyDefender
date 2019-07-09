using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in Param table.
	/// </summary>
	public struct ParamRow : IEquatable<ParamRow>
	{
		/// <summary>
		/// Binary flags characterizing the parameter.
		/// 2-byte unsigned integer.
		/// </summary>
		public ushort Flags;

		/// <summary>
		/// The sequence number of the parameter, with 0 corresponding to the method return.
		/// 2-byte unsigned integer.
		/// </summary>
		public ushort Sequence;

		/// <summary>
		/// The name of the parameter, which can be zero length (because the parameter name is used
		/// solely for Reflection and is not involved in any resolution by name). For the method return,
		/// it must be zero length.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		public bool Equals(ParamRow other)
		{
			if (Flags != other.Flags)
				return false;

			if (Sequence != other.Sequence)
				return false;

			if (Name != other.Name)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Flags ^
				(int)Sequence ^
				Name;
		}
	}
}
