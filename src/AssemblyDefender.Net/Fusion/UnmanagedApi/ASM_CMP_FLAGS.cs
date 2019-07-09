using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	[Flags]
	public enum ASM_CMP_FLAGS : int
	{
		NAME = 0x1,
		MAJOR_VERSION = 0x2,
		MINOR_VERSION = 0x4,
		BUILD_NUMBER = 0x8,
		REVISION_NUMBER = 0x10,
		PUBLIC_KEY_TOKEN = 0x20,
		CULTURE = 0x40,
		CUSTOM = 0x80,
		DEFAULT = 0x100,
		ALL = 0xffff,
	}
}
