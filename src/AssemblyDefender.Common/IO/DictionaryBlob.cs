using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Common.IO
{
	/// <summary>
	/// Represents in-memory buffer with dictionary hash.
	/// </summary>
	public class DictionaryBlob<TValue> : Blob, IEnumerable<TupleStruct<int, int, TValue>>
	{
		#region Fields

		private int _count;
		private int _freeCount;
		private int _freeList;
		private int[] _buckets;
		private Entry[] _entries;
		private ByteArrayEqualityComparer _comparer;

		#endregion

		#region Ctors

		public DictionaryBlob()
			: this(0, 0)
		{
		}

		public DictionaryBlob(int capacity, int bufferCapacity)
			: base(bufferCapacity)
		{
			if (capacity > 0)
			{
				Initialize(capacity);
			}
		}

		public DictionaryBlob(int capacity, byte[] buffer)
			: base(buffer)
		{
			if (capacity > 0)
			{
				Initialize(capacity);
			}
		}

		#endregion

		#region Properties

		public TValue this[byte[] key]
		{
			get
			{
				if (key == null)
					throw new ArgumentException("key");

				int index = FindEntry(key, 0, key.Length);
				if (index < 0)
					throw new KeyNotFoundException();

				return _entries[index].Value;
			}
			set
			{
				if (key == null)
					throw new ArgumentException("key");

				Insert(key, 0, key.Length, value, false);
			}
		}

		public int Count
		{
			get { return _count; }
		}

		#endregion

		#region Methods

		public bool Contains(byte[] key)
		{
			if (key == null)
				throw new ArgumentException("key");

			return Contains(key, 0, key.Length);
		}

		public bool Contains(byte[] key, int offset, int count)
		{
			return FindEntry(key, offset, count) >= 0;
		}

		public bool TryGetValue(byte[] key, out TValue value)
		{
			if (key == null)
				throw new ArgumentException("key");

			return TryGetValue(key, 0, key.Length, out value);
		}

		public bool TryGetValue(byte[] key, int offset, int count, out TValue value)
		{
			int index = FindEntry(key, offset, count);
			if (index >= 0)
			{
				value = _entries[index].Value;
				return true;
			}
			else
			{
				value = default(TValue);
				return false;
			}
		}

		public void Add(byte[] key, TValue value)
		{
			if (key == null)
				throw new ArgumentException("key");

			Add(key, 0, key.Length, value);
		}

		public void Add(byte[] key, int offset, int count, TValue value)
		{
			Insert(key, offset, count, value, true);
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
			_freeList = -1;
			_freeCount = 0;
		}

		public bool Remove(byte[] key)
		{
			if (key == null)
				throw new ArgumentException("key");

			return Remove(key, 0, key.Length);
		}

		public bool Remove(byte[] key, int offset, int count)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (_buckets == null)
				return false;

			int hashCode = _comparer.GetHashCode(key, offset, count) & 0x7fffffff;
			int index = hashCode % _buckets.Length;
			int prevEntry = -1;
			for (int i = _buckets[index]; i >= 0; i = _entries[i].Next)
			{
				if (_entries[i].HashCode == hashCode)
				{
					var entry = _entries[i];
					if (count == entry.Size && _comparer.Equals(key, 0, _buffer, entry.Offset, entry.Size))
					{
						if (prevEntry < 0)
							_buckets[index] = entry.Next;
						else
							_entries[prevEntry].Next = entry.Next;

						_entries[i] =
							new Entry()
							{
								HashCode = -1,
								Next = _freeList,
								Offset = -1,
								Size = -1,
								Value = default(TValue),
							};

						_freeList = i;
						_freeCount++;

						return true;
					}
				}

				prevEntry = i;
			}

			return false;
		}

		public IEnumerator<TupleStruct<int, int, TValue>> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		private void Initialize(int capacity)
		{
			if (capacity == 0)
				capacity = 10;

			_buckets = new int[capacity];
			for (int i = 0; i < _buckets.Length; i++)
			{
				_buckets[i] = -1;
			}

			_entries = new Entry[capacity];
			_comparer = ByteArrayEqualityComparer.Instance;
			_freeList = -1;
		}

		private int FindEntry(byte[] key, int offset, int count)
		{
			if (_buckets != null)
			{
				int hashCode = _comparer.GetHashCode(key, offset, count) & 0x7fffffff;
				for (int i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _entries[i].Next)
				{
					if (_entries[i].HashCode == hashCode)
					{
						var entry = _entries[i];
						if (count == entry.Size && _comparer.Equals(key, 0, _buffer, entry.Offset, entry.Size))
						{
							return i;
						}
					}
				}
			}

			return -1;
		}

		private void Insert(byte[] key, int offset, int count, TValue value, bool add)
		{
			if (_buckets == null)
			{
				Initialize(0);
			}

			int hashCode = _comparer.GetHashCode(key, offset, count) & 0x7fffffff;
			int index = hashCode % _buckets.Length;
			for (int i = _buckets[index]; i >= 0; i = _entries[i].Next)
			{
				if (_entries[i].HashCode == hashCode)
				{
					var entry = _entries[i];
					if (count == entry.Size && _comparer.Equals(key, 0, _buffer, entry.Offset, entry.Size))
					{
						if (add)
						{
							throw new ArgumentException(SR.CollectionAddingDuplicateKey);
						}

						_entries[i].Value = value;
						return;
					}
				}
			}

			int freeList;
			if (_freeCount > 0)
			{
				freeList = _freeList;
				_freeList = _entries[freeList].Next;
				_freeCount--;
			}
			else
			{
				if (_count == _entries.Length)
				{
					Resize();
					index = hashCode % _buckets.Length;
				}

				freeList = _count;
				_count++;
			}

			_entries[freeList] =
				new Entry()
				{
					HashCode = hashCode,
					Next = _buckets[index],
					Offset = _length,
					Size = count,
					Value = value,
				};

			_buckets[index] = freeList;

			int pos = _length;
			Write(ref pos, key, offset, count);
		}

		private void Resize()
		{
			int count = _count * 2;
			int[] numArray = new int[count];
			for (int i = 0; i < numArray.Length; i++)
			{
				numArray[i] = -1;
			}

			var destinationArray = new Entry[count];
			Array.Copy(_entries, 0, destinationArray, 0, _count);

			for (int i = 0; i < _count; i++)
			{
				int index = destinationArray[i].HashCode % count;
				destinationArray[i].Next = numArray[index];
				numArray[index] = i;
			}

			_buckets = numArray;
			_entries = destinationArray;
		}

		#endregion

		#region Nested types

		[Serializable]
		public struct Enumerator : IEnumerator<TupleStruct<int, int, TValue>>
		{
			private int _index;
			private TupleStruct<int, int, TValue> _current;
			private DictionaryBlob<TValue> _dictionary;

			internal Enumerator(DictionaryBlob<TValue> dictionary)
			{
				_dictionary = dictionary;
				_index = 0;
				_current = new TupleStruct<int, int, TValue>();
			}

			public TupleStruct<int, int, TValue> Current
			{
				get { return _current; }
			}

			object IEnumerator.Current
			{
				get { return _current; }
			}

			public bool MoveNext()
			{
				while (_index < _dictionary._count)
				{
					if (_dictionary._entries[_index].HashCode >= 0)
					{
						var entry = _dictionary._entries[_index];
						_current = new TupleStruct<int, int, TValue>(entry.Offset, entry.Size, entry.Value);
						_index++;
						return true;
					}

					_index++;
				}

				_index = _dictionary._count + 1;
				_current = new TupleStruct<int, int, TValue>();

				return false;
			}

			void IEnumerator.Reset()
			{
				_index = 0;
				_current = new TupleStruct<int, int, TValue>();
			}

			void IDisposable.Dispose()
			{
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct Entry
		{
			internal int HashCode;
			internal int Next;
			internal int Offset;
			internal int Size;
			internal TValue Value;
		}

		#endregion
	}
}
