using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public static class PESectionNames
	{
		public const string Text = ".text";
		public const string Data = ".data";
		public const string SData = ".sdata";
		public const string EData = ".edata";
		public const string IData = ".idata";
		public const string PData = ".pdata";
		public const string RData = ".rdata";
		public const string Tls = ".tls";
		public const string Rsrc = ".rsrc";
		public const string Reloc = ".reloc";
	}
}
