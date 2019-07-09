using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public enum FileLoadMode
	{
		OnDemand,
		InMemory,
		MemoryMappedFile,
	}
}
