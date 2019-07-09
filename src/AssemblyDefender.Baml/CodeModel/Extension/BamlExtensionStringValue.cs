using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlExtensionStringValue : BamlExtensionValue
	{
		private IBamlString _value;

		public BamlExtensionStringValue()
		{
		}

		public BamlExtensionStringValue(IBamlString value)
		{
			_value = value;
		}

		public IBamlString Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlExtensionValueType ValueType
		{
			get { return BamlExtensionValueType.String; }
		}

		public override string ToString()
		{
			return string.Format("ExtensionStringValue: Value={{{0}}}", _value != null ? _value.ToString() : "null");
		}
	}
}
