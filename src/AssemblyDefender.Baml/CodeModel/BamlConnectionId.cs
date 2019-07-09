using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlConnectionId : BamlNode
	{
		private int _value;

		public BamlConnectionId()
		{
		}

		public BamlConnectionId(int value)
		{
			_value = value;
		}

		public int Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.ConnectionId; }
		}

		public override string ToString()
		{
			return string.Format("ConnectionId: Value={0}", _value);
		}
	}
}
