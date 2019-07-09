using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using SysRef = System.Reflection;

namespace AssemblyDefender.Net
{
	public class SignaturePrinter
	{
		#region Fields

		private SignaturePrintingFlags _flags;
		private StringBuilder _builder;

		#endregion

		#region Ctors

		public SignaturePrinter(SignaturePrintingFlags flags)
			: this(flags, null)
		{
		}

		public SignaturePrinter(SignaturePrintingFlags flags, StringBuilder builder)
		{
			_flags = flags;
			_builder = builder ?? new StringBuilder(0x60);
		}

		#endregion

		#region Properties

		public SignaturePrintingFlags Flags
		{
			get { return _flags; }
			set { _flags = value; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return _builder.ToString();
		}

		private void PrintIdentifier(string identifier)
		{
			if ((_flags & SignaturePrintingFlags.EscapeIdentifiers) == SignaturePrintingFlags.EscapeIdentifiers)
			{
				_builder.Append("'");
				_builder.Append(identifier ?? "");
				_builder.Append("'");
			}
			else
			{
				_builder.Append(identifier ?? "");
			}
		}

		#endregion

		#region Assembly

		public void PrintAssembly(IAssemblySignature assemblySig)
		{
			PrintAssembly(_builder, assemblySig, _flags);
		}

		public void PrintAssembly(SysRef.Assembly assembly)
		{
			PrintAssembly(assembly.GetName());
		}

		public void PrintAssembly(SysRef.AssemblyName assembly)
		{
			_builder.Append(assembly.Name);

			if ((_flags & SignaturePrintingFlags.IgnoreAssemblyStrongName) != SignaturePrintingFlags.IgnoreAssemblyStrongName)
			{
				bool ignoreDefaultValues = (_flags & SignaturePrintingFlags.IgnoreAssemblyStrongNameDefaultValues) == SignaturePrintingFlags.IgnoreAssemblyStrongNameDefaultValues;

				// Version
				if (assembly.Version != null)
					_builder.AppendFormat(", Version={0}", assembly.Version.ToString());
				else if (!ignoreDefaultValues)
					_builder.Append(", Version=0.0.0.0");

				// Culture
				var culture = assembly.CultureInfo;
				if (culture != null)
					_builder.AppendFormat(", Culture={0}", culture.Name);
				else if (!ignoreDefaultValues)
					_builder.Append(", Culture=neutral");

				// PublicKeyToken
				var publicKeyToken = assembly.GetPublicKeyToken();
				if (publicKeyToken != null && publicKeyToken.Length > 0)
					_builder.AppendFormat(", PublicKeyToken={0}", ConvertUtils.ToHexString(publicKeyToken).ToLower());
				else if (!ignoreDefaultValues)
					_builder.Append(", PublicKeyToken=null");

				// ProcessorArchitecture
				string procArchString;
				switch (assembly.ProcessorArchitecture)
				{
					case SysRef.ProcessorArchitecture.MSIL:
						procArchString = "msil";
						break;

					case SysRef.ProcessorArchitecture.X86:
						procArchString = "x86";
						break;

					case SysRef.ProcessorArchitecture.Amd64:
						procArchString = "amd64";
						break;

					case SysRef.ProcessorArchitecture.IA64:
						procArchString = "ia64";
						break;

					default:
						procArchString = null;
						break;
				}

				if (procArchString != null)
					_builder.AppendFormat(", ProcessorArchitecture={0}", procArchString);
			}
		}

		internal static void PrintAssembly(StringBuilder builder, IAssemblySignature assemblySig, SignaturePrintingFlags flags)
		{
			builder.Append(assemblySig.Name);

			if ((flags & SignaturePrintingFlags.IgnoreAssemblyStrongName) != SignaturePrintingFlags.IgnoreAssemblyStrongName)
			{
				bool ignoreDefaultValues = (flags & SignaturePrintingFlags.IgnoreAssemblyStrongNameDefaultValues) == SignaturePrintingFlags.IgnoreAssemblyStrongNameDefaultValues;

				// Version
				if (assemblySig.Version != null)
					builder.AppendFormat(", Version={0}", assemblySig.Version.ToString());
				else if (!ignoreDefaultValues)
					builder.Append(", Version=0.0.0.0");

				// Culture
				if (!string.IsNullOrEmpty(assemblySig.Culture))
					builder.AppendFormat(", Culture={0}", assemblySig.Culture);
				else if (!ignoreDefaultValues)
					builder.Append(", Culture=neutral");

				// PublicKeyToken
				if (assemblySig.PublicKeyToken != null && assemblySig.PublicKeyToken.Length > 0)
					builder.AppendFormat(", PublicKeyToken={0}", ConvertUtils.ToHexString(assemblySig.PublicKeyToken).ToLower());
				else if (!ignoreDefaultValues)
					builder.Append(", PublicKeyToken=null");

				// ProcessorArchitecture
				string procArchString;
				switch (assemblySig.ProcessorArchitecture)
				{
					case ProcessorArchitecture.MSIL:
						procArchString = "msil";
						break;

					case ProcessorArchitecture.X86:
						procArchString = "x86";
						break;

					case ProcessorArchitecture.Amd64:
						procArchString = "amd64";
						break;

					case ProcessorArchitecture.IA64:
						procArchString = "ia64";
						break;

					default:
						procArchString = null;
						break;
				}

				if (procArchString != null)
					builder.AppendFormat(", ProcessorArchitecture={0}", procArchString);
			}
		}

		#endregion

		#region Module

		public void PrintModule(IModule module)
		{
			PrintModule(module.Name);
		}

		public void PrintModule(IModuleSignature moduleSig)
		{
			PrintModule(moduleSig.Name);
		}

		public void PrintModule(SysRef.Module module)
		{
			PrintModule(module.Name);
		}

		private void PrintModule(string name)
		{
			_builder.AppendFormat("module: {0}", name);
		}

		#endregion

		#region Type

		public void PrintType(IType type)
		{
			PrintType(type, type.Module);
		}

		public void PrintType(ITypeSignature typeSig, IModule module)
		{
			PrintType(
				typeSig,
				module,
				(_flags & SignaturePrintingFlags.IgnoreTypeOwner) == SignaturePrintingFlags.IgnoreTypeOwner);
		}

		public void PrintType(Type type)
		{
			PrintType(
				type,
				(_flags & SignaturePrintingFlags.IgnoreTypeOwner) == SignaturePrintingFlags.IgnoreTypeOwner);
		}

		private void PrintType(ITypeSignature typeSig, IModule module, bool ignoreOwner)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					{
						PrintType(typeSig.ElementType, module, ignoreOwner);
						PrintArrayDimensions(typeSig.ArrayDimensions);
					}
					break;

				case TypeElementCode.ByRef:
					{
						PrintType(typeSig.ElementType, module, ignoreOwner);
						_builder.Append("&");
					}
					break;

				case TypeElementCode.CustomModifier:
					{
						PrintType(typeSig.ElementType, module, ignoreOwner);

						CustomModifierType modifierType;
						var modifier = typeSig.GetCustomModifier(out modifierType);
						if (modifier != null)
						{
							if (modifierType == CustomModifierType.ModOpt)
								_builder.Append(" modopt(");
							else
								_builder.Append(" modreq(");

							PrintType(modifier, module, false);

							_builder.Append(")");
						}
					}
					break;

				case TypeElementCode.FunctionPointer:
					{
						var callSite = typeSig.GetFunctionPointer();
						if (callSite.IsStatic)
							_builder.Append("static ");

						PrintMethodCallConv(callSite.CallConv);
						_builder.Append("*");
						PrintMethodArguments(callSite.Arguments, callSite.VarArgIndex, module);
						_builder.Append(" : ");
						PrintType(callSite.ReturnType, module, false);
					}
					break;

				case TypeElementCode.GenericParameter:
					{
						bool isMethod;
						int position;
						typeSig.GetGenericParameter(out isMethod, out position);

						if (isMethod)
							_builder.Append("!!");
						else
							_builder.Append("!");

						_builder.Append(position);
					}
					break;

				case TypeElementCode.GenericType:
					{
						PrintDeclaringType(typeSig.DeclaringType, module, ignoreOwner);
						PrintGenericArguments(typeSig.GenericArguments, module);
					}
					break;

				case TypeElementCode.Pinned:
					{
						PrintType(typeSig.ElementType, module, ignoreOwner);
						_builder.Append(" pinned");
					}
					break;

				case TypeElementCode.Pointer:
					{
						PrintType(typeSig.ElementType, module, ignoreOwner);
						_builder.Append("*");
					}
					break;

				case TypeElementCode.DeclaringType:
					{
						if ((_flags & SignaturePrintingFlags.UsePrimitiveTypes) == SignaturePrintingFlags.UsePrimitiveTypes)
						{
							var typeCode = typeSig.GetTypeCode(module);
							if (typeCode != PrimitiveTypeCode.Undefined)
							{
								_builder.Append(PrimitiveTypeNames[(int)typeCode]);
								return;
							}
						}

						PrintDeclaringType(typeSig, module, ignoreOwner);
					}
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private void PrintDeclaringType(ITypeSignature typeSig, IModule module, bool ignoreOwner)
		{
			var owner = typeSig.Owner;
			if (owner != null)
			{
				if (owner.SignatureType == SignatureType.Type)
				{
					PrintDeclaringType((ITypeSignature)owner, module, ignoreOwner);
					_builder.Append("/");
				}
				else if (!ignoreOwner)
				{
					PrintResolutionScope(owner);
				}
			}
			else if (!ignoreOwner && module != null)
			{
				if (module.IsPrimeModule)
					PrintResolutionScope(module.Assembly);
				else
					PrintResolutionScope(module);
			}

			PrintIdentifier(CodeModelUtils.GetTypeName(typeSig.Name, typeSig.Namespace));
		}

		private void PrintResolutionScope(ISignature signature)
		{
			_builder.Append("[");

			if (signature.SignatureType == SignatureType.Assembly)
				PrintAssembly((IAssemblySignature)signature);
			else
				PrintModule((IModuleSignature)signature);

			_builder.Append("]");
		}

		private void PrintType(Type type, bool ignoreOwner)
		{
			if ((_flags & SignaturePrintingFlags.UsePrimitiveTypes) == SignaturePrintingFlags.UsePrimitiveTypes)
			{
				var typeCode = type.GetTypeCode();
				if (typeCode != PrimitiveTypeCode.Undefined)
				{
					_builder.Append(PrimitiveTypeNames[(int)typeCode]);
					return;
				}
			}

			PrintDeclaringType(type, ignoreOwner);
		}

		private void PrintDeclaringType(Type type, bool ignoreOwner)
		{
			if (type.IsNested)
			{
				PrintDeclaringType(type.DeclaringType, ignoreOwner);
				_builder.Append("/");
			}
			else if (!ignoreOwner)
			{
				_builder.Append("[");

				var module = type.Module;
				var assembly = module.Assembly;
				if (module == assembly.ManifestModule)
					PrintAssembly(assembly);
				else
					PrintModule(module);

				_builder.Append("]");
			}

			PrintIdentifier(CodeModelUtils.GetTypeName(type.Name, type.Namespace));
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

		private void PrintGenericArguments(IReadOnlyList<ITypeSignature> genericArguments, IModule module)
		{
			if (genericArguments.Count == 0)
				return;

			_builder.Append("<");

			for (int i = 0; i < genericArguments.Count; i++)
			{
				if (i > 0)
					_builder.Append(", ");

				PrintType(genericArguments[i], module, false);
			}

			_builder.Append(">");
		}

		#endregion

		#region Method

		public void PrintMethod(IMethod method)
		{
			PrintMethod(method, method.Module);
		}

		public void PrintMethod(IMethodSignature methodSig, IModule module)
		{
			if (methodSig.IsStatic)
				_builder.Append("static ");

			PrintMethodCallConv(methodSig.CallConv);

			if ((_flags & SignaturePrintingFlags.IgnoreMemberOwner) != SignaturePrintingFlags.IgnoreMemberOwner)
			{
				var owner = methodSig.Owner;
				if (owner != null)
					PrintType(owner, module);
				else
					_builder.Append(CodeModelUtils.GlobalTypeName);

				_builder.Append("::");
			}

			PrintIdentifier(methodSig.Name);

			if (methodSig.GenericParameterCount > 0)
			{
				PrintGenericArguments(methodSig.GenericArguments, module);
			}

			PrintMethodArguments(methodSig.Arguments, methodSig.VarArgIndex, module);

			_builder.Append(" : ");
			PrintType(methodSig.ReturnType, module, false);
		}

		public void PrintMethod(SysRef.MethodInfo method)
		{
			if (method.IsStatic)
				_builder.Append("static ");

			var callConv = method.CallingConvention;

			if ((callConv & SysRef.CallingConventions.VarArgs) == SysRef.CallingConventions.VarArgs)
				_builder.Append("vararg ");

			// Owner
			if ((_flags & SignaturePrintingFlags.IgnoreMemberOwner) != SignaturePrintingFlags.IgnoreMemberOwner)
			{
				PrintType(method.DeclaringType);
				_builder.Append("::");
			}

			// Name
			PrintIdentifier(method.Name);

			// Generic arguments
			{
				var genericArguments = method.GetGenericArguments();
				if (genericArguments.Length > 0)
				{
					_builder.Append("<");

					for (int i = 0; i < genericArguments.Length; i++)
					{
						if (i > 0)
							_builder.Append(", ");

						PrintType(genericArguments[i], false);
					}

					_builder.Append(">");
				}
			}

			// Parameters
			{
				var parameters = method.GetParameters();
				_builder.Append("(");

				for (int i = 0; i < parameters.Length; i++)
				{
					if (i > 0)
						_builder.Append(", ");

					PrintType(parameters[i].ParameterType, false);
				}

				_builder.Append(")");
			}

			// Return type
			_builder.Append(" : ");
			PrintType(method.ReturnType, false);
		}

		private void PrintMethodCallConv(MethodCallingConvention callConv)
		{
			switch (callConv)
			{
				case MethodCallingConvention.C:
					_builder.Append("cdecl ");
					break;

				case MethodCallingConvention.FastCall:
					_builder.Append("fastcall ");
					break;

				case MethodCallingConvention.StdCall:
					_builder.Append("stdcall ");
					break;

				case MethodCallingConvention.ThisCall:
					_builder.Append("thiscall ");
					break;

				case MethodCallingConvention.VarArgs:
					_builder.Append("vararg ");
					break;
			}
		}

		private void PrintMethodArguments(IReadOnlyList<ITypeSignature> arguments, int varArgIndex, IModule module)
		{
			_builder.Append("(");

			for (int i = 0; i < arguments.Count; i++)
			{
				if (i > 0)
					_builder.Append(", ");

				if (i == varArgIndex)
					_builder.Append("..., ");

				PrintType(arguments[i], module, false);
			}

			_builder.Append(")");
		}

		#endregion

		#region Field

		public void PrintField(IField field)
		{
			PrintField(field, field.Module);
		}

		public void PrintField(IFieldSignature fieldSig, IModule module)
		{
			if ((_flags & SignaturePrintingFlags.IgnoreMemberOwner) != SignaturePrintingFlags.IgnoreMemberOwner)
			{
				var owner = fieldSig.Owner;
				if (owner != null)
					PrintType(owner, module);
				else
					_builder.Append(CodeModelUtils.GlobalTypeName);

				_builder.Append("::");
			}

			PrintIdentifier(fieldSig.Name);
			_builder.Append(" : ");
			PrintType(fieldSig.FieldType, module, false);
		}

		public void PrintField(SysRef.FieldInfo field)
		{
			if ((_flags & SignaturePrintingFlags.IgnoreMemberOwner) != SignaturePrintingFlags.IgnoreMemberOwner)
			{
				PrintType(field.DeclaringType);
				_builder.Append("::");
			}

			PrintIdentifier(field.Name);
			_builder.Append(" : ");
			PrintType(field.FieldType, false);
		}

		#endregion

		#region Property

		public void PrintProperty(IProperty property)
		{
			PrintProperty(property, property.Module);
		}

		public void PrintProperty(IPropertySignature propertySig, IModule module)
		{
			if ((_flags & SignaturePrintingFlags.IgnoreMemberOwner) != SignaturePrintingFlags.IgnoreMemberOwner)
			{
				var owner = propertySig.Owner;
				if (owner != null)
					PrintType(owner, module);
				else
					_builder.Append(CodeModelUtils.GlobalTypeName);

				_builder.Append("::");
			}

			PrintIdentifier(propertySig.Name);

			PrintMethodArguments(propertySig.Arguments, -1, module);

			_builder.Append(" : ");
			PrintType(propertySig.ReturnType, module, false);
		}

		public void PrintProperty(SysRef.PropertyInfo property)
		{
			if ((_flags & SignaturePrintingFlags.IgnoreMemberOwner) != SignaturePrintingFlags.IgnoreMemberOwner)
			{
				PrintType(property.DeclaringType);
				_builder.Append("::");
			}

			PrintIdentifier(property.Name);

			// Properties
			{
				var parameters = property.GetIndexParameters();

				_builder.Append("(");

				for (int i = 0; i < parameters.Length; i++)
				{
					if (i > 0)
						_builder.Append(", ");

					PrintType(parameters[i].ParameterType, false);
				}

				_builder.Append(")");
			}

			_builder.Append(" : ");
			PrintType(property.PropertyType, false);
		}

		#endregion

		#region Event

		public void PrintEvent(IEvent e)
		{
			PrintEvent(e, e.Module);
		}

		public void PrintEvent(IEventSignature eventSig, IModule module)
		{
			if ((_flags & SignaturePrintingFlags.IgnoreMemberOwner) != SignaturePrintingFlags.IgnoreMemberOwner)
			{
				var owner = eventSig.Owner;
				if (owner != null)
					PrintType(owner, module);
				else
					_builder.Append(CodeModelUtils.GlobalTypeName);

				_builder.Append("::");
			}

			PrintIdentifier(eventSig.Name);
			_builder.Append(" : ");
			PrintType(eventSig.EventType, module, false);
		}

		public void PrintEvent(SysRef.EventInfo eventInfo)
		{
			if ((_flags & SignaturePrintingFlags.IgnoreMemberOwner) != SignaturePrintingFlags.IgnoreMemberOwner)
			{
				PrintType(eventInfo.DeclaringType);
				_builder.Append("::");
			}

			PrintIdentifier(eventInfo.Name);
			_builder.Append(" : ");
			PrintType(eventInfo.EventHandlerType, false);
		}

		#endregion

		#region Static

		public static string PrintAssembly(string name, string culture, Version version, byte[] publicKeyToken, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var builder = new StringBuilder();

			builder.Append(name);

			if ((flags & SignaturePrintingFlags.IgnoreAssemblyStrongName) != SignaturePrintingFlags.IgnoreAssemblyStrongName)
			{
				bool ignoreDefaultValues = (flags & SignaturePrintingFlags.IgnoreAssemblyStrongNameDefaultValues) == SignaturePrintingFlags.IgnoreAssemblyStrongNameDefaultValues;

				// Version
				if (version != null)
					builder.AppendFormat(", Version={0}", version.ToString());
				else if (!ignoreDefaultValues)
					builder.Append(", Version=0.0.0.0");

				// Culture
				if (!string.IsNullOrEmpty(culture))
					builder.AppendFormat(", Culture={0}", culture);
				else if (!ignoreDefaultValues)
					builder.Append(", Culture=neutral");

				// PublicKeyToken
				if (publicKeyToken != null && publicKeyToken.Length > 0)
					builder.AppendFormat(", PublicKeyToken={0}", ConvertUtils.ToHexString(publicKeyToken).ToLower());
				else if (!ignoreDefaultValues)
					builder.Append(", PublicKeyToken=null");
			}

			return builder.ToString();
		}

		private static string[] PrimitiveTypeNames = new string[]
		{
			"bool",
			"char",
			"sbyte",
			"short",
			"int",
			"long",
			"byte",
			"ushort",
			"uint",
			"ulong",
			"float",
			"double",
			"intptr",
			"uintptr",
			"type",
			"typedref",
			"object",
			"string",
			"void",
		};

		#endregion
	}
}
