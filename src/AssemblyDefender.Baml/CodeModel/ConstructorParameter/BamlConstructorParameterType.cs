using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlConstructorParameterType : BamlNode
	{
		private IBamlType _type;

		public BamlConstructorParameterType()
		{
		}

		public BamlConstructorParameterType(IBamlType type)
		{
			_type = type;
		}

		public IBamlType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.ConstructorParameterType; }
		}

		public override string ToString()
		{
			return string.Format("ConstructorParameterType: Type={{{0}}}", _type != null ? _type.ToString() : "null");
		}
	}
}
