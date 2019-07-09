using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlPIMapping : BamlNode
	{
		private string _xmlNamespace;
		private string _clrNamespace;
		private IBamlAssembly _assembly;

		public BamlPIMapping()
		{
		}

		public BamlPIMapping(string xmlNamespace, string clrNamespace, IBamlAssembly assembly)
		{
			_xmlNamespace = xmlNamespace;
			_clrNamespace = clrNamespace;
			_assembly = assembly;
		}

		public string XmlNamespace
		{
			get { return _xmlNamespace; }
			set { _xmlNamespace = value; }
		}

		public string ClrNamespace
		{
			get { return _clrNamespace; }
			set { _clrNamespace = value; }
		}

		public IBamlAssembly Assembly
		{
			get { return _assembly; }
			set { _assembly = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.PIMapping; }
		}

		public override string ToString()
		{
			return string.Format("PIMapping: XmlNamespace=\"{0}\"; ClrNamespace=\"{1}\"; Assembly={{{2}}}", _xmlNamespace ?? "", _clrNamespace ?? "", _assembly != null ? _assembly.ToString() : "null");
		}
	}
}
