using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public static class MethodDevirtualizer
	{
		#region Analyze

		public static void Analyze(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Analyze(module);
			}
		}

		public static void Analyze(BuildModule module)
		{
			foreach (BuildType type in module.Types)
			{
				Analyze(type);
			}
		}

		public static void Analyze(BuildType type)
		{
			if (type.IsInterface)
				return;

			var slots = new MethodSlotList(type);
			foreach (var slot in slots)
			{
				Analyze(slot);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Analyze(nestedType);
			}
		}

		private static void Analyze(MethodSlot slot)
		{
			if (!slot.MainMethod.Method.IsVirtual)
				return;

			if (slot.Count == 1)
			{
				var slotMethod = slot.MainMethod;
				if (slotMethod.BaseMethod == null && slotMethod.InterfaceMethods.Count == 0)
					return;
			}

			for (int i = 0; i < slot.Count; i++)
			{
				var slotMethod = slot[i];
				while (slotMethod != null)
				{
					var method = slotMethod.Method.DeclaringMethod as BuildMethod;
					if (method != null)
					{
						if (method.DevirtualizeMethodProcessed && !method.DevirtualizeMethod)
							break;

						method.DevirtualizeMethod = false;
						method.DevirtualizeMethodProcessed = true;
					}

					slotMethod = slotMethod.BaseMethod;
				}
			}
		}

		#endregion

		#region Change

		public static void Devirtualize(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Devirtualize(module);
			}
		}

		public static void Devirtualize(BuildModule module)
		{
			foreach (BuildType type in module.Types)
			{
				Devirtualize(type);
			}
		}

		public static void Devirtualize(BuildType type)
		{
			if (type.IsMainType)
				return;

			if (type.IsInterface)
				return;

			foreach (BuildMethod method in type.Methods)
			{
				Devirtualize(method);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Devirtualize(nestedType);
			}
		}

		public static void Devirtualize(BuildMethod method)
		{
			DevirtualizeMethod(method);
			DevirtualizeCalls(method);
		}

		private static void DevirtualizeMethod(BuildMethod method)
		{
			if (!method.DevirtualizeMethod)
				return;

			if (!method.IsVirtual)
				return;

			var ownerType = method.GetOwnerType();
			if (ownerType.IsInterface)
				return;

			method.IsVirtual = false;
			method.IsNewSlot = false;
		}

		private static void DevirtualizeCalls(BuildMethod method)
		{
			if (!MethodBody.IsValid(method))
				return;

			var methodBody = MethodBody.Load(method);
			if (methodBody == null)
				return;

			bool changed = false;
			var instructions = methodBody.Instructions;
			for (int i = 0; i < instructions.Count; i++)
			{
				var instruction = instructions[i];

				var opCode = instruction.OpCode;
				if (opCode.Type == OpCodeType.Prefix)
				{
					i++;
					continue;
				}

				if (opCode != OpCodes.Callvirt)
					continue;

				var calledMethodSig = instruction.Value as MethodSignature;
				if (calledMethodSig == null)
					continue;

				var calledMethod = calledMethodSig.Resolve(method.Module, true);
				if (calledMethod == null)
					continue;

				var calledDeclaringMethod = calledMethod.DeclaringMethod as BuildMethod;
				if (calledDeclaringMethod == null)
					continue;

				if (!calledDeclaringMethod.DevirtualizeMethod)
					continue;

				instructions[i] = new Instruction(OpCodes.Call, instruction.Value);
				changed = true;
			}

			if (changed)
			{
				methodBody.Build(method);
			}
		}

		#endregion
	}
}
