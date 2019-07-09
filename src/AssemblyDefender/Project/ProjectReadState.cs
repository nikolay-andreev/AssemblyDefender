using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	internal class ProjectReadState
	{
		internal string BasePath;
		internal string[] Strings;
		internal SignatureSerializer Signatures;

		internal string GetString(int rid)
		{
			if (rid == 0)
				return null;

			return Strings[rid - 1];
		}
	}
}
