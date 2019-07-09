using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public abstract class BamlNode
	{
		#region Fields

		internal BamlBlock _parent;
		internal BamlNode _nextSibling;
		internal BamlNode _previousSibling;

		#endregion

		#region Ctors

		protected BamlNode()
		{
		}

		#endregion

		#region Properties

		public virtual bool IsBlock
		{
			get { return false; }
		}

		public BamlBlock Parent
		{
			get { return _parent; }
		}

		public BamlNode NextSibling
		{
			get { return _nextSibling; }
		}

		public BamlNode PreviousSibling
		{
			get { return _previousSibling; }
		}

		public BamlNode First
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

		public BamlNode Last
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

		public abstract BamlNodeType NodeType
		{
			get;
		}

		#endregion

		#region Methods

		public BamlNode AddNext(BamlNode node)
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

		public BamlNode AddPrevious(BamlNode node)
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

		public BamlNode GetNext()
		{
			if (IsBlock)
			{
				var block = (BamlBlock)this;
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

		public BamlNode GetPrevious()
		{
			if (_previousSibling != null)
			{
				var node = _previousSibling;
				while (node.IsBlock)
				{
					var block = (BamlBlock)node;
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

		public T FindNext<T>(BamlNodeType type, bool throwIfMissing = false)
			where T : BamlNode
		{
			return (T)FindNext(n => n.NodeType == type, throwIfMissing);
		}

		public BamlNode FindNext(Predicate<BamlNode> filter, bool throwIfMissing = false)
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
				throw new BamlException(SR.BamlLoadError);
			}

			return null;
		}

		public T FindNextSibling<T>(BamlNodeType type, bool throwIfMissing = false)
			where T : BamlNode
		{
			return (T)FindNextSibling(n => n.NodeType == type, throwIfMissing);
		}

		public BamlNode FindNextSibling(Predicate<BamlNode> filter, bool throwIfMissing = false)
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
				throw new BamlException(SR.BamlLoadError);
			}

			return null;
		}

		public T FindPrevious<T>(BamlNodeType type, bool throwIfMissing = false)
			where T : BamlNode
		{
			return (T)FindPrevious(n => n.NodeType == type, throwIfMissing);
		}

		public BamlNode FindPrevious(Predicate<BamlNode> filter, bool throwIfMissing = false)
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
				throw new BamlException(SR.BamlLoadError);
			}

			return null;
		}

		public T FindPreviousSibling<T>(BamlNodeType type, bool throwIfMissing = false)
			where T : BamlNode
		{
			return (T)FindPreviousSibling(n => n.NodeType == type, throwIfMissing);
		}

		public BamlNode FindPreviousSibling(Predicate<BamlNode> filter, bool throwIfMissing = false)
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
				throw new BamlException(SR.BamlLoadError);
			}

			return null;
		}

		#endregion
	}
}
