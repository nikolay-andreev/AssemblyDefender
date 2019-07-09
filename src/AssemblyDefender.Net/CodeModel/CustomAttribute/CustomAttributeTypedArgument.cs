using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public struct CustomAttributeTypedArgument
	{
		#region Fields

		private object _value;
		private TypeReference _type;

		#endregion

		#region Ctors

		public CustomAttributeTypedArgument(object value, TypeReference type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			_value = value;
			_type = type;
		}

		#endregion

		#region Properties

		public object Value
		{
			get { return _value; }
		}

		public TypeReference Type
		{
			get { return _type; }
		}

		#endregion

		#region Static

		internal static CustomAttributeTypedArgument Load(
			IBinaryAccessor accessor, Module module, TypeReference typeRef,
			bool isArray, int arrayLength)
		{
			var typeCode = typeRef.GetTypeCode(module);
			if (typeCode != PrimitiveTypeCode.Undefined)
			{
				int elementType = CodeModelUtils.GetElementType(typeCode);
				return Load(accessor, module, elementType, isArray, arrayLength);
			}

			return LoadEnumValue(accessor, module, typeRef, isArray, arrayLength);
		}

		internal static CustomAttributeTypedArgument LoadEnumValue(
			IBinaryAccessor accessor, Module module, TypeReference typeRef,
			bool isArray, int arrayLength)
		{
			var type = typeRef.Resolve(module);
			if (type == null)
			{
				throw new InvalidDataException();
			}

			if (type.Fields.Count == 0)
			{
				throw new InvalidDataException();
			}

			// First value of enum is __value field.
			var firstField = type.Fields[0];

			var typeCode = ((IFieldSignature)firstField).FieldType.GetTypeCode(type.Module);
			if (typeCode == PrimitiveTypeCode.Undefined)
			{
				throw new InvalidDataException();
			}

			int elementType = CodeModelUtils.GetElementType(typeCode);

			return Load(accessor, module, elementType, isArray, arrayLength);
		}

		internal static CustomAttributeTypedArgument Load(
			IBinaryAccessor accessor, Module module,
			int elementType, bool isArray, int arrayLength)
		{
			switch (elementType)
			{
				case ElementType.I1:
					return LoadInt8(accessor, module, isArray, arrayLength);

				case ElementType.I2:
					return LoadInt16(accessor, module, isArray, arrayLength);

				case ElementType.I4:
					return LoadInt32(accessor, module, isArray, arrayLength);

				case ElementType.I8:
					return LoadInt64(accessor, module, isArray, arrayLength);

				case ElementType.U1:
					return LoadUInt8(accessor, module, isArray, arrayLength);

				case ElementType.U2:
					return LoadUInt16(accessor, module, isArray, arrayLength);

				case ElementType.U4:
					return LoadUInt32(accessor, module, isArray, arrayLength);

				case ElementType.U8:
					return LoadUInt64(accessor, module, isArray, arrayLength);

				case ElementType.R4:
					return LoadFloat32(accessor, module, isArray, arrayLength);

				case ElementType.R8:
					return LoadFloat64(accessor, module, isArray, arrayLength);

				case ElementType.Char:
					return LoadChar(accessor, module, isArray, arrayLength);

				case ElementType.Boolean:
					return LoadBool(accessor, module, isArray, arrayLength);

				case ElementType.String:
					return LoadString(accessor, module, isArray, arrayLength);

				case ElementType.Type:
					return LoadType(accessor, module, isArray, arrayLength);

				case ElementType.Object:
				case ElementType.Boxed:
					return LoadObject(accessor, module, isArray, arrayLength);

				default:
					throw new InvalidDataException();
			}
		}

		internal static CustomAttributeTypedArgument LoadEnum(IBinaryAccessor accessor, Module module, string enumTypeName, bool isArray, int arrayLength)
		{
			var enumType = TypeSignature.Parse(enumTypeName, true) as TypeReference;
			if (enumType == null)
			{
				throw new InvalidDataException();
			}

			enumType = (TypeReference)enumType.Relocate(module);

			object value = LoadEnumValue(accessor, module, enumType, isArray, arrayLength);

			return new CustomAttributeTypedArgument(value, enumType);
		}

		private static CustomAttributeTypedArgument LoadInt8(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int8, module.Assembly);

			if (isArray)
			{
				var value = new sbyte[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadSByte();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				sbyte value = accessor.ReadSByte();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadInt16(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int16, module.Assembly);

			if (isArray)
			{
				var value = new short[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadInt16();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				short value = accessor.ReadInt16();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadInt32(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, module.Assembly);

			if (isArray)
			{
				var value = new int[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadInt32();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				int value = accessor.ReadInt32();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadInt64(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, module.Assembly);

			if (isArray)
			{
				var value = new long[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadInt64();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				long value = accessor.ReadInt64();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadUInt8(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, module.Assembly);

			if (isArray)
			{
				var value = new byte[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadByte();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				byte value = accessor.ReadByte();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadUInt16(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt16, module.Assembly);

			if (isArray)
			{
				var value = new ushort[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadUInt16();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				ushort value = accessor.ReadUInt16();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadUInt32(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt32, module.Assembly);

			if (isArray)
			{
				var value = new uint[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadUInt32();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				uint value = accessor.ReadUInt32();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadUInt64(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt64, module.Assembly);

			if (isArray)
			{
				var value = new ulong[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadUInt64();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				ulong value = accessor.ReadUInt64();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadFloat32(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Float32, module.Assembly);

			if (isArray)
			{
				var value = new float[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadSingle();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				float value = accessor.ReadSingle();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadFloat64(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Float64, module.Assembly);

			if (isArray)
			{
				var value = new double[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadDouble();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				double value = accessor.ReadDouble();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadChar(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Char, module.Assembly);

			if (isArray)
			{
				var value = new short[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadInt16();
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				short value = accessor.ReadInt16();
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadBool(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, module.Assembly);

			if (isArray)
			{
				var value = new bool[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = accessor.ReadByte() != 0;
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				bool value = accessor.ReadByte() != 0;
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadString(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, module.Assembly);

			if (isArray)
			{
				var value = new string[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = CustomAttributeHelper.ReadBlobString(accessor);
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				string value = CustomAttributeHelper.ReadBlobString(accessor);
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadType(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, module.Assembly);

			if (isArray)
			{
				var value = new TypeSignature[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = LoadType(accessor, module);
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				var value = LoadType(accessor, module);
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static TypeSignature LoadType(IBinaryAccessor accessor, Module module)
		{
			string typeName = CustomAttributeHelper.ReadBlobString(accessor);
			if (typeName != null)
				typeName = typeName.TrimEnd(new char[] { '\0' });

			if (string.IsNullOrEmpty(typeName))
				return null;

			var typeSig = TypeSignature.Parse(typeName, true);
			typeSig = (TypeSignature)typeSig.Relocate(module);

			return typeSig;
		}

		private static CustomAttributeTypedArgument LoadObject(IBinaryAccessor accessor, Module module, bool isArray, int arrayLength)
		{
			var type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, module.Assembly);

			if (isArray)
			{
				var value = new CustomAttributeTypedArgument[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					value[i] = LoadObject(accessor, module);
				}

				return new CustomAttributeTypedArgument(value, type);
			}
			else
			{
				var value = LoadObject(accessor, module);
				return new CustomAttributeTypedArgument(value, type);
			}
		}

		private static CustomAttributeTypedArgument LoadObject(IBinaryAccessor accessor, Module module)
		{
			bool isArray = false;
			int elementType = (int)accessor.ReadByte();
			if (elementType == ElementType.SzArray)
			{
				isArray = true;
				elementType = (int)accessor.ReadByte();
			}

			switch (elementType)
			{
				case ElementType.Enum:
					{
						string enumTypeName = CustomAttributeHelper.ReadBlobString(accessor);
						int length = isArray ? CustomAttributeHelper.ReadArrayLength(accessor) : 1;
						return LoadEnum(accessor, module, enumTypeName, isArray, length);
					}

				default:
					{
						int length = isArray ? CustomAttributeHelper.ReadArrayLength(accessor) : 1;
						return Load(accessor, module, elementType, isArray, length);
					}
			}
		}

		#endregion
	}
}
