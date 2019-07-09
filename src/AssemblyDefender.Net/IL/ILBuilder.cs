using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	internal class ILBuilder
	{
		#region Fields

		private ILBody _body;
		private MethodBody _rawBody;
		private Instruction[] _rawInstructions;
		private int[] _offsets;
		private int[] _sizes;
		private List<ILInstruction> _instructions = new List<ILInstruction>(0x20);
		private List<ILInstruction> _branches = new List<ILInstruction>(0x10);
		private List<ILInstruction> _switches = new List<ILInstruction>(0x10);
		private List<ILExceptionHandlerBlock> _exceptionHandlerBlock = new List<ILExceptionHandlerBlock>();
		private Dictionary<ILInstruction, int> _indexByInstruction = new Dictionary<ILInstruction, int>(0x20);

		#endregion

		#region Ctors

		internal ILBuilder(ILBody body)
		{
			_body = body;
		}

		#endregion

		#region Methods

		internal MethodBody Build()
		{
			_rawBody = new MethodBody();
			_rawBody.MaxStackSize = _body.MaxStackSize;
			_rawBody.InitLocals = _body.InitLocals;

			CollectInstructions(_body);
			CollectExceptionHandlers(_body);
			BuildInstructions();
			BuildExceptionHandlers();
			BuildLocalVariables();

			return _rawBody;
		}

		private void BuildInstructions()
		{
			CreateInstructions();
			FixBranches();
			SetBranchOffsets();
			SetSwitchOffsets();
			AddInstructions();
		}

		private void CollectInstructions(ILNode node)
		{
			while (node != null)
			{
				switch (node.NodeType)
				{
					case ILNodeType.Block:
						CollectInstructions(((ILBlock)node).FirstChild);
						break;

					case ILNodeType.Instruction:
						_instructions.Add((ILInstruction)node);
						break;
				}

				node = node.NextSibling;
			}
		}

		private void CollectExceptionHandlers(ILNode node)
		{
			// Collect children
			var currentNode = node;
			while (currentNode != null)
			{
				var block = currentNode as ILBlock;
				if (block != null)
				{
					CollectExceptionHandlers(block.FirstChild);
				}

				currentNode = currentNode.NextSibling;
			}

			// Add current level
			currentNode = node;
			while (currentNode != null)
			{
				var tryBlock = currentNode as ILTryBlock;
				if (tryBlock != null)
				{
					foreach (var handlerBlock in tryBlock.Handlers)
					{
						_exceptionHandlerBlock.Add(handlerBlock);
					}
				}

				currentNode = currentNode.NextSibling;
			}
		}

		private void CreateInstructions()
		{
			int count = _instructions.Count;
			_rawInstructions = new Instruction[count];
			_offsets = new int[count];
			_sizes = new int[count];

			int currOffset = 0;
			for (int i = 0; i < count; i++)
			{
				var instruction = _instructions[i];
				switch (instruction.OpCode.OperandType)
				{
					case OperandType.InlineBrTarget:
					case OperandType.ShortInlineBrTarget:
						{
							OpCode opCode;
							if (instruction.OpCode.OperandType == OperandType.InlineBrTarget)
								opCode = instruction.OpCode.ToShortBranch();
							else
								opCode = instruction.OpCode;

							var label = (ILLabel)instruction.Value;
							var target = label.FindNext<ILInstruction>(ILNodeType.Instruction, true);
							var rawInstruction = new Instruction(opCode, target);
							_rawInstructions[i] = rawInstruction;

							// OpCode + ShortInlineBrTarget
							int size = instruction.OpCode.OpSize + 1;

							_offsets[i] = currOffset;
							_sizes[i] = size;
							_branches.Add(instruction);
							_indexByInstruction.Add(instruction, i);

							currOffset += size;
						}
						break;

					case OperandType.InlineSwitch:
						{
							var labels = (ILLabel[])instruction.Value;
							var targets = new ILInstruction[labels.Length];
							for (int j = 0; j < labels.Length; j++)
							{
								targets[j] = labels[j].FindNext<ILInstruction>(ILNodeType.Instruction, true);
							}

							var rawInstruction = new Instruction(instruction.OpCode, targets);
							_rawInstructions[i] = rawInstruction;

							int size = instruction.OpCode.OpSize;
							size += 4;
							size += targets.Length * 4;

							_offsets[i] = currOffset;
							_sizes[i] = size;
							_switches.Add(instruction);
							_indexByInstruction.Add(instruction, i);

							currOffset += size;
						}
						break;

					default:
						{
							var rawInstruction = new Instruction(instruction.OpCode, instruction.Value);
							_rawInstructions[i] = rawInstruction;

							int size = rawInstruction.Size;
							_offsets[i] = currOffset;
							_sizes[i] = size;
							_indexByInstruction.Add(instruction, i);

							currOffset += size;
						}
						break;
				}
			}
		}

		private void FixBranches()
		{
			if (_branches.Count == 0)
				return;

			while (true)
			{
				int buildIndex = -1;
				for (int i = 0; i < _branches.Count; i++)
				{
					var branch = _branches[i];

					int index = GetIndexByInstruction(branch);

					var rawInstruction = _rawInstructions[index];
					if (rawInstruction.OpCode.OperandType == OperandType.InlineBrTarget)
						continue;

					var target = (ILInstruction)rawInstruction.Value;

					int targetIndex = GetIndexByInstruction(target);

					int targetOffset = _offsets[targetIndex] - (_offsets[index] + _sizes[index]);
					if (targetOffset >= sbyte.MinValue && targetOffset <= sbyte.MaxValue)
						continue;

					rawInstruction =
						new Instruction(
							rawInstruction.OpCode.ToLongBranch(),
							rawInstruction.Value);

					_rawInstructions[index] = rawInstruction;

					if (buildIndex < 0 || buildIndex > index)
						buildIndex = index;
				}

				if (buildIndex == -1)
					break;

				// Fix sizes and offsets
				int currOffset = _offsets[buildIndex];
				for (int i = buildIndex; i < _rawInstructions.Length; i++)
				{
					_offsets[i] = currOffset;

					if (_rawInstructions[i].OpCode.OperandType == OperandType.InlineBrTarget)
					{
						_sizes[i] = _rawInstructions[i].OpCode.OpSize + 4;
					}

					currOffset += _sizes[i];
				}
			}
		}

		private void SetBranchOffsets()
		{
			for (int i = 0; i < _branches.Count; i++)
			{
				var branch = _branches[i];

				int index = GetIndexByInstruction(branch);
				var rawInstruction = _rawInstructions[index];
				var target = (ILInstruction)rawInstruction.Value;
				int targetIndex = GetIndexByInstruction(target);
				int targetOffset = _offsets[targetIndex] - (_offsets[index] + _sizes[index]);

				switch (rawInstruction.OpCode.OperandType)
				{
					case OperandType.InlineBrTarget:
						rawInstruction = new Instruction(rawInstruction.OpCode, (int)targetOffset);
						break;

					case OperandType.ShortInlineBrTarget:
						rawInstruction = new Instruction(rawInstruction.OpCode, (sbyte)targetOffset);
						break;

					default:
						throw new InvalidOperationException();
				}

				_rawInstructions[index] = rawInstruction;
			}
		}

		private void SetSwitchOffsets()
		{
			for (int i = 0; i < _switches.Count; i++)
			{
				var switchNode = _switches[i];

				int index = GetIndexByInstruction(switchNode);

				var rawInstruction = _rawInstructions[index];
				var targets = (ILInstruction[])rawInstruction.Value;
				int[] targetOffsets = new int[targets.Length];

				for (int j = 0; j < targets.Length; j++)
				{
					int targetIndex = GetIndexByInstruction(targets[j]);
					targetOffsets[j] = _offsets[targetIndex] - (_offsets[index] + _sizes[index]);
				}

				rawInstruction = new Instruction(rawInstruction.OpCode, targetOffsets);
				_rawInstructions[index] = rawInstruction;
			}
		}

		private void AddInstructions()
		{
			var instructions = _rawBody.Instructions;
			for (int i = 0; i < _rawInstructions.Length; i++)
			{
				instructions.Add(_rawInstructions[i]);
			}
		}

		private void BuildExceptionHandlers()
		{
			foreach (var handlerBlock in _exceptionHandlerBlock)
			{
				BuildExceptionHandler(handlerBlock.Try, handlerBlock);
			}
		}

		private void BuildExceptionHandler(ILTryBlock tryBlock, ILExceptionHandlerBlock handlerBlock)
		{
			var rawEH = new ExceptionHandler();

			int offset;
			int size;

			GetBlockOffsetSize(tryBlock, out offset, out size);
			rawEH.TryOffset = offset;
			rawEH.TryLength = size;

			GetBlockOffsetSize(handlerBlock, out offset, out size);
			rawEH.HandlerOffset = offset;
			rawEH.HandlerLength = size;

			rawEH.Type = handlerBlock.HandlerType;

			switch (handlerBlock.HandlerType)
			{
				case ExceptionHandlerType.Catch:
					{
						rawEH.CatchType = handlerBlock.CatchType;
					}
					break;

				case ExceptionHandlerType.Filter:
					{
						GetBlockOffsetSize(handlerBlock.Filter, out offset, out size);
						rawEH.FilterOffset = offset;

						if (offset + size != rawEH.HandlerOffset)
						{
							throw new ILException("Invalid filter handler. Filter block should be followed by handler block.");
						}
					}
					break;
			}

			_rawBody.ExceptionHandlers.Add(rawEH);
		}

		private void BuildLocalVariables()
		{
			var localVariables = _body.LocalVariables;
			var rawLocalVariables = _rawBody.LocalVariables;
			foreach (var localVariable in localVariables)
			{
				rawLocalVariables.Add(localVariable);
			}
		}

		private void GetBlockOffsetSize(ILBlock block, out int offset, out int size)
		{
			var startInstruction = block.FindFirstChild<ILInstruction>(ILNodeType.Instruction, true);
			var endInstruction = block.FindLastChild<ILInstruction>(ILNodeType.Instruction, true);
			int startIndex = GetIndexByInstruction(startInstruction);
			int endIndex = GetIndexByInstruction(endInstruction);
			offset = _offsets[startIndex];
			size = _offsets[endIndex] + _sizes[endIndex] - offset;
		}

		private int GetIndexByInstruction(ILInstruction instruction)
		{
			int index;
			if (!_indexByInstruction.TryGetValue(instruction, out index))
			{
				throw new InvalidOperationException();
			}

			return index;
		}

		#endregion
	}
}
