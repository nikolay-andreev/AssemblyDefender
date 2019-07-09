using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.UI.Model.Project
{
	internal class NodePrinter
	{
		#region Fields

		private StringBuilder _builder;
		private TypeDeclaration _type;
		private MethodDeclaration _method;

		#endregion

		#region Ctors

		private NodePrinter()
		{
			_builder = new StringBuilder();
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return _builder.ToString();
		}

		#endregion

		#region Type

		private void PrintType(TypeDeclaration type, bool ignoreEnclosingType, bool ignoreNamespace)
		{
			if (!ignoreEnclosingType)
			{
				var enclosingType = type.GetEnclosingType();
				if (enclosingType != null)
				{
					PrintType(enclosingType, ignoreEnclosingType, ignoreNamespace);
					_builder.Append(".");
				}
			}

			if (!ignoreNamespace)
			{
				if (!string.IsNullOrEmpty(type.Namespace))
				{
					_builder.Append(type.Namespace);
					_builder.Append(".");
				}
			}

			PrintTypeName(type.Name, type.GenericParameters.Count > 0);
			PrintGenericParameters(type.GenericParameters);
		}

		private void PrintTypeSig(TypeSignature typeSig)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					{
						if (typeSig.ElementType != null)
							PrintTypeSig(typeSig.ElementType);

						PrintArrayDimensions(typeSig.ArrayDimensions);
					}
					break;

				case TypeElementCode.ByRef:
					{
						if (typeSig.ElementType != null)
							PrintTypeSig(typeSig.ElementType);

						_builder.Append("&");
					}
					break;

				case TypeElementCode.CustomModifier:
					{
						if (typeSig.ElementType != null)
							PrintTypeSig(typeSig.ElementType);
					}
					break;

				case TypeElementCode.FunctionPointer:
					{
						var callSite = ((FunctionPointer)typeSig).CallSite;

						PrintTypeSig(callSite.ReturnType);
						_builder.Append(" *");
						PrintMethodArguments(callSite.Arguments);
					}
					break;

				case TypeElementCode.GenericParameter:
					{
						PrintGenericParameterType((GenericParameterType)typeSig);
					}
					break;

				case TypeElementCode.GenericType:
					{
						PrintTypeRef(typeSig.DeclaringType, typeSig.GenericArguments.Count > 0);
						PrintGenericArguments(typeSig.GenericArguments);
					}
					break;

				case TypeElementCode.Pinned:
					{
						if (typeSig.ElementType != null)
							PrintTypeSig(typeSig.ElementType);
					}
					break;

				case TypeElementCode.Pointer:
					{
						if (typeSig.ElementType != null)
							PrintTypeSig(typeSig.ElementType);

						_builder.Append("*");
					}
					break;

				case TypeElementCode.DeclaringType:
					PrintTypeRef((TypeReference)typeSig);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private void PrintArrayDimensions(IReadOnlyList<ArrayDimension> dimensions)
		{
			if (dimensions.Count > 0)
			{
				_builder.Append("[");

				for (int i = 0; i < dimensions.Count; i++)
				{
					if (i > 0)
						_builder.Append(",");

					PrintArrayDimension(dimensions[i]);
				}

				_builder.Append("]");
			}
			else
			{
				_builder.Append("[]");
			}
		}

		private void PrintArrayDimension(ArrayDimension dimension)
		{
			if (dimension.LowerBound.HasValue)
			{
				if (dimension.UpperBound.HasValue)
				{
					_builder.Append(dimension.LowerBound.Value);
					_builder.Append("...");
					_builder.Append(dimension.UpperBound.Value);
				}
				else
				{
					_builder.Append(dimension.LowerBound.Value);
					_builder.Append("...");
				}
			}
			else if (dimension.UpperBound.HasValue)
			{
				_builder.Append(dimension.UpperBound.Value);
			}
			else
			{
				_builder.Append("...");
			}
		}

		private void PrintGenericParameterType(GenericParameterType typeSig)
		{
			GenericParameterCollection genericParameters = null;
			if (typeSig.IsMethod)
			{
				if (_method != null)
					genericParameters = _method.GenericParameters;
			}
			else
			{
				genericParameters = _type.GenericParameters;
			}

			if (genericParameters != null && genericParameters.Count > typeSig.Position)
			{
				var genericParameter = genericParameters[typeSig.Position];
				if (!string.IsNullOrEmpty(genericParameter.Name))
				{
					_builder.Append(genericParameter.Name);
					return;
				}
			}

			if (typeSig.IsMethod)
				_builder.Append("!!");
			else
				_builder.Append("!");

			_builder.Append(typeSig.Position);
		}

		private void PrintTypeRef(TypeReference typeRef, bool stripGenericSuffix = false)
		{
			var typeCode = typeRef.GetTypeCode(_type.Module);
			switch (typeCode)
			{
				case PrimitiveTypeCode.Boolean:
					_builder.Append("Boolean");
					break;

				case PrimitiveTypeCode.Char:
					_builder.Append("Char");
					break;

				case PrimitiveTypeCode.Int8:
					_builder.Append("Sbyte");
					break;

				case PrimitiveTypeCode.Int16:
					_builder.Append("Int16");
					break;

				case PrimitiveTypeCode.Int32:
					_builder.Append("Int32");
					break;

				case PrimitiveTypeCode.Int64:
					_builder.Append("Int64");
					break;

				case PrimitiveTypeCode.UInt8:
					_builder.Append("Byte");
					break;

				case PrimitiveTypeCode.UInt16:
					_builder.Append("UInt16");
					break;

				case PrimitiveTypeCode.UInt32:
					_builder.Append("UInt32");
					break;

				case PrimitiveTypeCode.UInt64:
					_builder.Append("UInt64");
					break;

				case PrimitiveTypeCode.Float32:
					_builder.Append("Single");
					break;

				case PrimitiveTypeCode.Float64:
					_builder.Append("Double");
					break;

				case PrimitiveTypeCode.Object:
					_builder.Append("Object");
					break;

				case PrimitiveTypeCode.String:
					_builder.Append("String");
					break;

				case PrimitiveTypeCode.Void:
					_builder.Append("Void");
					break;

				default:
					PrintTypeName(typeRef, stripGenericSuffix);
					break;
			}
		}

		private void PrintTypeName(TypeReference typeRef, bool stripGenericSuffix)
		{
			var owner = typeRef.Owner;
			if (owner != null && owner.SignatureType == SignatureType.Type)
			{
				PrintTypeName((TypeReference)owner, stripGenericSuffix);
				_builder.Append(".");
			}

			PrintTypeName(typeRef.Name, stripGenericSuffix);
		}

		private void PrintTypeName(string typeName, bool stripGenericSuffix)
		{
			if (string.IsNullOrEmpty(typeName))
			{
				typeName = "<Unknown>";
			}
			else if (stripGenericSuffix)
			{
				// Strip ending generic count: `1
				for (int i = typeName.Length - 1; i >= 0; i--)
				{
					if (char.IsNumber(typeName[i]))
						continue;

					if (typeName[i] != '`' || i == typeName.Length - 1)
						break;

					typeName = typeName.Substring(0, i);
					break;
				}
			}

			_builder.Append(typeName);
		}

		private void PrintGenericParameters(GenericParameterCollection genericParameters)
		{
			if (genericParameters.Count == 0)
				return;

			_builder.Append("<");

			for (int i = 0; i < genericParameters.Count; i++)
			{
				if (i > 0)
					_builder.Append(", ");

				var genericParameter = genericParameters[i];
				if (!string.IsNullOrEmpty(genericParameter.Name))
				{
					_builder.Append(genericParameter.Name);
				}
			}

			_builder.Append(">");
		}

		private void PrintGenericArguments(IReadOnlyList<TypeSignature> genericArguments)
		{
			if (genericArguments.Count == 0)
				return;

			_builder.Append("<");

			for (int i = 0; i < genericArguments.Count; i++)
			{
				if (i > 0)
					_builder.Append(", ");

				PrintTypeSig(genericArguments[i]);
			}

			_builder.Append(">");
		}

		#endregion

		#region Method

		private void PrintMethod(MethodDeclaration method)
		{
			_builder.Append(method.Name);
			PrintGenericParameters(method.GenericParameters);
			PrintMethodParameters(method.Parameters);

			if (!method.IsConstructor())
			{
				_builder.Append(" : ");
				PrintTypeSig(method.ReturnType.Type);
			}
		}

		private void PrintMethodParameters(MethodParameterCollection parameters)
		{
			_builder.Append("(");

			for (int i = 0; i < parameters.Count; i++)
			{
				if (i > 0)
					_builder.Append(", ");

				PrintTypeSig(parameters[i].Type);
			}

			_builder.Append(")");
		}

		private void PrintMethodArguments(IReadOnlyList<TypeSignature> arguments)
		{
			_builder.Append("(");

			for (int i = 0; i < arguments.Count; i++)
			{
				if (i > 0)
					_builder.Append(", ");

				PrintTypeSig(arguments[i]);
			}

			_builder.Append(")");
		}

		#endregion

		#region Field

		private void PrintField(FieldDeclaration field, FieldNodeKind kind)
		{
			_builder.Append(field.Name);

			if (kind == FieldNodeKind.Field)
			{
				_builder.Append(" : ");
				PrintTypeSig(field.FieldType);
			}
		}

		#endregion

		#region Property

		private void PrintProperty(PropertyDeclaration property)
		{
			_builder.Append(property.Name);
			PrintPropertyParameters(property.Parameters);
			_builder.Append(" : ");
			PrintTypeSig(property.ReturnType);
		}

		private void PrintPropertyParameters(PropertyParameterCollection parameters)
		{
			if (parameters.Count == 0)
				return;

			_builder.Append("(");

			for (int i = 0; i < parameters.Count; i++)
			{
				if (i > 0)
					_builder.Append(", ");

				PrintTypeSig(parameters[i]);
			}

			_builder.Append(")");
		}

		#endregion

		#region Event

		private void PrintEvent(EventDeclaration e)
		{
			_builder.Append(e.Name);
		}

		#endregion

		#region Static

		internal static string Print(TypeDeclaration type, bool ignoreEnclosingType = false, bool ignoreNamespace = false)
		{
			var printer = new NodePrinter();
			printer._type = type;
			printer.PrintType(type, ignoreEnclosingType, ignoreNamespace);

			return printer.ToString();
		}

		internal static string Print(MethodDeclaration method)
		{
			var printer = new NodePrinter();
			printer._method = method;
			printer._type = method.GetOwnerType();
			printer.PrintMethod(method);

			return printer.ToString();
		}

		internal static string Print(FieldDeclaration field, FieldNodeKind kind)
		{
			var printer = new NodePrinter();
			printer._type = field.GetOwnerType();
			printer.PrintField(field, kind);

			return printer.ToString();
		}

		internal static string Print(PropertyDeclaration property)
		{
			var printer = new NodePrinter();
			printer._type = property.GetOwnerType();
			printer.PrintProperty(property);

			return printer.ToString();
		}

		internal static string Print(EventDeclaration e)
		{
			var printer = new NodePrinter();
			printer._type = e.GetOwnerType();
			printer.PrintEvent(e);

			return printer.ToString();
		}

		#endregion
	}
}
