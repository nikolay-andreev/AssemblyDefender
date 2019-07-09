using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlXmlnsProperty : BamlNode
	{
		private string _prefix;
		private string _xmlNamespace;
		private List<IBamlAssembly> _assemblies = new List<IBamlAssembly>();

		public BamlXmlnsProperty()
		{
		}

		public BamlXmlnsProperty(string prefix, string ns)
		{
			_prefix = prefix;
			_xmlNamespace = ns;
		}

		public string Prefix
		{
			get { return _prefix; }
			set { _prefix = value; }
		}

		public string XmlNamespace
		{
			get { return _xmlNamespace; }
			set { _xmlNamespace = value; }
		}

		public List<IBamlAssembly> Assemblies
		{
			get { return _assemblies; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.XmlnsProperty; }
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("XmlnsPropertyRecord: Prefix=\"{0}\"; XmlNamespace=\"{1}\"; Assemblies={{Count={2}", _prefix ?? "", _xmlNamespace ?? "", _assemblies.Count);

			for (int i = 0; i < _assemblies.Count; i++)
			{
				sb.AppendFormat("; {{[0] {1}}}", i + 1, _assemblies[i].ToString());
			}

			sb.Append("}}");

			return sb.ToString();
		}
	}
}
