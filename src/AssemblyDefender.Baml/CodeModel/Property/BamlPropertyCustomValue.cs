using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;

namespace AssemblyDefender.Baml
{
	public class BamlPropertyCustomValue : BamlPropertyValue
	{
		private byte[] _data;
		private BamlPropertyValueType _type;

		public BamlPropertyCustomValue()
		{
		}

		public BamlPropertyCustomValue(byte[] data, BamlPropertyValueType type)
		{
			_data = data;
			_type = type;
		}

		public byte[] Data
		{
			get { return _data; }
			set { _data = value; }
		}

		public override BamlPropertyValueType ValueType
		{
			get { return _type; }
		}

		public override string ToString()
		{
			return string.Format("PropertyCustomValue: Data={{{0}}}", _data != null ? ConvertUtils.ToString(_data) : "null");
		}
	}
}
