using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The Import Table and the Import Address Table (IAT) are used to import the _CorExeMain (for a .exe) or
	/// _CorDllMain (for a .dll) entries of the runtime engine (mscoree.dll). The Import Table directory entry
	/// points to a one element zero terminated array of Import Directory entries (in a general PE file there is
	/// one entry for each imported DLL).
	/// </summary>
	public class ImportTable : IEnumerable<ImportModuleTable>, ICloneable
	{
		#region Fields

		private List<ImportModuleTable> _list = new List<ImportModuleTable>();

		#endregion

		#region Ctors

		public ImportTable()
		{
		}

		#endregion

		#region Properties

		public ImportModuleTable this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int IndexOf(ImportModuleTable item)
		{
			return _list.IndexOf(item);
		}

		public void Add(ImportModuleTable item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, ImportModuleTable item)
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

		public IEnumerator<ImportModuleTable> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public ImportTable Clone()
		{
			ImportTable copy = new ImportTable();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(ImportTable copy)
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

		public static ImportTable TryLoad(PEImage image)
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

		public static ImportTable Load(PEImage image)
		{
			if (image == null)
				return null;

			var dd = image.Directories[DataDirectories.ImportTable];
			if (dd.IsNull)
				return null;

			var table = new ImportTable();

			using (var accessor = image.OpenImageToSectionData(dd.RVA))
			{
				Load(accessor, image, table);
			}

			return table;
		}

		private static void Load(IBinaryAccessor accessor, PEImage image, ImportTable table)
		{
			while (true)
			{
				var module = ImportModuleTable.Load(accessor, image);
				if (module == null)
					break;

				module._parent = table;
				table._list.Add(module);
			}
		}

		#endregion
	}
}
