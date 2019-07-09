using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	[Serializable]
	public struct ListNavigator<T> : IEnumerator<T>
	{
		#region Fields

		private int _index;
		private IList<T> _list;

		#endregion

		#region Ctors

		public ListNavigator(IList<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			_list = list;
			_index = -1;
		}

		#endregion

		#region Properties

		public bool EOF
		{
			get { return _index >= _list.Count; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public T Current
		{
			get
			{
				if (_index < 0 || _index >= _list.Count)
					return default(T);

				return _list[_index];
			}
		}

		public T Previous
		{
			get { return GetPrevious(1); }
		}

		public T Next
		{
			get { return GetNext(1); }
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}

		#endregion

		#region Methods

		public T GetNext(int offset, bool throwIfMissing = false)
		{
			int index = _index + offset;
			if (index < 0 || index >= _list.Count)
			{
				if (throwIfMissing)
				{
					throw new IndexOutOfRangeException("offset");
				}

				return default(T);
			}

			return _list[index];
		}

		public T GetPrevious(int offset, bool throwIfMissing = false)
		{
			int index = _index - offset;
			if (index < 0 || index >= _list.Count)
			{
				if (throwIfMissing)
				{
					throw new IndexOutOfRangeException("offset");
				}

				return default(T);
			}

			return _list[index];
		}

		public bool MoveNext()
		{
			int count = _list.Count;
			if (_index >= count)
				return false;

			_index++;
			return _index < count;
		}

		public void SetCurrent(int index)
		{
			if (_index < 0 || _index >= _list.Count)
			{
				throw new IndexOutOfRangeException("index");
			}

			_index = index;
		}

		public void Reset()
		{
			_index = -1;
		}

		public void Dispose()
		{
		}

		#endregion
	}
}
