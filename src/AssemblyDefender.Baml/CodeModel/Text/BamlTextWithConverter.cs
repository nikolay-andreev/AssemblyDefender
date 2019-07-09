using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlTextWithConverter : BamlNode
	{
		private string _value;
		private IBamlType _converterType;

		public BamlTextWithConverter()
		{
		}

		public BamlTextWithConverter(string value, IBamlType converterType)
		{
			_value = value;
			_converterType = converterType;
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public IBamlType ConverterType
		{
			get { return _converterType; }
			set { _converterType = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.TextWithConverter; }
		}

		public override string ToString()
		{
			return string.Format("TextWithConverter: Value=\"{0}\"; ConverterType={{{1}}}", _value ?? "", _converterType != null ? _converterType.ToString() : "null");
		}
	}
}
