using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	[Flags]
	public enum BamlTypeFlags : byte
	{
		None = 0,
		Internal = 1,
		UnusedTwo = 2,
		UnusedThree = 4,
	}
}
