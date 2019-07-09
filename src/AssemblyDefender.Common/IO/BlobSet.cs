using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public class BlobSet : IBinarySerialize
	{
		#region Fields

		private int _count;
		private int _capacity;
		private int _bufferLength;
		private int _lastValidIndex;
		private byte[] _buffer;
		private Entry[] _entries;

		#endregion

		#region Ctors

		public BlobSet()
			: this(0, 0)
		{
		}

		public BlobSet(int capacity, int bufferCapacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity");

			if (bufferCapacity < 0)
				throw new ArgumentOutOfRangeException("bufferCapacity");

			_capacity = (capacity > 0) ? capacity : 4;
			_entries = new Entry[_capacity];
			_buffer = (bufferCapacity > 0) ? new byte[bufferCapacity] : BufferUtils.EmptyArray;

			for (int i = 0; i < capacity; i++)
			{
				_entries[i].Size = -1;
			}
		}

		public BlobSet(IBinaryAccessor accessor)
		{
			Deserialize(accessor);
		}

		#endregion

		#region Properties

		public byte[] this[int index]
		{
			get
			{
				if (index < 0)
					throw new ArgumentOutOfRangeException("index");

				if (index > _capacity)
					return null;

				var entry = _entries[index];
				if (entry.Size == -1)
					return null;

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
				if (index >= _capacity)
				{
					Resize(index + 1);
				}

				if (_lastValidIndex < index)
				{
					Validate(index);
				}

				if (value == null)
				{
					Remove(index, false);
				}
				else if (value.Length == 0)
				{
					Remove(index, true);
				}
				else
				{
					Update(index, value);
				}

				if (_count < index + 1)
					_count = index + 1;
			}
		}

		public int Count
		{
			get { return _count; }
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
			_count = _capacity = accessor.Read7BitEncodedInt();
			_bufferLength = accessor.Read7BitEncodedInt();

			if (_capacity > 0)
			{
				_entries = new Entry[_capacity];

				for (int i = 0; i < _capacity; i++)
				{
					var entry = new Entry();
					entry.Offset = accessor.Read7BitEncodedInt();
					entry.Size = accessor.Read7BitEncodedInt();
					_entries[i] = entry;
				}

				_lastValidIndex = _capacity - 1;
			}

			if (_bufferLength > 0)
			{
				_buffer = accessor.ReadBytes(_bufferLength);
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
				int currentSize = (entry.Size > 0) ? entry.Size : 0;
				int addSize = valueSize - currentSize;
				int newBufferLength = _bufferLength + addSize;

				if (newBufferLength <= _buffer.Length)
				{
					// Existing buffer is sufficient
					int srcOffset = entry.Offset + currentSize;
					int dstOffset = entry.Offset + valueSize;
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
					if (entry.Offset > 0)
					{
						Buffer.BlockCopy(_buffer, 0, newBuffer, 0, entry.Offset);
					}

					// Copy end buffer.
					int srcOffset = entry.Offset + currentSize;
					int dstOffset = entry.Offset + valueSize;
					int copySize = _bufferLength - srcOffset;
					if (copySize > 0)
					{
						Buffer.BlockCopy(_buffer, srcOffset, newBuffer, dstOffset, copySize);
					}

					_buffer = newBuffer;
				}

				Buffer.BlockCopy(value, 0, _buffer, entry.Offset, valueSize);
				_entries[index].Size = valueSize;
				_bufferLength = newBufferLength;
				_lastValidIndex = index;
			}
		}

		private void Remove(int index, bool empty)
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

			_entries[index].Size = empty ? 0 : -1;
		}

		private void GetBufferRange(int index, out int offset, out int length)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index");

			offset = 0;
			length = 0;

			if (index > _capacity)
				return;

			var entry = _entries[index];
			if (entry.Size == -1)
				return;

			if (entry.Size == 0)
				return;

			if (_lastValidIndex < index)
			{
				Validate(index);
				entry = _entries[index];
			}

			offset = entry.Offset;
			length = entry.Size;
		}

		private void Resize(int min)
		{
			int capacity = (_capacity > 0) ? _capacity * 2 : 4;
			if (capacity < min)
			{
				capacity = min;
			}

			var entries = new Entry[capacity];

			if (_capacity > 0)
			{
				Array.Copy(_entries, 0, entries, 0, _capacity);
			}

			for (int i = _capacity; i < capacity; i++)
			{
				var entry = new Entry();
				entry.Offset = _bufferLength;
				entry.Size = -1;
				entries[i] = entry;
			}

			_capacity = capacity;
			_entries = entries;
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

		[StructLayout(LayoutKind.Sequential)]
		private struct Entry
		{
			internal int Offset;
			internal int Size;
		}

		#endregion
	}
}
