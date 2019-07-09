using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;

namespace AssemblyDefender.Common.Resources
{
	public static class ResourceUtils
	{
		public static bool IsResource(byte[] data)
		{
			return IsResource(data, 0, data.Length);
		}

		public static bool IsResource(byte[] data, int index, int count)
		{
			if (count - index < 4)
				return false;

			if (BitConverter.ToInt32(data, index) != ResourceManager.MagicNumber)
				return false;

			return true;
		}

		public static ResourceTypeCode GetTypeCode(string typeName)
		{
			ResourceTypeCode typeCode;
			if (_typeCodes.TryGetValue(typeName, out typeCode))
				return typeCode;

			return ResourceTypeCode.Object;
		}

		#region Type codes

		private static Dictionary<string, ResourceTypeCode> _typeCodes;

		static ResourceUtils()
		{
			_typeCodes = new Dictionary<string, ResourceTypeCode>(19, StringComparer.OrdinalIgnoreCase);
			_typeCodes.Add("ResourceTypeCode.Null", ResourceTypeCode.Null);
			_typeCodes.Add("ResourceTypeCode.String", ResourceTypeCode.String);
			_typeCodes.Add("ResourceTypeCode.Boolean", ResourceTypeCode.Boolean);
			_typeCodes.Add("ResourceTypeCode.Char", ResourceTypeCode.Char);
			_typeCodes.Add("ResourceTypeCode.Byte", ResourceTypeCode.Byte);
			_typeCodes.Add("ResourceTypeCode.SByte", ResourceTypeCode.SByte);
			_typeCodes.Add("ResourceTypeCode.Int16", ResourceTypeCode.Int16);
			_typeCodes.Add("ResourceTypeCode.UInt16", ResourceTypeCode.UInt16);
			_typeCodes.Add("ResourceTypeCode.Int32", ResourceTypeCode.Int32);
			_typeCodes.Add("ResourceTypeCode.UInt32", ResourceTypeCode.UInt32);
			_typeCodes.Add("ResourceTypeCode.Int64", ResourceTypeCode.Int64);
			_typeCodes.Add("ResourceTypeCode.UInt64", ResourceTypeCode.UInt64);
			_typeCodes.Add("ResourceTypeCode.Single", ResourceTypeCode.Single);
			_typeCodes.Add("ResourceTypeCode.Double", ResourceTypeCode.Double);
			_typeCodes.Add("ResourceTypeCode.Decimal", ResourceTypeCode.Decimal);
			_typeCodes.Add("ResourceTypeCode.DateTime", ResourceTypeCode.DateTime);
			_typeCodes.Add("ResourceTypeCode.TimeSpan", ResourceTypeCode.TimeSpan);
			_typeCodes.Add("ResourceTypeCode.ByteArray", ResourceTypeCode.ByteArray);
			_typeCodes.Add("ResourceTypeCode.Stream", ResourceTypeCode.Stream);
		}

		#endregion
	}
}
