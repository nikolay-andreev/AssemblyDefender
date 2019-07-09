using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public abstract class BamlPropertyValue
	{
		public abstract BamlPropertyValueType ValueType
		{
			get;
		}
	}
}
