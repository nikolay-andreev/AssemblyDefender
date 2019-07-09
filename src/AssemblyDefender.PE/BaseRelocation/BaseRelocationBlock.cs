using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The base relocation table is divided into blocks. Each block represents the base relocations for a 4K page.
	/// Each block must start on a 32-bit boundary.
	/// </summary>
	public class BaseRelocationBlock : IEnumerable<BaseRelocationEntry>, ICloneable
	{
		#region Fields

		private uint _pageRVA;
		private List<BaseRelocationEntry> _list = new List<BaseRelocationEntry>();
		internal BaseRelocationTable _parent;

		#endregion

		#region Ctors

		public BaseRelocationBlock()
		{
		}

		#endregion

		#region Properties

		public BaseRelocationEntry this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		/// <summary>
		/// The image base plus the page RVA is added to each offset to create the VA where the base relocation
		/// must be applied.
		/// </summary>
		public uint PageRVA
		{
			get { return _pageRVA; }
			set { _pageRVA = value; }
		}

		public BaseRelocationTable Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public int IndexOf(BaseRelocationEntry item)
		{
			return _list.IndexOf(item);
		}

		public void Add(BaseRelocationEntry item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, BaseRelocationEntry item)
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

		public IEnumerator<BaseRelocationEntry> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public BaseRelocationBlock Clone()
		{
			BaseRelocationBlock copy = new BaseRelocationBlock();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(BaseRelocationBlock copy)
		{
			copy._pageRVA = _pageRVA;

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

		internal static BaseRelocationBlock Load(IBinaryAccessor accessor)
		{
			var block = new BaseRelocationBlock();

			block.PageRVA = accessor.ReadUInt32();

			// The total number of bytes in the base relocation block, including the Page RVA and Block Size fields and
			// the Type/Offset fields that follow.
			int blockSize = accessor.ReadInt32();

			int count = (blockSize - 8) / 2;
			for (int i = 0; i < count; i++)
			{
				ushort value = accessor.ReadUInt16();
				if (value == 0) // if necessary, insert 2 bytes of 0 to pad to a multiple of 4 bytes in length.
					continue;

				var type = (BaseRelocationType)(value >> 12);
				uint offset = (uint)(value & 0x0fff);

				var entry = new BaseRelocationEntry(type, offset);
				entry._parent = block;
				block._list.Add(entry);
			}

			return block;
		}

		#endregion
	}
}
