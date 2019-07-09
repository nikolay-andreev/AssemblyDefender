using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in AssemblyOS table.
	/// </summary>
	public struct AssemblyOSRow : IEquatable<AssemblyOSRow>
	{
		public uint OSPlatformId;

		public uint OSMajorVersion;

		public uint OSMinorVersion;

		public bool Equals(AssemblyOSRow other)
		{
			if (OSPlatformId != other.OSPlatformId)
				return false;

			if (OSMajorVersion != other.OSMajorVersion)
				return false;

			if (OSMinorVersion != other.OSMinorVersion)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)OSPlatformId ^
				(int)OSMajorVersion ^
				(int)OSMinorVersion;
		}
	}
}
