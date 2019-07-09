using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Baml
{
	public class BamlDeferableContent : BamlBlock
	{
		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.DeferableContent; }
		}

		public override string ToString()
		{
			return string.Format("DeferableContent: FirstChild={{{0}}}", _firstChild != null ? _firstChild.ToString() : "null");
		}
	}
}
