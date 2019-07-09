using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Flags for use with strong name key gneeration methods.
	/// </summary>
	[Flags]
	public enum StrongNameGenerationFlags : int
	{
		/// <summary>
		/// No flags
		/// </summary>
		None = 0x00000000,

		/// <summary>
		/// Recompute all hashes for linked modules.
		/// </summary>
		SIGN_ALL_FILES = 0x00000001,

		/// <summary>
		/// Test-sign the assembly.
		/// </summary>
		TEST_SIGN = 0x00000002,
	}
}
