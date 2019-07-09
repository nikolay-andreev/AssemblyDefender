using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in ManifestResource table.
	/// </summary>
	public struct ManifestResourceRow : IEquatable<ManifestResourceRow>
	{
		/// <summary>
		/// Location of the resource within the managed resource
		/// segment to which the Resources data directory of the CLR header points. This is not an
		/// RVA; rather, it is an offset within the managed resource segment.
		/// 4-byte unsigned integer.
		/// </summary>
		public int Offset;

		/// <summary>
		/// Binary flags indicating whether the managed resource is public
		/// (accessible from outside the assembly) or private (accessible from within the current assembly only).
		/// 4-byte wide bitfield.
		/// </summary>
		public int Flags;

		/// <summary>
		/// Nonempty name of the resource, unique within the assembly.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// Token of the respective AssemblyRef record if the resource resides in another assembly or
		/// of the respective File record if the resource resides in another file of the current assembly.
		/// If the resource is embedded in the current module, this entry is set to 0. If the resource is
		/// imported from another assembly, the offset need not be specified; the loader will ignore it.
		/// Coded token of type Implementation.
		/// </summary>
		public int Implementation;

		public bool Equals(ManifestResourceRow other)
		{
			if (Offset != other.Offset)
				return false;

			if (Flags != other.Flags)
				return false;

			if (Name != other.Name)
				return false;

			if (Implementation != other.Implementation)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Offset ^
				Flags ^
				Name ^
				Implementation;
		}
	}
}
