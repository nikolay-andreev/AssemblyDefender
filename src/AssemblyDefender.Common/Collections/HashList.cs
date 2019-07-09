using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	public class HashList<T> : IReadOnlyList<T>
	{
		#region Fields

		private int _count;
		private int[] _buckets;
		private Entry[] _entries;
		private IEqualityComparer<T> _comparer;

		#endregion

		#region Ctors

		public HashList()
			: this(0, null)
		{
		}

		public HashList(int capacity)
			: this(0, null)
		{
		}

		public HashList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		public HashList(int capacity, IEqualityComparer<T> comparer)
		{
			if (capacity > 0)
			{
				Initialize(capacity);
			}

			if (comparer == null)
			{
				comparer = EqualityComparer<T>.Default;
			}

			_comparer = comparer;
		}

		#endregion

		#region Properties

		public T this[int index]
		{
			get { return _entries[index].Value; }
		}

		public int Count
		{
			get { return _count; }
		}

		#endregion

		#region Methods

		public bool Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		public int IndexOf(T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (_buckets == null)
			{
				Initialize(0);
			}

			int hashCode = _comparer.GetHashCode(item) & 0x7fffffff;
			int hashIndex = hashCode % _buckets.Length;
			for (int i = _buckets[hashIndex]; i >= 0; i = _entries[i].Next)
			{
				if (_entries[i].HashCode == hashCode && _comparer.Equals(_entries[i].Value, item))
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Add item or throw an exception if already exists.
		/// </summary>
		public void Add(T item)
		{
			int index = 0;
			if (!TryAdd(item, out index))
			{
				throw new ArgumentException(SR.CollectionAddingDuplicateKey);
			}
		}

		public void AddAll(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				Add(item);
			}
		}

		public bool TryAdd(T item)
		{
			int index = 0;
			return TryAdd(item, out index);
		}

		public bool TryAdd(T item, out int index)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (_buckets == null)
			{
				Initialize(0);
			}

			int hashCode = _comparer.GetHashCode(item) & 0x7fffffff;
			int hashIndex = hashCode % _buckets.Length;
			for (int i = _buckets[hashIndex]; i >= 0; i = _entries[i].Next)
			{
				if (_entries[i].HashCode == hashCode && _comparer.Equals(_entries[i].Value, item))
				{
					index = i;
					return false;
				}
			}

			if (_count == _entries.Length)
			{
				Resize();
				hashIndex = hashCode % _buckets.Length;
			}

			index = _count;

			_entries[index] =
				new Entry()
				{
					Next = _buckets[hashIndex],
					HashCode = hashCode,
					Value = item,
				};

			_buckets[hashIndex] = index;

			_count++;

			return true;
		}

		public void TryAddAll(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				TryAdd(item);
			}
		}

		/// <summary>
		/// Add item or replace if already exists.
		/// </summary>
		public int Set(T item)
		{
			int index = 0;
			if (!TryAdd(item, out index))
			{
				_entries[index].Value = item;
			}

			return index;
		}

		public void Clear()
		{
			if (_count == 0)
				return;

			for (int i = 0; i < _buckets.Length; i++)
			{
				_buckets[i] = -1;
			}

			Array.Clear(_entries, 0, _count);
			_count = 0;
		}

		public T[] ToArray()
		{
			var items = new T[_count];
			for (int i = 0; i < _count; i++)
			{
				items[i] = _entries[i].Value;
			}

			return items;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void Initialize(int capacity)
		{
			if (capacity == 0)
				capacity = 10;

			_buckets = new int[capacity];
			for (int i = 0; i < capacity; i++)
			{
				_buckets[i] = -1;
			}

			_entries = new Entry[capacity];
		}

		private void Resize()
		{
			int count = _count * 2;
			int[] newBuckets = new int[count];
			for (int i = 0; i < count; i++)
			{
				newBuckets[i] = -1;
			}

			var destinationArray = new Entry[count];
			Array.Copy(_entries, 0, destinationArray, 0, _count);

			for (int i = 0; i < _count; i++)
			{
				int index = destinationArray[i].HashCode % count;
				destinationArray[i].Next = newBuckets[index];
				newBuckets[index] = i;
			}

			_buckets = newBuckets;
			_entries = destinationArray;
		}

		#endregion

		#region Nested types

		[Serializable]
		public struct Enumerator : IEnumerable<T>, IEnumerator<T>
		{
			#region Fields

			private int _index;
			private HashList<T> _list;

			#endregion

			#region Ctors

			public Enumerator(HashList<T> list)
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
						return default(T);

					return _list._entries[_index].Value;
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
				if (_index + 1 == _list._count)
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

		[StructLayout(LayoutKind.Sequential)]
		private struct Entry
		{
			internal int Next;
			internal int HashCode;
			internal T Value;
		}

		#endregion
	}
}
