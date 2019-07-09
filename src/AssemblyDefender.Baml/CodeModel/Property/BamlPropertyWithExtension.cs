using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyWithExtension : BamlNode
	{
		private bool _isValueType;
		private bool _isStaticType;
		private BamlExtension _extension;
		private IBamlProperty _declaringProperty;

		public BamlPropertyWithExtension()
		{
		}

		public BamlPropertyWithExtension(bool isValueType, bool isStaticType, BamlExtension extension, IBamlProperty declaringProperty)
		{
			_isValueType = isValueType;
			_isStaticType = isStaticType;
			_extension = extension;
			_declaringProperty = declaringProperty;
		}

		public bool IsValueType
		{
			get { return _isValueType; }
			set { _isValueType = value; }
		}

		public bool IsStaticType
		{
			get { return _isStaticType; }
			set { _isStaticType = value; }
		}

		public BamlExtension Extension
		{
			get { return _extension; }
			set { _extension = value; }
		}

		public IBamlProperty DeclaringProperty
		{
			get { return _declaringProperty; }
			set { _declaringProperty = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.PropertyWithExtension; }
		}

		public override string ToString()
		{
			return string.Format("PropertyWithExtension: IsValueType={0}; IsStaticType={1}; Extension={{{2}}}; DeclaringProperty={{{3}}}", _isValueType, _isStaticType, _extension != null ? _extension.ToString() : "null", _declaringProperty != null ? _declaringProperty.ToString() : "null");
		}
	}
}
