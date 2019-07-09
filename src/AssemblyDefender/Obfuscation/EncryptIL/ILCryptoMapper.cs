using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Net.IL;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ILCryptoMapper
	{
		#region Fields

		private string _mainTypeNamespace;
		private BuildModule _module;

		#endregion

		#region Ctors

		public ILCryptoMapper(BuildModule module)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			_module = module;
			_mainTypeNamespace = _module.MainTypeNamespace;
		}

		#endregion

		#region Methods

		public void Map()
		{
			foreach (BuildType type in _module.Types)
			{
				Map(type);
			}
		}

		private void Map(BuildType type)
		{
			foreach (BuildMethod method in type.Methods)
			{
				Map(method);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Map(nestedType);
			}
		}

		private void Map(BuildMethod method)
		{
			if (!MethodBody.IsValid(method))
				return;

			var methodBody = MethodBody.Load(method);

			var ilBody = ILBody.Load(methodBody);

			if (Map(ilBody))
			{
				ilBody.CalculateMaxStackSize(method);
				methodBody = ilBody.Build();
				methodBody.Build(method);
			}
		}

		private bool Map(ILBlock block)
		{
			bool changed = false;

			var node = block.FirstChild;
			while (node != null)
			{
				switch (node.NodeType)
				{
					case ILNodeType.Block:
						changed |= Map((ILBlock)node);
						break;

					case ILNodeType.Instruction:
						changed |= Map((ILInstruction)node);
						break;
				}

				node = node.NextSibling;
			}

			return changed;
		}

		private bool Map(ILInstruction instruction)
		{
			if (instruction.OpCode != OpCodes.Call && instruction.OpCode != OpCodes.Callvirt)
				return false;

			var prevInstruction = instruction.GetPrevious() as ILInstruction;
			if (prevInstruction != null && prevInstruction.OpCode.Type == OpCodeType.Prefix)
				return false;

			var calledMethodRef = instruction.Value as MethodReference;
			if (calledMethodRef == null)
				return false;

			var calledResolveMethod = calledMethodRef.Resolve(_module);
			if (calledResolveMethod == null)
				return false;

			var calledMethod = calledResolveMethod.DeclaringMethod as BuildMethod;
			if (calledMethod == null)
				return false;

			if (!calledMethod.EncryptIL)
				return false;

			if (calledMethod.IsVirtual)
				return false;

			var targetModule = (BuildModule)calledMethod.Module;

			var cryptoMethod = calledMethod.ILCrypto;

			var invokeMethodSig = GetInvokeMethodSig(cryptoMethod, calledMethodRef, targetModule);

			instruction.AddPrevious(new ILInstruction(Instruction.GetLdc(cryptoMethod.MethodID)));

			instruction.OpCode = OpCodes.Call;
			instruction.Value = invokeMethodSig;

			return true;
		}

		private MethodSignature GetInvokeMethodSig(ILCryptoMethod cryptoMethod, MethodReference calledMethodRef, BuildModule targetModule)
		{
			var invokeMethod = cryptoMethod.InvokeMethod;

			var invokeType = invokeMethod.OwnerType;

			var ownerTypeSig = GetInvokeTypeSig(invokeType, calledMethodRef.Owner, targetModule);

			var callSite = (CallSite)invokeMethod.CallSite.Relocate(_module, targetModule, true);

			var methodRef = new MethodReference(
				invokeType.InvokeMethodName,
				ownerTypeSig,
				callSite);

			if (invokeMethod.GenericParameterCount > 0)
			{
				var genericArguments = new TypeSignature[invokeMethod.GenericParameterCount];
				var genericMethodArguments = cryptoMethod.InvokeGenericArguments;
				var genericTypeArguments = ownerTypeSig.GenericArguments;

				for (int i = 0; i < genericMethodArguments.Length; i++)
				{
					var genericMethodArgument = genericMethodArguments[i];

					genericMethodArgument = (TypeSignature)genericMethodArgument.Relocate(_module, targetModule, true);

					if (genericTypeArguments.Count > 0)
					{
						genericMethodArgument = (TypeSignature)genericMethodArgument.MapGenerics(genericTypeArguments, null);
					}

					genericArguments[i] = genericMethodArgument;
				}

				return new GenericMethodReference(methodRef, genericArguments);
			}
			else
			{
				return methodRef;
			}
		}

		private TypeSignature GetInvokeTypeSig(ILCryptoInvokeType invokeType, TypeSignature typeSig, BuildModule targetModule)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.GenericType:
					{
						return new GenericTypeReference(
							GetInvokeTypeRef(invokeType, typeSig.DeclaringType, targetModule),
							typeSig.GenericArguments);
					}

				case TypeElementCode.DeclaringType:
					{
						return GetInvokeTypeRef(invokeType, (TypeReference)typeSig, targetModule);
					}

				default:
					return typeSig;
			}
		}

		private TypeReference GetInvokeTypeRef(ILCryptoInvokeType invokeType, TypeReference typeRef, BuildModule targetModule)
		{
			return new TypeReference(
				invokeType.TypeName,
				_mainTypeNamespace,
				typeRef.ResolutionScope,
				false);
		}

		#endregion

		#region Static

		public static void Map(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Map(module);
			}
		}

		public static void Map(BuildModule module)
		{
			var mapper = new ILCryptoMapper(module);
			mapper.Map();
		}

		#endregion
	}
}
