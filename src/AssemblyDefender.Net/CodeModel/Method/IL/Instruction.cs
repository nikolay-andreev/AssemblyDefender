using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Instruction
	{
		#region Fields

		private OpCode _opCode;
		private object _value;

		#endregion

		#region Ctors

		public Instruction(OpCode opCode)
		{
			_opCode = opCode;
			_value = null;
		}

		public Instruction(OpCode opCode, object value)
		{
			_opCode = opCode;
			_value = value;
		}

		#endregion

		#region Properties

		public OpCode OpCode
		{
			get { return _opCode; }
		}

		public object Value
		{
			get { return _value; }
		}

		public int Size
		{
			get { return GetSize(_opCode, _value); }
		}

		public int GetStackChange(MethodDeclaration method)
		{
			return GetStackChange(_opCode, _value, method);
		}

		public int GetPopStackChange(MethodDeclaration method)
		{
			return GetPopStackChange(_opCode, _value, method);
		}

		public int GetPushStackChange(MethodDeclaration method)
		{
			return GetPushStackChange(_opCode, _value, method);
		}

		public override string ToString()
		{
			if (_value != null)
				return string.Format("{0} {1}", _opCode.Name, _value.ToString());
			else
				return _opCode.Name;
		}

		#endregion

		#region Static

		public static Instruction GetLdc(int value)
		{
			if (value >= 0 && value <= 8)
			{
				switch (value)
				{
					case 0:
						return new Instruction(OpCodes.Ldc_I4_0);

					case 1:
						return new Instruction(OpCodes.Ldc_I4_1);

					case 2:
						return new Instruction(OpCodes.Ldc_I4_2);

					case 3:
						return new Instruction(OpCodes.Ldc_I4_3);

					case 4:
						return new Instruction(OpCodes.Ldc_I4_4);

					case 5:
						return new Instruction(OpCodes.Ldc_I4_5);

					case 6:
						return new Instruction(OpCodes.Ldc_I4_6);

					case 7:
						return new Instruction(OpCodes.Ldc_I4_7);

					case 8:
						return new Instruction(OpCodes.Ldc_I4_8);

					default:
						throw new InvalidOperationException();
				}
			}
			else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
			{
				if (value == -1)
				{
					return new Instruction(OpCodes.Ldc_I4_M1);
				}
				else
				{
					return new Instruction(OpCodes.Ldc_I4_S, (byte)value);
				}
			}
			else
			{
				return new Instruction(OpCodes.Ldc_I4, value);
			}
		}

		public static Instruction GetLdarg(int value)
		{
			if (value < 0)
			{
				throw new ArgumentException("value");
			}

			if (value <= 3)
			{
				switch (value)
				{
					case 0:
						return new Instruction(OpCodes.Ldarg_0);

					case 1:
						return new Instruction(OpCodes.Ldarg_1);

					case 2:
						return new Instruction(OpCodes.Ldarg_2);

					case 3:
						return new Instruction(OpCodes.Ldarg_3);

					default:
						throw new InvalidOperationException();
				}
			}
			else if (value <= byte.MaxValue)
			{
				return new Instruction(OpCodes.Ldarg_S, (byte)value);
			}
			else
			{
				return new Instruction(OpCodes.Ldarg, value);
			}
		}

		public static Instruction GetLdarga(int value)
		{
			if (value < 0)
			{
				throw new ArgumentException("value");
			}

			if (value <= byte.MaxValue)
			{
				return new Instruction(OpCodes.Ldarga_S, (byte)value);
			}
			else
			{
				return new Instruction(OpCodes.Ldarga, value);
			}
		}

		public static Instruction GetLdloc(int value)
		{
			if (value < 0)
			{
				throw new ArgumentException("value");
			}

			if (value <= 3)
			{
				switch (value)
				{
					case 0:
						return new Instruction(OpCodes.Ldloc_0);

					case 1:
						return new Instruction(OpCodes.Ldloc_1);

					case 2:
						return new Instruction(OpCodes.Ldloc_2);

					case 3:
						return new Instruction(OpCodes.Ldloc_3);

					default:
						throw new InvalidOperationException();
				}
			}
			else if (value <= byte.MaxValue)
			{
				return new Instruction(OpCodes.Ldloc_S, (byte)value);
			}
			else
			{
				return new Instruction(OpCodes.Ldloc, value);
			}
		}

		public static Instruction GetLdloca(int value)
		{
			if (value < 0)
			{
				throw new ArgumentException("value");
			}

			if (value <= byte.MaxValue)
			{
				return new Instruction(OpCodes.Ldloca_S, (byte)value);
			}
			else
			{
				return new Instruction(OpCodes.Ldloca, value);
			}
		}

		public static Instruction GetStarg(int value)
		{
			if (value < 0)
			{
				throw new ArgumentException("value");
			}

			if (value <= byte.MaxValue)
			{
				return new Instruction(OpCodes.Starg_S, (byte)value);
			}
			else
			{
				return new Instruction(OpCodes.Starg, value);
			}
		}

		public static Instruction GetStloc(int value)
		{
			if (value < 0)
			{
				throw new ArgumentException("value");
			}

			if (value <= 3)
			{
				switch (value)
				{
					case 0:
						return new Instruction(OpCodes.Stloc_0);

					case 1:
						return new Instruction(OpCodes.Stloc_1);

					case 2:
						return new Instruction(OpCodes.Stloc_2);

					case 3:
						return new Instruction(OpCodes.Stloc_3);

					default:
						throw new InvalidOperationException();
				}
			}
			else if (value <= byte.MaxValue)
			{
				return new Instruction(OpCodes.Stloc_S, (byte)value);
			}
			else
			{
				return new Instruction(OpCodes.Stloc, value);
			}
		}

		public static int GetSize(OpCode opCode, object value)
		{
			int size = opCode.OpSize;

			switch (opCode.OperandType)
			{
				case OperandType.InlineNone:
					break;

				case OperandType.ShortInlineBrTarget:
				case OperandType.ShortInlineI:
				case OperandType.ShortInlineVar:
					size += 1;
					break;

				case OperandType.InlineVar:
					size += 2;
					break;

				case OperandType.InlineBrTarget:
				case OperandType.InlineField:
				case OperandType.InlineI:
				case OperandType.InlineMethod:
				case OperandType.InlineSig:
				case OperandType.InlineString:
				case OperandType.InlineType:
				case OperandType.ShortInlineR:
				case OperandType.InlineTok:
					size += 4;
					break;

				case OperandType.InlineSwitch:
					{
						// count of targets + each target
						size += 4;
						size += ((int[])value).Length * 4;
					}
					break;

				case OperandType.InlineI8:
				case OperandType.InlineR:
					size += 8;
					break;

				default:
					throw new InvalidOperationException();
			}

			return size;
		}

		public static int GetSize(IList<Instruction> instructions)
		{
			return GetSize(instructions, 0, instructions.Count);
		}

		public static int GetSize(IList<Instruction> instructions, int offset, int count)
		{
			int size = 0;

			for (int i = offset; i < count; i++)
			{
				size += instructions[i].Size;
			}

			return size;
		}

		public static int GetStackChange(OpCode opCode, object value, MethodDeclaration method)
		{
			if (value is MethodSignature)
			{
				var methodSig = (MethodSignature)value;

				switch (opCode.OpValue)
				{
					case OpCodeValues.Call:
					case OpCodeValues.Callvirt:
						{
							int stackChange = 0;

							if (methodSig.ReturnType.GetTypeCode(method.Module) != PrimitiveTypeCode.Void)
								stackChange++;

							stackChange -= methodSig.Arguments.Count;

							if (methodSig.HasThis)
								stackChange--;

							return stackChange;
						}

					case OpCodeValues.Calli:
						{
							// Function is popped from stack.
							int stackChange = -1;

							if (methodSig.ReturnType.GetTypeCode(method.Module) != PrimitiveTypeCode.Void)
								stackChange++;

							stackChange -= methodSig.Arguments.Count;

							if (methodSig.HasThis)
								stackChange--;

							return stackChange;
						}

					case OpCodeValues.Newobj:
						{
							int stackChange = 1;

							stackChange -= methodSig.Arguments.Count;

							return stackChange;
						}

					case OpCodeValues.Ret:
						{
							if (method.ReturnType.TypeCode != PrimitiveTypeCode.Void)
								return -1;
							else
								return 0;
						}
				}
			}

			return opCode.StackChange;
		}

		public static int GetPopStackChange(OpCode opCode, object value, MethodDeclaration method)
		{
			var behavior = opCode.StackBehaviorPop;
			if (behavior == StackBehavior.Varpop)
			{
				switch (opCode.OpValue)
				{
					case OpCodeValues.Call:
					case OpCodeValues.Callvirt:
						{
							var methodSig = (MethodSignature)value;

							int stackChange = (-1 * methodSig.Arguments.Count);

							if (methodSig.HasThis)
								stackChange--;

							return stackChange;
						}

					case OpCodeValues.Calli:
						{
							var methodSig = (MethodSignature)value;

							// Function is popped from stack.
							int stackChange = -1;

							stackChange -= methodSig.Arguments.Count;

							if (methodSig.HasThis)
								stackChange--;

							return stackChange;
						}

					case OpCodeValues.Newobj:
						{
							var methodSig = (MethodSignature)value;

							return (-1 * methodSig.Arguments.Count);
						}

					case OpCodeValues.Ret:
						{
							if (method.ReturnType.TypeCode != PrimitiveTypeCode.Void)
								return -1;
							else
								return 0;
						}

					default:
						throw new InvalidOperationException();
				}
			}
			else
			{
				return _stackChanges[(int)behavior];
			}
		}

		public static int GetPushStackChange(OpCode opCode, object value, MethodDeclaration method)
		{
			var behavior = opCode.StackBehaviorPush;
			if (behavior == StackBehavior.Varpush)
			{
				switch (opCode.OpValue)
				{
					case OpCodeValues.Call:
					case OpCodeValues.Callvirt:
					case OpCodeValues.Calli:
						{
							var methodSig = (MethodSignature)value;

							if (methodSig.ReturnType.GetTypeCode(method.Module) != PrimitiveTypeCode.Void)
								return 1;
							else
								return 0;
						}

					default:
						throw new InvalidOperationException();
				}
			}
			else
			{
				return _stackChanges[(int)behavior];
			}
		}

		public static int GetStackChange(StackBehavior behavior)
		{
			return _stackChanges[(int)behavior];
		}

		private static int[] _stackChanges = new int[]
		{
			0, // Pop0
			-1, // Pop1
			-2, // Pop1_pop1
			-1, // Popi
			-2, // Popi_pop1
			-2, // Popi_popi
			-2, // Popi_popi8
			-3, // Popi_popi_popi
			-2, // Popi_popr4
			-2, // Popi_popr8
			-1, // Popref
			-2, // Popref_pop1
			-2, // Popref_popi
			-3, // Popref_popi_pop1
			-3, // Popref_popi_popi
			-3, // Popref_popi_popi8
			-3, // Popref_popi_popr4
			-3, // Popref_popi_popr8
			-3, // Popref_popi_popref
			0, // Varpop
			0, // Push0
			1, // Push1
			2, // Push1_push1
			1, // Pushi
			1, // Pushi8
			1, // Pushr4
			1, // Pushr8
			1, // Pushref
			0, // Varpush
		};

		#endregion
	}
}
