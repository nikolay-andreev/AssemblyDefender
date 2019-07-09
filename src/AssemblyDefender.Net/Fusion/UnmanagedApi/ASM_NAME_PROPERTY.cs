using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	public enum ASM_NAME_PROPERTY : int
	{
		PUBLIC_KEY = 0,
		PUBLIC_KEY_TOKEN = 1,
		HASH_VALUE = 2,
		NAME = 3,
		MAJOR_VERSION = 4,
		MINOR_VERSION = 5,
		BUILD_NUMBER = 6,
		REVISION_NUMBER = 7,
		CULTURE = 8,
		PROCESSOR_ID_ARRAY = 9,
		OSINFO_ARRAY = 10,
		HASH_ALGID = 11,
		ALIAS = 12,
		CODEBASE_URL = 13,
		CODEBASE_LASTMOD = 14,
		NULL_PUBLIC_KEY = 15,
		NULL_PUBLIC_KEY_TOKEN = 16,
		CUSTOM = 17,
		NULL_CUSTOM = 18,
		MVID = 19,
		MAX_PARAMS = 20,
	}
}
