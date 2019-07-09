using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class ResourceBuilder : BuildTask
	{
		#region Fields

		private string _sectionName = PESectionNames.Rsrc;
		private int _blobPriority = 1000;
		private State _state;
		private ResourceTable _table;
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

		public ResourceTable Table
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

			_state = new State();

			var _tableData = new TableData();
			_tableData.Table = _table;

			BuildTable(_tableData);
			BuildEntries(_tableData);

			_state.StringPos = _state.TreeSize;
			_state.DataDescriptionPos = _state.StringPos + _state.StringSize;
			_state.DataPos = _state.DataDescriptionPos + _state.DataDescriptionSize;
			_state.DataPos = _state.DataPos.Align(4);

			WriteTable(_tableData);
			WriteChildTables(_tableData);

			// Set data directories
			PE.Fixups.Add(
				new PEBuilder.SetDataDirectoryFromBlobRVAFixup(
					DataDirectories.ResourceTable, _state.Blob));

			// Add blobs
			BuildSection section = PE.GetSection(_sectionName);
			section.Blobs.Add(_state.Blob, _blobPriority);

			// Set state
			_blob = _state.Blob;

			_state = null;
		}

		private void BuildTable(TableData tableData)
		{
			tableData.Offset = _state.TreeSize;

			_state.TreeSize += 16; // table header

			var namedEntries = new List<EntryData>();
			var idEntries = new List<EntryData>();
			foreach (var entry in tableData.Table)
			{
				_state.TreeSize += 8; // entry header

				var entryData = new EntryData();
				entryData.Entry = entry;

				if (entry is ResourceTableEntry)
				{
					var childTableData = new TableData();
					childTableData.Table = ((ResourceTableEntry)entry).Table;
					entryData.Table = childTableData;
				}
				else
				{
					var dataEntry = (ResourceDataEntry)entry;
					_state.DataDescriptionSize += 16;
					_state.DataSize += dataEntry.Length.Align(4);
				}

				if (entry.IdentifiedByName)
				{
					int length = entry.Name.Length * 2; // Unicode char has 2 bytes
					length += 2; // bytes for size
					length = length.Align(2);
					_state.StringSize += length;

					namedEntries.Add(entryData);
				}
				else
				{
					idEntries.Add(entryData);
				}
			}

			tableData.NumberOfNamedEntries = namedEntries.Count;
			tableData.NumberOfIdEntries = idEntries.Count;

			var entries = new EntryData[namedEntries.Count + idEntries.Count];
			namedEntries.CopyTo(entries, 0);
			idEntries.CopyTo(entries, namedEntries.Count);
			tableData.Entries = entries;
		}

		private void BuildEntries(TableData tableData)
		{
			foreach (var entryData in tableData.Entries)
			{
				if (entryData.Table != null)
				{
					BuildTable(entryData.Table);
				}
			}

			foreach (var entryData in tableData.Entries)
			{
				if (entryData.Table != null)
				{
					BuildEntries(entryData.Table);
				}
			}
		}

		private void WriteTable(TableData tableData)
		{
			ResourceTable table = tableData.Table;

			_state.Blob.Write(ref _state.TreePos, (int)0); // Characteristics
			_state.Blob.Write(ref _state.TreePos, (int)PE.TimeDateStamp.To_time_t());
			_state.Blob.Write(ref _state.TreePos, (ushort)table.MajorVersion);
			_state.Blob.Write(ref _state.TreePos, (ushort)table.MinorVersion);
			_state.Blob.Write(ref _state.TreePos, (ushort)tableData.NumberOfNamedEntries);
			_state.Blob.Write(ref _state.TreePos, (ushort)tableData.NumberOfIdEntries);

			foreach (EntryData entryData in tableData.Entries)
			{
				WriteEntry(entryData);
			}
		}

		private void WriteChildTables(TableData tableData)
		{
			foreach (var entryData in tableData.Entries)
			{
				if (entryData.Table != null)
				{
					WriteTable(entryData.Table);
				}
			}

			foreach (var entryData in tableData.Entries)
			{
				if (entryData.Table != null)
				{
					WriteChildTables(entryData.Table);
				}
			}
		}

		private void WriteEntry(EntryData entryData)
		{
			ResourceEntry entry = entryData.Entry;

			if (entry.IdentifiedByName)
			{
				uint stringOffset = (uint)_state.StringPos | 0x80000000;
				_state.Blob.Write(ref _state.TreePos, stringOffset);
				_state.Blob.Write(ref _state.StringPos, (ushort)entry.Name.Length);
				_state.Blob.Write(ref _state.StringPos, entry.Name, Encoding.Unicode);
			}
			else
			{
				uint entryId = (uint)entry.ID & 0x7fffffff;
				_state.Blob.Write(ref _state.TreePos, entryId);
			}

			if (entryData.Table != null)
			{
				uint tableOffset = (uint)entryData.Table.Offset | 0x80000000;
				_state.Blob.Write(ref _state.TreePos, tableOffset);
			}
			else
			{
				var dataEntry = (ResourceDataEntry)entry;

				uint dataDescriptionOffset = (uint)_state.DataDescriptionPos & 0x7fffffff;
				_state.Blob.Write(ref _state.TreePos, dataDescriptionOffset);

				WriteDataEntry(dataEntry);
			}
		}

		private void WriteDataEntry(ResourceDataEntry dataEntry)
		{
			PE.Fixups.Add(new WriteRVAFixup(_state.Blob, _state.DataDescriptionPos, _state.DataPos));
			_state.Blob.Write(ref _state.DataDescriptionPos, (uint)0);

			_state.Blob.Write(ref _state.DataDescriptionPos, (int)dataEntry.Length);
			_state.Blob.Write(ref _state.DataDescriptionPos, (uint)dataEntry.CodePage);
			_state.Blob.Write(ref _state.DataDescriptionPos, (uint)0);
			_state.Blob.Write(ref _state.DataPos, dataEntry.Data);
			_state.DataPos = _state.DataPos.Align(4);
		}

		#endregion

		#region Nested types

		private class State
		{
			public int TreePos;
			public int TreeSize;
			public int StringPos;
			public int StringSize;
			public int DataDescriptionPos;
			public int DataDescriptionSize;
			public int DataPos;
			public int DataSize;
			public BuildBlob Blob = new BuildBlob();
		}

		private class TableData
		{
			public ResourceTable Table;
			public EntryData[] Entries;
			public int Offset;
			public int NumberOfNamedEntries;
			public int NumberOfIdEntries;
		}

		private class EntryData
		{
			public ResourceEntry Entry;
			public TableData Table;
		}

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

		#endregion
	}
}
