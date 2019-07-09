using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Net.IL;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	public class ControlFlowObfuscator
	{
		#region Fields

		private bool _doNotUseFieldNumber;
		private int _branchCount;
		private ILBody _body;
		private BuildMethod _method;
		private MainType _mainType;
		private Random _random;
		private List<Block> _blocks;

		#endregion

		#region Ctors

		public ControlFlowObfuscator(ILBody body, BuildMethod method)
		{
			if (body == null)
				throw new ArgumentNullException("body");

			if (method == null)
				throw new ArgumentNullException("method");

			_body = body;
			_method = method;

			var module = (BuildModule)method.Module;
			_mainType = module.MainType;
			_random = module.RandomGenerator;
		}

		#endregion

		#region Properties

		public bool DoNotUseFieldNumber
		{
			get { return _doNotUseFieldNumber; }
			set { _doNotUseFieldNumber = value; }
		}

		public int BranchCount
		{
			get { return _branchCount; }
			set { _branchCount = value; }
		}

		#endregion

		#region Methods

		public void Obfuscate()
		{
			_body.RemoveNopInstructions();

			CreateBlocks();

			if (_blocks == null)
				return;

			foreach (var block in _blocks)
			{
				int[] shuffleMap = GetShuffleMap(block.Sections.Length);
				CreateProxy(block, shuffleMap);
				ShuffleSections(block, shuffleMap);
				AddNodes(block);
				LinkSections(block);
				RemoveNotUsedProxies(block);
			}

			_body.RemoveNopInstructions();

			// Calculate max stack size
			_body.CalculateMaxStackSize(_method);

			if (!_doNotUseFieldNumber)
			{
				_mainType.GenerateControlFlowValues();
			}
		}

		private void CreateBlocks()
		{
			var state = Load();

			int blockCount = state.Blocks.Count;
			if (blockCount == 0)
				return;

			int totalBranchCount = GetBranchCount(state.TotalInstructionCount);

			_blocks = new List<Block>(totalBranchCount);

			// Sort by instruction count
			if (blockCount > 1)
			{
				state.Blocks.Sort(new BlockSortComparer());
			}

			int index = 0;
			while (totalBranchCount > 0 && index < blockCount)
			{
				var blockState = state.Blocks[index++];

				int nextInstructionCount;
				if (index < blockCount)
					nextInstructionCount = state.Blocks[index].InstructionCount;
				else
					nextInstructionCount = 0;

				_blocks.Add(CreateBlock(blockState, nextInstructionCount, ref totalBranchCount));
			}
		}

		private Block CreateBlock(BlockLoadState blockState, int nextInstructionCount, ref int totalBranchCount)
		{
			// Calculate branch count for block
			int branchCount = 0;
			int zeroStackSizeNodeCount = blockState.ZeroStackSizeNodes.Count;
			do
			{
				branchCount++;
				zeroStackSizeNodeCount--;
				totalBranchCount--;
				blockState.InstructionCount /= 2;
			}
			while (totalBranchCount > 0 && zeroStackSizeNodeCount > 0 && blockState.InstructionCount > nextInstructionCount);

			var block = new Block();
			block.Node = blockState.Node;

			// Sections
			int sectionCount = branchCount + 1;
			var sections = new BlockSection[sectionCount];

			// Create
			for (int i = 0; i < sectionCount; i++)
			{
				sections[i] = new BlockSection();
			}

			// Init
			var firstSection = sections[0];
			firstSection.ProxyNode = blockState.Node.FirstChild.AddPrevious(OpCodes.Nop);
			firstSection.CodeNode = firstSection.ProxyNode.AddNext(OpCodes.Nop);

			var lastSection = sections[sectionCount - 1];
			lastSection.LastNode = blockState.Node.LastChild.AddNext(OpCodes.Nop);

			int sectionOffsetSize = blockState.ZeroStackSizeNodes.Count / sectionCount;

			for (int i = 0; i < branchCount; i++)
			{
				var section1 = sections[i];
				var section2 = sections[i + 1];

				int branchIndex = (sectionOffsetSize * (i + 1));
				var branchInstruction = blockState.ZeroStackSizeNodes[branchIndex];

				section1.Next = section2;
				section1.LastNode = branchInstruction.AddPrevious(OpCodes.Nop);
				section2.ProxyNode = section1.LastNode.AddNext(OpCodes.Nop);
				section2.CodeNode = section2.ProxyNode.AddNext(OpCodes.Nop);
			}

			block.Sections = sections;
			block.FirstSection = sections[0];

			return block;
		}

		private LoadState Load()
		{
			var state = new LoadState();
			state.Blocks = new List<BlockLoadState>();

			var stackSizeCalculator = new StackSizeCalculator(_body, _method);
			stackSizeCalculator.Calculate();
			state.StackSizeByInstruction = stackSizeCalculator.StackSizeByInstruction;

			Load(state, _body);

			return state;
		}

		private void Load(LoadState state, ILBlock block)
		{
			var zeroStackSizeNodes = new List<ILNode>();
			int instructionCount = 0;
			bool hasChildren = false;
			var node = block.FirstChild;
			while (node != null)
			{
				switch (node.NodeType)
				{
					case ILNodeType.Instruction:
						{
							var instruction = (ILInstruction)node;

							int stackSize;
							if (state.StackSizeByInstruction.TryGetValue(instruction, out stackSize) && stackSize == 0)
							{
								if (CanBranchAtInstruction(instruction))
								{
									zeroStackSizeNodes.Add(instruction);
								}
							}

							instructionCount++;
						}
						break;

					case ILNodeType.Block:
						{
							var tryBlock = node as ILTryBlock;
							if (tryBlock != null)
							{
								if (node.PreviousSibling != null)
								{
									zeroStackSizeNodes.Add(tryBlock);
								}

								hasChildren = true;
								instructionCount++;
							}
						}
						break;
				}

				node = node.NextSibling;
			}

			state.TotalInstructionCount += instructionCount;

			if (zeroStackSizeNodes.Count > 0)
			{
				var blockState = new BlockLoadState();
				blockState.Index = state.Blocks.Count;
				blockState.InstructionCount = instructionCount;
				blockState.Node = block;
				blockState.ZeroStackSizeNodes = zeroStackSizeNodes;
				state.Blocks.Add(blockState);
			}

			// Load child blocks
			if (hasChildren)
			{
				node = block.FirstChild;
				while (node != null)
				{
					var tryBlock = node as ILTryBlock;
					if (tryBlock != null)
					{
						Load(state, tryBlock);
					}

					node = node.NextSibling;
				}
			}
		}

		private bool CanBranchAtInstruction(ILInstruction instruction)
		{
			{
				var opCode = instruction.OpCode;
				if (opCode.Type == OpCodeType.Branch ||
					opCode.Type == OpCodeType.CondBranch ||
					opCode.Type == OpCodeType.Reserved)
					return false;
			}

			// Check previous
			var previousNode = instruction.FindPreviousSibling(n => n.NodeType != ILNodeType.Label);
			if (previousNode == null)
				return false;

			var previousInstruction = previousNode as ILInstruction;
			if (previousInstruction != null)
			{
				var opCode = previousInstruction.OpCode;
				if (opCode.Type == OpCodeType.Branch ||
					opCode.Type == OpCodeType.CondBranch ||
					opCode.Type == OpCodeType.Prefix ||
					opCode.Type == OpCodeType.Reserved)
					return false;
			}

			// Check next
			var nextNode = instruction.FindNextSibling(n => n.NodeType != ILNodeType.Label);
			if (nextNode == null)
				return false;

			var nextInstruction = nextNode as ILInstruction;
			if (nextInstruction != null)
			{
				var opCode = nextInstruction.OpCode;
				if (opCode.Type == OpCodeType.Branch ||
					opCode.Type == OpCodeType.CondBranch ||
					opCode.Type == OpCodeType.Reserved)
					return false;
			}

			return true;
		}

		private int GetBranchCount(int instructionCount)
		{
			if (_branchCount > 0)
				return _branchCount;
			else if (instructionCount < 15)
				return 1;
			else if (instructionCount < 30)
				return 2;
			else
				return 3;
		}

		private void CreateProxy(Block block, int[] map)
		{
			var sections = block.Sections;

			for (int i = 0; i < sections.Length; i++)
			{
				// Current section
				var section = sections[i];
				var proxy = new SectionProxy();
				section.Proxy1 = proxy;

				// Target section
				var section2 = sections[map[i]];
				var proxy2 = new SectionProxy();
				section2.Proxy2 = proxy2;
				var section2Label = new ILLabel();
				section2.CodeNode.AddNext(section2Label);

				int value = _random.Next(3, 7);
				var node = section.ProxyNode.AddNext(Instruction.GetLdc(value));

				proxy.Node = node;
				proxy2.Node = node;

				if (_random.NextBool())
				{
					node = node.AddNext(OpCodes.Bgt, section2Label);
					proxy.MinValue = 1;
					proxy.MaxValue = value;
					proxy2.MinValue = value + 1;
					proxy2.MaxValue = 8;
				}
				else
				{
					node = node.AddNext(OpCodes.Blt, section2Label);
					proxy.MinValue = value + 1;
					proxy.MaxValue = 8;
					proxy2.MinValue = 1;
					proxy2.MaxValue = value;
				}
			}
		}

		private void ShuffleSections(Block block, int[] map)
		{
			var sections = block.Sections;
			int count = sections.Length;

			var newSections = new BlockSection[count];
			for (int i = 0; i < count; i++)
			{
				newSections[map[i]] = sections[i];
			}

			block.Sections = newSections;
		}

		private void AddNodes(Block block)
		{
			var nodes = new List<ILNode>();

			foreach (var section in block.Sections)
			{
				var node = section.ProxyNode;
				while (node != null)
				{
					nodes.Add(node);

					if (object.ReferenceEquals(node, section.LastNode))
						break;

					node = node.NextSibling;
				}
			}

			// Copy nodes to body.
			var blockNode = block.Node;
			blockNode.FirstChild = null;

			ILNode lastChildNode = null;
			for (int i = 0; i < nodes.Count; i++)
			{
				blockNode.Add(nodes[i], ref lastChildNode);
			}
		}

		private void LinkSections(Block block)
		{
			var section = block.FirstSection;
			var node = block.Node.FirstChild;

			do
			{
				LinkSection(node, section);
				node = section.LastNode;
				section = section.Next;
			}
			while (section != null);
		}

		private void LinkSection(ILNode node, BlockSection section)
		{
			bool useProxy1 = _random.NextBool();
			var nextSibling = GetNextSiblingIgnoreNop(node);
			var proxy = useProxy1 ? section.Proxy1 : section.Proxy2;
			if (object.ReferenceEquals(nextSibling, proxy.Node))
			{
				proxy = useProxy1 ? section.Proxy2 : section.Proxy1;
			}

			// ldc.i4.X / ldslfd X
			int value = _random.Next(proxy.MinValue, proxy.MaxValue);
			if (_doNotUseFieldNumber)
				node = node.AddNext(Instruction.GetLdc(value));
			else
				node = node.AddNext(OpCodes.Ldsfld, GetIntFieldRef(value));

			// br [proxy]
			var targetLabel = new ILLabel();
			proxy.Node.AddPrevious(targetLabel);
			node = node.AddNext(OpCodes.Br, targetLabel);
		}

		private void RemoveNotUsedProxies(Block block)
		{
			foreach (var section in block.Sections)
			{
				var proxy = section.Proxy1;
				var prevNode = GetPreviousSiblingIgnoreNop(proxy.Node);
				if (prevNode == null || prevNode.NodeType != ILNodeType.Label)
				{
					var node = proxy.Node;
					node.NextSibling.Remove(); // bgt / blt
					node.Remove(); // ldfld
				}
			}
		}

		private ILNode GetPreviousSiblingIgnoreNop(ILNode node)
		{
			do
			{
				node = node.PreviousSibling;
			}
			while (node != null && node.NodeType == ILNodeType.Instruction && ((ILInstruction)node).OpCode == OpCodes.Nop);

			return node;
		}

		private ILNode GetNextSiblingIgnoreNop(ILNode node)
		{
			do
			{
				node = node.NextSibling;
			}
			while (node != null && node.NodeType == ILNodeType.Instruction && ((ILInstruction)node).OpCode == OpCodes.Nop);

			return node;
		}

		private int[] GetShuffleMap(int sectionCount)
		{
			switch (sectionCount)
			{
				case 2:
					return new int[] { 1, 0, };

				case 3:
					{
						switch (_random.Next(0, 2))
						{
							case 0:
								return new int[] { 2, 0, 1, };

							case 1:
								return new int[] { 1, 2, 0, };

							default:
								throw new InvalidOperationException();
						}
					}

				case 4:
					{
						switch (_random.Next(0, 6))
						{
							case 0:
								return new int[] { 1, 0, 3, 2, };

							case 1:
								return new int[] { 1, 3, 0, 2, };

							case 2:
								return new int[] { 2, 0, 3, 1, };

							case 3:
								return new int[] { 2, 3, 1, 0, };

							case 4:
								return new int[] { 3, 2, 1, 0, };

							case 5:
								return new int[] { 3, 2, 0, 1 };

							default:
								throw new InvalidOperationException();
						}
					}

				case 5:
					{
						switch (_random.Next(0, 6))
						{
							case 0:
								return new int[] { 2, 4, 3, 1, 0, };

							case 1:
								return new int[] { 3, 0, 4, 2, 1, };

							case 2:
								return new int[] { 1, 3, 0, 4, 2, };

							case 3:
								return new int[] { 4, 2, 3, 1, 0, };

							case 4:
								return new int[] { 4, 0, 3, 1, 2, };

							case 5:
								return new int[] { 4, 2, 1, 0, 3, };

							default:
								throw new InvalidOperationException();
						}
					}

				default:
					throw new InvalidOperationException();
			}
		}

		private FieldReference GetIntFieldRef(int value)
		{
			return new FieldReference("ControlFlow" + value.ToString(),
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _method.Assembly),
				new TypeReference(_mainType.Name, _mainType.Namespace, false));
		}

		#endregion

		#region Static

		public static void Obfuscate(BuildAssembly assembly, bool ignoreEncryptIL = false)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Obfuscate(module, ignoreEncryptIL);
			}
		}

		public static void Obfuscate(BuildModule module, bool ignoreEncryptIL = false)
		{
			foreach (BuildType type in module.Types)
			{
				Obfuscate(type, ignoreEncryptIL);
			}
		}

		private static void Obfuscate(BuildType type, bool ignoreEncryptIL)
		{
			if (type.IsMainType)
				return;

			foreach (BuildMethod method in type.Methods)
			{
				Obfuscate(method, ignoreEncryptIL);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Obfuscate(nestedType, ignoreEncryptIL);
			}
		}

		public static void Obfuscate(BuildMethod method, bool ignoreEncryptIL)
		{
			if (!method.ObfuscateControlFlow)
				return;

			if (ignoreEncryptIL && method.EncryptIL)
				return;

			if (!MethodBody.IsValid(method))
				return;

			var methodBody = MethodBody.Load(method);

			var ilBody = ILBody.Load(methodBody);

			// Obfuscate
			var obfuscator = new ControlFlowObfuscator(ilBody, method);
			obfuscator.Obfuscate();

			// Save
			methodBody = ilBody.Build();

			methodBody.Build(method);
		}

		#endregion

		#region Nested types

		private class Block
		{
			internal ILBlock Node;
			internal BlockSection FirstSection;
			internal BlockSection[] Sections;
		}

		private class BlockSection
		{
			internal ILNode ProxyNode;
			internal ILNode CodeNode;
			internal ILNode LastNode;
			internal SectionProxy Proxy1;
			internal SectionProxy Proxy2;
			internal BlockSection Next;
		}

		private class SectionProxy
		{
			internal int MinValue;
			internal int MaxValue;
			internal ILNode Node;
		}

		private class LoadState
		{
			internal int TotalInstructionCount;
			internal List<BlockLoadState> Blocks;
			internal Dictionary<ILInstruction, int> StackSizeByInstruction;
		}

		private class BlockLoadState
		{
			internal int Index;
			internal int InstructionCount;
			internal ILBlock Node;
			internal List<ILNode> ZeroStackSizeNodes;
		}

		private class BlockSortComparer : IComparer<BlockLoadState>
		{
			int IComparer<BlockLoadState>.Compare(BlockLoadState x, BlockLoadState y)
			{
				return y.InstructionCount.CompareTo(x.InstructionCount);
			}
		}

		#endregion
	}
}
