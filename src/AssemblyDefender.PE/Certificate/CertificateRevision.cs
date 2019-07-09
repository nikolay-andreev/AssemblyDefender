using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public enum CertificateRevision : ushort
	{
		/// <summary>
		/// Version 1, legacy version of the Win_Certificate structure. It is supported only for purposes of
		/// verifying legacy Authenticode signatures
		/// </summary>
		REVISION_1_0 = 0x0100,

		/// <summary>
		/// Version 2 is the current version of the Win_Certificate structure.
		/// </summary>
		REVISION_2_0 = 0x0200,
	}
}
