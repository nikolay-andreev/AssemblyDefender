using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlStaticResource : BamlBlock
	{
		private IBamlType _type;
		private BamlElementFlags _flags;

		public BamlStaticResource()
		{
		}

		public BamlStaticResource(IBamlType type, BamlElementFlags flags)
		{
			_type = type;
			_flags = flags;
		}

		public IBamlType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public BamlElementFlags Flags
		{
			get { return _flags; }
			set { _flags = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.StaticResource; }
		}

		public override string ToString()
		{
			return string.Format("StaticResource: Type={{{0}}}; Flags={1}", _type != null ? _type.ToString() : "null", _flags);
		}
	}
}
