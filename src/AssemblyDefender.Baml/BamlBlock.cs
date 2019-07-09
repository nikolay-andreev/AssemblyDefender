using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public abstract class BamlBlock : BamlNode
	{
		#region Fields

		internal bool _isClosed;
		internal BamlNode _firstChild;

		#endregion

		#region Ctors

		protected BamlBlock()
		{
		}

		#endregion

		#region Properties

		public BamlNode this[int index]
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

		public override bool IsBlock
		{
			get { return true; }
		}

		public bool IsClosed
		{
			get { return _isClosed; }
			set { _isClosed = value; }
		}

		public BamlNode FirstChild
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

		public BamlNode LastChild
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

		#endregion

		#region Methods

		public void Add(BamlNode node)
		{
			BamlNode notUsed = null;
			Add(node, ref notUsed);
		}

		public void Add(BamlNode node, ref BamlNode lastChildNode)
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

		public void AddRange(IEnumerable<BamlNode> nodes)
		{
			BamlNode lastChildNode = null;
			AddRange(nodes, ref lastChildNode);
		}

		public void AddRange(IEnumerable<BamlNode> nodes, ref BamlNode lastChildNode)
		{
			foreach (var node in nodes)
			{
				Add(node, ref lastChildNode);
			}
		}

		public void Insert(int index, BamlNode node)
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

		public T FindFirstChild<T>(BamlNodeType type, bool throwIfMissing = false)
			where T : BamlNode
		{
			return (T)FindFirstChild(n => n.NodeType == type, throwIfMissing);
		}

		public BamlNode FindFirstChild(Predicate<BamlNode> filter, bool throwIfMissing = false)
		{
			var node = _firstChild;
			while (node != null)
			{
				if (filter(node))
					return node;

				if (node.IsBlock)
				{
					var childNode = ((BamlBlock)node).FindFirstChild(filter);
					if (childNode != null)
						return childNode;
				}

				node = node.NextSibling;
			}

			if (throwIfMissing)
			{
				throw new BamlException(SR.BamlLoadError);
			}

			return null;
		}

		public T FindLastChild<T>(BamlNodeType type, bool throwIfMissing = false)
			where T : BamlNode
		{
			return (T)FindLastChild(n => n.NodeType == type, throwIfMissing);
		}

		public BamlNode FindLastChild(Predicate<BamlNode> filter, bool throwIfMissing = false)
		{
			var node = LastChild;
			while (node != null)
			{
				if (filter(node))
					return node;

				if (node.IsBlock)
				{
					var childNode = ((BamlBlock)node).FindLastChild(filter);
					if (childNode != null)
						return childNode;
				}

				node = node.PreviousSibling;
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
