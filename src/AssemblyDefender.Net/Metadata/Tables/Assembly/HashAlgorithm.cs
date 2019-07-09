using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public enum HashAlgorithm : int
	{
		None = 0x0000,

		/// <summary>
		/// MD5 hash
		/// </summary>
		MD5 = 0x8003,

		/// <summary>
		/// SHA-1 hash
		/// </summary>
		SHA1 = 0x8004,
	}
}
