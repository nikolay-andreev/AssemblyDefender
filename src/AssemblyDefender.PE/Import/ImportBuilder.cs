using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public class ImportBuilder : BuildTask
	{
		#region Fields

		private string _sectionName = PESectionNames.IData;
		private string _iatSectionName = PESectionNames.Text;
		private int _blobPriority = 1000;
		private int _iatBlobPriority = 1000;
		private ImportTable _table;
		private BuildBlob _blob;
		private BuildBlob _iatBlob;

		#endregion

		#region Properties

		public string SectionName
		{
			get { return _sectionName; }
			set { _sectionName = value; }
		}

		public string IATSectionName
		{
			get { return _iatSectionName; }
			set { _iatSectionName = value; }
		}

		public int BlobPriority
		{
			get { return _blobPriority; }
			set { _blobPriority = value; }
		}

		public int IATBlobPriority
		{
			get { return _iatBlobPriority; }
			set { _iatBlobPriority = value; }
		}

		public ImportTable Table
		{
			get { return _table; }
			set { _table = value; }
		}

		public BuildBlob Blob
		{
			get { return _blob; }
			set { _blob = value; }
		}

		public BuildBlob IATBlob
		{
			get { return _iatBlob; }
			set { _iatBlob = value; }
		}

		#endregion

		#region Methods

		public override void Build()
		{
			if (_table == null || _table.Count == 0)
				return;

			_blob = new BuildBlob();
			_iatBlob = new BuildBlob();

			// Calculate
			int lookupTableSize = 0;
			int hintNameTableSize = 0;
			int lookupEntrySize = PE.Is32Bits ? 4 : 8;
			for (int i = 0; i < _table.Count; i++)
			{
				var module = _table[i];

				for (int j = 0; j < module.Count; j++)
				{
					var entry = module[j];

					lookupTableSize += lookupEntrySize;

					if (!string.IsNullOrEmpty(entry.Name))
					{
						hintNameTableSize += 2; // hint
						hintNameTableSize += entry.Name.Length + 1;
					}
				}

				lookupTableSize += lookupEntrySize; // null
			}

			int iatPos = 0;
			int lookupTablePos = (_table.Count + 1) * 20; // header + null
			int hintNameTablePos = lookupTablePos + lookupTableSize;
			int dllNamePos = hintNameTablePos + hintNameTableSize;

			// Write
			int headerPos = 0;
			for (int i = 0; i < _table.Count; i++)
			{
				var module = _table[i];

				// Header
				PE.Fixups.Add(new WriteRVAFixup(_blob, headerPos, lookupTablePos));
				_blob.Write(ref headerPos, (uint)0); // ImportLookupTableRVA

				_blob.Write(ref headerPos, (uint)0); // TimeDateStamp

				_blob.Write(ref headerPos, (int)module.ForwarderChain); // ForwarderChain

				PE.Fixups.Add(new WriteRVAFixup(_blob, headerPos, dllNamePos));
				_blob.Write(ref headerPos, (uint)0); // Name

				PE.Fixups.Add(new WriteRVAFixup(_blob, _iatBlob, headerPos, iatPos));
				_blob.Write(ref headerPos, (uint)0); // ImportAddressTableRVA

				// DllName
				string dllName = (module.DllName ?? "") + '\0';
				_blob.Write(ref dllNamePos, dllName, Encoding.ASCII);

				// ImportLookupTable / ImportAddressTable
				for (int j = 0; j < module.Count; j++)
				{
					var entry = module[j];

					if (PE.Is32Bits)
					{
						if (!string.IsNullOrEmpty(entry.Name))
						{
							// Import by name.
							PE.Fixups.Add(
								new WriteHintNameRVAFixup(
									_blob, _iatBlob, lookupTablePos, iatPos, hintNameTablePos));
							_blob.Write(ref lookupTablePos, (uint)0);
							_iatBlob.Write(ref iatPos, (uint)0);

							// Hint/Name
							_blob.Write(ref hintNameTablePos, (ushort)entry.Ordinal);
							string name = entry.Name + '\0';
							_blob.Write(ref hintNameTablePos, name, Encoding.ASCII);
						}
						else
						{
							// Import by ordinal.
							uint ordinal = (uint)entry.Ordinal | 0x80000000;
							_blob.Write(ref lookupTablePos, (uint)ordinal);
							_iatBlob.Write(ref iatPos, (uint)ordinal);
						}
					}
					else
					{
						if (!string.IsNullOrEmpty(entry.Name))
						{
							// Import by name.
							PE.Fixups.Add(
								new WriteHintNameRVAFixup(
									_blob, _iatBlob, lookupTablePos, iatPos, hintNameTablePos));
							_blob.Write(ref lookupTablePos, (ulong)0);
							_iatBlob.Write(ref iatPos, (ulong)0);

							// Hint/Name
							_blob.Write(ref hintNameTablePos, (ushort)entry.Ordinal);
							string name = entry.Name + '\0';
							_blob.Write(ref hintNameTablePos, name, Encoding.ASCII);
						}
						else
						{
							// Import by ordinal.
							ulong ordinal = (uint)entry.Ordinal | 0x8000000000000000;
							_blob.Write(ref lookupTablePos, (ulong)ordinal);
							_iatBlob.Write(ref iatPos, (ulong)ordinal);
						}
					}
				}

				if (PE.Is32Bits)
				{
					// Null ImportLookupTable / ImportAddressTable
					_blob.Write(ref lookupTablePos, 0, 4);
					_iatBlob.Write(ref iatPos, 0, 4);
				}
				else
				{
					// Null ImportLookupTable / ImportAddressTable
					_blob.Write(ref lookupTablePos, 0, 8);
					_iatBlob.Write(ref iatPos, 0, 8);
				}
			}

			// Null header
			_blob.Write(ref headerPos, 0, 20);

			// Set data directories
			PE.Fixups.Add(
				new PEBuilder.SetDataDirectoryFromBlobRVAFixup(
					DataDirectories.ImportTable, _blob));

			PE.Fixups.Add(
				new PEBuilder.SetDataDirectoryFromBlobRVAFixup(
					DataDirectories.IAT, _iatBlob));

			// Add _blobs
			BuildSection section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);

			BuildSection iatSection = PE.GetSection(_iatSectionName);
			iatSection.Blobs.Add(_iatBlob, _iatBlobPriority);
		}

		public int GetIATOffset(ImportEntry searchEntry)
		{
			int sizeOfEntry = PE.Is32Bits ? 4 : 8;

			int offset = 0;
			foreach (var module in _table)
			{
				foreach (var entry in module)
				{
					if (object.ReferenceEquals(entry, searchEntry))
					{
						return offset;
					}

					offset += sizeOfEntry;
				}

				offset += sizeOfEntry; // Null
			}

			throw new InvalidOperationException();
		}

		public ImportTable GetOrCreateTable()
		{
			if (_table == null)
			{
				_table = new ImportTable();
			}

			return _table;
		}

		#endregion

		#region Nested types

		private class WriteRVAFixup : BuildFixup
		{
			private BuildBlob _blob;
			private BuildBlob _rvaBlob;
			private int _pos;
			private int _offset;

			public WriteRVAFixup(BuildBlob blob, int pos, int offset)
				: this(blob, blob, pos, offset)
			{
			}

			public WriteRVAFixup(BuildBlob blob, BuildBlob rvaBlob, int pos, int offset)
			{
				_blob = blob;
				_rvaBlob = rvaBlob;
				_pos = pos;
				_offset = offset;
			}

			public override void ApplyFixup()
			{
				uint rva = (uint)(_rvaBlob.RVA + _offset);
				_blob.Write(ref _pos, rva);
			}
		}

		private class WriteHintNameRVAFixup : BuildFixup
		{
			private BuildBlob _importBlob;
			private BuildBlob _iatBlob;
			private int _importPos;
			private int _iatPos;
			private int _hintNameTablePos;

			public WriteHintNameRVAFixup(BuildBlob importBlob, BuildBlob iatBlob,
				int importPos, int iatPos, int hintNameTablePos)
			{
				_importBlob = importBlob;
				_iatBlob = iatBlob;
				_importPos = importPos;
				_iatPos = iatPos;
				_hintNameTablePos = hintNameTablePos;
			}

			public override void ApplyFixup()
			{
				if (PE.Is32Bits)
				{
					uint hintNameRVA = (uint)((_importBlob.RVA + _hintNameTablePos) & 0x7fffffff);
					_importBlob.Write(ref _importPos, (uint)hintNameRVA);
					_iatBlob.Write(ref _iatPos, (uint)hintNameRVA);
				}
				else
				{
					ulong hintNameRVA = (ulong)((_importBlob.RVA + _hintNameTablePos) & 0x7fffffffffffffff);
					_importBlob.Write(ref _importPos, (ulong)hintNameRVA);
					_iatBlob.Write(ref _iatPos, (ulong)hintNameRVA);
				}
			}
		}

		#endregion
	}
}
