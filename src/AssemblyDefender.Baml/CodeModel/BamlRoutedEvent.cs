using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlRoutedEvent : BamlNode
	{
		private string _value;
		private IBamlProperty _property;

		public BamlRoutedEvent()
		{
		}

		public BamlRoutedEvent(string value, IBamlProperty property)
		{
			_value = value;
			_property = property;
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public IBamlProperty Property
		{
			get { return _property; }
			set { _property = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.RoutedEvent; }
		}

		public override string ToString()
		{
			return string.Format("RoutedEvent: Value=\"{0}\"; Property={{{1}}}", _value ?? "", _property != null ? _property.ToString() : "null");
		}
	}
}
