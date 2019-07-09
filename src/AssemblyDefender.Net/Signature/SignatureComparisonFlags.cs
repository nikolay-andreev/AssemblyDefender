using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	[Flags]
	public enum SignatureComparisonFlags
	{
		None = 0,

		/// <summary>
		/// Ignore assembly culture, version and public key token.
		/// </summary>
		IgnoreAssemblyStrongName = 1,

		/// <summary>
		/// Ignore type owner.
		/// </summary>
		IgnoreTypeOwner = IgnoreAssemblyStrongName << 1,

		/// <summary>
		/// Ignore member owner.
		/// </summary>
		IgnoreMemberOwner = IgnoreTypeOwner << 1,

		IgnoreOwner = IgnoreTypeOwner | IgnoreMemberOwner,
	}
}
