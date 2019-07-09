using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyWithConverter : BamlNode
	{
		private string _value;
		private IBamlType _converterType;
		private IBamlProperty _declaringProperty;

		public BamlPropertyWithConverter()
		{
		}

		public BamlPropertyWithConverter(string value, IBamlType converterType, IBamlProperty declaringProperty)
		{
			_value = value;
			_converterType = converterType;
			_declaringProperty = declaringProperty;
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public IBamlType ConverterType
		{
			get { return _converterType; }
			set { _converterType = value; }
		}

		public IBamlProperty DeclaringProperty
		{
			get { return _declaringProperty; }
			set { _declaringProperty = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.PropertyWithConverter; }
		}

		public override string ToString()
		{
			return string.Format("PropertyWithConverter: Value=\"{0}\"; ConverterType={{{1}}}; DeclaringProperty={{{2}}}", _value ?? "", _converterType != null ? _converterType.ToString() : "null", _declaringProperty != null ? _declaringProperty.ToString() : "null");
		}
	}
}
