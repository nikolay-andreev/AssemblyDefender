using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlTextWithId : BamlNode
	{
		private IBamlString _value;

		public BamlTextWithId()
		{
		}

		public BamlTextWithId(IBamlString value)
		{
			_value = value;
		}

		public IBamlString Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.TextWithId; }
		}

		public override string ToString()
		{
			return string.Format("TextWithId: Value={{{0}}}", _value != null ? _value.ToString() : "null");
		}
	}
}
