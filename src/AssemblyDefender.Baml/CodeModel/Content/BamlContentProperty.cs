using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlContentProperty : BamlNode
	{
		private IBamlProperty _property;

		public BamlContentProperty()
		{
		}

		public BamlContentProperty(IBamlProperty property)
		{
			_property = property;
		}

		public IBamlProperty Property
		{
			get { return _property; }
			set { _property = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.ContentProperty; }
		}

		public override string ToString()
		{
			return string.Format("ContentProperty: Property={{{0}}}", _property != null ? _property.ToString() : "null");
		}
	}
}
