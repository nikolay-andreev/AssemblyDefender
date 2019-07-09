using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Baml
{
	public class BamlTypeInfo : BamlNode, IBamlType
	{
		private string _name;
		private BamlTypeFlags _flags;
		private IBamlAssembly _assembly;

		public BamlTypeInfo()
		{
		}

		public BamlTypeInfo(string name, IBamlAssembly assembly)
			: this(name, assembly, BamlTypeFlags.None)
		{
		}

		public BamlTypeInfo(string name, IBamlAssembly assembly, BamlTypeFlags flags)
		{
			_name = name;
			_assembly = assembly;
			_flags = flags;
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public BamlTypeFlags Flags
		{
			get { return _flags; }
			set { _flags = value; }
		}

		public IBamlAssembly Assembly
		{
			get { return _assembly; }
			set { _assembly = value; }
		}

		BamlTypeKind IBamlType.Kind
		{
			get { return BamlTypeKind.Declaration; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.TypeInfo; }
		}

		public TypeDeclaration Resolve(Assembly ownerAssembly)
		{
			if (string.IsNullOrEmpty(_name))
				return null;

			Assembly assembly;
			if (_assembly != null)
			{
				assembly = _assembly.Resolve(ownerAssembly);
			}
			else
			{
				assembly = ownerAssembly;
			}

			if (assembly == null)
				return null;

			return (TypeDeclaration)assembly.GetTypeOrExportedType(_name);
		}

		public override string ToString()
		{
			return string.Format("TypeInfo: Name=\"{0}\"; Flags={1}; Assembly={{{2}}}", _name ?? "", _flags, _assembly != null ? _assembly.ToString() : "null");
		}
	}
}
