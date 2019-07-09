using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyBoolValue : BamlPropertyValue
	{
		private bool _value;

		public BamlPropertyBoolValue()
		{
		}

		public BamlPropertyBoolValue(bool value)
		{
			_value = value;
		}

		public bool Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlPropertyValueType ValueType
		{
			get { return BamlPropertyValueType.Boolean; }
		}

		public override string ToString()
		{
			return string.Format("PropertyBoolValue: Value={0}", _value);
		}
	}
}
