using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlOptimizedStaticResource : BamlNode
	{
		private BamlExtensionValue _value;
		private BamlOptimizedStaticResourceFlags _flags;

		public BamlOptimizedStaticResource()
		{
		}

		public BamlOptimizedStaticResource(BamlExtensionValue value, BamlOptimizedStaticResourceFlags flags)
		{
			_value = value;
			_flags = flags;
		}

		public BamlExtensionValue Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public BamlOptimizedStaticResourceFlags Flags
		{
			get { return _flags; }
			set { _flags = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.OptimizedStaticResource; }
		}

		public override string ToString()
		{
			return string.Format("OptimizedStaticResource: Value={{{0}}}; Flags={1}", _value != null ? _value.ToString() : "null", _flags);
		}
	}
}
