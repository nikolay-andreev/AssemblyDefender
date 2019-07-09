using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlDefAttribute : BamlNode
	{
		private IBamlString _name;
		private string _value;

		public BamlDefAttribute()
		{
		}

		public BamlDefAttribute(IBamlString name, string value)
		{
			_name = name;
			_value = value;
		}

		public IBamlString Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.DefAttribute; }
		}

		public override string ToString()
		{
			return string.Format("DefAttribute: Name={{{0}}}; Value=\"{1}\"", _name != null ? _name.ToString() : "null", _value ?? "");
		}
	}
}
