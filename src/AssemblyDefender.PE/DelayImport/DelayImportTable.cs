using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// These tables were added to the image to support a uniform mechanism for applications to delay the loading of a
	/// DLL until the first call into that DLL. The layout of the tables matches that of the traditional import tables.
	/// </summary>
	public class DelayImportTable : IEnumerable<DelayImportModuleTable>, ICloneable
	{
		#region Fields

		private List<DelayImportModuleTable> _list = new List<DelayImportModuleTable>();

		#endregion

		#region Ctors

		public DelayImportTable()
		{
		}

		#endregion

		#region Properties

		public DelayImportModuleTable this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int IndexOf(DelayImportModuleTable item)
		{
			return _list.IndexOf(item);
		}

		public void Add(DelayImportModuleTable item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, DelayImportModuleTable item)
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

		public IEnumerator<DelayImportModuleTable> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public DelayImportTable Clone()
		{
			var copy = new DelayImportTable();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(DelayImportTable copy)
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

		public static DelayImportTable TryLoad(PEImage image)
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

		public static DelayImportTable Load(PEImage image)
		{
			if (image == null)
				return null;

			var dd = image.Directories[DataDirectories.DelayImportDescriptor];
			if (dd.IsNull)
				return null;

			var table = new DelayImportTable();

			using (var accessor = image.OpenImageToSectionData(dd.RVA))
			{
				Load(accessor, image, table);
			}

			return table;
		}

		private static void Load(IBinaryAccessor accessor, PEImage image, DelayImportTable table)
		{
			while (true)
			{
				var module = DelayImportModuleTable.Load(accessor, image);
				if (module == null)
					break;

				module._parent = table;
				table._list.Add(module);
			}
		}

		#endregion
	}
}
