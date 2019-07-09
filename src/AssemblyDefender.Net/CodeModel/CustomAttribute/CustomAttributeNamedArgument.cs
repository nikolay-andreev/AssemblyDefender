using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public struct CustomAttributeNamedArgument
	{
		#region Fields

		private string _name;
		private CustomAttributeNamedArgumentType _type;
		private CustomAttributeTypedArgument _value;

		#endregion

		#region Ctors

		public CustomAttributeNamedArgument(string name, CustomAttributeNamedArgumentType type, CustomAttributeTypedArgument value)
		{
			_name = name;
			_type = type;
			_value = value;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		public CustomAttributeNamedArgumentType Type
		{
			get { return _type; }
		}

		public CustomAttributeTypedArgument TypedValue
		{
			get { return _value; }
		}

		#endregion

		#region Static

		internal static CustomAttributeNamedArgument Load(IBinaryAccessor accessor, Module module)
		{
			var argument = new CustomAttributeNamedArgument();

			int argumentType = accessor.ReadByte();
			switch (argumentType)
			{
				case Metadata.ElementType.Field:
					argument._type = CustomAttributeNamedArgumentType.Field;
					break;

				case Metadata.ElementType.Property:
					argument._type = CustomAttributeNamedArgumentType.Property;
					break;

				default:
					throw new InvalidDataException();
			}

			bool isArray = false;
			int elementType = accessor.ReadByte();
			if (elementType == ElementType.SzArray)
			{
				elementType = accessor.ReadByte();
				isArray = true;
			}

			switch (elementType)
			{
				case ElementType.Enum:
					{
						string enumTypeName = CustomAttributeHelper.ReadBlobString(accessor);
						enumTypeName = enumTypeName.TrimEnd(new char[] { '\0' });
						argument._name = CustomAttributeHelper.ReadBlobString(accessor);
						int arrayLength = isArray ? CustomAttributeHelper.ReadArrayLength(accessor) : 1;
						argument._value = CustomAttributeTypedArgument.LoadEnum(accessor, module, enumTypeName, isArray, arrayLength);
					}
					break;

				default:
					{
						argument._name = CustomAttributeHelper.ReadBlobString(accessor);
						int arrayLength = isArray ? CustomAttributeHelper.ReadArrayLength(accessor) : 1;
						argument._value = CustomAttributeTypedArgument.Load(accessor, module, elementType, isArray, arrayLength);
					}
					break;
			}

			return argument;
		}

		#endregion
	}
}
