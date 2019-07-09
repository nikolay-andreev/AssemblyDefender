using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlElement : BamlBlock
	{
		private BamlElementFlags _flags;
		private IBamlType _type;

		public BamlElement()
		{
		}

		public BamlElement(IBamlType type)
			: this(type, BamlElementFlags.None)
		{
		}

		public BamlElement(IBamlType type, BamlElementFlags flags)
		{
			_type = type;
			_flags = flags;
		}

		public BamlElementFlags Flags
		{
			get { return _flags; }
			set { _flags = value; }
		}

		public IBamlType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.Element; }
		}

		public override string ToString()
		{
			return string.Format("Element: Flags={0}; Type={{{1}}}", _flags, _type != null ? _type.ToString() : "null");
		}
	}
}
