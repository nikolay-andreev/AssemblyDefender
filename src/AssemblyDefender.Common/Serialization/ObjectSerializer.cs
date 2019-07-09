using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Common.Serialization
{
	public abstract class ObjectSerializer<T> : IBinarySerialize
	{
		#region Fields

		protected bool _keepAlive = true;
		protected bool _delayWrite;
		protected int _count;
		protected int[] _buckets;
		protected Entry[] _entries;
		protected IEqualityComparer<T> _comparer;
		protected Blob _blob;

		#endregion

		#region Ctors

		public ObjectSerializer()
			: this(0, null)
		{
		}

		public ObjectSerializer(int capacity)
			: this(capacity, null)
		{
		}

		public ObjectSerializer(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		public ObjectSerializer(int capacity, IEqualityComparer<T> comparer)
		{
			if (capacity > 0)
			{
				Initialize(capacity);
			}

			_comparer = comparer ?? EqualityComparer<T>.Default;
			_blob = new Blob();
		}

		public ObjectSerializer(IBinaryAccessor accessor)
			: this(accessor, null)
		{
		}

		public ObjectSerializer(IBinaryAccessor accessor, IEqualityComparer<T> comparer)
		{
			_comparer = comparer ?? EqualityComparer<T>.Default;
			Deserialize(accessor);
		}

		#endregion

		#region Properties

		public T this[int index]
		{
			get
			{
				var value = _entries[index].Value;
				if (value == null)
				{
					value = Read(_entries[index].Offset);
					if (_keepAlive)
					{
						_entries[index].Value = value;
					}
				}

				return value;
			}
		}

		public int Count
		{
			get { return _count; }
		}

		public bool KeepAlive
		{
			get { return _keepAlive; }
			set
			{
				_keepAlive = value;
				if (!_keepAlive)
					_delayWrite = false;
			}
		}

		public bool DelayWrite
		{
			get { return _delayWrite; }
			set
			{
				_delayWrite = value;
				if (_delayWrite)
					_keepAlive = true;
			}
		}

		#endregion

		#region Methods

		public int Add(T item)
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
				if (_entries[i].HashCode == hashCode)
				{
					var value = _entries[i].Value;
					if (value == null)
					{
						value = Read(_entries[i].Offset);

						if (_keepAlive)
						{
							_entries[i].Value = value;
						}
					}

					if (_comparer.Equals(value, item))
					{
						return i;
					}
				}
			}

			if (_count == _entries.Length)
			{
				Resize();
				hashIndex = hashCode % _buckets.Length;
			}

			int index = _count;

			bool keepAlive = CanKeepAlive(item);
			bool delayWrite = _delayWrite && keepAlive;

			_entries[index] =
				new Entry()
				{
					Next = _buckets[hashIndex],
					HashCode = hashCode,
					Offset = delayWrite ? -1 : _blob.Length,
					Value = keepAlive ? item : default(T),
				};

			_buckets[hashIndex] = index;

			_count++;

			if (!delayWrite)
			{
				Write(item);
			}

			return index;
		}

		public byte[] Save()
		{
			var blob = new Blob();
			Serialize(new BlobAccessor(blob));

			return blob.ToArray();
		}

		public virtual void Serialize(IBinaryAccessor accessor)
		{
			// Write lazy load
			for (int i = 0; i < _count; i++)
			{
				var entry = _entries[i];
				if (entry.Offset < 0)
				{
					_entries[i].Offset = _blob.Length;
					Write(entry.Value);
				}
			}

			accessor.Write7BitEncodedInt(_count);
			accessor.Write7BitEncodedInt(_blob.Length);

			for (int i = 0; i < _count; i++)
			{
				var entry = _entries[i];
				accessor.Write7BitEncodedInt(entry.Offset);
				accessor.Write7BitEncodedInt(entry.HashCode);
			}

			if (_blob.Length > 0)
			{
				accessor.Write(_blob.GetBuffer(), 0, _blob.Length);
			}
		}

		public virtual void Deserialize(IBinaryAccessor accessor)
		{
			_count = accessor.Read7BitEncodedInt();
			int bufferLength = accessor.Read7BitEncodedInt();

			if (_count > 0)
			{
				_buckets = new int[_count];
				for (int i = 0; i < _count; i++)
				{
					_buckets[i] = -1;
				}

				_entries = new Entry[_count];

				for (int i = 0; i < _count; i++)
				{
					var entry = new Entry();
					entry.Offset = accessor.Read7BitEncodedInt();
					entry.HashCode = accessor.Read7BitEncodedInt();

					int hashIndex = entry.HashCode % _buckets.Length;
					entry.Next = _buckets[hashIndex];
					_buckets[hashIndex] = i;

					_entries[i] = entry;
				}
			}

			_blob = new Blob(accessor.ReadBytes(bufferLength));
		}

		protected abstract T Read(int pos);

		protected abstract void Write(T value);

		protected virtual bool CanKeepAlive(T item)
		{
			return _keepAlive;
		}

		private void Initialize(int capacity)
		{
			if (capacity == 0)
				capacity = 4;

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
			int[] buckets = new int[count];
			for (int i = 0; i < count; i++)
			{
				buckets[i] = -1;
			}

			var entries = new Entry[count];
			Array.Copy(_entries, 0, entries, 0, _count);

			for (int i = 0; i < _count; i++)
			{
				int index = entries[i].HashCode % count;
				entries[i].Next = buckets[index];
				buckets[index] = i;
			}

			_buckets = buckets;
			_entries = entries;
		}

		#endregion

		#region Nested types

		[StructLayout(LayoutKind.Sequential)]
		protected struct Entry
		{
			public int Next;
			public int Offset;
			public int HashCode;
			public T Value;
		}

		#endregion
	}
}
