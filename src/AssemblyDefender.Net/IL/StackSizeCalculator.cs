using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	public class StackSizeCalculator
	{
		#region Fields

		private int _maxStackSize;
		private ILBody _body;
		private MethodDeclaration _method;
		private Stack<BranchInfo> _branchStack = new Stack<BranchInfo>();
		private Dictionary<ILInstruction, int> _stackSizeByInstruction = new Dictionary<ILInstruction, int>();

		#endregion

		#region Ctors

		public StackSizeCalculator(ILBody body, MethodDeclaration method)
		{
			_body = body;
			_method = method;
		}

		#endregion

		#region Properties

		public int MaxStackSize
		{
			get { return _maxStackSize; }
		}

		public Dictionary<ILInstruction, int> StackSizeByInstruction
		{
			get { return _stackSizeByInstruction; }
		}

		#endregion

		#region Methods

		public void Calculate()
		{
			int stackSize = 0;
			Process(_body, ref stackSize);

			while (_branchStack.Count > 0)
			{
				var branchInfo = _branchStack.Pop();
				stackSize = branchInfo.StackSize;
				Process(branchInfo.Node, ref stackSize);
			}
		}

		private bool Process(ILNode node, ref int stackSize)
		{
			while (node != null)
			{
				switch (node.NodeType)
				{
					case ILNodeType.Block:
						{
							var block = (ILBlock)node;

							// Catch handler, filter handler, and filter blocks begin with stack of 1 (Exception).
							switch (block.BlockType)
							{
								case ILBlockType.ExceptionFilter:
									{
										if (stackSize < 1)
											stackSize = 1;
									}
									break;

								case ILBlockType.ExceptionHandle:
									{
										var handler = (ILExceptionHandlerBlock)block;
										if (handler.HandlerType == ExceptionHandlerType.Catch || handler.HandlerType == ExceptionHandlerType.Filter)
										{
											if (stackSize < 1)
												stackSize = 1;
										}
									}
									break;
							}

							if (block.FirstChild != null)
							{
								if (!Process(block.FirstChild, ref stackSize))
									return false;
							}
						}
						break;

					case ILNodeType.Instruction:
						{
							var instruction = (ILInstruction)node;

							// Stack size is already calcualated.
							int currentStackSize;
							if (_stackSizeByInstruction.TryGetValue(instruction, out currentStackSize) && currentStackSize >= stackSize)
								return false;

							Process(instruction, ref stackSize);
						}
						break;
				}

				node = node.NextSibling;
			}

			return true;
		}

		private void Process(ILInstruction instruction, ref int stackSize)
		{
			// Prevents invalid IL.
			if (stackSize < short.MinValue || stackSize > short.MaxValue)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			// Set stack size.
			_stackSizeByInstruction[instruction] = stackSize;

			if (_maxStackSize < stackSize)
				_maxStackSize = stackSize;

			// Calculate current.
			stackSize += instruction.GetStackChange(_method);

			var opCode = instruction.OpCode;

			// Enqueue branches.
			switch (opCode.OperandType)
			{
				case OperandType.InlineBrTarget:
				case OperandType.ShortInlineBrTarget:
					{
						var label = (ILLabel)instruction.Value;

						_branchStack.Push(
							new BranchInfo()
							{
								Node = label,
								StackSize = stackSize,
							});
					}
					break;

				case OperandType.InlineSwitch:
					{
						var labels = (ILLabel[])instruction.Value;

						foreach (var label in labels)
						{
							_branchStack.Push(
								new BranchInfo()
								{
									Node = label,
									StackSize = stackSize,
								});
						}
					}
					break;
			}

			if (opCode.BranchType == FlowBranchType.Unconditional)
			{
				stackSize = 0;
			}
		}

		#endregion

		#region Nested types

		internal struct BranchInfo
		{
			internal ILNode Node;
			internal int StackSize;
		}

		#endregion
	}
}
