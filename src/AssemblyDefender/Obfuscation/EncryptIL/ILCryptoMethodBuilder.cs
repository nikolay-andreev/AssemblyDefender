using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ILCryptoMethodBuilder
	{
		#region Fields

		private BuildModule _module;
		private DelegateTypeBuilder _delegateTypeBuilder;
		private Dictionary<int, InvokeTypeInfo> _invokeTypeByGenericParameter;

		#endregion

		#region Ctors

		public ILCryptoMethodBuilder(BuildAssembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_module = (BuildModule)assembly.Module;
		}

		#endregion

		#region Methods

		public void Build()
		{
			_delegateTypeBuilder = new DelegateTypeBuilder(_module);
			_invokeTypeByGenericParameter = new Dictionary<int, InvokeTypeInfo>();

			foreach (BuildType type in _module.Types)
			{
				Build(type);
			}
		}

		private void Build(BuildType type)
		{
			foreach (BuildMethod method in type.Methods)
			{
				if (method.EncryptIL)
				{
					Build(method);
				}
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Build(nestedType);
			}
		}

		private void Build(BuildMethod method)
		{
			var cryptoMethod = method.CreateILCrypto();

			var type = method.GetOwnerType();

			var invokeTypeInfo = GetInvokeType(type.GenericParameters.Count);

			var invokeType = invokeTypeInfo.Instance;

			cryptoMethod.MethodID = invokeType.MethodCount++;

			// Delegate type
			var delegateGenericArguments = new List<TypeSignature>();
			var delegateType = _delegateTypeBuilder.Build(method, delegateGenericArguments);
			if (delegateGenericArguments.Count > 0)
			{
				cryptoMethod.DelegateGenericArguments = delegateGenericArguments.ToArray();
			}

			// Invoke method
			var invokeGenericArguments = new List<TypeSignature>();
			var invokeMethod = BuildInvokeMethod(method, invokeTypeInfo, invokeGenericArguments);
			invokeMethod.DelegateType = delegateType;

			cryptoMethod.InvokeMethod = invokeMethod;

			if (invokeGenericArguments.Count > 0)
			{
				cryptoMethod.InvokeGenericArguments = invokeGenericArguments.ToArray();
			}
		}

		private ILCryptoInvokeMethod BuildInvokeMethod(BuildMethod method, InvokeTypeInfo invokeTypeInfo, List<TypeSignature> genericArguments)
		{
			var invokeMethodInfo = BuildInvokeMethod(method, genericArguments);

			int index;
			if (invokeTypeInfo.Methods.TryAdd(invokeMethodInfo, out index))
			{
				var invokeMethod = invokeTypeInfo.Instance.InvokeMethods.Add();
				invokeMethod.GenericParameterCount = invokeMethodInfo.GenericParameterCount;
				invokeMethod.ParameterFlags = invokeMethodInfo.ParameterFlags;
				invokeMethod.CallSite = invokeMethodInfo.CallSite;
				invokeMethod.OwnerType = invokeTypeInfo.Instance;
				invokeMethodInfo.Instance = invokeMethod;

				return invokeMethod;
			}
			else
			{
				return invokeTypeInfo.Methods[index].Instance;
			}
		}

		private InvokeMethodInfo BuildInvokeMethod(BuildMethod method, List<TypeSignature> genericArguments)
		{
			var invokeMethodInfo = new InvokeMethodInfo();

			int argumentCount = method.Parameters.Count + 1;
			if (method.HasThis)
				argumentCount++;

			var arguments = new TypeSignature[argumentCount];
			var parameterFlags = new int[argumentCount];

			// Arguments
			{
				int index = 0;
				if (method.HasThis)
				{
					arguments[index++] = new GenericParameterType(true, 0);
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

				// index : [mscorlib]System.Int32
				arguments[index++] = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
			}

			var returnType = BuildType(method.ReturnType.Type, genericArguments);

			invokeMethodInfo.CallSite =
				new CallSite(
					false,
					false,
					MethodCallingConvention.Default,
					returnType,
					arguments,
					-1,
					genericArguments.Count);

			invokeMethodInfo.ParameterFlags = parameterFlags;

			invokeMethodInfo.GenericParameterCount = genericArguments.Count;

			return invokeMethodInfo;
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
						return new GenericParameterType(true, count);
					}

				case TypeElementCode.GenericType:
					{
						int count = genericArguments.Count;
						genericArguments.Add(typeSig);
						return new GenericParameterType(true, count);
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
							return new GenericParameterType(true, count);
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

		private InvokeTypeInfo GetInvokeType(int genericParameterCount)
		{
			InvokeTypeInfo invokeTypeInfo;
			if (!_invokeTypeByGenericParameter.TryGetValue(genericParameterCount, out invokeTypeInfo))
			{
				var invokeType = _module.ILCryptoInvokeTypes.Add();
				invokeType.GenericParameterCount = genericParameterCount;

				invokeTypeInfo = new InvokeTypeInfo();
				invokeTypeInfo.Instance = invokeType;
				invokeTypeInfo.Methods = new HashList<InvokeMethodInfo>();
				_invokeTypeByGenericParameter.Add(genericParameterCount, invokeTypeInfo);
			}

			return invokeTypeInfo;
		}

		#endregion

		#region Static

		public static void Build(BuildAssembly assembly)
		{
			var builder = new ILCryptoMethodBuilder(assembly);
			builder.Build();
		}

		#endregion

		#region Nested types

		private struct InvokeTypeInfo
		{
			internal ILCryptoInvokeType Instance;
			internal HashList<InvokeMethodInfo> Methods;
		}

		private class InvokeMethodInfo
		{
			internal int GenericParameterCount;
			internal int[] ParameterFlags;
			internal CallSite CallSite;
			internal ILCryptoInvokeMethod Instance;

			public override bool Equals(object obj)
			{
				var other = (InvokeMethodInfo)obj;
				if (GenericParameterCount != other.GenericParameterCount)
					return false;

				if (!SignatureComparer.Default.Equals(CallSite, other.CallSite))
					return false;

				if (ParameterFlags.Length != other.ParameterFlags.Length)
					return false;

				for (int i = 0; i < ParameterFlags.Length; i++)
				{
					if (ParameterFlags[i] != other.ParameterFlags[i])
						return false;
				}

				return true;
			}

			public override int GetHashCode()
			{
				int hashCode = 0x56723;

				hashCode ^= GenericParameterCount;

				hashCode ^= SignatureComparer.Default.GetHashCode(CallSite);

				for (int i = 0; i < ParameterFlags.Length; i++)
				{
					hashCode += ParameterFlags[i] * (i + 1);
				}

				return hashCode;
			}
		}

		#endregion
	}
}
