using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Net
{
	public class OpCode
	{
		#region Fields

		private string _name;
		private byte _opByte1;
		private byte _opByte2;
		private OpCodeType _type;
		private OperandType _operand;
		private FlowControl _flowControl;
		private StackBehavior _pop;
		private StackBehavior _push;
		private sbyte _stackChange;

		#endregion

		#region Ctors

		public OpCode(
			string name, byte opByte1, byte opByte2,
			OpCodeType type, OperandType operand, FlowControl flowControl,
			StackBehavior pop, StackBehavior push, sbyte stackChange)
		{
			_name = name;
			_opByte1 = opByte1;
			_opByte2 = opByte2;
			_type = type;
			_operand = operand;
			_flowControl = flowControl;
			_pop = pop;
			_push = push;
			_stackChange = stackChange;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// If OpCode is one byte size this value is 0xFF. If it's two bytes this value is 0xFE.
		/// Two bytes OpCodes allways start with 0xFE. Not unique.
		/// </summary>
		public byte OpByte1
		{
			get { return _opByte1; }
		}

		/// <summary>
		/// OpCodes. Not unique.
		/// </summary>
		public byte OpByte2
		{
			get { return _opByte2; }
		}

		/// <summary>
		/// Combined value of OpByte1 and OpByte2. Unique.
		/// </summary>
		public int OpValue
		{
			get
			{
				if (_opByte1 == 0xFF)
					return _opByte2; // one byte opcode
				else
					return (int)((_opByte1 << 8) | _opByte2); // two bytes opcode
			}
		}

		public int Index
		{
			get
			{
				if (_opByte1 == 0xFF)
					return _opByte2; // one byte opcode
				else
					return 256 + _opByte2;
			}
		}

		public int OpSize
		{
			get { return _opByte1 == 0xFF ? 1 : 2; }
		}

		public OpCodeType Type
		{
			get { return _type; }
		}

		public OperandType OperandType
		{
			get { return _operand; }
		}

		public FlowControl FlowControl
		{
			get { return _flowControl; }
		}

		public StackBehavior StackBehaviorPop
		{
			get { return _pop; }
		}

		public StackBehavior StackBehaviorPush
		{
			get { return _push; }
		}

		public FlowBranchType BranchType
		{
			get
			{
				if (_type == OpCodeType.Branch)
					return FlowBranchType.Unconditional;
				else if (_type == OpCodeType.CondBranch)
					return FlowBranchType.Conditional;
				else
					return FlowBranchType.None;
			}
		}

		public int StackChange
		{
			get { return _stackChange; }
		}

		#endregion

		#region Methods

		public bool Equals(OpCode opCode)
		{
			if (object.ReferenceEquals(opCode, null))
				return false;

			return opCode._opByte1 == _opByte1 && opCode._opByte2 == _opByte2;
		}

		public override bool Equals(object obj)
		{
			OpCode opCode = obj as OpCode;
			if (object.ReferenceEquals(opCode, null))
				return false;

			return opCode._opByte1 == _opByte1 && opCode._opByte2 == _opByte2;
		}

		public override int GetHashCode()
		{
			if (_opByte1 == 0xFF)
				return _opByte2; // one byte opcode
			else
				return (int)((_opByte1 << 8) | _opByte2); // two bytes opcode
		}

		public override string ToString()
		{
			return _name;
		}

		public OpCode ToLongBranch()
		{
			switch (_operand)
			{
				case OperandType.InlineBrTarget:
					return this;

				case OperandType.ShortInlineBrTarget:
					{
						switch (OpValue)
						{
							case OpCodeValues.Br_S:
								return OpCodes.Br;

							case OpCodeValues.Brfalse_S:
								return OpCodes.Brfalse;

							case OpCodeValues.Brtrue_S:
								return OpCodes.Brtrue;

							case OpCodeValues.Leave_S:
								return OpCodes.Leave;

							case OpCodeValues.Beq_S:
								return OpCodes.Beq;

							case OpCodeValues.Bge_S:
								return OpCodes.Bge;

							case OpCodeValues.Bge_Un_S:
								return OpCodes.Bge_Un;

							case OpCodeValues.Bgt_S:
								return OpCodes.Bgt;

							case OpCodeValues.Bgt_Un_S:
								return OpCodes.Bgt_Un;

							case OpCodeValues.Ble_S:
								return OpCodes.Ble;

							case OpCodeValues.Ble_Un_S:
								return OpCodes.Ble_Un;

							case OpCodeValues.Blt_S:
								return OpCodes.Blt;

							case OpCodeValues.Blt_Un_S:
								return OpCodes.Blt_Un;

							case OpCodeValues.Bne_Un_S:
								return OpCodes.Bne_Un;

							default:
								throw new InvalidOperationException();
						}
					}

				default:
					throw new InvalidOperationException();
			}
		}

		public OpCode ToShortBranch()
		{
			switch (_operand)
			{
				case OperandType.InlineBrTarget:
					{
						switch (OpValue)
						{
							case OpCodeValues.Br:
								return OpCodes.Br_S;

							case OpCodeValues.Brfalse:
								return OpCodes.Brfalse_S;

							case OpCodeValues.Brtrue:
								return OpCodes.Brtrue_S;

							case OpCodeValues.Leave:
								return OpCodes.Leave_S;

							case OpCodeValues.Beq:
								return OpCodes.Beq_S;

							case OpCodeValues.Bge:
								return OpCodes.Bge_S;

							case OpCodeValues.Bge_Un:
								return OpCodes.Bge_Un_S;

							case OpCodeValues.Bgt:
								return OpCodes.Bgt_S;

							case OpCodeValues.Bgt_Un:
								return OpCodes.Bgt_Un_S;

							case OpCodeValues.Ble:
								return OpCodes.Ble_S;

							case OpCodeValues.Ble_Un:
								return OpCodes.Ble_Un_S;

							case OpCodeValues.Blt:
								return OpCodes.Blt_S;

							case OpCodeValues.Blt_Un:
								return OpCodes.Blt_Un_S;

							case OpCodeValues.Bne_Un:
								return OpCodes.Bne_Un_S;

							default:
								throw new InvalidOperationException();
						}
					}

				case OperandType.ShortInlineBrTarget:
					return this;

				default:
					throw new InvalidOperationException();
			}
		}

		#endregion

		#region Static

		public static OpCode Get(int opValue)
		{
			if (opValue < 256)
				return OpCodes.OpCodeArray[opValue];
			else
				return OpCodes.OpCodeArray[256 + (opValue & 0xff)];
		}

		public static bool operator ==(OpCode a, OpCode b)
		{
			if (object.ReferenceEquals(a, null))
				return object.ReferenceEquals(b, null);

			return a.Equals(b);
		}

		public static bool operator !=(OpCode a, OpCode b)
		{
			return !(a == b);
		}

		#endregion
	}
}
