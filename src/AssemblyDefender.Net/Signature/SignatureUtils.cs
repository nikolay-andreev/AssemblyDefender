using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public static class SignatureUtils
	{
		#region Assembly

		public static IAssembly Resolve(this IAssemblySignature assemblySig, IModule context, bool throwOnFailure = false)
		{
			return context.AssemblyManager.Resolve(assemblySig, context, throwOnFailure);
		}

		public static string ToString(this IAssemblySignature assemblySig, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintAssembly(assemblySig);

			return printer.ToString();
		}

		public static string ToReflectionString(this IAssemblySignature assemblySig, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new ReflectionSignaturePrinter(flags);
			printer.PrintAssembly(assemblySig);

			return printer.ToString();
		}

		#endregion

		#region Module

		public static IModule Resolve(this IModuleSignature moduleSig, IModule context, bool throwOnFailure = false)
		{
			return context.AssemblyManager.Resolve(moduleSig, context, throwOnFailure);
		}

		public static string ToString(this IModuleSignature moduleSig, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintModule(moduleSig);

			return printer.ToString();
		}

		#endregion

		#region Type

		public static bool IsGlobal(this ITypeSignature typeSig)
		{
			if (typeSig.Name != CodeModelUtils.GlobalTypeName)
				return false;

			if (typeSig.Namespace != null)
				return false;

			if (typeSig.IsNested)
				return false;

			return true;
		}

		public static bool IsArray(this ITypeSignature typeSig)
		{
			return typeSig.ElementCode == TypeElementCode.Array;
		}

		public static bool IsByRef(this ITypeSignature typeSig)
		{
			return typeSig.ElementCode == TypeElementCode.ByRef;
		}

		public static bool IsPointer(this ITypeSignature typeSig)
		{
			return typeSig.ElementCode == TypeElementCode.Pointer;
		}

		public static bool IsPinned(this ITypeSignature typeSig)
		{
			return typeSig.ElementCode == TypeElementCode.Pinned;
		}

		public static bool IsFunctionPointer(this ITypeSignature typeSig)
		{
			return typeSig.ElementCode == TypeElementCode.FunctionPointer;
		}

		public static bool IsGenericParameter(this ITypeSignature typeSig)
		{
			return typeSig.ElementCode == TypeElementCode.GenericParameter;
		}

		public static bool IsGenericType(this ITypeSignature typeSig)
		{
			return typeSig.ElementCode == TypeElementCode.GenericType;
		}

		public static bool Equals(this ITypeSignature typeSig, IModule module, string fullName, string assemblyName)
		{
			if (typeSig.FullName != fullName)
				return false;

			if (GetAssembly(typeSig, module).Name != assemblyName)
				return false;

			return true;
		}

		public static bool Equals(this ITypeSignature typeSig, IModule module, string name, string ns, string assemblyName)
		{
			if (typeSig.Name != name)
				return false;

			if (typeSig.Namespace != ns)
				return false;

			if (GetAssembly(typeSig, module).Name != assemblyName)
				return false;

			return true;
		}

		public static PrimitiveTypeCode GetTypeCode(this ITypeSignature typeSig, IModule module)
		{
			if (typeSig.Namespace != "System")
				return PrimitiveTypeCode.Undefined;

			if (typeSig.IsNested)
				return PrimitiveTypeCode.Undefined;

			var assemblySig = typeSig.Owner as IAssemblySignature;
			if (assemblySig == null)
			{
				if (module == null)
					return PrimitiveTypeCode.Undefined;

				assemblySig = module.Assembly;
			}

			if (assemblySig.Name != "mscorlib")
				return PrimitiveTypeCode.Undefined;

			PrimitiveTypeCode typeCode;
			if (!CodeModelUtils.SystemTypeCodes.TryGetValue(typeSig.Name, out typeCode))
				return PrimitiveTypeCode.Undefined;

			return typeCode;
		}

		public static ITypeSignature GetOutermostType(this ITypeSignature typeSig)
		{
			while (true)
			{
				var enclosingTypeSig = typeSig.EnclosingType;
				if (enclosingTypeSig == null)
					break;

				typeSig = enclosingTypeSig;
			}

			return typeSig;
		}

		public static ITypeSignature GetElementType(this ITypeSignature typeSig, TypeElementCode type)
		{
			while (typeSig != null && typeSig.ElementCode != type)
			{
				typeSig = typeSig.ElementType;
			}

			return typeSig;
		}

		public static ITypeSignature GetDeclaringType(this ITypeSignature typeSig)
		{
			typeSig = typeSig.GetLastElementType();
			if (typeSig.ElementCode == TypeElementCode.GenericType)
			{
				typeSig = typeSig.DeclaringType;
			}

			if (typeSig.ElementCode == TypeElementCode.DeclaringType)
				return typeSig;

			return null;
		}

		public static ITypeSignature GetLastElementType(this ITypeSignature typeSig)
		{
			while (typeSig.ElementType != null)
			{
				typeSig = typeSig.ElementType;
			}

			return typeSig;
		}

		public static IAssemblySignature GetAssembly(this ITypeSignature typeSig, IModule module)
		{
			var resScope = typeSig.ResolutionScope;
			if (resScope != null && resScope.SignatureType == SignatureType.Assembly)
				return (IAssemblySignature)resScope;
			else
				return module.Assembly;
		}

		public static IType Resolve(this ITypeSignature typeSig, ICodeNode context, bool throwOnFailure = false)
		{
			return context.AssemblyManager.Resolve(typeSig, context, throwOnFailure);
		}

		public static TypeDeclaration ResolveDeclaringType(this ITypeSignature typeSig, IModule context, bool throwOnFailure = false)
		{
			TypeDeclaration type = null;
			var declaringTypeSig = typeSig.GetDeclaringType();
			if (declaringTypeSig != null)
			{
				type = context.AssemblyManager.Resolve(declaringTypeSig, context, false) as TypeDeclaration;
			}

			if (type == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(SR.TypeResolveError, typeSig.ToReflectionString()));
				}

				return null;
			}

			return type;
		}

		public static string ToString(this ITypeSignature typeSig, IModule module = null, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintType(typeSig, module);

			return printer.ToString();
		}

		public static string ToReflectionString(this ITypeSignature typeSig, IModule module = null, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new ReflectionSignaturePrinter(flags);
			printer.PrintType(typeSig, module);

			return printer.ToString();
		}

		internal static bool? TryGetIsValueType(ITypeSignature typeSig)
		{
			if (typeSig is IType)
				return ((IType)typeSig).IsValueType();
			else if (typeSig is TypeReference)
				return ((TypeReference)typeSig).IsValueType;
			else
				return null;
		}

		internal static bool EqualsValueTypeOrEnum(ITypeSignature typeSig, IAssembly owner)
		{
			if (typeSig.Namespace != "System")
				return false;

			if (typeSig.Name != "ValueType" && typeSig.Name != "Enum")
				return false;

			if (typeSig.IsNested)
				return false;

			var assemblySig = typeSig.Owner as IAssemblySignature;
			if (assemblySig == null)
				assemblySig = owner;

			if (assemblySig.Name != "mscorlib")
				return false;

			return true;
		}

		#endregion

		#region Method

		public static bool IsConstructor(this IMethodSignature methodSig)
		{
			return
				methodSig.Name == CodeModelUtils.MethodConstructorName ||
				methodSig.Name == CodeModelUtils.MethodStaticConstructorName;
		}

		public static bool IsInstanceConstructor(this IMethodSignature methodSig)
		{
			return methodSig.Name == CodeModelUtils.MethodConstructorName;
		}

		public static bool IsStaticConstructor(this IMethodSignature methodSig)
		{
			return methodSig.Name == CodeModelUtils.MethodStaticConstructorName;
		}

		public static int GetArgumentCountNoVarArgs(this IMethodSignature methodSig)
		{
			if (methodSig.CallConv == MethodCallingConvention.VarArgs && methodSig.VarArgIndex >= 0)
				return methodSig.VarArgIndex;

			return methodSig.Arguments.Count;
		}

		public static IMethod Resolve(this IMethodSignature methodSig, ICodeNode context, bool polymorphic = false, bool throwOnFailure = false)
		{
			return context.AssemblyManager.Resolve(methodSig, context, polymorphic, throwOnFailure);
		}

		public static string ToString(this IMethodSignature methodSig, IModule module = null, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintMethod(methodSig, module);

			return printer.ToString();
		}

		#endregion

		#region Field

		public static IField Resolve(this IFieldSignature fieldSig, ICodeNode context, bool throwOnFailure = false)
		{
			return context.AssemblyManager.Resolve(fieldSig, context, throwOnFailure);
		}

		public static string ToString(this IFieldSignature fieldSig, IModule module = null, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintField(fieldSig, module);

			return printer.ToString();
		}

		#endregion

		#region Property

		public static IProperty Resolve(this IPropertySignature propertySig, ICodeNode context, bool throwOnFailure = false)
		{
			return context.AssemblyManager.Resolve(propertySig, context, throwOnFailure);
		}

		public static string ToString(this IPropertySignature propertySig, IModule module = null, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintProperty(propertySig, module);

			return printer.ToString();
		}

		#endregion

		#region Event

		public static IEvent Resolve(this IEventSignature eventSig, ICodeNode context, bool throwOnFailure = false)
		{
			return context.AssemblyManager.Resolve(eventSig, context, throwOnFailure);
		}

		public static string ToString(this IEventSignature eventSig, IModule module = null, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintEvent(eventSig, module);

			return printer.ToString();
		}

		#endregion

		#region Map generic type

		public static Signature MapGenerics(this Signature signature, IReadOnlyList<TypeSignature> typeArguments, IReadOnlyList<TypeSignature> methodArguments)
		{
			var builder = new ReplaceGenericTypeBuilder(typeArguments, methodArguments);
			builder.Build(ref signature);

			return signature;
		}

		private class ReplaceGenericTypeBuilder : SignatureBuilder
		{
			#region Fields

			private int _typeArgumentCount;
			private int _methodArgumentCount;
			private IReadOnlyList<TypeSignature> _typeArguments;
			private IReadOnlyList<TypeSignature> _methodArguments;

			#endregion

			#region Ctors

			internal ReplaceGenericTypeBuilder(IReadOnlyList<TypeSignature> typeArguments, IReadOnlyList<TypeSignature> methodArguments)
			{
				if (typeArguments != null)
				{
					_typeArguments = typeArguments;
					_typeArgumentCount = typeArguments.Count;
				}

				if (methodArguments != null)
				{
					_methodArguments = methodArguments;
					_methodArgumentCount = methodArguments.Count;
				}
			}

			#endregion

			#region Methods

			public override bool Build(ref TypeSignature typeSig)
			{
				if (typeSig.ElementCode == TypeElementCode.GenericParameter)
				{
					var genericType = (GenericParameterType)typeSig;
					int position = genericType.Position;
					if (genericType.IsMethod)
					{
						// Method generic argument.
						if (position >= 0 && position < _methodArgumentCount)
						{
							typeSig = _methodArguments[position];
							return true;
						}
					}
					else
					{
						// Type generic argument.
						if (position >= 0 && position < _typeArgumentCount)
						{
							typeSig = _typeArguments[position];
							return true;
						}
					}
				}

				return base.Build(ref typeSig);
			}

			#endregion
		}

		#endregion

		#region Relocate owner

		public static Signature Relocate(this Signature signature, Module newModule, Module oldModule = null, bool ignoreStrongName = false)
		{
			var builder = new RelocateBuilder(newModule, oldModule, ignoreStrongName);
			builder.Build(ref signature);

			return signature;
		}

		private class RelocateBuilder : SignatureBuilder
		{
			#region Fields

			private bool _ignoreStrongName;
			private Assembly _newAssembly;
			private Module _newModule;
			private Signature _targetToOldModule;

			#endregion

			#region Ctors

			internal RelocateBuilder(Module newModule, Module oldModule, bool ignoreStrongName)
			{
				if (newModule == null)
					throw new ArgumentNullException("newModule");

				_newModule = newModule;
				_newAssembly = _newModule.Assembly;
				_ignoreStrongName = ignoreStrongName;

				if (oldModule != null)
				{
					// Same assembly ?
					if (oldModule.Assembly.Equals(newModule.Assembly))
					{
						// Same module ?
						if (oldModule.Name == newModule.Name)
						{
							_targetToOldModule = null;
						}
						else // Different module.
						{
							_targetToOldModule = oldModule.ToReference();
						}
					}
					else // Different assembly.
					{
						_targetToOldModule = oldModule.Assembly.ToReference();
					}
				}
			}

			#endregion

			#region Methods

			public override bool Build(ref Signature signature)
			{
				switch (signature.SignatureType)
				{
					case SignatureType.Assembly:
						{
							var assemblyRef = (AssemblyReference)signature;

							if (_ignoreStrongName)
							{
								if (assemblyRef.Name == _newAssembly.Name)
								{
									signature = null;
									return true;
								}
							}
							else
							{
								if (assemblyRef.Equals(_newAssembly))
								{
									signature = null;
									return true;
								}
							}

							return false;
						}

					case SignatureType.Module:
						{
							var moduleRef = (ModuleReference)signature;
							if (moduleRef.Name == _newModule.Name)
							{
								signature = null;
								return true;
							}

							// If different module.
							if (_targetToOldModule != null && _targetToOldModule.SignatureType == SignatureType.Assembly)
							{
								signature = _targetToOldModule;
								return true;
							}

							return false;
						}

					case SignatureType.File:
						{
							var fileRef = (FileReference)signature;
							if (!Build(ref fileRef))
								return false;

							signature = fileRef;
							return true;
						}

					case SignatureType.Type:
						{
							var typeSig = (TypeSignature)signature;
							if (!Build(ref typeSig))
								return false;

							signature = typeSig;
							return true;
						}

					case SignatureType.Method:
						{
							var methodSig = (MethodSignature)signature;
							if (!Build(ref methodSig))
								return false;

							signature = methodSig;
							return true;
						}

					case SignatureType.Field:
						{
							var fieldRef = (FieldReference)signature;
							if (!Build(ref fieldRef))
								return false;

							signature = fieldRef;
							return true;
						}

					default:
						throw new InvalidOperationException();
				}
			}

			public override bool Build(ref TypeReference typeRef)
			{
				bool changed = false;

				var owner = typeRef.Owner;
				if (owner != null)
				{
					changed |= Build(ref owner);
				}
				else
				{
					if (_targetToOldModule != null)
					{
						owner = _targetToOldModule;
						changed = true;
					}
				}

				if (!changed)
					return false;

				typeRef = new TypeReference(typeRef.Name, typeRef.Namespace, owner, typeRef.IsValueType);
				return true;
			}

			#endregion
		}

		#endregion
	}
}
