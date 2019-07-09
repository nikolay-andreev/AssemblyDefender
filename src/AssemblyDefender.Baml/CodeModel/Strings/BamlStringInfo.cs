using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Baml
{
	public class BamlStringInfo : BamlNode, IBamlString
	{
		private string _value;

		public BamlStringInfo()
		{
		}

		public BamlStringInfo(string value)
		{
			_value = value;
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		BamlStringKind IBamlString.Kind
		{
			get { return BamlStringKind.Declaration; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.StringInfo; }
		}

		public override string ToString()
		{
			return string.Format("StringInfo: Value=\"{0}\"", _value ?? "");
		}
	}
}
