using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	internal class ILLoader
	{
		#region Fields

		private ILBody _body;
		private MethodBody _rawBody;
		private int _codeSize;
		private int[] _offsets;
		private int[] _sizes;
		private ILInstruction[] _instructions;
		private Dictionary<int, int> _instructionIndexByOffset = new Dictionary<int, int>();

		#endregion

		#region Ctors

		internal ILLoader(MethodBody rawBody)
		{
			_rawBody = rawBody;
		}

		#endregion

		#region Methods

		internal ILBody Load()
		{
			_body = new ILBody();
			_body.MaxStackSize = _rawBody.MaxStackSize;
			_body.InitLocals = _rawBody.InitLocals;

			LoadInstructions();
			LoadExceptionHandlers();
			LoadLocalVariables();
			LoadBranches();

			return _body;
		}

		private void LoadInstructions()
		{
			var rawInstructions = _rawBody.Instructions;
			int count = rawInstructions.Count;
			_instructions = new ILInstruction[count];
			_offsets = new int[count];
			_sizes = new int[count];

			// Create nodes
			int currOffset = 0;
			for (int i = 0; i < count; i++)
			{
				var rawInstruction = rawInstructions[i];
				_instructions[i] = new ILInstruction(rawInstruction);

				_offsets[i] = currOffset;
				_sizes[i] = rawInstruction.Size;
				_instructionIndexByOffset.Add(currOffset, i);
				currOffset += _sizes[i];
			}

			_codeSize = currOffset;

			// Add nodes
			ILBlock block = _body;
			ILNode lastChildNode = null;
			for (int i = 0; i < _instructions.Length; i++)
			{
				block.Add(_instructions[i], ref lastChildNode);
			}
		}

		private void LoadExceptionHandlers()
		{
			var rawExceptionHandlers = _rawBody.ExceptionHandlers;

			for (int i = 0; i < rawExceptionHandlers.Count; i++)
			{
				var rawEH = rawExceptionHandlers[i];

				switch (rawEH.Type)
				{
					case ExceptionHandlerType.Catch:
						LoadCatchHandler(rawEH);
						break;

					case ExceptionHandlerType.Filter:
						LoadFilterHandler(rawEH);
						break;

					case ExceptionHandlerType.Finally:
						LoadFinallyHandler(rawEH);
						break;

					case ExceptionHandlerType.Fault:
						LoadFaultHandler(rawEH);
						break;

					default:
						throw new InvalidOperationException();
				}
			}
		}

		private void LoadCatchHandler(ExceptionHandler rawEH)
		{
			// Create blocks.
			var tryBlock = LoadBlock<ILTryBlock>(rawEH.TryOffset, rawEH.TryOffset + rawEH.TryLength);

			var handlerBlock = LoadBlock<ILExceptionHandlerBlock>(rawEH.HandlerOffset, rawEH.HandlerOffset + rawEH.HandlerLength);
			if (handlerBlock.Try != null)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			// Set blocks.
			tryBlock.Handlers.Add(handlerBlock);
			handlerBlock.Try = tryBlock;
			handlerBlock.CatchType = rawEH.CatchType;
			handlerBlock.HandlerType = ExceptionHandlerType.Catch;
		}

		private void LoadFilterHandler(ExceptionHandler rawEH)
		{
			// Create blocks.
			var tryBlock = LoadBlock<ILTryBlock>(rawEH.TryOffset, rawEH.TryOffset + rawEH.TryLength);

			// Group filter and filter handler blocks in one.
			LoadBlock<ILBlock>(rawEH.FilterOffset, rawEH.HandlerOffset + rawEH.HandlerLength);

			var filterBlock = LoadBlock<ILExceptionFilterBlock>(rawEH.FilterOffset, rawEH.HandlerOffset);
			if (filterBlock.Try != null)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			var handlerBlock = LoadBlock<ILExceptionHandlerBlock>(rawEH.HandlerOffset, rawEH.HandlerOffset + rawEH.HandlerLength);
			if (handlerBlock.Try != null)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			// Set blocks.
			tryBlock.Handlers.Add(handlerBlock);
			handlerBlock.Try = tryBlock;
			handlerBlock.Filter = filterBlock;
			handlerBlock.HandlerType = ExceptionHandlerType.Filter;
			filterBlock.Try = tryBlock;
			filterBlock.Handler = handlerBlock;
		}

		private void LoadFinallyHandler(ExceptionHandler rawEH)
		{
			// Create blocks.
			var tryBlock = LoadBlock<ILTryBlock>(rawEH.TryOffset, rawEH.TryOffset + rawEH.TryLength);

			var handlerBlock = LoadBlock<ILExceptionHandlerBlock>(rawEH.HandlerOffset, rawEH.HandlerOffset + rawEH.HandlerLength);
			if (handlerBlock.Try != null)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			// Set blocks.
			tryBlock.Handlers.Add(handlerBlock);
			handlerBlock.Try = tryBlock;
			handlerBlock.HandlerType = ExceptionHandlerType.Finally;
		}

		private void LoadFaultHandler(ExceptionHandler rawEH)
		{
			var tryBlock = LoadBlock<ILTryBlock>(rawEH.TryOffset, rawEH.TryOffset + rawEH.TryLength);

			var handlerBlock = LoadBlock<ILExceptionHandlerBlock>(rawEH.HandlerOffset, rawEH.HandlerOffset + rawEH.HandlerLength);
			if (handlerBlock.Try != null)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			// Set blocks.
			tryBlock.Handlers.Add(handlerBlock);
			handlerBlock.Try = tryBlock;
			handlerBlock.HandlerType = ExceptionHandlerType.Fault;
		}

		private T LoadBlock<T>(int startOffset, int endOffset)
			where T : ILBlock, new()
		{
			if (startOffset >= endOffset)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			var startNode = GetInstructionByOffset(startOffset);

			ILNode endNode;
			if (endOffset >= _codeSize)
			{
				endNode = _instructions[_instructions.Length - 1];
			}
			else
			{
				int index;
				if (!_instructionIndexByOffset.TryGetValue(endOffset, out index))
				{
					throw new ILException(SR.MethodILNotValid);
				}

				endNode = _instructions[index - 1];
			}

			return ILBlock.Group<T>(startNode, endNode);
		}

		private void LoadLocalVariables()
		{
			_body.LocalVariables.Capacity = _rawBody.LocalVariables.Count;

			foreach (var localVariable in _rawBody.LocalVariables)
			{
				_body.LocalVariables.Add(localVariable);
			}
		}

		private void LoadBranches()
		{
			var rawInstructions = _rawBody.Instructions;

			for (int i = 0; i < _instructions.Length; i++)
			{
				var rawInstruction = rawInstructions[i];
				switch (rawInstruction.OpCode.OperandType)
				{
					case OperandType.InlineBrTarget:
						{
							var instruction = (ILInstruction)_instructions[i];
							int targetOffset = _offsets[i] + _sizes[i] + (int)rawInstruction.Value;
							instruction.Value = CreateLabelByOffset(instruction, targetOffset);
						}
						break;

					case OperandType.ShortInlineBrTarget:
						{
							var instruction = (ILInstruction)_instructions[i];
							int targetOffset = _offsets[i] + _sizes[i] + (sbyte)rawInstruction.Value;
							instruction.Value = CreateLabelByOffset(instruction, targetOffset);
						}
						break;

					case OperandType.InlineSwitch:
						{
							var instruction = (ILInstruction)_instructions[i];
							int nextOffset = _offsets[i] + _sizes[i];
							var branchOffsets = (int[])rawInstruction.Value;
							var targets = new ILLabel[branchOffsets.Length];
							for (int j = 0; j < branchOffsets.Length; j++)
							{
								int targetOffset = nextOffset + branchOffsets[j];
								targets[j] = CreateLabelByOffset(instruction, targetOffset);
							}

							instruction.Value = targets;
						}
						break;
				}
			}
		}

		private ILLabel CreateLabelByOffset(ILInstruction branch, int targetOffset)
		{
			int index;
			if (!_instructionIndexByOffset.TryGetValue(targetOffset, out index))
			{
				throw new ILException(SR.MethodILNotValid);
			}

			var node = (ILNode)_instructions[index];

			if (node.PreviousSibling == null && node.Parent != null)
			{
				int nodeLevel = node.Level;
				int branchLevel = branch.Level;

				var opCode = branch.OpCode;
				if (opCode == OpCodes.Leave || opCode == OpCodes.Leave_S)
				{
					// Leave instructions target one level below.
					while (node.PreviousSibling == null && node.Parent != null && nodeLevel >= branchLevel)
					{
						node = node.Parent;
						nodeLevel--;
					}
				}
				else
				{
					// Branch instructions target the same level.
					while (node.PreviousSibling == null && node.Parent != null && nodeLevel > branchLevel)
					{
						node = node.Parent;
						nodeLevel--;
					}
				}
			}

			var label = new ILLabel(branch);
			node.AddPrevious(label);

			return label;
		}

		private ILInstruction GetInstructionByOffset(int offset)
		{
			int index;
			if (!_instructionIndexByOffset.TryGetValue(offset, out index))
			{
				throw new ILException(SR.MethodILNotValid);
			}

			return _instructions[index];
		}

		#endregion
	}
}
