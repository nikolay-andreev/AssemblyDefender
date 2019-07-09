using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	/// <summary>
	/// Redirect method references to exact method in base type.
	/// </summary>
	public static class ExplicitMethodCallBuilder
	{
		public static void Build(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Build(module);
			}
		}

		public static void Build(BuildModule module)
		{
			foreach (BuildType type in module.Types)
			{
				Build(type);
			}
		}

		public static void Build(BuildType type)
		{
			if (type.IsMainType)
				return;

			foreach (BuildMethod method in type.Methods)
			{
				Build(method);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Build(nestedType);
			}
		}

		public static void Build(BuildMethod method)
		{
			if (!MethodBody.IsValid(method))
				return;

			var methodBody = MethodBody.Load(method);
			if (methodBody == null)
				return;

			var builder = new Builder(method);

			bool changed = false;
			var instructions = methodBody.Instructions;
			for (int i = 0; i < instructions.Count; i++)
			{
				var instruction = instructions[i];
				if (instruction.OpCode != OpCodes.Call && instruction.OpCode != OpCodes.Callvirt)
					continue;

				var calledMethodSig = instruction.Value as MethodSignature;
				if (calledMethodSig == null)
					continue;

				if (builder.Build(ref calledMethodSig))
				{
					instructions[i] = new Instruction(instruction.OpCode, calledMethodSig);
					changed = true;
				}
			}

			if (changed)
			{
				methodBody.Build(method);
			}
		}

		private class Builder : SignatureBuilder
		{
			private BuildMethod _owner;
			private IModule _module;
			private AssemblyManager _assemblyManager;

			public Builder(BuildMethod owner)
			{
				_owner = owner;
				_module = owner.Module;
				_assemblyManager = owner.AssemblyManager;
			}

			public override bool Build(ref MethodReference methodRef)
			{
				var ownerTypeRef = TypeReference.Get(methodRef.Owner);
				if (ownerTypeRef == null)
					return false;

				var resolvedMethod = _assemblyManager.Resolve(methodRef, _owner, true);
				if (resolvedMethod == null)
					return false;

				var resolvedType = resolvedMethod.Owner.DeclaringType;
				var ownerType = ownerTypeRef.Resolve(_module);
				if (object.ReferenceEquals(resolvedType, ownerType))
					return false;

				var resolvedDeclaringMethod = resolvedMethod.DeclaringMethod;

				var buildOwnerTypeSig = resolvedMethod.Owner.ToSignature(_module);

				var buildReturnTypeSig = resolvedDeclaringMethod.ReturnType.ToSignature(_module);

				var resolvedParameters = resolvedDeclaringMethod.Parameters;
				var buildArguments = new TypeSignature[resolvedParameters.Count];
				for (int i = 0; i < buildArguments.Length; i++)
				{
					buildArguments[i] = resolvedParameters[i].Type.ToSignature(_module);
				}

				methodRef = new MethodReference(
					methodRef.Name,
					buildOwnerTypeSig,
					new CallSite(
						methodRef.HasThis,
						methodRef.ExplicitThis,
						methodRef.CallConv,
						buildReturnTypeSig,
						buildArguments,
						methodRef.VarArgIndex,
						methodRef.GenericParameterCount));

				return true;
			}
		}
	}
}
