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
	public enum StrongNameKeyGenFlags : int
	{
		/// <summary>
		/// No flags
		/// </summary>
		None = 0x00000000,

		/// <summary>
		/// Save the key in the key container.
		/// </summary>
		LeaveKey = 0x00000001,
	}
}
