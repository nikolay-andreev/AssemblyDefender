using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Baml
{
	public class BamlAssemblyInfo : BamlNode, IBamlAssembly
	{
		private string _name;

		public BamlAssemblyInfo()
		{
		}

		public BamlAssemblyInfo(string name)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		BamlAssemblyKind IBamlAssembly.Kind
		{
			get { return BamlAssemblyKind.Declaration; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.AssemblyInfo; }
		}

		public Assembly Resolve(Assembly ownerAssembly)
		{
			var assemblyRef = AssemblyReference.Parse(_name);
			if (assemblyRef == null)
				return null;

			return (Assembly)assemblyRef.Resolve(ownerAssembly.Module);
		}

		public AssemblyReference ToReference(Assembly ownerAssembly)
		{
			return AssemblyReference.Parse(_name);
		}

		public override string ToString()
		{
			return string.Format("AssemblyInfo: Name=\"{0}\"", _name ?? "");
		}
	}
}
