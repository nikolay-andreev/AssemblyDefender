using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	public class ReadOnlyList<T> : IReadOnlyList<T>
	{
		private int _count;
		private T[] _items;
		private static ReadOnlyList<T> _empty;

		public ReadOnlyList()
		{
		}

		public ReadOnlyList(T[] items)
		{
			if (items != null)
			{
				_items = items;
				_count = items.Length;
			}
		}

		public T this[int index]
		{
			get { return _items[index]; }
		}

		public int Count
		{
			get { return _count; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new ArrayEnumerator<T>(_items);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public static ReadOnlyList<T> Empty
		{
			get
			{
				if (_empty == null)
				{
					_empty = new ReadOnlyList<T>();
				}

				return _empty;
			}
		}

		public static ReadOnlyList<T> Create(T[] items)
		{
			if (items == null || items.Length == 0)
				return Empty;

			return new ReadOnlyList<T>(items);
		}

		public static ReadOnlyList<T> Create(IList<T> list)
		{
			if (list == null || list.Count == 0)
				return Empty;

			return new ReadOnlyList<T>(list.ToArray());
		}
	}
}
