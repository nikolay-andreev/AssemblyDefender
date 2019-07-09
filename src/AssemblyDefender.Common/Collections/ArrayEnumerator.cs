using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	[Serializable]
	public struct ArrayEnumerator<T> : IEnumerable<T>, IEnumerator<T>
	{
		#region Fields

		private int _offset;
		private int _index;
		private int _count;
		private T[] _array;

		#endregion

		#region Ctors

		public ArrayEnumerator(T[] array)
			: this(array, 0, array != null ? array.Length : 0)
		{
		}

		public ArrayEnumerator(T[] array, int offset, int length)
		{
			_array = array;
			_offset = offset;
			_count = length;
			_index = -1;
		}

		#endregion

		#region Properties

		public T Current
		{
			get
			{
				if (_index < 0)
					return default(T);

				return _array[_index + _offset];
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
			if (_index + 1 == _count)
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

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		#endregion
	}
}
