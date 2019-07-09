using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Baml
{
	public class BamlTypeSerializerInfo : BamlTypeInfo
	{
		private IBamlType _serializerType;

		public BamlTypeSerializerInfo()
		{
		}

		public BamlTypeSerializerInfo(IBamlType serializerType)
		{
			_serializerType = serializerType;
		}

		public IBamlType SerializerType
		{
			get { return _serializerType; }
			set { _serializerType = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.TypeSerializerInfo; }
		}

		public override string ToString()
		{
			return string.Format("TypeSerializerInfo: SerializerType={{{0}}}", _serializerType != null ? _serializerType.ToString() : "null");
		}
	}
}
