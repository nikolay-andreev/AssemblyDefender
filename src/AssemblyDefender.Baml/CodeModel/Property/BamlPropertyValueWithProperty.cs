using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyValueWithProperty : BamlPropertyValue
	{
		private string _name;
		private IBamlType _type;
		private IBamlProperty _property;

		public BamlPropertyValueWithProperty()
		{
		}

		public BamlPropertyValueWithProperty(IBamlProperty property)
		{
			_property = property;

			var declaringProperty = property as BamlPropertyInfo;
			if (declaringProperty != null)
			{
				_name = declaringProperty.Name;
				_type = declaringProperty.Type;
			}
		}

		public BamlPropertyValueWithProperty(string name, IBamlType type)
		{
			_name = name;
			_type = type;
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public IBamlType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public IBamlProperty Property
		{
			get { return _property; }
			set { _property = value; }
		}

		public override BamlPropertyValueType ValueType
		{
			get { return BamlPropertyValueType.Property; }
		}

		public override string ToString()
		{
			return string.Format("PropertyValueWithProperty: Name=\"{0}\"; Type={{{1}}}; Property={{{2}}}", _name ?? "", _type != null ? _type.ToString() : "null", _property != null ? _property.ToString() : "null");
		}
	}
}
