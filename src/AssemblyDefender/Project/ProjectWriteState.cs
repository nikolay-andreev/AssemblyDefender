using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	internal class ProjectWriteState
	{
		internal string BasePath;
		internal HashList<string> Strings;
		internal SignatureSerializer Signatures;

		internal int SetString(string s)
		{
			if (s == null)
				return 0;

			return Strings.Set(s) + 1;
		}
	}
}
