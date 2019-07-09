using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum SignaturePrintingFlags
	{
		None = 0,

		/// <summary>
		/// Ignore assembly culture, version and public key token.
		/// </summary>
		IgnoreAssemblyStrongName = 1,

		/// <summary>
		/// If assembly property is null, do not set default value.
		/// Culture=neutral
		/// Version=0.0.0.0
		/// PublicKeyToken=null
		/// </summary>
		IgnoreAssemblyStrongNameDefaultValues = IgnoreAssemblyStrongName << 1,

		IgnoreTypeOwner = IgnoreAssemblyStrongNameDefaultValues << 1,

		IgnoreMemberOwner = IgnoreTypeOwner << 1,

		/// <summary>
		/// Escape identifiers with single quote.
		/// </summary>
		EscapeIdentifiers = IgnoreMemberOwner << 1,

		UsePrimitiveTypes = EscapeIdentifiers << 1,
	}
}
