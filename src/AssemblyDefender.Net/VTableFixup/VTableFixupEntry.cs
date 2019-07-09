using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Fixup entry
	/// </summary>
	public class VTableFixupEntry : IEnumerable<int>, ICloneable
	{
		#region Fields

		private VTableFixupType _type;
		private List<int> _list = new List<int>();
		internal VTableFixupTable _parent;

		#endregion

		#region Ctors

		public VTableFixupEntry()
		{
		}

		#endregion

		#region Properties

		public int this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public VTableFixupType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public VTableFixupTable Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public int IndexOf(int token)
		{
			return _list.IndexOf(token);
		}

		public void Add(int token)
		{
			_list.Add(token);
		}

		public void Insert(int index, int token)
		{
			_list.Insert(index, token);
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		public void Clear()
		{
			_list.Clear();
		}

		public IEnumerator<int> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public VTableFixupEntry Clone()
		{
			var copy = new VTableFixupEntry();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(VTableFixupEntry copy)
		{
			copy._type = _type;

			foreach (var childNode in _list)
			{
				copy._list.Add(childNode);
			}
		}

		#endregion

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		#region Static

		internal static VTableFixupEntry Load(IBinaryAccessor accessor, PEImage pe)
		{
			var fixup = new VTableFixupEntry();

			// In this definition, RVA points to the location of the v-table slot containing the method token(s).
			uint rva = accessor.ReadUInt32();

			// Indicating the number of v-table slots grouped into one entry because their flags are identical.
			// This grouping has no effect other than saving some space�you can emit a single slot per entry,
			// but then you�ll have to emit as many v-table fixups as there are slots.
			int count = accessor.ReadUInt16();

			fixup._type = (VTableFixupType)accessor.ReadUInt16();

			bool is32Bits = ((fixup.Type & VTableFixupType.SlotSize32Bit) == VTableFixupType.SlotSize32Bit);

			// Save position
			long position = accessor.Position;

			accessor.Position = pe.ResolvePositionToSectionData(rva);
			for (int i = 0; i < count; i++)
			{
				fixup._list.Add(accessor.ReadInt32());

				if (!is32Bits)
				{
					accessor.ReadUInt32();
				}
			}

			// Restore position
			accessor.Position = position;

			return fixup;
		}

		#endregion
	}
}
