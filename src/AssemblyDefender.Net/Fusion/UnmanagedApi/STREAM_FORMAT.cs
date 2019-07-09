using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	[Flags]
	public enum STREAM_FORMAT : int
	{
		COMPLIB_MODULE = 0,
		COMPLIB_MANIFEST = 1,
		WIN32_MODULE = 2,
		WIN32_MANIFEST = 4,
	}
}
