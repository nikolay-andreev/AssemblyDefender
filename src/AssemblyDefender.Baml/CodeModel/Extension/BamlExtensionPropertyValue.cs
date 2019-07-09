using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlExtensionPropertyValue : BamlExtensionValue
	{
		private IBamlProperty _value;

		public BamlExtensionPropertyValue()
		{
		}

		public BamlExtensionPropertyValue(IBamlProperty value)
		{
			_value = value;
		}

		public IBamlProperty Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlExtensionValueType ValueType
		{
			get { return BamlExtensionValueType.Property; }
		}

		public override string ToString()
		{
			return string.Format("ExtensionPropertyValue: Value={{{0}}}", _value != null ? _value.ToString() : "null");
		}
	}
}
