using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public enum CertificateType : ushort
	{
		/// <summary>
		/// X.509 Certificate. Not Supported.
		/// </summary>
		X509 = 0x0001,

		/// <summary>
		/// PKCS#7 SignedData structure.
		/// </summary>
		PKCS_SIGNED_DATA = 0x0002,

		/// <summary>
		/// Terminal Server Protocol Stack Certificate signing. Not Supported.
		/// </summary>
		TS_STACK_SIGNED = 0x0004,
	}
}
