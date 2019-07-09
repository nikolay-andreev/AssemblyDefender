using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyTypeReference : BamlNode
	{
		private IBamlType _value;
		private IBamlProperty _declaringProperty;

		public BamlPropertyTypeReference()
		{
		}

		public BamlPropertyTypeReference(IBamlType value, IBamlProperty declaringProperty)
		{
			_value = value;
			_declaringProperty = declaringProperty;
		}

		public IBamlType Value
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
			get { return BamlNodeType.PropertyTypeReference; }
		}

		public override string ToString()
		{
			return string.Format("PropertyTypeReference: Value={{{0}}}; DeclaringProperty={{{1}}}", _value != null ? _value.ToString() : "null", _declaringProperty != null ? _declaringProperty.ToString() : "null");
		}
	}
}
