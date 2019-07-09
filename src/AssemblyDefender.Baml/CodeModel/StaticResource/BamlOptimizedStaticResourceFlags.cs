using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public enum BamlOptimizedStaticResourceFlags : byte
	{
		None = 0,
		ValueType = 1,
		StaticType = 2,
	}
}
