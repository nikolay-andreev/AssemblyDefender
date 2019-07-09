using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// Represents imported methods from one module.
	/// </summary>
	public class ImportModuleTable : IEnumerable<ImportEntry>, ICloneable
	{
		#region Fields

		private string _dllName;
		private int _forwarderChain;
		private List<ImportEntry> _list = new List<ImportEntry>();
		internal ImportTable _parent;

		#endregion

		#region Ctors

		public ImportModuleTable()
		{
		}

		#endregion

		#region Properties

		public ImportEntry this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		/// <summary>
		/// The name of the DLL.
		/// </summary>
		public string DllName
		{
			get { return _dllName; }
			set { _dllName = value; }
		}

		/// <summary>
		/// The index of the first forwarder reference. -1 if no forwarders
		/// </summary>
		public int ForwarderChain
		{
			get { return _forwarderChain; }
			set { _forwarderChain = value; }
		}

		public ImportTable Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public int IndexOf(ImportEntry item)
		{
			return _list.IndexOf(item);
		}

		public void Add(ImportEntry item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, ImportEntry item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		public void Clear()
		{
			_list.Clear();
		}

		public IEnumerator<ImportEntry> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public ImportModuleTable Clone()
		{
			var copy = new ImportModuleTable();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(ImportModuleTable copy)
		{
			copy._dllName = _dllName;
			copy._forwarderChain = _forwarderChain;

			foreach (var childNode in _list)
			{
				var copyChildNode = childNode.Clone();
				copy._list.Add(copyChildNode);
				copyChildNode._parent = this;
			}
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		#region Static

		internal static unsafe ImportModuleTable Load(IBinaryAccessor accessor, PEImage image)
		{
			ImportTableHeader header;
			fixed (byte* pBuff = accessor.ReadBytes(sizeof(ImportTableHeader)))
			{
				header = *(ImportTableHeader*)pBuff;
			}

			if (header.Name == 0 || (header.ImportLookupTableRVA == 0 && header.ImportAddressTableRVA == 0))
				return null;

			// Save position
			long position = accessor.Position;

			var module = new ImportModuleTable();
			module._forwarderChain = header.ForwarderChain;

			// Dll name
			accessor.Position = image.ResolvePositionToSectionData(header.Name);
			module._dllName = accessor.ReadNullTerminatedString(Encoding.ASCII);

			// Set RVA to ImportLookupTable or ImportAddressTable. Both tables are equivalent.
			if (header.ImportLookupTableRVA != 0)
				accessor.Position = image.ResolvePositionToSectionData(header.ImportLookupTableRVA);
			else
				accessor.Position = image.ResolvePositionToSectionData(header.ImportAddressTableRVA);

			if (image.Is32Bits)
			{
				// Read IMAGE_THUNK_DATA32 structures
				var thunkDataList = new List<uint>();
				while (true)
				{
					uint entryData = accessor.ReadUInt32();
					if (entryData == 0)
						break;

					thunkDataList.Add(entryData);
				}

				foreach (uint thunkData in thunkDataList)
				{
					string name = null;
					int ordinal;
					if ((thunkData & 0x80000000) != 0)
					{
						// Import by ordinal.
						ordinal = (int)(thunkData & 0x7fffffff);
					}
					else
					{
						// Import by name.
						uint hintNameRVA = (thunkData & 0x7fffffff);
						accessor.Position = image.ResolvePositionToSectionData(hintNameRVA);
						ordinal = accessor.ReadUInt16();
						name = accessor.ReadNullTerminatedString(Encoding.ASCII);
					}

					var entry = new ImportEntry(name, ordinal);
					entry._parent = module;
					module._list.Add(entry);
				}
			}
			else
			{
				// Read IMAGE_THUNK_DATA64 structures
				var thunkDataList = new List<ulong>();
				while (true)
				{
					ulong entryData = accessor.ReadUInt64();
					if (entryData == 0)
						break;

					thunkDataList.Add(entryData);
				}

				foreach (ulong thunkData in thunkDataList)
				{
					string name = null;
					int ordinal;
					if ((thunkData & 0x8000000000000000) != 0)
					{
						// Import by ordinal.
						ordinal = (int)(thunkData & 0x7fffffffffffffff);
					}
					else
					{
						// Import by name.
						uint hintNameRVA = (uint)(thunkData & 0x7fffffffffffffff);
						accessor.Position = image.ResolvePositionToSectionData(hintNameRVA);
						ordinal = accessor.ReadUInt16();
						name = accessor.ReadNullTerminatedString(Encoding.ASCII);
					}

					var entry = new ImportEntry(name, ordinal);
					entry._parent = module;
					module._list.Add(entry);
				}
			}

			// Restore position
			accessor.Position = position;

			return module;
		}

		#endregion
	}
}
