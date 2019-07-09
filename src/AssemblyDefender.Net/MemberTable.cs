using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class MemberTable<T>
		where T : MemberNode
	{
		#region Fields

		private const int BlockSize = 0x100;
		private bool _isCached;
		private int _count;
		private int _metadataCount;
		private int _index;
		private object[] _items;
		private BitArray _isMetadataArray;
		private Func<int, int, T> _loadAction;

		#endregion

		#region Ctors

		internal MemberTable(int metadataCount, bool isCached, Func<int, int, T> loadAction)
		{
			_metadataCount = _count = _index = metadataCount;
			_isCached = isCached;
			_loadAction = loadAction;
		}

		#endregion

		#region Methods

		internal IEnumerable<T> EnumLiveNodes()
		{
			return new TryConvertEnumerator<T, object>(
				new ArrayEnumerator<object>(_items),
				TryGetNode);
		}

		internal T Get(int rid, int parent)
		{
			if (_items == null)
			{
				Initialize(_metadataCount);
			}

			int index = rid - 1;
			if (index < 0 || index >= _items.Length)
				return null;

			T node;

			object o = _items[index];
			if (o != null)
			{
				node = o as T;
				if (node != null)
					return node;

				node = ((WeakReference)o).Target as T;
				if (node != null)
					return node;
			}

			if (index < _metadataCount && _isMetadataArray[index])
			{
				node = _loadAction(rid, parent);

				if (_isCached)
					_items[index] = node;
				else
					_items[index] = new WeakReference(node);

				return node;
			}

			return null;
		}

		internal int Add(T node)
		{
			if (_items == null)
			{
				Initialize(Math.Max(_metadataCount, BlockSize));
			}

			// Resize items.
			if (_index == _items.Length)
			{
				int newLength = _items.Length + BlockSize;
				var items = new object[newLength];
				Array.Copy(_items, 0, items, 0, _items.Length);
				_items = items;
			}

			// Set item.
			_items[_index++] = node;
			_count++;

			int rid = _index;

			// Move to next free slot.
			while (_index < _items.Length)
			{
				if (_items[_index] == null && (_index >= _metadataCount || !_isMetadataArray[_index]))
					break;

				_index++;
			}

			return rid;
		}

		internal void Remove(int rid)
		{
			if (_items == null)
			{
				Initialize(_metadataCount);
			}

			int index = rid - 1;

			_items[index] = null;
			_count--;

			if (index < _metadataCount)
				_isMetadataArray[index] = false;

			// Set index to the lowest free slot.
			if (_index > index)
				_index = index;
		}

		internal void Change(int rid)
		{
			if (_items == null)
			{
				Initialize(_metadataCount);
			}

			int index = rid - 1;

			var weakRef = _items[index] as WeakReference;
			if (weakRef != null)
			{
				_items[index] = (T)weakRef.Target;
			}
		}

		private void Initialize(int capacity)
		{
			_items = new object[capacity];
			_isMetadataArray = new BitArray(_metadataCount, true);
		}

		private bool TryGetNode(object obj, out T node)
		{
			if (obj != null)
			{
				node = obj as T;
				if (node != null)
					return true;

				node = ((WeakReference)obj).Target as T;
				if (node != null)
					return true;
			}

			node = null;
			return false;
		}

		#endregion
	}
}
