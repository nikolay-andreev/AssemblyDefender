using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in AssemblyRefOS table.
	/// </summary>
	public struct AssemblyRefOSRow : IEquatable<AssemblyRefOSRow>
	{
		public uint OSPlatformId;

		public uint OSMajorVersion;

		public uint OSMinorVersion;

		public int AssemblyRef;

		public bool Equals(AssemblyRefOSRow other)
		{
			if (OSPlatformId != other.OSPlatformId)
				return false;

			if (OSMajorVersion != other.OSMajorVersion)
				return false;

			if (OSMinorVersion != other.OSMinorVersion)
				return false;

			if (AssemblyRef != other.AssemblyRef)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)OSPlatformId ^
				(int)OSMajorVersion ^
				(int)OSMinorVersion ^
				AssemblyRef;
		}
	}
}
