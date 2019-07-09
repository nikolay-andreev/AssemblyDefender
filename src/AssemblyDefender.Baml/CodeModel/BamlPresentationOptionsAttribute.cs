using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlPresentationOptionsAttribute : BamlNode
	{
		private string _value;
		private IBamlString _name;

		public BamlPresentationOptionsAttribute()
		{
		}

		public BamlPresentationOptionsAttribute(string value, IBamlString name)
		{
			_value = value;
			_name = name;
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public IBamlString Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.PresentationOptionsAttribute; }
		}

		public override string ToString()
		{
			return string.Format("PresentationOptionsAttribute: Value=\"{0}\"; Name={{{1}}}", _value ?? "", _name != null ? _name.ToString() : "null");
		}
	}
}
