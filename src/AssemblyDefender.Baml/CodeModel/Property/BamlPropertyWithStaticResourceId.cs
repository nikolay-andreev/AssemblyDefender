using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyWithStaticResourceId : BamlNode
	{
		private short _staticResourceId;
		private IBamlProperty _declaringProperty;

		public BamlPropertyWithStaticResourceId()
		{
		}

		public BamlPropertyWithStaticResourceId(short staticResourceId, IBamlProperty declaringProperty)
		{
			_staticResourceId = staticResourceId;
			_declaringProperty = declaringProperty;
		}

		public short StaticResourceId
		{
			get { return _staticResourceId; }
			set { _staticResourceId = value; }
		}

		public IBamlProperty DeclaringProperty
		{
			get { return _declaringProperty; }
			set { _declaringProperty = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.PropertyWithStaticResourceId; }
		}

		public override string ToString()
		{
			return string.Format("PropertyWithStaticResourceId: StaticResourceId={0}; DeclaringProperty={{{1}}}", _staticResourceId, _declaringProperty != null ? _declaringProperty.ToString() : "null");
		}
	}
}
