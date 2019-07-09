using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	[Flags]
	public enum BamlElementFlags : byte
	{
		None = 0,
		CreateUsingTypeConverter = 1,
		Injected = 2,
	}
}
