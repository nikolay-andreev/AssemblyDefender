using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class DelayImportBuilder : BuildTask
	{
		#region Fields

		private string _sectionName = PESectionNames.IData;
		private int _blobPriority = 1000;
		private DelayImportTable _table;
		private BuildBlob _blob;

		#endregion

		#region Properties

		public string SectionName
		{
			get { return _sectionName; }
			set { _sectionName = value; }
		}

		public int BlobPriority
		{
			get { return _blobPriority; }
			set { _blobPriority = value; }
		}

		public DelayImportTable Table
		{
			get { return _table; }
			set { _table = value; }
		}

		public BuildBlob Blob
		{
			get { return _blob; }
			set { _blob = value; }
		}

		#endregion

		#region Methods

		public override void Build()
		{
			if (_table == null || _table.Count == 0)
				return;

			_blob = new BuildBlob();

			// Calculate
			int iatSize = 0;
			int namePointerTableSize = 0;
			int namePointerSize = PE.Is32Bits ? 4 : 8;
			int hintNameTableSize = 0;

			for (int i = 0; i < _table.Count; i++)
			{
				var module = _table[i];

				for (int j = 0; j < module.Count; j++)
				{
					var entry = module[j];

					iatSize += 4;
					namePointerTableSize += namePointerSize;

					if (!string.IsNullOrEmpty(entry.Name))
					{
						hintNameTableSize += 2; // hint
						hintNameTableSize += entry.Name.Length + 1;
					}
				}

				iatSize += 4; // null IAT
				namePointerTableSize += namePointerSize; // null
			}

			int iatPos = (_table.Count + 1) * 32; // header + null;
			int namePointerTablePos = iatPos + iatSize;
			int hintNameTablePos = namePointerTablePos + namePointerTableSize;
			int dllNamePos = hintNameTablePos + hintNameTableSize;

			// Write
			int headerPos = 0;
			for (int i = 0; i < _table.Count; i++)
			{
				var module = _table[i];

				// Header
				_blob.Write(ref headerPos, (uint)0); // Attributes

				PE.Fixups.Add(new WriteRVAFixup(_blob, headerPos, dllNamePos));
				_blob.Write(ref headerPos, (uint)0); // Name

				_blob.Write(ref headerPos, (uint)module.ModuleHandleRVA); // ModuleHandle

				PE.Fixups.Add(new WriteRVAFixup(_blob, headerPos, iatPos));
				_blob.Write(ref headerPos, (uint)0); // DelayImportAddressTable

				PE.Fixups.Add(new WriteRVAFixup(_blob, headerPos, namePointerTablePos));
				_blob.Write(ref headerPos, (uint)0); // DelayImportNameTable
				_blob.Write(ref headerPos, (uint)0); // BoundDelayImportTable
				_blob.Write(ref headerPos, (uint)0); // UnloadDelayImportTable
				_blob.Write(ref headerPos, (uint)0); // TimeDateStamp

				// DllName
				string dllName = (module.DllName ?? "") + '\0';
				_blob.Write(ref dllNamePos, dllName, Encoding.ASCII);

				// DelayImportNameTable
				for (int j = 0; j < module.Count; j++)
				{
					var entry = module[j];

					_blob.Write(ref iatPos, (uint)entry.FuncRVA);

					if (PE.Is32Bits)
					{
						if (!string.IsNullOrEmpty(entry.Name))
						{
							// DelayImport by name.
							PE.Fixups.Add(
								new WriteHintNameRVAFixup(
									_blob, namePointerTablePos, hintNameTablePos));
							_blob.Write(ref namePointerTablePos, (uint)0);

							// Hint/Name
							_blob.Write(ref hintNameTablePos, (ushort)entry.Ordinal);
							string name = entry.Name + '\0';
							_blob.Write(ref hintNameTablePos, name, Encoding.ASCII);
						}
						else
						{
							// DelayImport by ordinal.
							uint ordinal = (uint)entry.Ordinal | 0x80000000;
							_blob.Write(ref namePointerTablePos, (uint)ordinal);
						}
					}
					else
					{
						if (!string.IsNullOrEmpty(entry.Name))
						{
							// DelayImport by name.
							PE.Fixups.Add(
								new WriteHintNameRVAFixup(
									_blob, namePointerTablePos, hintNameTablePos));
							_blob.Write(ref namePointerTablePos, (ulong)0);

							// Hint/Name
							_blob.Write(ref hintNameTablePos, (ushort)entry.Ordinal);
							string name = entry.Name + '\0';
							_blob.Write(ref hintNameTablePos, name, Encoding.ASCII);
						}
						else
						{
							// DelayImport by ordinal.
							ulong ordinal = (uint)entry.Ordinal | 0x8000000000000000;
							_blob.Write(ref namePointerTablePos, (ulong)ordinal);
						}
					}
				}

				// Null IAT
				_blob.Write(ref iatPos, 0, 4);

				// Null DelayImportNameTable
				if (PE.Is32Bits)
					_blob.Write(ref namePointerTablePos, 0, 4);
				else
					_blob.Write(ref namePointerTablePos, 0, 8);
			}

			// Null header
			_blob.Write(ref headerPos, 0, 32);

			// Set data directories
			PE.Fixups.Add(
				new PEBuilder.SetDataDirectoryFromBlobRVAFixup(
					DataDirectories.DelayImportDescriptor, _blob));

			// Add blobs
			BuildSection section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);
		}

		#endregion

		#region Nested types

		private class WriteRVAFixup : BuildFixup
		{
			private BuildBlob _blob;
			private int _pos;
			private int _offset;

			public WriteRVAFixup(BuildBlob blob, int pos, int offset)
			{
				_blob = blob;
				_pos = pos;
				_offset = offset;
			}

			public override void ApplyFixup()
			{
				uint rva = (uint)(_blob.RVA + _offset);
				_blob.Write(ref _pos, rva);
			}
		}

		private class WriteHintNameRVAFixup : BuildFixup
		{
			private BuildBlob _blob;
			private int _pos;
			private int _hintNameTablePos;

			public WriteHintNameRVAFixup(BuildBlob blob, int pos, int hintNameTablePos)
			{
				_blob = blob;
				_pos = pos;
				_hintNameTablePos = hintNameTablePos;
			}

			public override void ApplyFixup()
			{
				if (PE.Is32Bits)
				{
					uint hintNameRVA = (uint)((_blob.RVA + _hintNameTablePos) & 0x7fffffff);
					_blob.Write(ref _pos, (uint)hintNameRVA);
				}
				else
				{
					ulong hintNameRVA = (ulong)((_blob.RVA + _hintNameTablePos) & 0x7fffffffffffffff);
					_blob.Write(ref _pos, (ulong)hintNameRVA);
				}
			}
		}

		#endregion
	}
}
