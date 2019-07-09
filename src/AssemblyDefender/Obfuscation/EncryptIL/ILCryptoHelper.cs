using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	public static class ILCryptoHelper
	{
		public static bool CanEncrypt(MethodDeclaration method)
		{
			if (method.IsAbstract)
				return false;

			if (method.CodeType != MethodCodeTypeFlags.CIL)
				return false;

			if (method.CallConv != MethodCallingConvention.Default)
				return false;

			if (method.IsConstructor())
				return false;

			if (method.GenericParameters.Count > 0)
				return false;

			var ownerType = method.GetOwnerType();
			if (ownerType.IsInterface)
				return false;

			if (ownerType.IsValueType())
				return false;

			if (!MethodBody.IsValid(method))
				return false;

			var methodBody = MethodBody.Load(method);
			if (HasVarArgCall(methodBody.Instructions))
				return false;

			return true;
		}

		public static bool CanEncrypt(MethodDeclaration method, MethodBody methodBody)
		{
			if (method.IsAbstract)
				return false;

			if (method.CodeType != MethodCodeTypeFlags.CIL)
				return false;

			if (method.CallConv != MethodCallingConvention.Default)
				return false;

			if (method.IsConstructor())
				return false;

			if (method.GenericParameters.Count > 0)
				return false;

			var ownerType = method.GetOwnerType();
			if (ownerType.IsInterface)
				return false;

			if (ownerType.IsValueType())
				return false;

			if (!MethodBody.IsValid(method))
				return false;

			if (HasVarArgCall(methodBody.Instructions))
				return false;

			return true;
		}

		private static bool HasVarArgCall(List<Instruction> instructions)
		{
			for (int i = 0; i < instructions.Count; i++)
			{
				var methodSig = instructions[i].Value as MethodSignature;
				if (methodSig == null)
					continue;

				if (methodSig.CallConv == MethodCallingConvention.VarArgs)
					return true;
			}

			return false;
		}
	}
}
