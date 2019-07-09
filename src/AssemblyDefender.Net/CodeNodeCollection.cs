using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public abstract class CodeNodeCollection<T> : CodeNode, IReadOnlyList<T>, IEnumerable<T>
	{
		#region Fields

		protected List<T> _list = new List<T>();

		#endregion

		#region Ctors

		protected CodeNodeCollection(CodeNode parent)
			: base(parent)
		{
		}

		#endregion

		#region Properties

		public T this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int IndexOf(T item)
		{
			return _list.IndexOf(item);
		}

		public T Add()
		{
			return Insert(_list.Count);
		}

		private T Insert(int index)
		{
			var item = CreateItem();
			_list.Insert(index, item);
			OnChanged();

			return item;
		}

		public bool Remove(T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			int index = IndexOf(item);
			if (index < 0)
				return false;

			RemoveAt(index);
			return true;
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
			OnChanged();
		}

		public void Clear()
		{
			_list.Clear();
			OnChanged();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		protected abstract T CreateItem();

		#endregion
	}
}
