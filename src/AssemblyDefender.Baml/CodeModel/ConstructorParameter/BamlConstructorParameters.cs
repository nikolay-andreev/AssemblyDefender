using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlConstructorParameters : BamlBlock
	{
		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.ConstructorParameters; }
		}

		public override string ToString()
		{
			return "ConstructorParameters";
		}
	}
}
