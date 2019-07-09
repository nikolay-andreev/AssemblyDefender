using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlExtensionResourceValue : BamlExtensionValue
	{
		private bool _isKey;
		private SystemResourceKeyID _value;

		public BamlExtensionResourceValue()
		{
		}

		public BamlExtensionResourceValue(SystemResourceKeyID value, bool isKey)
		{
			_value = value;
			_isKey = isKey;
		}

		public bool IsKey
		{
			get { return _isKey; }
			set { _isKey = value; }
		}

		public SystemResourceKeyID Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlExtensionValueType ValueType
		{
			get { return BamlExtensionValueType.Resource; }
		}

		public override string ToString()
		{
			return string.Format("ExtensionResourceValue: IsKey={0}; Value={1}", _isKey, _value);
		}
	}
}
