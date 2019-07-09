using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Flags indicating whether the managed resource is public (accessible from outside the assembly)
	/// or private (accessible from within the current assembly only).
	/// </summary>
	public static class ManifestResourceFlags
	{
		/// <summary>
		/// The resource is exported from the Assembly
		/// </summary>
		public const int Public = 0x0001;

		/// <summary>
		/// The resource is private to the Assembly
		/// </summary>
		public const int Private = 0x0002;
	}
}
