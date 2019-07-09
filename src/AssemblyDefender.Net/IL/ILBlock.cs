using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	public class ILBlock : ILNode
	{
		#region Fields

		internal ILNode _firstChild;

		#endregion

		#region Ctors

		public ILBlock()
		{
		}

		#endregion

		#region Properties

		public ILNode this[int index]
		{
			get
			{
				int count = 0;
				var node = _firstChild;
				while (count < index)
				{
					count++;
					node = node.NextSibling;
				}

				if (node == null)
				{
					throw new IndexOutOfRangeException("index");
				}

				return node;
			}
		}

		public int Count
		{
			get
			{
				int count = 0;
				var node = _firstChild;
				while (node != null)
				{
					count++;
					node = node.NextSibling;
				}

				return count;
			}
		}

		public bool IsEmpty
		{
			get { return _firstChild == null; }
		}

		public ILNode FirstChild
		{
			get { return _firstChild; }
			set
			{
				_firstChild = value;
				if (_firstChild != null)
				{
					// Set parent.
					var node = _firstChild;
					while (node != null)
					{
						node._parent = this;
						node = node.NextSibling;
					}
				}
			}
		}

		public ILNode LastChild
		{
			get
			{
				var node = _firstChild;
				if (node != null)
				{
					while (node.NextSibling != null)
					{
						node = node.NextSibling;
					}
				}

				return node;
			}
		}

		public virtual ILBlockType BlockType
		{
			get { return ILBlockType.Block; }
		}

		public override ILNodeType NodeType
		{
			get { return ILNodeType.Block; }
		}

		#endregion

		#region Methods

		public void Add(ILNode node)
		{
			ILNode notUsed = null;
			Add(node, ref notUsed);
		}

		public void Add(ILNode node, ref ILNode lastChildNode)
		{
			if (lastChildNode != null)
			{
				lastChildNode.AddNext(node);
				lastChildNode = node;
			}
			else
			{
				if (_firstChild != null)
				{
					lastChildNode = _firstChild.Last;
					lastChildNode.AddNext(node);
					lastChildNode = node;
				}
				else
				{
					_firstChild = node;
					_firstChild._parent = this;
					_firstChild._previousSibling = null;
					_firstChild._nextSibling = null;
					lastChildNode = _firstChild;
				}
			}
		}

		public void AddRange(IEnumerable<ILNode> nodes)
		{
			ILNode lastChildNode = null;
			AddRange(nodes, ref lastChildNode);
		}

		public void AddRange(IEnumerable<ILNode> nodes, ref ILNode lastChildNode)
		{
			foreach (var node in nodes)
			{
				Add(node, ref lastChildNode);
			}
		}

		public void Insert(int index, ILNode node)
		{
			if (index == 0 && _firstChild == null)
			{
				_firstChild = node;
				_firstChild._parent = this;
				_firstChild._previousSibling = null;
				_firstChild._nextSibling = null;
			}
			else
			{
				this[index].AddPrevious(node);
			}
		}

		public void RemoveAt(int index)
		{
			this[index].Remove();
		}

		public void Clear()
		{
			var node = _firstChild;
			while (node != null)
			{
				var nextNode = node.NextSibling;
				node._previousSibling = null;
				node._nextSibling = null;
				node._parent = null;
				node = nextNode;
			}

			_firstChild = null;
		}

		public T FindFirstChild<T>(ILNodeType type, bool throwIfMissing = false)
			where T : ILNode
		{
			return (T)FindFirstChild(n => n.NodeType == type, throwIfMissing);
		}

		public ILNode FindFirstChild(Predicate<ILNode> filter, bool throwIfMissing = false)
		{
			var node = _firstChild;
			while (node != null)
			{
				if (filter(node))
					return node;

				if (node.NodeType == ILNodeType.Block)
				{
					var childNode = ((ILBlock)node).FindFirstChild(filter);
					if (childNode != null)
						return childNode;
				}

				node = node.NextSibling;
			}

			if (throwIfMissing)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			return null;
		}

		public T FindLastChild<T>(ILNodeType type, bool throwIfMissing = false)
			where T : ILNode
		{
			return (T)FindLastChild(n => n.NodeType == type, throwIfMissing);
		}

		public ILNode FindLastChild(Predicate<ILNode> filter, bool throwIfMissing = false)
		{
			var node = LastChild;
			while (node != null)
			{
				if (filter(node))
					return node;

				if (node.NodeType == ILNodeType.Block)
				{
					var childNode = ((ILBlock)node).FindLastChild(filter);
					if (childNode != null)
						return childNode;
				}

				node = node.PreviousSibling;
			}

			if (throwIfMissing)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			return null;
		}

		public void RemoveNopInstructions()
		{
			var node = _firstChild;
			while (node != null)
			{
				var nextSibling = node.NextSibling;

				switch (node.NodeType)
				{
					case ILNodeType.Block:
						{
							((ILBlock)node).RemoveNopInstructions();
						}
						break;

					case ILNodeType.Instruction:
						{
							var instruction = (ILInstruction)node;
							if (instruction.OpCode == OpCodes.Nop)
							{
								instruction.Remove();
							}
						}
						break;
				}

				node = nextSibling;
			}
		}

		public override string ToString()
		{
			return "{Block}";
		}

		#endregion

		#region Static

		public static T Group<T>(ILNode startNode, ILNode endNode)
			where T : ILBlock, new()
		{
			// Start and end nodes are in the same block.
			if (startNode.Parent == endNode.Parent)
			{
				if (startNode.PreviousSibling == null && endNode.NextSibling == null)
				{
					// Block already exists.
					var block = startNode.Parent as T;
					if (block == null)
					{
						throw new ILException(SR.MethodILNotValid);
					}

					return block;
				}
				else
				{
					// Add new block.
					return CreateGroup<T>(startNode, endNode);
				}
			}
			else // Start and end nodes are in different blocks.
			{
				int startLevel = startNode.Level;
				int endLevel = endNode.Level;

				var startNode2 = startNode;
				var endNode2 = endNode;
				if (!FindNodesWithSameParent(ref startNode2, ref endNode2))
				{
					throw new ILException(SR.MethodILNotValid);
				}

				if (startNode != startNode2 && startNode.PreviousSibling != null)
				{
					throw new ILException(SR.MethodILNotValid);
				}

				if (endNode != endNode2 && endNode.NextSibling != null)
				{
					throw new ILException(SR.MethodILNotValid);
				}

				startNode = startNode2;
				endNode = endNode2;

				if (startNode.PreviousSibling == null && endNode.NextSibling == null)
				{
					// Block already exists.
					var block = startNode.Parent as T;
					if (block == null)
					{
						throw new ILException(SR.MethodILNotValid);
					}

					return block;
				}
				else
				{
					// Add new block.
					return CreateGroup<T>(startNode, endNode);
				}
			}
		}

		private static T CreateGroup<T>(ILNode startNode, ILNode endNode)
			where T : ILBlock, new()
		{
			if (startNode.Parent != endNode.Parent)
			{
				throw new ArgumentException();
			}

			var parentBlock = startNode.Parent;
			var previousNode = startNode.PreviousSibling;

			// Collect nodes in list.
			var nodes = new List<ILNode>();
			for (var node = startNode; node != endNode; node = node.NextSibling)
			{
				if (node == null)
				{
					throw new ILException(SR.MethodILNotValid);
				}

				nodes.Add(node);
			}

			nodes.Add(endNode);

			// Remove nodes from old block.
			foreach (var node in nodes)
			{
				node.Remove();
			}

			// Add new block to old block.
			var newBlock = new T();
			if (previousNode != null)
			{
				previousNode.AddNext(newBlock);
			}
			else
			{
				parentBlock.Insert(0, newBlock);
			}

			// Add nodes to new block.
			ILNode firstChildNode = null;
			foreach (var node in nodes)
			{
				newBlock.Add(node, ref firstChildNode);
			}

			return newBlock;
		}

		private static bool FindNodesWithSameParent(ref ILNode node1, ref ILNode node2)
		{
			int level1 = node1.Level;
			int level2 = node2.Level;
			if (level1 > level2)
			{
				while (level1 != level2)
				{
					node1 = node1.Parent;
					level1--;
				}
			}
			else if (level1 < level2)
			{
				while (level1 != level2)
				{
					node2 = node2.Parent;
					level2--;
				}
			}

			while (node1.Parent != node2.Parent)
			{
				node1 = node1.Parent;
				node2 = node2.Parent;

				if (node1 == null || node2 == null)
					return false;
			}

			return true;
		}

		#endregion
	}
}
