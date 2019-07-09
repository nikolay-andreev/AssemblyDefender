using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlNamedElement : BamlBlock
	{
		private IBamlType _type;
		private string _runtimeName;

		public BamlNamedElement()
		{
		}

		public BamlNamedElement(IBamlType type, string runtimeName)
		{
			_type = type;
			_runtimeName = runtimeName;
		}

		public IBamlType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public string RuntimeName
		{
			get { return _runtimeName; }
			set { _runtimeName = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.NamedElement; }
		}

		public override string ToString()
		{
			return string.Format("NamedElement: Type={{{0}}}; RuntimeName=\"{1}\"", _type != null ? _type.ToString() : "null", _runtimeName ?? "");
		}
	}
}
