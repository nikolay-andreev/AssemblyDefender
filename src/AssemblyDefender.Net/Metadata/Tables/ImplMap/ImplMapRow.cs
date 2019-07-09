using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in ImplMap table.
	/// </summary>
	public struct ImplMapRow : IEquatable<ImplMapRow>
	{
		/// <summary>
		/// The validity mask (bits that can be set) is 0x3777.
		/// Unsigned 2-byte integer.
		/// </summary>
		public ushort MappingFlags;

		/// <summary>
		/// An index to the MethodDef or Field tables, identifying the P/Invoke thunk.
		/// This must be a valid index. The indexed method or field must have the pinvokeimpl and static flags set.
		/// Coded token of type MemberForwarded.
		/// </summary>
		public int MemberForwarded;

		/// <summary>
		/// The name of the unmanaged method as it is defined in the export table of the
		/// unmanaged module. The name must be nonempty and fewer than 1,024 bytes long in UTF-8 encoding.
		/// Offset in the #Strings stream.
		/// </summary>
		public int ImportName;

		/// <summary>
		/// The index of the ModuleRef record containing the name of the unmanaged module. It must be a valid RID.
		/// RID in the ModuleRef table.
		/// </summary>
		public int ImportScope;

		public bool Equals(ImplMapRow other)
		{
			if (MappingFlags != other.MappingFlags)
				return false;

			if (MemberForwarded != other.MemberForwarded)
				return false;

			if (ImportName != other.ImportName)
				return false;

			if (ImportScope != other.ImportScope)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)MappingFlags ^
				MemberForwarded ^
				ImportName ^
				ImportScope;
		}
	}
}
