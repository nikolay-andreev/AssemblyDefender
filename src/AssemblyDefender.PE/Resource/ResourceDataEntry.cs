using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class ResourceDataEntry : ResourceEntry, ICloneable
	{
		#region Fields

		private byte[] _data;
		private bool _loadData;
		private uint _codePage;
		private uint _rva;
		private int _length;

		#endregion

		#region Ctors

		public ResourceDataEntry()
		{
			_data = BufferUtils.EmptyArray;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Address of a unit of resource data in the Resource Data area.
		/// </summary>
		public byte[] Data
		{
			get
			{
				if (_loadData)
				{
					return ReadData() ?? new byte[_length];
				}

				return _data;
			}
			set
			{
				_loadData = false;
				_data = value ?? BufferUtils.EmptyArray;
			}
		}

		public int Length
		{
			get
			{
				if (_loadData)
					return _length;
				else if (_data != null)
					return _data.Length;
				else
					return 0;
			}
		}

		/// <summary>
		/// Code page used to decode code point values within the resource data.
		/// Typically, the code page would be the Unicode code page.
		/// </summary>
		public uint CodePage
		{
			get { return _codePage; }
			set { _codePage = value; }
		}

		public uint RVA
		{
			get { return _rva; }
		}

		#endregion

		#region Methods

		public new ResourceDataEntry Clone()
		{
			var copy = new ResourceDataEntry();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(ResourceDataEntry copy)
		{
			copy._data = Data;
			copy._codePage = _codePage;

			base.CopyTo(copy);
		}

		private byte[] ReadData()
		{
			if (Parent == null)
				return null;

			PEImage image = Parent.Image;
			if (image == null)
				return null;

			long position;
			if (!image.ResolvePositionToSectionData(_rva, out position))
				return null;

			IBinaryAccessor accessor;
			if (!image.TryOpenImageToSectionData(_rva, out accessor))
				return null;

			using (accessor)
			{
				return accessor.ReadBytes(_length);
			}
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		#region Static

		internal static ResourceDataEntry Load(IBinaryAccessor accessor)
		{
			ResourceDataEntry entry = new ResourceDataEntry();
			entry._loadData = true;
			entry._rva = accessor.ReadUInt32();
			entry._length = accessor.ReadInt32();
			entry._codePage = accessor.ReadUInt32();
			accessor.ReadUInt32();

			return entry;
		}

		#endregion
	}
}
