using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public static class CodeModelUtils
	{
		public static readonly string ExeExtension = ".exe";
		public static readonly string DllExtension = ".dll";
		public static readonly string MethodConstructorName = ".ctor";
		public static readonly string MethodStaticConstructorName = ".cctor";
		public static readonly string GlobalTypeName = "<Module>";
		public static readonly string MscorlibName = "mscorlib";
		public static readonly string NeutralCulture = "neutral";

		/// <summary>
		/// Ecma defined public key token.
		/// </summary>
		public static readonly byte[] EcmaPublicKeyToken = new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89, };

		#region CodeNode

		public static Signature ToSignature(this ICodeNode node, IModule owner = null)
		{
			switch (node.EntityType)
			{
				case EntityType.Assembly:
					return ToReference((IAssembly)node);

				case EntityType.Module:
					return ToReference((IModule)node);

				case EntityType.Type:
					return ToSignature((IType)node, owner);

				case EntityType.Method:
					return ToSignature((IMethod)node, owner);

				case EntityType.Field:
					return ToReference((IField)node, owner);

				case EntityType.Property:
					return ToReference((IProperty)node, owner);

				case EntityType.Event:
					return ToReference((IEvent)node, owner);

				default:
					throw new NotImplementedException();
			}
		}

		#endregion

		#region Assembly

		public static IModule GetModule(this IAssembly assembly, string name, bool throwIfMissing = false)
		{
			foreach (var module in assembly.Modules)
			{
				if (module.Name == name)
					return module;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.ModuleNotFound, name));
			}

			return null;
		}

		public static IType GetType(this IAssembly assembly, string name, string ns, bool throwIfMissing = false)
		{
			foreach (var type in assembly.Types)
			{
				if (type.Name == name && type.Namespace == ns)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, GetTypeName(name, ns)));
			}

			return null;
		}

		public static IType GetType(this IAssembly assembly, string fullName, bool throwIfMissing = false)
		{
			foreach (var type in assembly.Types)
			{
				if (type.FullName == fullName)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, fullName));
			}

			return null;
		}

		public static IType GetExportedType(this IAssembly assembly, string name, string ns, bool throwIfMissing = false)
		{
			foreach (var type in assembly.ExportedTypes)
			{
				if (type.Name == name && type.Namespace == ns)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, GetTypeName(name, ns)));
			}

			return null;
		}

		public static IType GetExportedType(this IAssembly assembly, string fullName, bool throwIfMissing = false)
		{
			foreach (var type in assembly.ExportedTypes)
			{
				if (type.FullName == fullName)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, fullName));
			}

			return null;
		}

		public static IType GetTypeOrExportedType(this IAssembly assembly, string name, string ns, bool throwIfMissing = false)
		{
			foreach (var type in assembly.Types)
			{
				if (type.Name == name && type.Namespace == ns)
					return type;
			}

			foreach (var type in assembly.ExportedTypes)
			{
				if (type.Name == name && type.Namespace == ns && !type.IsNested)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, GetTypeName(name, ns)));
			}

			return null;
		}

		public static IType GetTypeOrExportedType(this IAssembly assembly, string fullName, bool throwIfMissing = false)
		{
			foreach (var type in assembly.Types)
			{
				if (type.FullName == fullName)
					return type;
			}

			foreach (var type in assembly.ExportedTypes)
			{
				if (type.FullName == fullName)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, fullName));
			}

			return null;
		}

		public static IAssembly Find(this IReadOnlyList<IAssembly> assemblys, IAssemblySignature searchAssembly, bool throwIfMissing = false)
		{
			return Find(assemblys, searchAssembly, SignatureComparer.Default, throwIfMissing);
		}

		public static IAssembly Find(this IReadOnlyList<IAssembly> assemblys, IAssemblySignature searchAssembly, SignatureComparer comparer, bool throwIfMissing = false)
		{
			foreach (var assembly in assemblys)
			{
				if (comparer.Equals(assembly, searchAssembly))
					return assembly;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.AssemblyNotFound, searchAssembly.ToString()));
			}

			return null;
		}

		public static AssemblyReference ToReference(this IAssembly assembly)
		{
			return new AssemblyReference(
				assembly.Name,
				assembly.Culture,
				assembly.Version,
				assembly.PublicKeyToken,
				assembly.ProcessorArchitecture);
		}

		public static AssemblyReference ToReference(this System.Reflection.AssemblyName assembly)
		{
			return new AssemblyReference(
				assembly.Name,
				assembly.CultureInfo != null ? assembly.CultureInfo.Name : null,
				assembly.Version,
				assembly.GetPublicKeyToken().NullIfEmpty(),
				(ProcessorArchitecture)assembly.ProcessorArchitecture);
		}

		#endregion

		#region Module

		public static IType GetType(this IModule module, string name, string ns, bool throwIfMissing = false)
		{
			foreach (var type in module.Types)
			{
				if (type.Name == name && type.Namespace == ns)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, GetTypeName(name, ns)));
			}

			return null;
		}

		public static ModuleReference ToReference(this IModule module)
		{
			return new ModuleReference(module.Name);
		}

		#endregion

		#region Type

		public static string GetTypeName(string name, string ns)
		{
			if (ns != null)
				return ns + "." + name;
			else
				return name;
		}

		public static bool IsValueType(this IType type)
		{
			if (type.IsInterface)
				return false;

			var baseTypeSig = ((ITypeBase)type).BaseType;
			if (baseTypeSig == null)
				return false;

			if (!SignatureUtils.EqualsValueTypeOrEnum(baseTypeSig, type.Assembly))
				return false;

			if (SignatureUtils.EqualsValueTypeOrEnum(type, type.Assembly))
				return false;

			return true;
		}

		public static bool IsVisibleOutsideAssembly(this IType type)
		{
			var enclosingType = type.EnclosingType;
			if (enclosingType != null && !IsVisibleOutsideAssembly(enclosingType))
				return false;

			switch (type.Visibility)
			{
				case TypeVisibilityFlags.Public:
				case TypeVisibilityFlags.NestedPublic:
					return true;

				case TypeVisibilityFlags.NestedFamily:
				case TypeVisibilityFlags.NestedFamOrAssem:
					return enclosingType != null && !enclosingType.IsSealed;
			}

			return false;
		}

		public static bool HasMembers(this IType type)
		{
			return
				type.Fields.Count > 0 ||
				type.Methods.Count > 0 ||
				type.Properties.Count > 0 ||
				type.Events.Count > 0 ||
				type.NestedTypes.Count > 0;
		}

		public static bool Equals(this IType type, string fullName, string assemblyName)
		{
			if (type.FullName != fullName)
				return false;

			if (type.Assembly.Name != assemblyName)
				return false;

			return true;
		}

		public static bool Equals(this IType type, string name, string ns, string assemblyName)
		{
			if (type.Name != name)
				return false;

			if (type.Namespace != ns)
				return false;

			if (type.Assembly.Name != assemblyName)
				return false;

			return true;
		}

		public static PrimitiveTypeCode GetTypeCode(this IType type)
		{
			if (type.Namespace != "System")
				return PrimitiveTypeCode.Undefined;

			if (type.IsNested)
				return PrimitiveTypeCode.Undefined;

			if (type.Assembly.Name != "mscorlib")
				return PrimitiveTypeCode.Undefined;

			PrimitiveTypeCode typeCode;
			if (!SystemTypeCodes.TryGetValue(type.Name, out typeCode))
				return PrimitiveTypeCode.Undefined;

			return typeCode;
		}

		public static PrimitiveTypeCode GetTypeCode(this Type type)
		{
			if (type.Namespace != "System")
				return PrimitiveTypeCode.Undefined;

			if (type.IsNested)
				return PrimitiveTypeCode.Undefined;

			if (type.Assembly.GetName().Name != "mscorlib")
				return PrimitiveTypeCode.Undefined;

			PrimitiveTypeCode typeCode;
			if (!CodeModelUtils.SystemTypeCodes.TryGetValue(type.Name, out typeCode))
				return PrimitiveTypeCode.Undefined;

			return typeCode;
		}

		public static bool IsTypeOf(this IType type, IType baseType)
		{
			while (type != null)
			{
				if (type == baseType)
					return true;

				type = type.BaseType;
			}

			return false;
		}

		public static bool IsTypeOf(this IType type, string name, string ns, string assemblyName)
		{
			while (type != null)
			{
				if (Equals(type, name, ns, assemblyName))
					return true;

				type = type.BaseType;
			}

			return false;
		}

		public static bool IsSubclassOf(this IType type, IType baseType)
		{
			type = type.BaseType;
			while (type != null)
			{
				if (type == baseType)
					return true;

				type = type.BaseType;
			}

			return false;
		}

		public static bool IsSubclassOf(this IType type, string name, string ns, string assemblyName)
		{
			type = type.BaseType;
			while (type != null)
			{
				if (Equals(type, name, ns, assemblyName))
					return true;

				type = type.BaseType;
			}

			return false;
		}

		public static bool IsNestedIn(this IType type, IType enclosingType)
		{
			type = type.EnclosingType;
			while (type != null)
			{
				if (type == enclosingType)
					return true;

				type = type.EnclosingType;
			}

			return false;
		}

		public static IType GetOutermostType(this IType type)
		{
			while (true)
			{
				var enclosingType = type.EnclosingType;
				if (enclosingType == null)
					break;

				type = enclosingType;
			}

			return type;
		}

		public static IType GetBasestType(this IType type)
		{
			while (true)
			{
				var baseType = type.BaseType;
				if (baseType == null)
					break;

				type = baseType;
			}

			return type;
		}

		public static IMethod GetMethod(this IType type, string name, bool throwIfMissing = false)
		{
			foreach (var method in type.Methods)
			{
				if (method.Name == name)
					return method;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.MethodNotFound2, name));
			}

			return null;
		}

		public static IMethod GetMethod(this IType type, IMethodSignature methodSig, bool throwIfMissing = false)
		{
			var comparer = SignatureComparer.Default;

			foreach (var method in type.Methods)
			{
				if (comparer.Equals(method, methodSig))
					return method;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.MethodNotFound2, methodSig.ToString()));
			}

			return null;
		}

		public static IField GetField(this IType type, string name, bool throwIfMissing = false)
		{
			foreach (var field in type.Fields)
			{
				if (field.Name == name)
					return field;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.FieldNotFound2, name));
			}

			return null;
		}

		public static IField GetField(this IType type, IFieldSignature fieldSig, bool throwIfMissing = false)
		{
			var comparer = SignatureComparer.Default;

			foreach (var field in type.Fields)
			{
				if (comparer.Equals(field, fieldSig))
					return field;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.FieldNotFound2, fieldSig.ToString()));
			}

			return null;
		}

		public static IProperty GetProperty(this IType type, string name, bool throwIfMissing = false)
		{
			foreach (var property in type.Properties)
			{
				if (property.Name == name)
					return property;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.PropertyNotFound2, name));
			}

			return null;
		}

		public static IProperty GetProperty(this IType type, IPropertySignature propertySig, bool throwIfMissing = false)
		{
			var comparer = SignatureComparer.Default;

			foreach (var property in type.Properties)
			{
				if (comparer.Equals(property, propertySig))
					return property;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.PropertyNotFound2, propertySig.ToString()));
			}

			return null;
		}

		public static IEvent GetEvent(this IType type, string name, bool throwIfMissing = false)
		{
			foreach (var e in type.Events)
			{
				if (e.Name == name)
					return e;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.EventNotFound2, name));
			}

			return null;
		}

		public static IEvent GetEvent(this IType type, IEventSignature eventSig, bool throwIfMissing = false)
		{
			var comparer = SignatureComparer.Default;

			foreach (var e in type.Events)
			{
				if (comparer.Equals(e, eventSig))
					return e;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.EventNotFound2, eventSig.ToString()));
			}

			return null;
		}

		public static IType GetNestedType(this IType type, string name, string ns, bool throwIfMissing = false)
		{
			foreach (var nestedType in type.NestedTypes)
			{
				if (nestedType.Name == name && nestedType.Namespace == ns)
					return nestedType;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, name));
			}

			return null;
		}

		public static string ToString(this IType type, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintType(type);

			return printer.ToString();
		}

		public static IType MakeArrayType(this IType type, IReadOnlyList<ArrayDimension> dimensions)
		{
			return (new ReferencedArrayType(type, dimensions)).Intern();
		}

		public static IType MakeGenericType(this IType type, IReadOnlyList<IType> genericArguments)
		{
			return (new ReferencedGenericType(type, genericArguments)).Intern();
		}

		public static IType MakePointerType(this IType type)
		{
			return (new ReferencedPointerType(type)).Intern();
		}

		public static IType MakePinnedType(this IType type)
		{
			return (new ReferencedPinnedType(type)).Intern();
		}

		public static IType MakeByRefType(this IType type)
		{
			return (new ReferencedByRefType(type)).Intern();
		}

		public static IType MakeCustomModifierType(this IType type, IType modifier, CustomModifierType modifierType)
		{
			return (new ReferencedCustomModifier(type, modifier, modifierType)).Intern();
		}

		public static IType Find(this IReadOnlyList<IType> types, ITypeSignature searchType, bool throwIfMissing = false)
		{
			return Find(types, searchType, SignatureComparer.Default, throwIfMissing);
		}

		public static IType Find(this IReadOnlyList<IType> types, ITypeSignature searchType, SignatureComparer comparer, bool throwIfMissing = false)
		{
			foreach (var type in types)
			{
				if (comparer.Equals(type, searchType))
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, searchType.ToString()));
			}

			return null;
		}

		public static TypeSignature ToSignature(this IType type, IModule owner = null)
		{
			switch (type.ElementCode)
			{
				case TypeElementCode.Array:
					{
						var elementType = ToSignature(type.ElementType, owner);
						var dimensions = type.ArrayDimensions;
						return new ArrayType(elementType, dimensions);
					}

				case TypeElementCode.ByRef:
					{
						var elementType = ToSignature(type.ElementType, owner);
						return new ByRefType(elementType);
					}

				case TypeElementCode.CustomModifier:
					{
						CustomModifierType modifierType;
						var modifier = ToSignature(type.GetCustomModifier(out modifierType), owner);
						var elementType = ToSignature(type.ElementType, owner);
						return new CustomModifier(elementType, modifier, modifierType);
					}

				case TypeElementCode.FunctionPointer:
					{
						var callSite = ToCallSite(type.GetFunctionPointer(), owner);
						return new FunctionPointer(callSite);
					}

				case TypeElementCode.GenericParameter:
					{
						bool isMethod;
						int position;
						type.GetGenericParameter(out isMethod, out position);
						return new GenericParameterType(isMethod, position);
					}

				case TypeElementCode.GenericType:
					{
						var genericArguments = ToSignature(type.GenericArguments, owner);
						var declaringType = ToReference(type.DeclaringType, owner);
						return new GenericTypeReference(declaringType, genericArguments);
					}

				case TypeElementCode.Pinned:
					{
						var elementType = ToSignature(type.ElementType, owner);
						return new PinnedType(elementType);
					}

				case TypeElementCode.Pointer:
					{
						var elementType = ToSignature(type.ElementType, owner);
						return new PointerType(elementType);
					}

				case TypeElementCode.DeclaringType:
					{
						var typeRef = ToReference(type, owner);
						var typeSig = (TypeSignature)typeRef;
						if (type.GenericParameters.Count > 0)
						{
							var genericArguments = new TypeSignature[type.GenericParameters.Count];
							for (int i = 0; i < genericArguments.Length; i++)
							{
								genericArguments[i] = new GenericParameterType(false, i);
							}

							typeSig = new GenericTypeReference(typeRef, genericArguments);
						}

						return typeSig;
					}

				default:
					throw new NotImplementedException();
			}
		}

		public static TypeReference ToReference(this IType type, IModule owner = null)
		{
			Signature ownerSig;
			if (type.IsNested)
			{
				ownerSig = ToReference(type.EnclosingType, owner);
			}
			else if (owner != null)
			{
				if (!object.ReferenceEquals(owner.Assembly, type.Assembly))
					ownerSig = type.Assembly.ToReference();
				else if (!object.ReferenceEquals(owner.Module, type.Module))
					ownerSig = type.Module.ToReference();
				else
					ownerSig = null;
			}
			else
			{
				ownerSig = type.Assembly.ToReference();
			}

			return new TypeReference(
				type.Name,
				type.Namespace,
				ownerSig,
				type.IsValueType());
		}

		public static IReadOnlyList<TypeSignature> ToSignature(IReadOnlyList<IType> types, IModule owner = null)
		{
			var array = new TypeSignature[types.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = types[i].ToSignature(owner);
			}

			return ReadOnlyList<TypeSignature>.Create(array);
		}

		public static TypeSignature ToSignature(this Type type, IModule owner = null)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (type.IsByRef)
			{
				return new ByRefType(ToSignature(type.GetElementType(), owner));
			}
			if (type.IsPointer)
			{
				return new PointerType(ToSignature(type.GetElementType(), owner));
			}
			else if (type.IsArray)
			{
				return new ArrayType(ToSignature(type.GetElementType(), owner), type.GetArrayRank());
			}
			else if (type.IsGenericParameter)
			{
				return new GenericParameterType(type);
			}
			else if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				return new GenericTypeReference(
					ToReference(type, owner),
					ToSignature(type.GetGenericArguments(), owner));
			}
			else
			{
				return ToReference(type, owner);
			}
		}

		public static TypeReference ToReference(this Type type, IModule owner = null)
		{
			Signature ownerSig;
			if (type.IsNested)
			{
				ownerSig = ToReference(type.DeclaringType, owner);
			}
			else if (owner != null)
			{
				if (!SignatureComparer.Equals(owner.Assembly, type.Assembly.GetName(), SignatureComparisonFlags.IgnoreAssemblyStrongName))
					ownerSig = ToReference(type.Assembly.GetName());
				else if (!SignatureComparer.Equals(owner.Module, type.Module))
					ownerSig = new ModuleReference(type.Module.Name);
				else
					ownerSig = null;
			}
			else
			{
				ownerSig = ToReference(type.Assembly.GetName());
			}

			return new TypeReference(
				type.Name,
				type.Namespace,
				ownerSig,
				type.IsValueType);
		}

		public static IReadOnlyList<TypeSignature> ToSignature(this Type[] types, IModule owner = null)
		{
			var array = new TypeSignature[types.Length];

			for (int i = 0; i < types.Length; i++)
			{
				array[i] = ToSignature(types[i], owner);
			}

			return ReadOnlyList<TypeSignature>.Create(array);
		}

		internal static PrimitiveTypeCode GetTypeCode(int elementType)
		{
			switch (elementType)
			{
				case Metadata.ElementType.Boolean:
					return PrimitiveTypeCode.Boolean;

				case Metadata.ElementType.Object:
					return PrimitiveTypeCode.Object;

				case Metadata.ElementType.String:
					return PrimitiveTypeCode.String;

				case Metadata.ElementType.Char:
					return PrimitiveTypeCode.Char;

				case Metadata.ElementType.I1:
					return PrimitiveTypeCode.Int8;

				case Metadata.ElementType.I2:
					return PrimitiveTypeCode.Int16;

				case Metadata.ElementType.I4:
					return PrimitiveTypeCode.Int32;

				case Metadata.ElementType.I8:
					return PrimitiveTypeCode.Int64;

				case Metadata.ElementType.U1:
					return PrimitiveTypeCode.UInt8;

				case Metadata.ElementType.U2:
					return PrimitiveTypeCode.UInt16;

				case Metadata.ElementType.U4:
					return PrimitiveTypeCode.UInt32;

				case Metadata.ElementType.U8:
					return PrimitiveTypeCode.UInt64;

				case Metadata.ElementType.R4:
					return PrimitiveTypeCode.Float32;

				case Metadata.ElementType.R8:
					return PrimitiveTypeCode.Float64;

				case Metadata.ElementType.I:
					return PrimitiveTypeCode.IntPtr;

				case Metadata.ElementType.U:
					return PrimitiveTypeCode.UIntPtr;

				case Metadata.ElementType.TypedByRef:
					return PrimitiveTypeCode.TypedReference;

				case Metadata.ElementType.Type:
					return PrimitiveTypeCode.Type;

				case Metadata.ElementType.Void:
					return PrimitiveTypeCode.Void;

				default:
					throw new InvalidOperationException();
			}
		}

		internal static int GetElementType(PrimitiveTypeCode typeCode)
		{
			return ElementTypes[(int)typeCode];
		}

		#endregion

		#region Method

		public static bool IsVisibleOutsideAssembly(this IMethod method)
		{
			var ownerType = method.Owner;
			if (!IsVisibleOutsideAssembly(ownerType))
				return false;

			switch (method.Visibility)
			{
				case MethodVisibilityFlags.Public:
					return true;

				case MethodVisibilityFlags.Family:
				case MethodVisibilityFlags.FamOrAssem:
					return !ownerType.IsSealed;
			}

			return false;
		}

		public static string ToString(this IMethod method, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintMethod(method);

			return printer.ToString();
		}

		public static IMethod Find(this IReadOnlyList<IMethod> methods, IMethodSignature searchMethod, bool throwIfMissing = false)
		{
			return Find(methods, searchMethod, SignatureComparer.Default, throwIfMissing);
		}

		public static IMethod Find(this IReadOnlyList<IMethod> methods, IMethodSignature searchMethod, SignatureComparer comparer, bool throwIfMissing = false)
		{
			foreach (var method in methods)
			{
				if (comparer.Equals(method, searchMethod))
					return method;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.MethodNotFound2, searchMethod.ToString()));
			}

			return null;
		}

		public static IMethod GetBaseMethod(this IMethod method, ref int depth)
		{
			if (!method.IsVirtual)
				return null;

			if (method.IsNewSlot)
				return null;

			var ownerType = method.Owner;
			if (ownerType.IsInterface)
				return null;

			var baseType = ownerType.BaseType;
			var declaringMethod = method.DeclaringMethod;
			var declaringBaseType = declaringMethod.Owner.BaseType;
			int currDepth = 1;
			var comparer = SignatureComparer.IgnoreMemberOwner_IgnoreAssemblyStrongName;

			while (declaringBaseType != null)
			{
				var declaringBaseMethods = declaringBaseType.Methods;
				for (int i = 0; i < declaringBaseMethods.Count; i++)
				{
					var declaringBaseMethod = declaringBaseMethods[i];
					if (!declaringBaseMethod.IsVirtual)
						continue;

					if (EqualsPolymorphic(declaringMethod, declaringBaseMethod))
					{
						depth += currDepth;
						return baseType.Methods[i];
					}
				}

				baseType = baseType.BaseType;
				declaringBaseType = declaringBaseType.BaseType;
				currDepth++;
			}

			return null;
		}

		public static IMethod GetBaseMethod(this IMethod method)
		{
			int depth = 0;
			return GetBaseMethod(method, ref depth);
		}

		public static IMethod GetBaseMethod(this IMethod method, bool throwIfMissing)
		{
			int depth = 0;
			return GetBaseMethod(method, ref depth, throwIfMissing);
		}

		public static IMethod GetBaseMethod(this IMethod method, ref int depth, bool throwIfMissing)
		{
			var baseMethod = GetBaseMethod(method, ref depth);
			if (baseMethod == null)
			{
				if (throwIfMissing)
				{
					throw new CodeModelException(string.Format(SR.MethodBaseNotFound, method.Owner.ToString(), method.Name));
				}

				return null;
			}

			return baseMethod;
		}

		public static IMethod GetBottomMethod(this IMethod method)
		{
			int depth = 0;
			return GetBottomMethod(method, ref depth);
		}

		public static IMethod GetBottomMethod(this IMethod method, ref int depth)
		{
			while (true)
			{
				var baseMethod = GetBaseMethod(method, ref depth);
				if (baseMethod == null)
					break;

				method = baseMethod;
			}

			return method;
		}

		public static IMethod MakeGenericMethod(this IMethod method, IReadOnlyList<IType> genericArguments)
		{
			return (new ReferencedMethod(method.DeclaringMethod, method.Owner, genericArguments)).Intern();
		}

		public static MethodSignature ToSignature(this IMethod method, IModule owner = null)
		{
			var genericArguments = method.GenericArguments;
			if (genericArguments.Count > 0)
			{
				return new GenericMethodReference(
					ToReference(method.DeclaringMethod, owner),
					ToSignature(genericArguments, owner));
			}
			else
			{
				return ToReference(method, owner);
			}
		}

		public static MethodReference ToReference(this IMethod method, IModule owner = null)
		{
			return new MethodReference(
				method.Name,
				ToSignature(method.Owner, owner),
				ToCallSite(method.DeclaringMethod, owner));
		}

		public static CallSite ToCallSite(this IMethod method, IModule owner = null)
		{
			return new CallSite(
				method.HasThis,
				method.ExplicitThis,
				method.CallConv,
				ToSignature(method.ReturnType, owner),
				ToSignature(method.Parameters, owner),
				method.VarArgIndex,
				method.GenericParameterCount);
		}

		public static CallSite ToCallSite(this ICallSite callSite, IModule owner = null)
		{
			return new CallSite(
				callSite.HasThis,
				callSite.ExplicitThis,
				callSite.CallConv,
				ToSignature(callSite.ReturnType, owner),
				ToSignature(callSite.Arguments, owner),
				callSite.VarArgIndex,
				callSite.GenericParameterCount);
		}

		private static IReadOnlyList<TypeSignature> ToSignature(IReadOnlyList<IMethodParameter> parameters, IModule owner)
		{
			var array = new TypeSignature[parameters.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = parameters[i].Type.ToSignature(owner);
			}

			return ReadOnlyList<TypeSignature>.Create(array);
		}

		public static MethodSignature ToSignature(this System.Reflection.MethodInfo method, IModule owner = null)
		{
			if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
			{
				return new GenericMethodReference(
					ToReference(method, owner),
					ToSignature(method.GetGenericArguments(), owner));
			}
			else
			{
				return ToReference(method, owner);
			}
		}

		public static MethodReference ToReference(this System.Reflection.MethodInfo method, IModule owner = null)
		{
			return new MethodReference(
				method.Name,
				ToSignature(method.DeclaringType, owner),
				ToCallSite(method, owner));
		}

		public static CallSite ToCallSite(this System.Reflection.MethodInfo method, IModule owner = null)
		{
			var callingConvention = method.CallingConvention;

			bool hasThis = false;
			bool explicitThis = false;
			if ((callingConvention & System.Reflection.CallingConventions.HasThis) == System.Reflection.CallingConventions.HasThis)
			{
				hasThis = true;

				if ((callingConvention & System.Reflection.CallingConventions.ExplicitThis) == System.Reflection.CallingConventions.ExplicitThis)
					explicitThis = true;
			}

			MethodCallingConvention callConv;
			if ((callingConvention & System.Reflection.CallingConventions.VarArgs) == System.Reflection.CallingConventions.VarArgs)
				callConv = MethodCallingConvention.VarArgs;
			else
				callConv = MethodCallingConvention.Default;

			int genericParameterCount;
			if (method.IsGenericMethodDefinition)
				genericParameterCount = method.GetGenericArguments().Length;
			else
				genericParameterCount = 0;

			return new CallSite(
				hasThis,
				explicitThis,
				callConv,
				ToSignature(method.ReturnType, owner),
				ToSignature(method.GetParameters(), owner),
				-1,
				genericParameterCount);
		}

		private static IReadOnlyList<TypeSignature> ToSignature(System.Reflection.ParameterInfo[] parameters, IModule owner)
		{
			var array = new TypeSignature[parameters.Length];

			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = ToSignature(parameters[i].ParameterType, owner);
			}

			return ReadOnlyList<TypeSignature>.Create(array);
		}

		internal static IMethod FindPolymorphic(this IReadOnlyList<IMethod> methods, IMethod searchMethod, bool throwIfMissing = false)
		{
			foreach (var method in methods)
			{
				if (EqualsPolymorphic(method, searchMethod))
					return method;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.MethodNotFound2, searchMethod.ToString()));
			}

			return null;
		}

		internal static bool EqualsPolymorphic(IMethod x, IMethod y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if (x.HasThis != y.HasThis)
				return false;

			if (x.ExplicitThis != y.ExplicitThis)
				return false;

			if (x.CallConv != y.CallConv)
				return false;

			if (x.Parameters.Count != y.Parameters.Count)
				return false;

			if (x.GenericParameters.Count != y.GenericParameters.Count)
				return false;

			if (x.GenericArguments.Count != y.GenericArguments.Count)
				return false;

			var comparer = SignatureComparer.IgnoreAssemblyStrongName;

			for (int i = 0; i < x.Parameters.Count; i++)
			{
				if (!comparer.Equals(x.Parameters[i].Type, y.Parameters[i].Type))
					return false;
			}

			for (int i = 0; i < x.GenericArguments.Count; i++)
			{
				if (!comparer.Equals(x.GenericArguments[i], y.GenericArguments[i]))
					return false;
			}

			if (!comparer.Equals(x.ReturnType, y.ReturnType))
				return false;

			return true;
		}

		#endregion

		#region Field

		public static bool IsVisibleOutsideAssembly(this IField field)
		{
			var ownerType = field.Owner;
			if (!IsVisibleOutsideAssembly(ownerType))
				return false;

			switch (field.Visibility)
			{
				case FieldVisibilityFlags.Public:
					return true;

				case FieldVisibilityFlags.Family:
				case FieldVisibilityFlags.FamOrAssem:
					return !ownerType.IsSealed;
			}

			return false;
		}

		public static IField Find(this IReadOnlyList<IField> fields, IFieldSignature searchField, bool throwIfMissing = false)
		{
			return Find(fields, searchField, SignatureComparer.Default, throwIfMissing);
		}

		public static IField Find(this IReadOnlyList<IField> fields, IFieldSignature searchField, SignatureComparer comparer, bool throwIfMissing = false)
		{
			foreach (var field in fields)
			{
				if (comparer.Equals(field, searchField))
					return field;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.FieldNotFound2, searchField.ToString()));
			}

			return null;
		}

		public static string ToString(this IField field, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintField(field);

			return printer.ToString();
		}

		public static FieldReference ToReference(this IField field, IModule owner = null)
		{
			var declaringField = field.DeclaringField;

			return new FieldReference(
				field.Name,
				ToSignature(declaringField.FieldType, owner),
				ToSignature(field.Owner, owner));
		}

		public static FieldReference ToReference(this System.Reflection.FieldInfo field, IModule owner = null)
		{
			return new FieldReference(
				field.Name,
				ToSignature(field.FieldType, owner),
				ToSignature(field.DeclaringType, owner));
		}

		#endregion

		#region Property

		public static bool IsVisibleOutsideAssembly(this IProperty property)
		{
			var ownerType = property.Owner;
			if (!IsVisibleOutsideAssembly(ownerType))
				return false;
			
			// Get
			if (property.GetMethod != null && IsVisibleOutsideAssembly(property.GetMethod))
				return true;

			// Set
			if (property.SetMethod != null && IsVisibleOutsideAssembly(property.SetMethod))
				return true;

			return false;
		}

		public static IProperty Find(this IReadOnlyList<IProperty> properties, IPropertySignature searchProperty, bool throwIfMissing = false)
		{
			return Find(properties, searchProperty, SignatureComparer.Default, throwIfMissing);
		}

		public static IProperty Find(this IReadOnlyList<IProperty> properties, IPropertySignature searchProperty, SignatureComparer comparer, bool throwIfMissing = false)
		{
			foreach (var property in properties)
			{
				if (comparer.Equals(property, searchProperty))
					return property;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.PropertyNotFound2, searchProperty.ToString()));
			}

			return null;
		}

		public static string ToString(this IProperty property, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintProperty(property);

			return printer.ToString();
		}

		public static PropertyReference ToReference(this IProperty property, IModule owner = null)
		{
			var declaringProperty = property.DeclaringProperty;

			return new PropertyReference(
				property.Name,
				ToSignature(property.Owner, owner),
				ToSignature(declaringProperty.ReturnType, owner),
				ToSignature(declaringProperty.Parameters, owner));
		}

		public static PropertyReference ToReference(this System.Reflection.PropertyInfo property, IModule owner = null)
		{
			return new PropertyReference(
				property.Name,
				ToSignature(property.DeclaringType, owner),
				ToSignature(property.PropertyType, owner),
				ToSignature(property.GetIndexParameters(), owner));
		}

		#endregion

		#region Event

		public static bool IsVisibleOutsideAssembly(this IEvent e)
		{
			var ownerType = e.Owner;
			if (!IsVisibleOutsideAssembly(ownerType))
				return false;

			// Add
			if (e.AddMethod != null && IsVisibleOutsideAssembly(e.AddMethod))
				return true;

			// Remove
			if (e.RemoveMethod != null && IsVisibleOutsideAssembly(e.RemoveMethod))
				return true;

			// Invoke
			if (e.InvokeMethod != null && IsVisibleOutsideAssembly(e.InvokeMethod))
				return true;

			return false;
		}

		public static IEvent Find(this IReadOnlyList<IEvent> events, IEventSignature searchEvent, bool throwIfMissing = false)
		{
			return Find(events, searchEvent, SignatureComparer.Default, throwIfMissing);
		}

		public static IEvent Find(this IReadOnlyList<IEvent> events, IEventSignature searchEvent, SignatureComparer comparer, bool throwIfMissing = false)
		{
			foreach (var e in events)
			{
				if (comparer.Equals(e, searchEvent))
					return e;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.EventNotFound2, searchEvent.ToString()));
			}

			return null;
		}

		public static string ToString(this IEvent e, SignaturePrintingFlags flags = SignaturePrintingFlags.None)
		{
			var printer = new SignaturePrinter(flags);
			printer.PrintEvent(e);

			return printer.ToString();
		}

		public static EventReference ToReference(this IEvent e, IModule owner = null)
		{
			var declaringEvent = e.DeclaringEvent;

			return new EventReference(
				e.Name,
				ToSignature(declaringEvent.EventType, owner),
				ToSignature(e.Owner, owner));
		}

		public static EventReference ToReference(this System.Reflection.EventInfo e, IModule owner = null)
		{
			return new EventReference(
				e.Name,
				ToSignature(e.EventHandlerType, owner),
				ToSignature(e.DeclaringType, owner));
		}

		#endregion

		#region Structures

		internal static PrimitiveTypeInfo[] PrimitiveTypes = new PrimitiveTypeInfo[]
		{
			new PrimitiveTypeInfo("Boolean", true),
			new PrimitiveTypeInfo("Char", true),
			new PrimitiveTypeInfo("SByte", true),
			new PrimitiveTypeInfo("Int16", true),
			new PrimitiveTypeInfo("Int32", true),
			new PrimitiveTypeInfo("Int64", true),
			new PrimitiveTypeInfo("Byte", true),
			new PrimitiveTypeInfo("UInt16", true),
			new PrimitiveTypeInfo("UInt32", true),
			new PrimitiveTypeInfo("UInt64", true),
			new PrimitiveTypeInfo("Single", true),
			new PrimitiveTypeInfo("Double", true),
			new PrimitiveTypeInfo("IntPtr", true),
			new PrimitiveTypeInfo("UIntPtr", true),
			new PrimitiveTypeInfo("Type", false),
			new PrimitiveTypeInfo("TypedReference", true),
			new PrimitiveTypeInfo("Object", false),
			new PrimitiveTypeInfo("String", false),
			new PrimitiveTypeInfo("Void", true),
		};

		internal static Dictionary<string, PrimitiveTypeCode> SystemTypeCodes = new Dictionary<string, PrimitiveTypeCode>()
		{
			{ "Boolean", PrimitiveTypeCode.Boolean },
			{ "Char", PrimitiveTypeCode.Char },
			{ "SByte", PrimitiveTypeCode.Int8 },
			{ "Int16", PrimitiveTypeCode.Int16 },
			{ "Int32", PrimitiveTypeCode.Int32 },
			{ "Int64", PrimitiveTypeCode.Int64 },
			{ "Byte", PrimitiveTypeCode.UInt8 },
			{ "UInt16", PrimitiveTypeCode.UInt16 },
			{ "UInt32", PrimitiveTypeCode.UInt32 },
			{ "UInt64", PrimitiveTypeCode.UInt64 },
			{ "Single", PrimitiveTypeCode.Float32 },
			{ "Double", PrimitiveTypeCode.Float64 },
			{ "IntPtr", PrimitiveTypeCode.IntPtr },
			{ "UIntPtr", PrimitiveTypeCode.UIntPtr },
			{ "Type", PrimitiveTypeCode.Type },
			{ "TypedReference", PrimitiveTypeCode.TypedReference },
			{ "Object", PrimitiveTypeCode.Object },
			{ "String", PrimitiveTypeCode.String },
			{ "Void", PrimitiveTypeCode.Void },
		};

		internal static readonly int[] ElementTypes = new int[]
		{
			Metadata.ElementType.Boolean,
			Metadata.ElementType.Char,
			Metadata.ElementType.I1,
			Metadata.ElementType.I2,
			Metadata.ElementType.I4,
			Metadata.ElementType.I8,
			Metadata.ElementType.U1,
			Metadata.ElementType.U2,
			Metadata.ElementType.U4,
			Metadata.ElementType.U8,
			Metadata.ElementType.R4,
			Metadata.ElementType.R8,
			Metadata.ElementType.I,
			Metadata.ElementType.U,
			Metadata.ElementType.Type,
			Metadata.ElementType.TypedByRef,
			Metadata.ElementType.Object,
			Metadata.ElementType.String,
			Metadata.ElementType.Void,
		};

		internal struct PrimitiveTypeInfo
		{
			internal string Name;
			internal bool IsValueType;

			internal PrimitiveTypeInfo(string name, bool isValueType)
			{
				Name = name;
				IsValueType = isValueType;
			}
		}

		#endregion
	}
}
