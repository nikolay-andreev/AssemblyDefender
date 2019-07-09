using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public static class ILExceptionFlag
	{
		public const int OFFSETLEN = 0x0000; // Deprecated
		public const int CATCH = 0x0000; // Catch
		public const int FILTER = 0x0001; // If this bit is on, then this EH entry is for a filter
		public const int FINALLY = 0x0002; // This clause is a finally clause
		public const int FAULT = 0x0004; // Fault clause (finally that is called on exception only)
		public const int MASK = 0x0007;
		public const int DUPLICATED = 0x0008; // duplicated clase..  this clause was duplicated down to a funclet which was pulled out of line
	}
}
