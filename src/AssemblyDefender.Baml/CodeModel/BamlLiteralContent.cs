using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlLiteralContent : BamlNode
	{
		private string _value;
		private int _num1;
		private int _num2;

		public BamlLiteralContent()
		{
		}

		public BamlLiteralContent(string value)
		{
			_value = value;
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public int Num1
		{
			get { return _num1; }
			set { _num1 = value; }
		}

		public int Num2
		{
			get { return _num2; }
			set { _num2 = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.LiteralContent; }
		}

		public override string ToString()
		{
			return string.Format("LiteralContent: Value=\"{0}\"; Num1={1}; Num2={2}", _value ?? "", _num1, _num2);
		}
	}
}
