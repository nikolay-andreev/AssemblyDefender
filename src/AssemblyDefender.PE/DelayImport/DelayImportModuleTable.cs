using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class DelayImportModuleTable : IEnumerable<DelayImportEntry>, ICloneable
	{
		#region Fields

		private uint _moduleHandleRVA;
		private string _dllName;
		private List<DelayImportEntry> _list = new List<DelayImportEntry>();
		internal DelayImportTable _parent;

		#endregion

		#region Ctors

		public DelayImportModuleTable()
		{
		}

		#endregion

		#region Properties

		public DelayImportEntry this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		/// <summary>
		/// The RVA of the module handle (in the data section of the image) of the DLL to be delay-loaded.
		/// It is used for storage by the routine that is supplied to manage delay-loading.
		/// </summary>
		public uint ModuleHandleRVA
		{
			get { return _moduleHandleRVA; }
			set { _moduleHandleRVA = value; }
		}

		/// <summary>
		/// The name of the DLL.
		/// </summary>
		public string DllName
		{
			get { return _dllName; }
			set { _dllName = value; }
		}

		public DelayImportTable Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public int IndexOf(DelayImportEntry item)
		{
			return _list.IndexOf(item);
		}

		public void Add(DelayImportEntry item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, DelayImportEntry item)
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

		public IEnumerator<DelayImportEntry> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public DelayImportModuleTable Clone()
		{
			var copy = new DelayImportModuleTable();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(DelayImportModuleTable copy)
		{
			copy._moduleHandleRVA = _moduleHandleRVA;
			copy._dllName = _dllName;

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

		internal static unsafe DelayImportModuleTable Load(IBinaryAccessor accessor, PEImage image)
		{
			DelayImportTableHeader header;
			fixed (byte* pBuff = accessor.ReadBytes(sizeof(DelayImportTableHeader)))
			{
				header = *(DelayImportTableHeader*)pBuff;
			}

			if (header.Name == 0 || header.DelayImportAddressTable == 0)
				return null;

			// Save position
			long position = accessor.Position;

			var module = new DelayImportModuleTable();

			module._moduleHandleRVA = header.ModuleHandle;

			// Dll name
			accessor.Position = image.ResolvePositionToSectionData(header.Name);
			module._dllName = accessor.ReadNullTerminatedString(Encoding.ASCII);

			// DelayImportAddressTable
			accessor.Position = image.ResolvePositionToSectionData(header.DelayImportAddressTable);

			var funcRVAList = new List<uint>();
			while (true)
			{
				uint funcRVA = accessor.ReadUInt32();
				if (funcRVA == 0)
					break;

				funcRVAList.Add(funcRVA);
			}

			accessor.Position = image.ResolvePositionToSectionData(header.DelayImportNameTable);

			if (image.Is32Bits)
			{
				var entryDataList = new List<uint>();
				for (int i = 0; i < funcRVAList.Count; i++)
				{
					entryDataList.Add(accessor.ReadUInt32());
				}

				for (int i = 0; i < funcRVAList.Count; i++)
				{
					uint funcRVA = funcRVAList[i];
					uint entryData = entryDataList[i];

					string name = null;
					int ordinal;
					if ((entryData & 0x80000000) != 0)
					{
						// DelayImport by ordinal.
						ordinal = (int)(entryData & 0x7fffffff);
					}
					else
					{
						// DelayImport by name.
						uint hintNameRVA = (entryData & 0x7fffffff);
						accessor.Position = image.ResolvePositionToSectionData(hintNameRVA);
						ordinal = accessor.ReadUInt16();
						name = accessor.ReadNullTerminatedString(Encoding.ASCII);
					}

					var entry = new DelayImportEntry(funcRVA, name, ordinal);
					entry._parent = module;
					module._list.Add(entry);
				}
			}
			else
			{
				var entryDataList = new List<ulong>();
				for (int i = 0; i < funcRVAList.Count; i++)
				{
					entryDataList.Add(accessor.ReadUInt64());
				}

				for (int i = 0; i < funcRVAList.Count; i++)
				{
					uint funcRVA = funcRVAList[i];
					ulong entryData = entryDataList[i];

					string name = null;
					int ordinal;
					if ((entryData & 0x8000000000000000) != 0)
					{
						// Import by ordinal.
						ordinal = (int)(entryData & 0x7fffffffffffffff);
					}
					else
					{
						// Import by name.
						uint hintNameRVA = (uint)(entryData & 0x7fffffffffffffff);
						accessor.Position = image.ResolvePositionToSectionData(hintNameRVA);
						ordinal = accessor.ReadUInt16();
						name = accessor.ReadNullTerminatedString(Encoding.ASCII);
					}

					var entry = new DelayImportEntry(funcRVA, name, ordinal);
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
