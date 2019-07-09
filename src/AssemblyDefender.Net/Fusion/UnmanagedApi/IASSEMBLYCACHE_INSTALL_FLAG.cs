using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	[Flags]
	public enum IASSEMBLYCACHE_INSTALL_FLAG
	{
		REFRESH = 1,
		FORCE_REFRESH = 2,
	}
}
