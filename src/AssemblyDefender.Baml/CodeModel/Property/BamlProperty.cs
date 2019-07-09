using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlProperty : BamlNode
	{
		private string _value;
		private IBamlProperty _declaringProperty;

		public BamlProperty()
		{
		}

		public BamlProperty(string value, IBamlProperty declaringProperty)
		{
			_value = value;
			_declaringProperty = declaringProperty;
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public IBamlProperty DeclaringProperty
		{
			get { return _declaringProperty; }
			set { _declaringProperty = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.Property; }
		}

		public override string ToString()
		{
			return string.Format("Property: Value=\"{0}\"; DeclaringProperty={{{1}}}", _value ?? "", _declaringProperty != null ? _declaringProperty.ToString() : "null");
		}
	}
}
