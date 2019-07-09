using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Net.IL;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	internal class MainTypeCallProxyBuilder
	{
		#region Fields

		private BuildModule _module;
		private MainType _mainType;
		private Random _random;
		private Dictionary<MethodReference, ProxyMethodInfo> _proxyMethods = new Dictionary<MethodReference, ProxyMethodInfo>(SignatureComparer.IgnoreAssemblyStrongName);

		#endregion

		#region Ctors

		internal MainTypeCallProxyBuilder(BuildModule module)
		{
			_module = module;
			_mainType = module.MainType;
			_random = module.RandomGenerator;
		}

		#endregion

		#region Methods

		internal void Generate()
		{
			foreach (var kvp in _proxyMethods)
			{
				Generate(kvp.Key, kvp.Value.Method, kvp.Value.OpCode);
			}
		}

		private void Generate(MethodReference methodRef, MethodReference proxyMethodRef, OpCode opCode)
		{
			var method = _mainType.Methods.Add();
			method.Name = proxyMethodRef.Name;
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type = proxyMethodRef.ReturnType;
			}

			// Parameters
			{
				var parameters = method.Parameters;

				foreach (var argumentType in proxyMethodRef.Arguments)
				{
					var parameter = parameters.Add();
					parameter.Type = argumentType;
				}
			}

			// Body
			{
				int paramCount = proxyMethodRef.Arguments.Count;

				var methodBody = new MethodBody();
				methodBody.MaxStackSize = paramCount;
				methodBody.InitLocals = true;

				// Instructions
				{
					var instructions = methodBody.Instructions;

					for (int i = 0; i < paramCount; i++)
					{
						instructions.Add(Instruction.GetLdarg(i));
					}

					instructions.Add(new Instruction(opCode, methodRef));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		internal void Build(ILBlock block)
		{
			var node = block.FirstChild;
			while (node != null)
			{
				switch (node.NodeType)
				{
					case ILNodeType.Block:
						Build((ILBlock)node);
						break;

					case ILNodeType.Instruction:
						Build((ILInstruction)node);
						break;
				}

				node = node.NextSibling;
			}
		}

		private bool Build(ILInstruction instruction)
		{
			var methodRef = instruction.Value as MethodReference;
			if (methodRef == null)
				return false;

			var opCode = instruction.OpCode;
			if (opCode != OpCodes.Call && opCode != OpCodes.Callvirt)
				return false;

			if (!methodRef.HasThis ||
				methodRef.CallConv != MethodCallingConvention.Default ||
				methodRef.GenericParameterCount > 0)
				return false;

			ProxyMethodInfo proxyMethod;
			if (!_proxyMethods.TryGetValue(methodRef, out proxyMethod))
			{
				proxyMethod = new ProxyMethodInfo();
				proxyMethod.Method = CreateProxy(methodRef);
				proxyMethod.OpCode = opCode;
				_proxyMethods.Add(methodRef, proxyMethod);
			}

			instruction.Value = proxyMethod.Method;
			instruction.OpCode = OpCodes.Call;

			return true;
		}

		private MethodReference CreateProxy(MethodReference methodRef)
		{
			var typeGenericArguments = methodRef.Owner.GenericArguments;

			var returnType = BuildType(methodRef.ReturnType, typeGenericArguments);

			var arguments = new TypeSignature[methodRef.Arguments.Count + 1];
			arguments[0] = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);
			for (int i = 1; i < arguments.Length; i++)
			{
				arguments[i] = BuildType(methodRef.Arguments[i - 1], typeGenericArguments);
			}

			return new MethodReference(
				_random.NextString(12),
				new TypeReference(_mainType.Name, _mainType.Namespace, false),
				new CallSite(
					false,
					false,
					MethodCallingConvention.Default,
					returnType,
					arguments,
					-1,
					0));
		}

		private TypeSignature BuildType(TypeSignature typeSig, IReadOnlyList<TypeSignature> typeGenericArguments)
		{
			if (typeGenericArguments.Count > 0)
			{
				typeSig = (TypeSignature)typeSig.MapGenerics(typeGenericArguments, null);
			}

			return typeSig;
		}


		#endregion

		#region Nested types

		private struct ProxyMethodInfo
		{
			internal OpCode OpCode;
			internal MethodReference Method;
		}

		#endregion
	}
}
