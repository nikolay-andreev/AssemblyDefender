using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlText : BamlNode
	{
		private string _value;

		public BamlText()
		{
		}

		public BamlText(string value)
		{
			_value = value;
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.Text; }
		}

		public override string ToString()
		{
			return string.Format("Text: Value=\"{0}\"", _value ?? "");
		}
	}
}
