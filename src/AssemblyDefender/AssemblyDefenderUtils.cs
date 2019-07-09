using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender
{
	public static class AssemblyDefenderUtils
	{
		public static string DecodeStackTrace(BuildLog log, string text)
		{
			var decoder = new StackTraceDecoder(log, text);
			decoder.Decode();

			return decoder.Text;
		}
	}
}
