using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyCustom : BamlNode
	{
		private bool _isValueType;
		private BamlPropertyValue _value;
		private IBamlProperty _declaringProperty;

		public BamlPropertyCustom()
		{
		}

		public BamlPropertyCustom(bool isValueType, BamlPropertyValue value, IBamlProperty declaringProperty)
		{
			_isValueType = isValueType;
			_value = value;
			_declaringProperty = declaringProperty;
		}

		public bool IsValueType
		{
			get { return _isValueType; }
			set { _isValueType = value; }
		}

		public BamlPropertyValue Value
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
			get { return BamlNodeType.PropertyCustom; }
		}

		public override string ToString()
		{
			return string.Format("PropertyCustom: IsValueType={0}; Value={{{1}}}; DeclaringProperty={{{2}}}", _isValueType, _value != null ? _value.ToString() : "null", _declaringProperty != null ? _declaringProperty.ToString() : "null");
		}
	}
}
