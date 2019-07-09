using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	internal class ModuleObjectState
	{
		#region Fields

		private int _count;
		private byte[] _buffer;
		private Entry[] _entries;
		private StateObject[] _items;
		private ModuleState _state;

		#endregion

		#region Ctors

		internal ModuleObjectState(ModuleState state)
		{
			_state = state;
			_items = new StateObject[0x10];
		}

		internal ModuleObjectState(ModuleState state, IBinaryAccessor accessor)
		{
			_state = state;
			Deserialize(accessor);
		}

		#endregion

		#region Methods

		internal T Get<T>(int index)
			where T : StateObject, new()
		{
			var item = _items[index];
			if (item == null)
			{
				item = new T();
				_items[index] = item;
				item.Init(index + 1, _state);
				item.Read(new BlobAccessor(_buffer, _entries[index].Offset, _entries[index].Size));
			}

			return (T)item;
		}

		internal int Add(StateObject item)
		{
			if (_count == _items.Length)
			{
				ResizeItems();
			}

			int index = _count++;
			_items[index] = item;
			item.Init(_count, _state);

			return index;
		}

		internal void Scavenge()
		{
			Array.Clear(_items, 0, _count);
		}

		private void ResizeItems()
		{
			int newCount = Math.Max(_count * 2, 0x10);
			var newItems = new StateObject[newCount];
			if (_items != null)
			{
				Array.Copy(_items, 0, newItems, 0, _count);
			}

			_items = newItems;
		}

		internal void Serialize(StreamAccessor accessor)
		{
			var bufferBlob = new Blob();
			var bufferAccessor = new BlobAccessor(bufferBlob);

			accessor.Write7BitEncodedInt(_count);

			var entries = new Entry[_count];
			for (int i = 0; i < _count; i++)
			{
				int offset = (int)bufferAccessor.Position;

				var item = _items[i];
				if (item != null && item.IsChanged)
				{
					item.Write(bufferAccessor);
				}
				else if (_entries != null && _entries.Length > i)
				{
					int existingSize = _entries[i].Size;
					if (existingSize > 0)
					{
						bufferAccessor.Write(_buffer, _entries[i].Offset, existingSize);
					}
				}

				int size = (int)bufferAccessor.Position - offset;

				accessor.Write7BitEncodedInt(offset);
				accessor.Write7BitEncodedInt(size);

				entries[i] =
					new Entry()
					{
						Offset = offset,
						Size = size,
					};
			}

			_entries = entries;
			_buffer = bufferBlob.ToArray();
			Array.Clear(_items, 0, _count);

			accessor.Write7BitEncodedInt(_buffer.Length);

			if (_buffer.Length > 0)
			{
				accessor.Write(_buffer, 0, _buffer.Length);
			}
		}

		private void Deserialize(IBinaryAccessor accessor)
		{
			_count = accessor.Read7BitEncodedInt();
			_entries = new Entry[_count];
			_items = new StateObject[Math.Max(_count, 0x10)];

			for (int i = 0; i < _count; i++)
			{
				_entries[i] =
					new Entry()
					{
						Offset = accessor.Read7BitEncodedInt(),
						Size = accessor.Read7BitEncodedInt(),
					};
			}

			int bufferLength = accessor.Read7BitEncodedInt();
			if (bufferLength > 0)
			{
				_buffer = accessor.ReadBytes(bufferLength);
			}
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
