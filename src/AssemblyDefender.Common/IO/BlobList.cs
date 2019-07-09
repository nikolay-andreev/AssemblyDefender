using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public class BlobList : IList<byte[]>, IBinarySerialize
	{
		#region Fields

		private int _count;
		private int _bufferLength;
		private int _lastValidIndex;
		private byte[] _buffer;
		private Entry[] _entries;

		#endregion

		#region Ctors

		public BlobList()
			: this(0, 0)
		{
		}

		public BlobList(int capacity, int bufferCapacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity");

			if (bufferCapacity < 0)
				throw new ArgumentOutOfRangeException("bufferCapacity");

			_entries = new Entry[(capacity > 0) ? capacity : 4];
			_buffer = (bufferCapacity > 0) ? new byte[bufferCapacity] : BufferUtils.EmptyArray;
			_lastValidIndex = -1;
		}

		public BlobList(IBinaryAccessor accessor)
		{
			Deserialize(accessor);
		}

		#endregion

		#region Properties

		public byte[] this[int index]
		{
			get
			{
				if (index < 0 || index >= _count)
					throw new ArgumentNullException("index");

				var entry = _entries[index];
				if (entry.Size == 0)
					return BufferUtils.EmptyArray;

				if (_lastValidIndex < index)
				{
					Validate(index);
					entry = _entries[index];
				}

				byte[] value = new byte[entry.Size];
				Buffer.BlockCopy(_buffer, entry.Offset, value, 0, entry.Size);

				return value;
			}
			set
			{
				if (index < 0 || index >= _count)
					throw new ArgumentNullException("index");

				if (value == null)
					throw new ArgumentNullException("item");

				if (_lastValidIndex < index)
				{
					Validate(index);
				}

				if (value.Length > 0)
					Update(index, value);
				else
					Empty(index);
			}
		}

		public int Count
		{
			get { return _count; }
		}

		public int Capacity
		{
			get { return _entries.Length; }
			set
			{
				if (value < _count)
					throw new ArgumentOutOfRangeException("Capacity");

				if (value != _entries.Length)
				{
					var entries = new Entry[value];
					if (_count > 0)
					{
						Array.Copy(_entries, 0, entries, 0, _count);
					}

					_entries = entries;
				}
			}
		}

		public int BufferLength
		{
			get { return _bufferLength; }
		}

		public int BufferCapacity
		{
			get { return _buffer.Length; }
			set
			{
				if (value < _bufferLength)
					throw new ArgumentOutOfRangeException("BufferCapacity");

				if (value != _buffer.Length)
				{
					byte[] buffer = new byte[value];
					Buffer.BlockCopy(_buffer, 0, buffer, 0, _bufferLength);
					_buffer = buffer;
				}
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region Methods

		public IBinaryAccessor Open(int index)
		{
			int offset;
			int length;
			GetBufferRange(index, out offset, out length);

			return new BlobAccessor(_buffer, offset, length);
		}

		public Stream OpenStream(int index)
		{
			int offset;
			int length;
			GetBufferRange(index, out offset, out length);

			return new BlobStream(_buffer, offset, length);
		}

		/// <summary>
		/// Gets internal buffer which can be bigger than the data stored.
		/// </summary>
		public byte[] GetBuffer()
		{
			return _buffer;
		}

		/// <summary>
		/// Gets a copy of buffer data.
		/// </summary>
		public byte[] ToArray()
		{
			if (_bufferLength == 0)
				return BufferUtils.EmptyArray;

			byte[] dst = new byte[_bufferLength];
			Buffer.BlockCopy(_buffer, 0, dst, 0, _bufferLength);
			return dst;
		}

		public void Serialize(IBinaryAccessor accessor)
		{
			Validate();

			accessor.Write7BitEncodedInt(_count);
			accessor.Write7BitEncodedInt(_bufferLength);

			for (int i = 0; i < _count; i++)
			{
				var entry = _entries[i];
				accessor.Write7BitEncodedInt(entry.Offset);
				accessor.Write7BitEncodedInt(entry.Size);
			}

			if (_bufferLength > 0)
			{
				accessor.Write(_buffer, 0, _bufferLength);
			}
		}

		public void Deserialize(IBinaryAccessor accessor)
		{
			_count = accessor.Read7BitEncodedInt();
			_bufferLength = accessor.Read7BitEncodedInt();
			_lastValidIndex = _count - 1;
			_entries = new Entry[_count];

			for (int i = 0; i < _count; i++)
			{
				var entry = new Entry();
				entry.Offset = accessor.Read7BitEncodedInt();
				entry.Size = accessor.Read7BitEncodedInt();
				_entries[i] = entry;
			}

			if (_bufferLength > 0)
			{
				_buffer = accessor.ReadBytes(_bufferLength);
			}
			else
			{
				_buffer = BufferUtils.EmptyArray;
			}
		}

		public bool Contains(byte[] item)
		{
			return IndexOf(item) >= 0;
		}

		public int IndexOf(byte[] item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			for (int i = 0; i < _count; i++)
			{
				var entry = _entries[i];

				if (item.Length != entry.Size)
					continue;

				if (CompareUtils.Equals(_buffer, entry.Offset, item, 0, entry.Size, false))
					return i;
			}

			return -1;
		}

		public void Add(byte[] item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (_count == _entries.Length)
			{
				Resize(_count + 1);
			}

			if (_lastValidIndex == _count - 1)
				_lastValidIndex++;

			int length = item.Length;

			_entries[_count++] =
				new Entry()
				{
					Offset = _bufferLength,
					Size = length,
				};

			if (length > 0)
			{
				int newBufferLength = _bufferLength + length;
				if (newBufferLength > _buffer.Length)
				{
					ResizeBuffer(newBufferLength);
				}

				Buffer.BlockCopy(item, 0, _buffer, _bufferLength, length);
				_bufferLength = newBufferLength;
			}
		}

		public void Insert(int index, byte[] item)
		{
			if (index == _count)
			{
				Add(item);
				return;
			}

			if (index < 0 || index > _count)
				throw new ArgumentNullException("index");

			if (item == null)
				throw new ArgumentNullException("item");

			if (_count == _entries.Length)
			{
				Resize(_count + 1);
			}

			if (index < _count)
			{
				Array.Copy(_entries, index, _entries, index + 1, _count - index);
			}

			int length = item.Length;
			int offset = 0;
			if (index > 0)
			{
				int prevIndex = index - 1;

				if (_lastValidIndex < prevIndex)
				{
					Validate(prevIndex);
					_lastValidIndex++;
				}

				var prevEntry = _entries[prevIndex];
				offset = prevEntry.Offset + prevEntry.Size;
			}

			_entries[index] =
				new Entry()
				{
					Offset = offset,
					Size = length,
				};

			_count++;

			if (length > 0)
			{
				int newBufferLength = _bufferLength + length;
				if (newBufferLength > _buffer.Length)
				{
					int newBufferCapacity = _buffer.Length * 2;
					if (newBufferCapacity < newBufferLength)
						newBufferCapacity = newBufferLength;

					byte[] buffer = new byte[newBufferCapacity];

					// Copy start buffer.
					if (offset > 0)
					{
						Buffer.BlockCopy(_buffer, 0, buffer, 0, offset);
					}

					// Copy end buffer.
					Buffer.BlockCopy(_buffer, offset, buffer, offset + length, _bufferLength - offset);

					_buffer = buffer;
				}
				else
				{
					Buffer.BlockCopy(_buffer, offset, _buffer, offset + length, _bufferLength - offset);
				}

				Buffer.BlockCopy(item, 0, _buffer, offset, length);
				_bufferLength = newBufferLength;
			}
		}

		public bool Remove(byte[] item)
		{
			int index = IndexOf(item);
			if (index < 0)
				return false;

			RemoveAt(index);
			return true;
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentNullException("index");

			var entry = _entries[index];
			if (entry.Size > 0)
			{
				int srcOffset = entry.Offset + entry.Size;
				int copySize = _bufferLength - srcOffset;
				if (copySize > 0)
				{
					Buffer.BlockCopy(_buffer, srcOffset, _buffer, entry.Offset, copySize);
				}

				_bufferLength -= entry.Size;
				_lastValidIndex = index - 1;
			}

			_count--;

			if (index < _count)
			{
				Array.Copy(_entries, index + 1, _entries, index, _count - index);
			}
		}

		public void Clear()
		{
			if (_count > 0)
			{
				Array.Clear(_entries, 0, _count);
				_count = 0;
			}

			_lastValidIndex = -1;
			_bufferLength = 0;
			_buffer = BufferUtils.EmptyArray;
		}

		public void CopyTo(byte[][] array, int arrayIndex)
		{
			for (int i = 0, j = arrayIndex; i < _count; i++, j++)
			{
				array[j] = this[i];
			}
		}

		public int GetSize(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentNullException("index");

			return _entries[index].Size;
		}

		public int GetOffset(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentNullException("index");

			return _entries[index].Offset;
		}

		public IEnumerator<byte[]> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void GetBufferRange(int index, out int offset, out int length)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentOutOfRangeException("index");

			var entry = _entries[index];
			if (entry.Size > 0)
			{
				if (_lastValidIndex < index)
				{
					Validate(index);
					entry = _entries[index];
				}

				offset = entry.Offset;
				length = entry.Size;
			}
			else
			{
				offset = 0;
				length = 0;
			}
		}

		private void Update(int index, byte[] value)
		{
			var entry = _entries[index];

			int valueSize = value.Length;
			if (entry.Size == valueSize)
			{
				// Current is the same size
				Buffer.BlockCopy(value, 0, _buffer, entry.Offset, entry.Size);
			}
			else if (entry.Size > valueSize)
			{
				// Current is bigger
				int srcOffset = entry.Offset + entry.Size;
				int dstOffset = entry.Offset + valueSize;
				Buffer.BlockCopy(_buffer, srcOffset, _buffer, dstOffset, _buffer.Length - srcOffset);
				Buffer.BlockCopy(value, 0, _buffer, entry.Offset, valueSize);
				_entries[index].Size = valueSize;
				_bufferLength -= entry.Size - valueSize;
				_lastValidIndex = index;
			}
			else
			{
				// Current is smaller
				int currentSize = entry.Size;
				int addSize = valueSize - currentSize;
				int newBufferLength = _bufferLength + addSize;

				int currentOffset = entry.Offset;
				if (entry.Size == 0)
				{
					if (index > 0)
					{
						var prevEntry = _entries[index - 1];
						currentOffset = prevEntry.Offset + prevEntry.Size;
					}
					else
					{
						currentOffset = 0;
					}
				}

				if (newBufferLength <= _buffer.Length)
				{
					// Existing buffer is sufficient
					int srcOffset = currentOffset + currentSize;
					int dstOffset = currentOffset + valueSize;
					Buffer.BlockCopy(_buffer, srcOffset, _buffer, dstOffset, _bufferLength - srcOffset);
				}
				else
				{
					// Existing buffer is not sufficient
					int newBufferCapacity = _buffer.Length * 2;
					if (newBufferCapacity < newBufferLength)
						newBufferCapacity = newBufferLength;

					byte[] newBuffer = new byte[newBufferCapacity];

					// Copy start buffer.
					if (currentOffset > 0)
					{
						Buffer.BlockCopy(_buffer, 0, newBuffer, 0, currentOffset);
					}

					// Copy end buffer.
					int srcOffset = currentOffset + currentSize;
					int dstOffset = currentOffset + valueSize;
					int copySize = _bufferLength - srcOffset;
					if (copySize > 0)
					{
						Buffer.BlockCopy(_buffer, srcOffset, newBuffer, dstOffset, copySize);
					}

					_buffer = newBuffer;
				}

				Buffer.BlockCopy(value, 0, _buffer, currentOffset, valueSize);
				_entries[index].Size = valueSize;
				_bufferLength = newBufferLength;
				_lastValidIndex = index;
			}
		}

		private void Empty(int index)
		{
			var entry = _entries[index];
			if (entry.Size > 0)
			{
				int srcOffset = entry.Offset + entry.Size;
				int copySize = _bufferLength - srcOffset;
				if (copySize > 0)
				{
					Buffer.BlockCopy(_buffer, srcOffset, _buffer, entry.Offset, copySize);
				}

				_bufferLength -= entry.Size;
				_lastValidIndex = index;
			}

			_entries[index].Size = 0;
		}

		private void Resize(int min)
		{
			int capacity = (_entries.Length > 0) ? _entries.Length * 2 : 4;
			if (capacity < min)
			{
				capacity = min;
			}

			var entries = new Entry[capacity];
			if (_count > 0)
			{
				Array.Copy(_entries, 0, entries, 0, _count);
			}

			_entries = entries;
		}

		private void ResizeBuffer(int min)
		{
			int capacity = _buffer.Length * 2;
			if (capacity < min)
				capacity = min;

			byte[] buffer = new byte[capacity];
			Buffer.BlockCopy(_buffer, 0, buffer, 0, _bufferLength);
			_buffer = buffer;
		}

		private void Validate()
		{
			int lastIndex = _count - 1;
			if (_lastValidIndex < lastIndex)
			{
				Validate(lastIndex);
			}
		}

		private void Validate(int index)
		{
			int offset = _entries[_lastValidIndex].Offset;
			int size = _entries[_lastValidIndex].Size;

			for (int i = _lastValidIndex + 1; i <= index; i++)
			{
				offset += (size > 0) ? size : 0;
				_entries[i].Offset = offset;
				size = _entries[i].Size;
			}

			_lastValidIndex = index;
		}

		#endregion

		#region Nested types

		[Serializable]
		public struct Enumerator : IEnumerable<byte[]>, IEnumerator<byte[]>
		{
			#region Fields

			private int _index;
			private BlobList _list;

			#endregion

			#region Ctors

			public Enumerator(BlobList list)
			{
				_list = list;
				_index = -1;
			}

			#endregion

			#region Properties

			public byte[] Current
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

			IEnumerator<byte[]> IEnumerable<byte[]>.GetEnumerator()
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
			internal int Offset;
			internal int Size;
		}

		#endregion
	}
}
