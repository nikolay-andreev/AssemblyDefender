using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlExtensionTypeValue : BamlExtensionValue
	{
		private IBamlType _value;

		public BamlExtensionTypeValue()
		{
		}

		public BamlExtensionTypeValue(IBamlType value)
		{
			_value = value;
		}

		public IBamlType Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlExtensionValueType ValueType
		{
			get { return BamlExtensionValueType.Type; }
		}

		public override string ToString()
		{
			return string.Format("ExtensionTypeValue: Value={{{0}}}", _value != null ? _value.ToString() : "null");
		}
	}
}
