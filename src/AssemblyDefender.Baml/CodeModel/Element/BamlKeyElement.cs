using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlKeyElement : BamlBlock
	{
		private IBamlType _type;
		private BamlElementFlags _typeFlags;
		private BamlNode _valueNode;
		private bool _shared;
		private bool _sharedSet;

		public BamlKeyElement()
		{
		}

		public BamlKeyElement(IBamlType type, BamlElementFlags typeFlags, BamlNode valueNode)
		{
			_type = type;
			_typeFlags = typeFlags;
			_valueNode = valueNode;
		}

		public BamlKeyElement(IBamlType type, BamlElementFlags typeFlags, BamlNode valueNode, bool shared, bool sharedSet)
			: this(type, typeFlags, valueNode)
		{
			_shared = shared;
			_sharedSet = sharedSet;
		}

		public IBamlType Type
		{
			get { return _type; }
			set { _type = value; }
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
			get { return BamlNodeType.KeyElement; }
		}

		public override string ToString()
		{
			return string.Format("KeyElement: Type={{{0}}}; TypeFlags={1}; ValueNode={{{2}}}; Shared={3}; SharedSet={4}", _type != null ? _type.ToString() : "null", _typeFlags, _valueNode != null ? _valueNode.ToString() : "null", _shared, _sharedSet);
		}
	}
}
