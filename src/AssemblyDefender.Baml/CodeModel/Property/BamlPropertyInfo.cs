using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyInfo : BamlNode, IBamlProperty
	{
		private string _name;
		private BamlPropertyUsage _usage;
		private IBamlType _type;

		public BamlPropertyInfo()
		{
		}

		public BamlPropertyInfo(string name, IBamlType type)
			: this(name, type, BamlPropertyUsage.Default)
		{
		}

		public BamlPropertyInfo(string name, IBamlType type, BamlPropertyUsage usage)
		{
			_name = name;
			_type = type;
			_usage = usage;
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public BamlPropertyUsage Usage
		{
			get { return _usage; }
			set { _usage = value; }
		}

		public IBamlType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		BamlPropertyKind IBamlProperty.Kind
		{
			get { return BamlPropertyKind.Declaration; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.AttributeInfo; }
		}

		/// <summary>
		/// Resolve member
		/// </summary>
		public MemberNode Resolve(Assembly ownerAssembly)
		{
			if (_type == null)
				return null;

			if (string.IsNullOrEmpty(_name))
				return null;

			var type = _type.Resolve(ownerAssembly);
			if (type == null)
				return null;

			var property = type.Properties.Find(_name);
			if (property != null)
				return property;

			var e = type.Events.Find(_name);
			if (e != null)
				return e;

			return type.Fields.Find(_name);
		}

		public override string ToString()
		{
			return string.Format("PropertyInfo: Name=\"{0}\"; Usage={1}; Type={{{2}}}", _name ?? "", _usage, _type != null ? _type.ToString() : "null");
		}
	}
}
