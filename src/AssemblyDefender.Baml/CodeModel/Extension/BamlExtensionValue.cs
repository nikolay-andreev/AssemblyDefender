using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public abstract class BamlExtensionValue
	{
		public abstract BamlExtensionValueType ValueType
		{
			get;
		}
	}
}
