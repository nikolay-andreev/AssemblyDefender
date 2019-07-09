using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class TLS : ICloneable
	{
		#region Fields

		private byte[] _data;
		private bool _loadData;
		private uint _startAddressOfRawDataRVA;
		private int _length;
		private PEImage _image;

		#endregion

		#region Ctors

		public TLS()
		{
			_data = BufferUtils.EmptyArray;
		}

		#endregion

		#region Properties

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

		#endregion

		#region Methods

		public bool Contains(uint rva)
		{
			if (rva >= _startAddressOfRawDataRVA &&
				rva < _startAddressOfRawDataRVA + _length)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public TLS Clone()
		{
			TLS copy = new TLS();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(TLS copy)
		{
			copy._data = Data;
		}

		private byte[] ReadData()
		{
			if (_image == null)
				return null;

			IBinaryAccessor accessor;
			if (!_image.TryOpenImageToSectionData(_startAddressOfRawDataRVA, out accessor))
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

		public static TLS TryLoad(PEImage image)
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

		public static unsafe TLS Load(PEImage image)
		{
			if (image == null)
				return null;

			var dd = image.Directories[DataDirectories.TlsTable];
			if (dd.IsNull)
				return null;

			var tls = new TLS();
			tls._image = image;

			using (var accessor = image.OpenImageToSectionData(dd.RVA))
			{
				if (image.Is32Bits)
				{
					TLSHeader32 header;
					fixed (byte* pBuff = accessor.ReadBytes(sizeof(TLSHeader32)))
					{
						header = *(TLSHeader32*)pBuff;
					}

					uint startAddressOfRawDataRVA = (uint)(header.StartAddressOfRawData - image.ImageBase);
					uint endAddressOfRawDataRVA = (uint)(header.EndAddressOfRawData - image.ImageBase);

					tls._loadData = true;
					tls._startAddressOfRawDataRVA = startAddressOfRawDataRVA;
					tls._length = (int)(endAddressOfRawDataRVA - startAddressOfRawDataRVA);
				}
				else
				{
					TLSHeader64 header;
					fixed (byte* pBuff = accessor.ReadBytes(sizeof(TLSHeader64)))
					{
						header = *(TLSHeader64*)pBuff;
					}

					uint startAddressOfRawDataRVA = (uint)(header.StartAddressOfRawData - image.ImageBase);
					uint endAddressOfRawDataRVA = (uint)(header.EndAddressOfRawData - image.ImageBase);

					tls._loadData = true;
					tls._startAddressOfRawDataRVA = startAddressOfRawDataRVA;
					tls._length = (int)(endAddressOfRawDataRVA - startAddressOfRawDataRVA);
				}
			}

			return tls;
		}

		#endregion
	}
}
