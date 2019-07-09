using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// Each directory table is followed by a series of directory entries, which give the name or ID for that
	/// level (Type, Name, or Language level) and an address of either a data description or another directory table.
	/// If a data description is pointed to, then the data is a leaf in the tree. If another directory table is
	/// pointed to, then that table lists directory entries at the next level down.
	/// </summary>
	public class ResourceTable : IEnumerable<ResourceEntry>, ICloneable
	{
		#region Fields

		private ushort _majorVersion;
		private ushort _minorVersion;
		private DateTime _timeDateStamp;
		private List<ResourceEntry> _list = new List<ResourceEntry>();
		private PEImage _image;
		internal ResourceTableEntry _parent;

		#endregion

		#region Ctors

		public ResourceTable()
		{
		}

		public ResourceTable(PEImage image)
		{
			_image = image;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The major version number. The major and minor version numbers can be set by the user.
		/// </summary>
		public ushort MajorVersion
		{
			get { return _majorVersion; }
			set { _majorVersion = value; }
		}

		/// <summary>
		/// The minor version number.
		/// </summary>
		public ushort MinorVersion
		{
			get { return _minorVersion; }
			set { _minorVersion = value; }
		}

		/// <summary>
		/// The time and date that the export data was created.
		/// </summary>
		public DateTime TimeDateStamp
		{
			get { return _timeDateStamp; }
		}

		public ResourceEntry this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public PEImage Image
		{
			get
			{
				if (_image != null)
					return _image;

				if (_parent != null && _parent._parent != null)
					return _parent._parent.Image;

				return null;
			}
		}

		public ResourceTableEntry Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public int IndexOf(ResourceEntry item)
		{
			return _list.IndexOf(item);
		}

		public void Add(ResourceEntry item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, ResourceEntry item)
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

		public IEnumerator<ResourceEntry> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public ResourceTable Clone()
		{
			var copy = new ResourceTable();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(ResourceTable copy)
		{
			copy._majorVersion = _majorVersion;
			copy._minorVersion = _minorVersion;
			copy._timeDateStamp = _timeDateStamp;

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

		public static ResourceTable TryLoad(PEImage image)
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

		public static ResourceTable Load(PEImage image)
		{
			if (image == null)
				return null;

			var dd = image.Directories[DataDirectories.ResourceTable];
			if (dd.IsNull)
				return null;

			using (var accessor = image.OpenImageToSectionData(dd.RVA))
			{
				var table = new ResourceTable(image);
				Load(accessor, table, accessor.Position);

				return table;
			}
		}

		internal static ResourceTable Load(IBinaryAccessor accessor, long basePosition)
		{
			var table = new ResourceTable();
			Load(accessor, table, basePosition);

			return table;
		}

		internal static unsafe void Load(IBinaryAccessor accessor, ResourceTable table, long basePosition)
		{
			ResourceTableHeader tableHeader;
			fixed (byte* pBuff = accessor.ReadBytes(sizeof(ResourceTableHeader)))
			{
				tableHeader = *(ResourceTableHeader*)pBuff;
			}

			table._majorVersion = tableHeader.MajorVersion;
			table._minorVersion = tableHeader.MinorVersion;
			table._timeDateStamp = ConvertUtils.ToDateTime(tableHeader.TimeDateStamp);

			int numberOfEntries = tableHeader.NumberOfNamedEntries + tableHeader.NumberOfIdEntries;
			for (int i = 0; i < numberOfEntries; i++)
			{
				var entry = ResourceEntry.Load(accessor, basePosition);
				entry._parent = table;
				table._list.Add(entry);
			}
		}

		#endregion
	}
}
