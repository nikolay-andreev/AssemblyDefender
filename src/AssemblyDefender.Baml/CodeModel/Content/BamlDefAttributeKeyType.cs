using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlDefAttributeKeyType : BamlNode
	{
		private IBamlType _value;
		private BamlElementFlags _typeFlags;
		private BamlNode _valueNode;
		private bool _shared;
		private bool _sharedSet;

		public BamlDefAttributeKeyType()
		{
		}

		public BamlDefAttributeKeyType(IBamlType value, BamlElementFlags typeFlags, BamlNode valueNode)
		{
			_value = value;
			_typeFlags = typeFlags;
			_valueNode = valueNode;
		}

		public BamlDefAttributeKeyType(IBamlType value, BamlElementFlags typeFlags, BamlNode valueNode, bool shared, bool sharedSet)
			: this(value, typeFlags, valueNode)
		{
			_shared = shared;
			_sharedSet = sharedSet;
		}

		public IBamlType Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public BamlElementFlags TypeFlags
		{
			get { return _typeFlags; }
			set { _typeFlags = value; }
		}

		public BamlNode ValueNode
		{
			get { return _valueNode; }
			set { _valueNode = value; }
		}

		public bool Shared
		{
			get { return _shared; }
			set { _shared = value; }
		}

		public bool SharedSet
		{
			get { return _sharedSet; }
			set { _sharedSet = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.DefAttributeKeyType; }
		}

		public override string ToString()
		{
			return string.Format("DefAttributeKeyType: Value={{{0}}}; TypeFlags={1}; ValueNode={{{2}}}; Shared={3}; SharedSet={4}", _value != null ? _value.ToString() : "null", _typeFlags, _valueNode != null ? _valueNode.ToString() : "null", _shared, _sharedSet);
		}
	}
}
