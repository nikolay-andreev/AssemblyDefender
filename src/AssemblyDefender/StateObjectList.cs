using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class StateObjectList<T> : StateObject, IEnumerable<T>
		where T : StateObject, new()
	{
		#region Fields

		private List<int> _list = new List<int>();

		#endregion

		#region Properties

		public T this[int index]
		{
			get { return _state.GetObject<T>(_list[index], true); }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public T Add()
		{
			var item = new T();
			int rid = _state.AddObject(item);
			_list.Add(rid);
			OnChanged();

			return item;
		}

		protected internal override void Read(IBinaryAccessor accessor)
		{
			var count = accessor.Read7BitEncodedInt();

			_list.Capacity = count;

			for (int i = 0; i < count; i++)
			{
				_list.Add(accessor.Read7BitEncodedInt());
			}

			base.Read(accessor);
		}

		protected internal override void Write(IBinaryAccessor accessor)
		{
			accessor.Write7BitEncodedInt(_list.Count);

			for (int i = 0; i < _list.Count; i++)
			{
				accessor.Write7BitEncodedInt(_list[i]);
			}

			base.Write(accessor);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Nested types

		[Serializable]
		private struct Enumerator : IEnumerator<T>
		{
			#region Fields

			private int _index;
			private StateObjectList<T> _list;

			#endregion

			#region Ctors

			public Enumerator(StateObjectList<T> list)
			{
				_list = list;
				_index = -1;
			}

			#endregion

			#region Properties

			public T Current
			{
				get
				{
					if (_index < 0)
						return null;

					return _list[_index];
				}
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			#endregion

			#region Methods

			public bool MoveNext()
			{
				if (_index + 1 == _list.Count)
					return false;

				_index++;
				return true;
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

		#endregion
	}
}
