using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace AssemblyDefender.Common
{
	public class MRUList : IEnumerable<string>
	{
		#region Fields

		private const int DefaultCapacity = 10;
		private bool _useLocalMachine;
		private string _registryPath;
		private int _capacity;
		private int _count;
		private int _head;
		private string[] _array;

		#endregion

		#region Ctors

		public MRUList(string registryPath)
			: this(registryPath, DefaultCapacity, false)
		{
		}

		public MRUList(string registryPath, int capacity)
			: this(registryPath, capacity, false)
		{
		}

		public MRUList(string registryPath, int capacity, bool useLocalMachine)
		{
			_registryPath = registryPath;
			_capacity = capacity;
			_useLocalMachine = useLocalMachine;
			_array = new string[capacity];

			Load();
		}

		#endregion

		#region Properties

		public string this[int index]
		{
			get
			{
				if (_count == 0)
					throw new IndexOutOfRangeException("index");

				index = _head - index - 1;

				if (index < 0)
					index += _capacity;

				return _array[index];
			}
		}

		public int Count
		{
			get { return _count; }
		}

		public int Capacity
		{
			get { return _capacity; }
		}

		public bool UseLocalMachine
		{
			get { return _useLocalMachine; }
		}

		public string RegistryPath
		{
			get { return _registryPath; }
		}

		#endregion

		#region Methods

		public void Add(string item)
		{
			int index = IndexOf(item);
			if (index >= 0) // Existing item.
			{
				if (index == _head - 1)
					return;

				if (index < _head)
				{
					for (int i = index + 1; i < _head; i++)
					{
						_array[i - 1] = _array[i];
					}
				}
				else
				{
					for (int i = index + 1; i < _capacity; i++)
					{
						_array[i - 1] = _array[i];
					}

					if (_head > 0)
					{
						_array[_capacity - 1] = _array[0];

						for (int i = 1; i < _head; i++)
						{
							_array[i - 1] = _array[i];
						}
					}
				}

				index = _head > 0 ? _head - 1 : _capacity - 1;
				_array[index] = item;
			}
			else // New item.
			{
				_array[_head] = item;

				// Move head.
				if (_head < _capacity - 1)
					_head++;
				else
					_head = 0;

				// Increment count.
				if (_count < _capacity)
					_count++;
			}

			Save();
		}

		public void Clear()
		{
			_count = 0;
			_head = 0;

			Save();
		}

		public IEnumerator<string> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private int IndexOf(string item)
		{
			for (int i = _head - 1; i >= 0; i--)
			{
				if (EqualsItem(_array[i], item))
					return i;
			}

			for (int i = _capacity - 1, j = _count - _head - 1; j >= 0; i--, j--)
			{
				if (EqualsItem(_array[i], item))
					return i;
			}

			return -1;
		}

		private void Load()
		{
			var list = new List<string>(_capacity);

			using (var key = GetRootRegistryKey().OpenSubKey(_registryPath))
			{
				if (key == null)
					return;

				for (int i = 0; i < _capacity; i++)
				{
					string keyName = GetRegistryKeyName(i + 1);
					string value = key.GetValue(keyName) as string;
					if (string.IsNullOrEmpty(value))
						break;

					if (!list.Contains(value, StringComparer.OrdinalIgnoreCase))
						list.Add(value);
				}
			}

			_count = list.Count;

			_head = (_count < _capacity) ? _count : 0;

			for (int i = 0, j = list.Count - 1; i < _count; i++, j--)
			{
				_array[i] = list[j];
			}
		}

		private void Save()
		{
			using (var key = GetRootRegistryKey().CreateSubKey(_registryPath))
			{
				int id = 1;

				for (int i = _head - 1; i >= 0; i--, id++)
				{
					string keyName = GetRegistryKeyName(id);
					key.SetValue(keyName, _array[i], RegistryValueKind.String);
				}

				for (int i = _capacity - 1, j = _count - _head - 1; j >= 0; i--, j--, id++)
				{
					string keyName = GetRegistryKeyName(id);
					key.SetValue(keyName, _array[i], RegistryValueKind.String);
				}

				for (int i = id; i <= _capacity; i++)
				{
					string keyName = GetRegistryKeyName(i);
					key.DeleteValue(keyName, false);
				}
			}
		}

		private RegistryKey GetRootRegistryKey()
		{
			return _useLocalMachine ? Registry.LocalMachine : Registry.CurrentUser;
		}

		private string GetRegistryKeyName(int id)
		{
			return "File" + id.ToString();
		}

		private bool EqualsItem(string x, string y)
		{
			return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region Nested types

		public struct Enumerator : IEnumerator<string>, IDisposable, IEnumerator
		{
			private int _index;
			private string _currentElement;
			private MRUList _list;

			internal Enumerator(MRUList list)
			{
				_list = list;
				_index = -1;
				_currentElement = null;
			}

			public string Current
			{
				get
				{
					if (_index < 0)
					{
						throw new InvalidOperationException();
					}

					return _currentElement;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					if (_index < 0)
					{
						throw new InvalidOperationException();
					}

					return _currentElement;
				}
			}

			void IEnumerator.Reset()
			{
				_index = -1;
				_currentElement = null;
			}

			public bool MoveNext()
			{
				if (_index == -2)
					return false;

				_index++;

				if (_index == _list._count)
				{
					_index = -2;
					_currentElement = null;
					return false;
				}

				_currentElement = _list[_index];
				return true;
			}

			public void Dispose()
			{
				_index = -2;
				_currentElement = null;
			}
		}

		#endregion
	}
}
