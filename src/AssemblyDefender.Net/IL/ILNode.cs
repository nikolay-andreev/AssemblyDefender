using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	public abstract class ILNode
	{
		#region Fields

		internal ILBlock _parent;
		internal ILNode _nextSibling;
		internal ILNode _previousSibling;

		#endregion

		#region Properties

		public ILBlock Parent
		{
			get { return _parent; }
		}

		public ILNode NextSibling
		{
			get { return _nextSibling; }
		}

		public ILNode PreviousSibling
		{
			get { return _previousSibling; }
		}

		public ILNode First
		{
			get
			{
				var node = this;
				while (node._previousSibling != null)
				{
					node = node._previousSibling;
				}

				return node;
			}
		}

		public ILNode Last
		{
			get
			{
				var node = this;
				while (node._nextSibling != null)
				{
					node = node._nextSibling;
				}

				return node;
			}
		}

		public ILBlock Root
		{
			get
			{
				var node = this;
				while (node._parent != null)
				{
					node = node._parent;
				}

				return node as ILBlock;
			}
		}

		public ILBody Body
		{
			get
			{
				var node = this;
				while (node._parent != null)
				{
					node = node._parent;
				}

				return node as ILBody;
			}
		}

		public int Level
		{
			get
			{
				int level = 0;
				var node = _parent;
				while (node != null)
				{
					level++;
					node = node._parent;
				}

				return level;
			}
		}

		public abstract ILNodeType NodeType
		{
			get;
		}

		#endregion

		#region Methods

		public ILNode AddNext(ILNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			var currNext = _nextSibling;

			_nextSibling = node;
			_nextSibling._previousSibling = this;
			_nextSibling._parent = _parent;

			if (currNext != null)
			{
				_nextSibling._nextSibling = currNext;
				currNext._previousSibling = _nextSibling;
			}
			else
			{
				_nextSibling._nextSibling = null;
			}

			return node;
		}

		public ILInstruction AddNext(OpCode opCode)
		{
			var instruction = new ILInstruction(opCode);
			AddNext(instruction);

			return instruction;
		}

		public ILInstruction AddNext(OpCode opCode, object value)
		{
			var instruction = new ILInstruction(opCode, value);
			AddNext(instruction);

			return instruction;
		}

		public ILInstruction AddNext(OpCode opCode, ILLabel label)
		{
			var instruction = new ILInstruction(opCode, label);
			AddNext(instruction);

			return instruction;
		}

		public ILInstruction AddNext(Net.Instruction instr)
		{
			var instruction = new ILInstruction(instr);
			AddNext(instruction);

			return instruction;
		}

		public ILNode AddPrevious(ILNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			var currPrev = _previousSibling;

			_previousSibling = node;
			_previousSibling._nextSibling = this;
			_previousSibling._parent = _parent;

			if (currPrev != null)
			{
				_previousSibling._previousSibling = currPrev;
				currPrev._nextSibling = _previousSibling;
			}
			else
			{
				_previousSibling._previousSibling = null;

				if (_parent != null)
				{
					_parent._firstChild = _previousSibling;
				}
			}

			return node;
		}

		public ILInstruction AddPrevious(OpCode opCode)
		{
			var instruction = new ILInstruction(opCode);
			AddPrevious(instruction);

			return instruction;
		}

		public ILInstruction AddPrevious(OpCode opCode, object value)
		{
			var instruction = new ILInstruction(opCode, value);
			AddPrevious(instruction);

			return instruction;
		}

		public ILInstruction AddPrevious(OpCode opCode, ILLabel label)
		{
			var instruction = new ILInstruction(opCode, label);
			AddPrevious(instruction);

			return instruction;
		}

		public ILInstruction AddPrevious(Net.Instruction instr)
		{
			var instruction = new ILInstruction(instr);
			AddPrevious(instruction);

			return instruction;
		}

		public void Remove()
		{
			if (_previousSibling != null)
			{
				if (_nextSibling != null)
				{
					// Middle node
					_previousSibling._nextSibling = _nextSibling;
					_nextSibling._previousSibling = _previousSibling;
				}
				else
				{
					// Last node
					_previousSibling._nextSibling = null;
				}
			}
			else
			{
				if (_parent != null)
				{
					_parent._firstChild = _nextSibling;
				}

				if (_nextSibling != null)
				{
					// First node
					_nextSibling._previousSibling = null;
					_nextSibling._parent = _parent;
				}
			}

			_nextSibling = null;
			_previousSibling = null;
			_parent = null;
		}

		public ILNode GetNext()
		{
			if (NodeType == ILNodeType.Block)
			{
				var block = (ILBlock)this;
				if (block.FirstChild != null)
					return block.FirstChild;
			}

			var node = this;
			while (node.NextSibling == null && node.Parent != null)
			{
				node = node.Parent;
			}

			return node.NextSibling;
		}

		public ILNode GetPrevious()
		{
			if (_previousSibling != null)
			{
				var node = _previousSibling;
				while (node.NodeType == ILNodeType.Block)
				{
					var block = (ILBlock)node;
					node = block.LastChild;
					if (node == null)
						return block;
				}

				return node;
			}
			else
			{
				return _parent;
			}
		}

		public T FindNext<T>(ILNodeType type, bool throwIfMissing = false)
			where T : ILNode
		{
			return (T)FindNext(n => n.NodeType == type, throwIfMissing);
		}

		public ILNode FindNext(Predicate<ILNode> filter, bool throwIfMissing = false)
		{
			var node = GetNext();
			while (node != null)
			{
				if (filter(node))
					return node;

				node = node.GetNext();
			}

			if (throwIfMissing)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			return null;
		}

		public T FindNextSibling<T>(ILNodeType type, bool throwIfMissing = false)
			where T : ILNode
		{
			return (T)FindNextSibling(n => n.NodeType == type, throwIfMissing);
		}

		public ILNode FindNextSibling(Predicate<ILNode> filter, bool throwIfMissing = false)
		{
			var node = _nextSibling;
			while (node != null)
			{
				if (filter(node))
					return node;

				node = node._nextSibling;
			}

			if (throwIfMissing)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			return null;
		}

		public T FindPrevious<T>(ILNodeType type, bool throwIfMissing = false)
			where T : ILNode
		{
			return (T)FindPrevious(n => n.NodeType == type, throwIfMissing);
		}

		public ILNode FindPrevious(Predicate<ILNode> filter, bool throwIfMissing = false)
		{
			var node = GetPrevious();
			while (node != null)
			{
				if (filter(node))
					return node;

				node = node.GetPrevious();
			}

			if (throwIfMissing)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			return null;
		}

		public T FindPreviousSibling<T>(ILNodeType type, bool throwIfMissing = false)
			where T : ILNode
		{
			return (T)FindPreviousSibling(n => n.NodeType == type, throwIfMissing);
		}

		public ILNode FindPreviousSibling(Predicate<ILNode> filter, bool throwIfMissing = false)
		{
			var node = _previousSibling;
			while (node != null)
			{
				if (filter(node))
					return node;

				node = node._previousSibling;
			}

			if (throwIfMissing)
			{
				throw new ILException(SR.MethodILNotValid);
			}

			return null;
		}

		#endregion
	}
}
