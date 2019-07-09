using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// Attribute certificates can be associated with an image by adding an attribute certificate table.
	/// </summary>
	public class CertificateTable : IEnumerable<CertificateEntry>, ICloneable
	{
		#region Fields

		private List<CertificateEntry> _list = new List<CertificateEntry>();

		#endregion

		#region Ctors

		public CertificateTable()
		{
		}

		#endregion

		#region Properties

		public CertificateEntry this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int IndexOf(CertificateEntry item)
		{
			return _list.IndexOf(item);
		}

		public void Add(CertificateEntry item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, CertificateEntry item)
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

		public IEnumerator<CertificateEntry> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public CertificateTable Clone()
		{
			CertificateTable copy = new CertificateTable();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(CertificateTable copy)
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

		public static CertificateTable TryLoad(PEImage image)
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

		public static CertificateTable Load(PEImage image)
		{
			if (image == null)
				return null;

			var dd = image.Directories[DataDirectories.CertificateTable];
			if (dd.IsNull)
				return null;

			var table = new CertificateTable();

			using (var accessor = image.OpenImage((long)dd.RVA))
			{
				long endOffset = accessor.Position + dd.Size;
				while (accessor.Position < endOffset)
				{
					var entry = CertificateEntry.Load(accessor);
					entry._parent = table;
					table._list.Add(entry);

					accessor.Align(8);
				}
			}

			return table;
		}

		#endregion
	}
}
