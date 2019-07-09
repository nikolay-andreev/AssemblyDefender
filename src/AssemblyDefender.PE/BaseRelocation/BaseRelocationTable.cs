using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The base relocation table contains entries for all base relocations in the image.
	/// When a field hold VA rather than RVA, you need to define the base relocation for it so the
	/// loader fix the field with the correct ImageBase (note that ImageBase set in PE is only recommended).
	/// </summary>
	public class BaseRelocationTable : IEnumerable<BaseRelocationBlock>, ICloneable
	{
		#region Fields

		private List<BaseRelocationBlock> _list = new List<BaseRelocationBlock>();

		#endregion

		#region Ctors

		public BaseRelocationTable()
		{
		}

		#endregion

		#region Properties

		public BaseRelocationBlock this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int IndexOf(BaseRelocationBlock item)
		{
			return _list.IndexOf(item);
		}

		public void Add(BaseRelocationBlock item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, BaseRelocationBlock item)
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

		public IEnumerator<BaseRelocationBlock> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public BaseRelocationTable Clone()
		{
			var copy = new BaseRelocationTable();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(BaseRelocationTable copy)
		{
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

		public static BaseRelocationTable TryLoad(PEImage image)
		{
			try
			{
				return Load(image);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static BaseRelocationTable Load(PEImage image)
		{
			if (image == null)
				return null;

			var dd = image.Directories[DataDirectories.BaseRelocationTable];
			if (dd.IsNull)
				return null;

			var table = new BaseRelocationTable();

			using (var accessor = image.OpenImageToSectionData(dd.RVA))
			{
				long endOffset = accessor.Position + dd.Size;

				while (accessor.Position < endOffset)
				{
					var block = BaseRelocationBlock.Load(accessor);
					block._parent = table;
					table._list.Add(block);

					// Each block must start on a 32-bit boundary.
					accessor.Align(4);
				}
			}

			return table;
		}

		#endregion
	}
}
