using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlStaticResourceId : BamlNode
	{
		private short _value;

		public BamlStaticResourceId()
		{
		}

		public BamlStaticResourceId(short value)
		{
			_value = value;
		}

		public short Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.StaticResourceId; }
		}

		public override string ToString()
		{
			return string.Format("StaticResourceId: Value={0}", _value);
		}
	}
}
