using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlDefAttributeKeyString : BamlNode
	{
		private IBamlString _value;
		private BamlNode _valueNode;
		private bool _shared;
		private bool _sharedSet;

		public BamlDefAttributeKeyString()
		{
		}

		public BamlDefAttributeKeyString(IBamlString value, BamlNode valueNode)
		{
			_value = value;
			_valueNode = valueNode;
		}

		public BamlDefAttributeKeyString(IBamlString value, BamlNode valueNode, bool shared, bool sharedSet)
			: this(value, valueNode)
		{
			_shared = shared;
			_sharedSet = sharedSet;
		}

		public IBamlString Value
		{
			get { return _value; }
			set { _value = value; }
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
			get { return BamlNodeType.DefAttributeKeyString; }
		}

		public override string ToString()
		{
			return string.Format("DefAttributeKeyString: Value={{{0}}}; ValueNode={{{1}}}; Shared={2}; SharedSet={3}", _value != null ? _value.ToString() : "null", _valueNode != null ? _valueNode.ToString() : "null", _shared, _sharedSet);
		}
	}
}
