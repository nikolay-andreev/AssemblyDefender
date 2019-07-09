using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	public class ILInstruction : ILNode
	{
		#region Fields

		private OpCode _opCode;
		private object _value;

		#endregion

		#region Ctors

		public ILInstruction()
		{
		}

		public ILInstruction(OpCode opCode)
		{
			_opCode = opCode;
		}

		public ILInstruction(OpCode opCode, object value)
		{
			_opCode = opCode;
			_value = value;
		}

		public ILInstruction(OpCode opCode, ILLabel label)
		{
			_opCode = opCode;
			_value = label;

			if (label != null)
			{
				label.Branch = this;
			}
		}

		public ILInstruction(Net.Instruction instruction)
		{
			_opCode = instruction.OpCode;
			_value = instruction.Value;
		}

		#endregion

		#region Properties

		public OpCode OpCode
		{
			get { return _opCode; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("OpCode");

				_opCode = value;
			}
		}

		/// <remarks>
		/// branch: Label
		/// switch: Label[]
		/// </remarks>
		public object Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public int Size
		{
			get
			{
				int size = _opCode.OpSize;

				switch (_opCode.OperandType)
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
							size += ((List<ILInstruction>)_value).Count * 4;
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
		}

		public override ILNodeType NodeType
		{
			get { return ILNodeType.Instruction; }
		}

		#endregion

		#region Methods

		public int GetStackChange(MethodDeclaration method)
		{
			return Net.Instruction.GetStackChange(_opCode, _value, method);
		}

		public int GetPopStackChange(MethodDeclaration method)
		{
			return Net.Instruction.GetPopStackChange(_opCode, _value, method);
		}

		public int GetPushStackChange(MethodDeclaration method)
		{
			return Net.Instruction.GetPushStackChange(_opCode, _value, method);
		}

		public override string ToString()
		{
			if (_value != null && _opCode.BranchType == FlowBranchType.None)
			{
				return string.Format("{0} {1}", _opCode.Name, _value.ToString());
			}
			else
			{
				return _opCode.Name;
			}
		}

		#endregion
	}
}
