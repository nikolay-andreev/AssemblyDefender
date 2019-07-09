using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public class DelegateTypeBuilder
	{
		#region Fields

		private BuildModule _module;
		private HashList<DelegateTypeInfo> _types;

		#endregion

		#region Ctors

		public DelegateTypeBuilder(BuildModule module)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			_module = module;
			Load();
		}

		#endregion

		#region Methods

		public DelegateType Build(BuildMethod method, List<TypeSignature> genericArguments)
		{
			var typeInfo = BuildInfo(method, genericArguments);

			int index;
			if (_types.TryAdd(typeInfo, out index))
			{
				var delegateType = _module.DelegateTypes.Add();
				delegateType.GenericParameterCount = typeInfo.GenericParameterCount;
				delegateType.InvokeParameterFlags = typeInfo.InvokeParameterFlags;
				delegateType.InvokeCallSite = typeInfo.InvokeCallSite;
				typeInfo.Instance = delegateType;

				return delegateType;
			}
			else
			{
				return _types[index].Instance;
			}
		}

		private DelegateTypeInfo BuildInfo(BuildMethod method, List<TypeSignature> genericArguments)
		{
			var typeInfo = new DelegateTypeInfo();

			int argumentCount = method.Parameters.Count;
			if (method.HasThis)
				argumentCount++;

			var arguments = new TypeSignature[argumentCount];
			var parameterFlags = new int[argumentCount];

			if (argumentCount > 0)
			{
				int index = 0;
				if (method.HasThis)
				{
					arguments[index++] = new GenericParameterType(false, 0);
					genericArguments.Add(TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
				}

				var parameters = method.Parameters;

				for (int i = 0; i < parameters.Count; i++)
				{
					var parameter = parameters[i];
					arguments[index] = BuildType(parameter.Type, genericArguments);
					parameterFlags[index] = GetParameterFlags(parameter);
					index++;
				}
			}

			var returnType = BuildType(method.ReturnType.Type, genericArguments);

			typeInfo.InvokeCallSite =
				new CallSite(
					true,
					false,
					MethodCallingConvention.Default,
					returnType,
					arguments,
					-1,
					0);

			typeInfo.InvokeParameterFlags = parameterFlags;

			typeInfo.GenericParameterCount = genericArguments.Count;

			return typeInfo;
		}

		private TypeSignature BuildType(TypeSignature typeSig, List<TypeSignature> genericArguments)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					{
						var arrayType = (ArrayType)typeSig;
						var elementType = BuildType(typeSig.ElementType, genericArguments);
						return new ArrayType(elementType, arrayType.ArrayDimensions);
					}

				case TypeElementCode.ByRef:
					{
						var elementType = BuildType(typeSig.ElementType, genericArguments);
						return new ByRefType(elementType);
					}

				case TypeElementCode.CustomModifier:
					{
						return BuildType(typeSig.ElementType, genericArguments);
					}

				case TypeElementCode.FunctionPointer:
					return typeSig;

				case TypeElementCode.GenericParameter:
					{
						int count = genericArguments.Count;
						genericArguments.Add(typeSig);
						return new GenericParameterType(false, count);
					}

				case TypeElementCode.GenericType:
					{
						int count = genericArguments.Count;
						genericArguments.Add(typeSig);
						return new GenericParameterType(false, count);
					}

				case TypeElementCode.Pinned:
					{
						var elementType = BuildType(typeSig.ElementType, genericArguments);
						return new PinnedType(elementType);
					}

				case TypeElementCode.Pointer:
					return typeSig;

				case TypeElementCode.DeclaringType:
					{
						if (typeSig.GetTypeCode(_module) != PrimitiveTypeCode.Void)
						{
							int count = genericArguments.Count;
							genericArguments.Add(typeSig);
							return new GenericParameterType(false, count);
						}
						else
						{
							return typeSig;
						}
					}

				default:
					throw new InvalidOperationException();
			}
		}

		private int GetParameterFlags(MethodParameter parameter)
		{
			int flags = 0;

			if (parameter.IsIn)
				flags |= 1;

			if (parameter.IsOut)
				flags |= 2;

			if (parameter.IsOptional)
				flags |= 4;

			return flags;
		}

		private DelegateTypeInfo CreateTypeInfo(DelegateType delegateType)
		{
			return new DelegateTypeInfo()
			{
				GenericParameterCount = delegateType.GenericParameterCount,
				InvokeParameterFlags = delegateType.InvokeParameterFlags,
				InvokeCallSite = delegateType.InvokeCallSite,
				Instance = delegateType,
			};
		}

		public void Load()
		{
			var delegateTypes = _module.DelegateTypes;

			_types = new HashList<DelegateTypeInfo>(delegateTypes.Count);

			foreach (var delegateType in delegateTypes)
			{
				_types.Add(CreateTypeInfo(delegateType));
			}

			// Load mscorlib delegates.
			if (_module.Assembly.Framework.Version >= MicrosoftNetFramework.Version40.Version)
			{
				LoadActions();
				LoadFuncs();
			}
		}

		private void LoadActions()
		{
			var mscorlib = AssemblyReference.GetMscorlib(_module.Assembly);

			for (int i = 0; i < 9; i++)
			{
				var delegateType = new DelegateType();

				var arguments = new TypeSignature[i];
				for (int j = 0; j < i; j++)
				{
					arguments[j] = new GenericParameterType(false, j);
				}

				delegateType.InvokeCallSite =
					new CallSite(
						true,
						false,
						MethodCallingConvention.Default,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
						arguments,
						-1,
						0);

				string name = "Action";
				if (i > 0)
					name += "`" + i.ToString();

				delegateType.DeclaringType = new TypeReference(
					name,
					"System",
					mscorlib,
					false);

				delegateType.GenericParameterCount = i;

				delegateType.InvokeParameterFlags = new int[i];

				_types.TryAdd(CreateTypeInfo(delegateType));
			}
		}

		private void LoadFuncs()
		{
			var mscorlib = AssemblyReference.GetMscorlib(_module.Assembly);

			for (int i = 0; i < 9; i++)
			{
				var delegateType = new DelegateType();

				var arguments = new TypeSignature[i];
				for (int j = 0; j < i; j++)
				{
					arguments[j] = new GenericParameterType(false, j);
				}

				delegateType.InvokeCallSite =
					new CallSite(
						true,
						false,
						MethodCallingConvention.Default,
						new GenericParameterType(false, i),
						arguments,
						-1,
						0);

				delegateType.DeclaringType = new TypeReference(
					"Func`" + (i + 1).ToString(),
					"System",
					mscorlib,
					false);

				delegateType.GenericParameterCount = i + 1;

				delegateType.InvokeParameterFlags = new int[i];

				_types.TryAdd(CreateTypeInfo(delegateType));
			}
		}

		#endregion

		#region Nested types

		private class DelegateTypeInfo
		{
			internal int GenericParameterCount;
			internal int[] InvokeParameterFlags;
			internal CallSite InvokeCallSite;
			internal DelegateType Instance;

			public override bool Equals(object obj)
			{
				var other = (DelegateTypeInfo)obj;
				if (GenericParameterCount != other.GenericParameterCount)
					return false;

				if (!SignatureComparer.Default.Equals(InvokeCallSite, other.InvokeCallSite))
					return false;

				if (InvokeParameterFlags.Length != other.InvokeParameterFlags.Length)
					return false;

				for (int i = 0; i < InvokeParameterFlags.Length; i++)
				{
					if (InvokeParameterFlags[i] != other.InvokeParameterFlags[i])
						return false;
				}

				return true;
			}

			public override int GetHashCode()
			{
				int hashCode = 0x56723;

				hashCode ^= GenericParameterCount;

				hashCode ^= SignatureComparer.Default.GetHashCode(InvokeCallSite);

				for (int i = 0; i < InvokeParameterFlags.Length; i++)
				{
					hashCode += InvokeParameterFlags[i] * (i + 1);
				}

				return hashCode;
			}
		}

		#endregion
	}
}
