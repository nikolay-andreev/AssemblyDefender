using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum GenericParameterVariance
	{
		NonVariant = 0,

		/// <remarks>
		/// IL: +
		/// </remarks>
		Covariant = 0x1,

		/// <remarks>
		/// IL: -
		/// </remarks>
		Contravariant = 0x2,
	}
}
