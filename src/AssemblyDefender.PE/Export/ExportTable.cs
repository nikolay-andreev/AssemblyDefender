using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class ExportTable : IEnumerable<ExportEntry>, ICloneable
	{
		#region Fields

		private string _dllName;
		private ushort _majorVersion;
		private ushort _minorVersion;
		private int _ordinalBase = 1;
		private DateTime _timeDateStamp;
		private List<ExportEntry> _list = new List<ExportEntry>();

		#endregion

		#region Properties

		/// <summary>
		/// The name of the DLL.
		/// </summary>
		public string DllName
		{
			get { return _dllName; }
			set { _dllName = value; }
		}

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
		/// The starting ordinal number for exports in this image. This field specifies the starting ordinal number
		/// for the export address table. It is usually set to 1.
		/// </summary>
		public int OrdinalBase
		{
			get { return _ordinalBase; }
			set { _ordinalBase = value; }
		}

		/// <summary>
		/// The time and date that the export data was created.
		/// </summary>
		public DateTime TimeDateStamp
		{
			get { return _timeDateStamp; }
		}

		public ExportEntry this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int IndexOf(ExportEntry item)
		{
			return _list.IndexOf(item);
		}

		public void Add(ExportEntry item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item._parent = this;
			_list.Add(item);
		}

		public void Insert(int index, ExportEntry item)
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

		public IEnumerator<ExportEntry> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public ExportTable Clone()
		{
			var copy = new ExportTable();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(ExportTable copy)
		{
			copy._dllName = _dllName;
			copy._majorVersion = _majorVersion;
			copy._minorVersion = _minorVersion;

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

		public static ExportTable TryLoad(PEImage image)
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

		public static ExportTable Load(PEImage image)
		{
			if (image == null)
				return null;

			var dd = image.Directories[DataDirectories.ExportTable];
			if (dd.IsNull)
				return null;

			var table = new ExportTable();
			using (var accessor = image.OpenImageToSectionData(dd.RVA))
			{
				Load(accessor, image, table);
			}

			return table;
		}

		private static unsafe void Load(IBinaryAccessor accessor, PEImage image, ExportTable table)
		{
			ExportTableHeader header;
			fixed (byte* pBuff = accessor.ReadBytes(sizeof(ExportTableHeader)))
			{
				header = *(ExportTableHeader*)pBuff;
			}

			table._ordinalBase = (int)header.Base;
			table._timeDateStamp = ConvertUtils.ToDateTime(header.TimeDateStamp);

			// Name
			accessor.Position = image.ResolvePositionToSectionData(header.Name);
			table._dllName = accessor.ReadNullTerminatedString(Encoding.ASCII);

			if (header.AddressOfFunctions != 0)
			{
				// Export Address Table
				accessor.Position = image.ResolvePositionToSectionData(header.AddressOfFunctions);
				uint[] arrayOfExportRVA = new uint[header.NumberOfFunctions];
				for (int i = 0; i < header.NumberOfFunctions; i++)
				{
					arrayOfExportRVA[i] = accessor.ReadUInt32();
				}

				// Name pointer table
				accessor.Position = image.ResolvePositionToSectionData(header.AddressOfNames);
				uint[] arrayOfNameRVA = new uint[header.NumberOfNames];
				for (int i = 0; i < header.NumberOfNames; i++)
				{
					arrayOfNameRVA[i] = accessor.ReadUInt32();
				}

				// Ordinal table
				accessor.Position = image.ResolvePositionToSectionData(header.AddressOfNameOrdinals);
				ushort[] arrayOfOrdinals = new ushort[header.NumberOfNames];
				for (int i = 0; i < header.NumberOfNames; i++)
				{
					arrayOfOrdinals[i] = accessor.ReadUInt16();
				}

				// Read names and map against export rva
				string[] names = new string[header.NumberOfFunctions];
				for (int i = 0; i < header.NumberOfNames; i++)
				{
					accessor.Position = image.ResolvePositionToSectionData(arrayOfNameRVA[i]);
					string name = accessor.ReadNullTerminatedString(Encoding.ASCII);

					int ordinal = arrayOfOrdinals[i];
					names[ordinal] = name;
				}

				var exportDirectory = image.Directories[DataDirectories.ExportTable];

				// Build entries
				for (int i = 0; i < header.NumberOfFunctions; i++)
				{
					uint exportRVA = arrayOfExportRVA[i];
					string name = names[i];

					ExportEntry entry;

					// Each entry in the export address table is a field that uses one of two formats in the
					// following table. If the address specified is not within the export section (as defined
					// by the address and length that are indicated in the optional header), the field is an
					// export RVA, which is an actual address in code or data. Otherwise, the field is a
					// forwarder RVA, which names a symbol in another DLL.
					if (exportDirectory.Contains(exportRVA))
					{
						accessor.Position = image.ResolvePositionToSectionData(exportRVA);
						string forwarder = accessor.ReadNullTerminatedString(Encoding.ASCII);
						entry = new ExportForwarderEntry(name, forwarder);
					}
					else
					{
						entry = new ExportRVAEntry(name, exportRVA);
					}

					entry._parent = table;
					table._list.Add(entry);
				}
			}
		}

		#endregion
	}
}
