using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlExtension
	{
		private BamlExtensionType _type;
		private BamlExtensionValue _value;

		public BamlExtension()
		{
		}

		public BamlExtension(BamlExtensionType type, BamlExtensionValue value)
		{
			_type = type;
			_value = value;
		}

		public BamlExtensionType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public BamlExtensionValue Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override string ToString()
		{
			return string.Format("ExtensionRecord: Type={0}; Value={{{1}}}", _type.ToString(), _value != null ? _value.ToString() : "null");
		}
	}
}
