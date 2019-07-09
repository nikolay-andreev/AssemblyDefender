using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public abstract class MemberNodeCollection<T> : CodeNode, IReadOnlyList<T>, IEnumerable<T>
		where T : MemberNode
	{
		#region Fields

		protected List<int> _list = new List<int>();

		#endregion

		#region Ctors

		protected MemberNodeCollection(CodeNode parent)
			: base(parent)
		{
		}

		#endregion

		#region Properties

		public T this[int index]
		{
			get { return GetItem(_list[index]); }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int GetRID(int index)
		{
			return _list[index];
		}

		public int IndexOf(T item)
		{
			return IndexOf(item.RID);
		}

		public int IndexOf(int rid)
		{
			return _list.IndexOf(rid);
		}

		public T Add()
		{
			var item = CreateItem();
			_list.Add(item.RID);
			OnChanged();

			return item;
		}

		internal void Add(T item)
		{
			_list.Add(item.RID);
			OnChanged();
		}

		public T Insert(int index)
		{
			var item = CreateItem();
			_list.Insert(index, item.RID);
			OnChanged();

			return item;
		}

		internal void Insert(int index, T item)
		{
			_list.Insert(index, item.RID);
			OnChanged();
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
			var item = this[index];
			item.OnDeleted();
			_list.RemoveAt(index);
			OnChanged();
		}

		public void Clear()
		{
			foreach (var item in this)
			{
				item.OnDeleted();
			}

			_list.Clear();
			OnChanged();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new ConvertEnumerator<T, int>(_list.GetEnumerator(), GetItem);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		protected abstract T GetItem(int rid);

		protected abstract T CreateItem();

		#endregion
	}
}
