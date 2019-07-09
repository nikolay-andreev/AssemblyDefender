using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public interface IBamlString
	{
		string Value
		{
			get;
		}

		BamlStringKind Kind
		{
			get;
		}
	}
}
