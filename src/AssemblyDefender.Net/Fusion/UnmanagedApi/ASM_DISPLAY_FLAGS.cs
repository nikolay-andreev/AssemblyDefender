using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	[Flags]
	public enum ASM_DISPLAY_FLAGS : int
	{
		VERSION = 0x1,
		CULTURE = 0x2,
		PUBLIC_KEY_TOKEN = 0x4,
		PUBLIC_KEY = 0x8,
		CUSTOM = 0x10,
		PROCESSORARCHITECTURE = 0x20,
		LANGUAGEID = 0x40,
		ALL = 0xff,
	}
}
