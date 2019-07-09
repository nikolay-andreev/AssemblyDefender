using System;
using System.Text;
using AssemblyDefender.Net.Metadata;

namespace AssemblyDefender.Net
{
	public struct ConstantInfo
	{
		private object _value;
		private ConstantType _type;

		public ConstantInfo(object value, ConstantType type)
		{
			_value = value;
			_type = type;
		}

		public object Value
		{
			get { return _value; }
		}

		public ConstantType Type
		{
			get { return _type; }
		}

		internal static ConstantInfo? Load(Module module, int parentToken)
		{
			var image = module.Image;

			int rid;
			if (!image.GetConstantByParent(MetadataToken.CompressHasConstant(parentToken), out rid))
				return null;

			ConstantRow row;
			image.GetConstant(rid, out row);

			byte[] data = image.GetBlob(row.Value);

			switch (row.Type)
			{
				case ConstantTableType.Boolean:
					return new ConstantInfo(BitConverter.ToBoolean(data, 0), ConstantType.Bool);

				case ConstantTableType.I1:
					return new ConstantInfo((sbyte)data[0], ConstantType.Int8);

				case ConstantTableType.I2:
					return new ConstantInfo(BitConverter.ToInt16(data, 0), ConstantType.Int16);

				case ConstantTableType.I4:
					return new ConstantInfo(BitConverter.ToInt32(data, 0), ConstantType.Int32);

				case ConstantTableType.I8:
					return new ConstantInfo(BitConverter.ToInt64(data, 0), ConstantType.Int64);

				case ConstantTableType.U1:
					return new ConstantInfo(data[0], ConstantType.UInt8);

				case ConstantTableType.U2:
					return new ConstantInfo(BitConverter.ToUInt16(data, 0), ConstantType.UInt16);

				case ConstantTableType.U4:
					return new ConstantInfo(BitConverter.ToUInt32(data, 0), ConstantType.UInt32);

				case ConstantTableType.U8:
					return new ConstantInfo(BitConverter.ToUInt64(data, 0), ConstantType.UInt64);

				case ConstantTableType.R4:
					return new ConstantInfo(BitConverter.ToSingle(data, 0), ConstantType.Float32);

				case ConstantTableType.R8:
					return new ConstantInfo(BitConverter.ToDouble(data, 0), ConstantType.Float64);

				case ConstantTableType.Char:
					return new ConstantInfo(BitConverter.ToInt16(data, 0), ConstantType.Char);

				case ConstantTableType.String:
					return new ConstantInfo(
						data != null ? Encoding.Unicode.GetString(data) : string.Empty,
						ConstantType.String);

				case ConstantTableType.Class:
					return new ConstantInfo(null, ConstantType.Nullref);

				default:
					{
						if (data == null)
							return null;

						return new ConstantInfo(data, ConstantType.ByteArray);
					}
			}
		}
	}
}
