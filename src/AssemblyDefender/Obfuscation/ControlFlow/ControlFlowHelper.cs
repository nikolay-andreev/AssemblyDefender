using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	public static class ControlFlowHelper
	{
		public static bool IsAtomic(MethodBody methodBody)
		{
			if (methodBody.LocalVariables.Count > 0)
				return false;

			if (methodBody.ExceptionHandlers.Count > 0)
				return false;

			var instructions = methodBody.Instructions;
			int count = instructions.Count;
			if (count > 16)
				return false;

			bool isDefinedOnce = false;

			for (int i = 0; i < count; i++)
			{
				var instruction = instructions[i];
				var opCode = instruction.OpCode;

				switch (opCode.Type)
				{
					case OpCodeType.Prefix:
					case OpCodeType.Reserved:
					case OpCodeType.Ldarg:
					case OpCodeType.Starg:
					case OpCodeType.Ldfld:
					case OpCodeType.Ldc:
					case OpCodeType.Object:
						break;

					case OpCodeType.Call:
					case OpCodeType.Stfld:
					case OpCodeType.Math:
						{
							if (isDefinedOnce)
								return false;

							isDefinedOnce = true;
						}
						break;

					case OpCodeType.Branch:
						{
							if (opCode != OpCodes.Ret &&
								opCode != OpCodes.Throw &&
								opCode != OpCodes.Rethrow)
							{
								return false;
							}
						}
						break;

					default:
						return false;
				}
			}

			return true;
		}
	}
}
