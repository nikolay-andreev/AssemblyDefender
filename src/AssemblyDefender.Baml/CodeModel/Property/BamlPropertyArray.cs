using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyArray : BamlBlock
	{
		private IBamlProperty _declaringProperty;

		public BamlPropertyArray()
		{
		}

		public BamlPropertyArray(IBamlProperty declaringProperty)
		{
			_declaringProperty = declaringProperty;
		}

		public IBamlProperty DeclaringProperty
		{
			get { return _declaringProperty; }
			set { _declaringProperty = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.PropertyArray; }
		}

		public override string ToString()
		{
			return string.Format("PropertyArray: DeclaringProperty={{{0}}}", _declaringProperty != null ? _declaringProperty.ToString() : "null");
		}
	}
}
