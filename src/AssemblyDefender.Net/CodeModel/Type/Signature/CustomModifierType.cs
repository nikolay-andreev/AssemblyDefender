using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum CustomModifierType
	{
		/// <summary>
		/// Compiler can freely ignore this modifier.
		/// </summary>
		ModOpt = 1,

		/// <summary>
		/// Compiler shall understand the semantic implied by this custom modifier.
		/// </summary>
		ModReq = 2,
	}
}
