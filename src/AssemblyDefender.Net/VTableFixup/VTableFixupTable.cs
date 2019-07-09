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
	/// The v-table and the VTableFixup table of a managed module serve two purposes. One purpose�relevant only
	/// to the VC++ compiler, the only compiler that produces mixed-code modules�is to provide the intramodule
	/// managed/unmanaged code interoperation. Another purpose is to provide the means for the unmanaged export
	/// of managed methods.
	/// </summary>
	public class VTableFixupTable : IEnumerable<VTableFixupEntry>, ICloneable
	{
		#region Fields

		private List<VTableFixupEntry> _list = new List<VTableFixupEntry>();

		#endregion

		#region Ctors

		public VTableFixupTable()
		{
		}

		#endregion

		#region Properties

		public VTableFixupEntry this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int IndexOf(VTableFixupEntry item)
		{
			return _list.IndexOf(item);
		}

		public void Add(VTableFixupEntry item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, VTableFixupEntry item)
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

		public IEnumerator<VTableFixupEntry> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public VTableFixupTable Clone()
		{
			var copy = new VTableFixupTable();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(VTableFixupTable copy)
		{
			foreach (var childNode in _list)
			{
				var copyChildNode = childNode.Clone();
				copy._list.Add(copyChildNode);
				copyChildNode._parent = this;
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

		public static VTableFixupTable TryLoad(PEImage image)
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

		public static unsafe VTableFixupTable Load(PEImage pe)
		{
			var dd = pe.Directories[DataDirectories.CLIHeader];
			if (dd.IsNull)
				return null;

			using (var accessor = pe.OpenImageToSectionData(dd.RVA))
			{
				CorHeader corHeader;
				fixed (byte* pBuff = accessor.ReadBytes(sizeof(CorHeader)))
				{
					corHeader = *(CorHeader*)pBuff;
				}

				if (corHeader.Cb != MetadataConstants.CorHeaderSignature)
				{
					throw new BadImageFormatException(string.Format(SR.MetadataImageNotValid, pe.Location ?? ""));
				}

				dd = corHeader.VTableFixups;
				if (dd.IsNull)
					return null;

				accessor.Position = pe.ResolvePositionToSectionData(dd.RVA);
				return Load(accessor, pe, dd.Size);
			}
		}

		public static VTableFixupTable Load(PEImage pe, DataDirectory dd)
		{
			if (pe == null || dd.IsNull)
				return null;

			using (var accessor = pe.OpenImageToSectionData(dd.RVA))
			{
				return Load(accessor, pe, dd.Size);
			}
		}

		private static VTableFixupTable Load(IBinaryAccessor accessor, PEImage pe, int size)
		{
			var table = new VTableFixupTable();

			long endOffset = accessor.Position + size;
			while (accessor.Position < endOffset)
			{
				var entry = VTableFixupEntry.Load(accessor, pe);
				entry._parent = table;
				table._list.Add(entry);
			}

			return table;
		}

		#endregion
	}
}
