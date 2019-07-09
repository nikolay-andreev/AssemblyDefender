using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class ExportBuilder : BuildTask
	{
		#region Fields

		private string _sectionName = PESectionNames.EData;
		private int _blobPriority = 1000;
		private BuildTable _table;
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

		public BuildTable Table
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
			if (_table == null)
				return;

			_blob = new BuildBlob();

			// Calculate
			int addressTablePos = 40; // entry header
			int addressTableSize = _table.Count * 4;
			int nameTableSize = (_table.DllName ?? "").Length + 1;
			var ordinals = new List<int>();

			for (int i = 0; i < _table.Count; i++)
			{
				var entry = _table[i];

				if (!string.IsNullOrEmpty(entry.Name))
				{
					nameTableSize += entry.Name.Length + 1;
					ordinals.Add(i);
				}

				if (entry is BuildForwarderEntry)
				{
					var forwarderEntry = (BuildForwarderEntry)entry;
					nameTableSize += forwarderEntry.Forwarder.Length + 1;
				}
			}

			ordinals.Sort(delegate(int x, int y)
			{
				return string.Compare(_table[x].Name, _table[y].Name);
			});

			int namePointerTablePos = addressTablePos + addressTableSize;
			int namePointerTableSize = ordinals.Count * 4;
			int ordinalTablePos = namePointerTablePos + namePointerTableSize;
			int ordinalTableSize = ordinals.Count * 2;
			int nameTablePos = ordinalTablePos + ordinalTableSize;

			// Write
			// Header
			int headerPos = 0;
			_blob.Write(ref headerPos, (uint)0); // Characteristics
			_blob.Write(ref headerPos, (uint)PE.TimeDateStamp.To_time_t());
			_blob.Write(ref headerPos, (ushort)_table.MajorVersion);
			_blob.Write(ref headerPos, (ushort)_table.MinorVersion);

			PE.Fixups.Add(new WriteRVAFixup(_blob, headerPos, nameTablePos));
			_blob.Write(ref headerPos, (uint)0); // Name

			_blob.Write(ref headerPos, (uint)_table.OrdinalBase); // Base
			_blob.Write(ref headerPos, (uint)_table.Count); // NumberOfFunctions
			_blob.Write(ref headerPos, (uint)ordinals.Count); // NumberOfNames

			PE.Fixups.Add(new WriteRVAFixup(_blob, headerPos, addressTablePos));
			_blob.Write(ref headerPos, (uint)0); // AddressOfFunctions

			PE.Fixups.Add(new WriteRVAFixup(_blob, headerPos, namePointerTablePos));
			_blob.Write(ref headerPos, (uint)0); // AddressOfNames

			PE.Fixups.Add(new WriteRVAFixup(_blob, headerPos, ordinalTablePos));
			_blob.Write(ref headerPos, (uint)0); // AddressOfNameOrdinals

			// Dll name
			string dllName = _table.DllName + '\0';
			_blob.Write(ref nameTablePos, dllName, Encoding.ASCII);

			// Export Address Table
			for (int i = 0; i < _table.Count; i++)
			{
				var entry = _table[i];
				if (entry is BuildRVAEntry)
				{
					var rvaEntry = (BuildRVAEntry)entry;

					PE.Fixups.Add(new WriteFunctionRVAFixup(_blob, addressTablePos, rvaEntry));
					_blob.Write(ref addressTablePos, (uint)0);
				}
				else
				{
					var forwarderEntry = (BuildForwarderEntry)entry;

					PE.Fixups.Add(new WriteRVAFixup(_blob, addressTablePos, nameTablePos));
					_blob.Write(ref addressTablePos, (uint)0);

					string forwarder = forwarderEntry.Forwarder + '\0';
					_blob.Write(ref nameTablePos, forwarder, Encoding.ASCII);
				}
			}

			// Name pointer/Ordinal
			for (int i = 0; i < ordinals.Count; i++)
			{
				int ordinal = ordinals[i];

				PE.Fixups.Add(new WriteRVAFixup(_blob, namePointerTablePos, nameTablePos));
				_blob.Write(ref namePointerTablePos, (uint)0);

				_blob.Write(ref ordinalTablePos, (ushort)ordinal);

				string name = _table[ordinal].Name + '\0';
				_blob.Write(ref nameTablePos, name, Encoding.ASCII);
			}

			// Set data directories
			PE.Fixups.Add(
				new PEBuilder.SetDataDirectoryFromBlobRVAFixup(
					DataDirectories.ExportTable, _blob));

			// Add blobs
			BuildSection section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);
		}

		#endregion

		#region Nested types

		public class BuildTable : IEnumerable<BuildEntry>
		{
			#region Fields

			private string _dllName;
			private ushort _majorVersion;
			private ushort _minorVersion;
			private int _ordinalBase = 1;
			private List<BuildEntry> _list = new List<BuildEntry>();

			#endregion

			#region Properties

			public string DllName
			{
				get { return _dllName; }
				set { _dllName = value; }
			}

			public ushort MajorVersion
			{
				get { return _majorVersion; }
				set { _majorVersion = value; }
			}

			public ushort MinorVersion
			{
				get { return _minorVersion; }
				set { _minorVersion = value; }
			}

			public int OrdinalBase
			{
				get { return _ordinalBase; }
				set { _ordinalBase = value; }
			}

			public BuildEntry this[int index]
			{
				get { return _list[index]; }
			}

			public int Count
			{
				get { return _list.Count; }
			}

			#endregion

			#region Methods

			public void AddForwarderEntry(string name, string forwarder)
			{
				_list.Add(
					new BuildForwarderEntry()
					{
						Name = name,
						Forwarder = forwarder,
					});
			}

			public void AddRVAEntry(string name, BuildBlob blob)
			{
				AddRVAEntry(name, blob, 0);
			}

			public void AddRVAEntry(string name, BuildBlob blob, int offset)
			{
				_list.Add(
					new BuildRVAEntry()
					{
						Name = name,
						Blob = blob,
						Offset = offset,
					});
			}

			public void Add(BuildEntry item)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				_list.Add(item);
			}

			public IEnumerator<BuildEntry> GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			#endregion
		}

		public abstract class BuildEntry
		{
			private string _name;

			public string Name
			{
				get { return _name; }
				set { _name = value; }
			}
		}

		public class BuildForwarderEntry : BuildEntry
		{
			private string _forwarder;

			public string Forwarder
			{
				get { return _forwarder; }
				set { _forwarder = value; }
			}
		}

		public class BuildRVAEntry : BuildEntry
		{
			private BuildBlob _blob;
			private int _offset;

			public BuildBlob Blob
			{
				get { return _blob; }
				set { _blob = value; }
			}

			public int Offset
			{
				get { return _offset; }
				set { _offset = value; }
			}
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

		private class WriteFunctionRVAFixup : BuildFixup
		{
			private BuildBlob _blob;
			private int _pos;
			private BuildRVAEntry _rvaEntry;

			public WriteFunctionRVAFixup(BuildBlob blob, int pos, BuildRVAEntry rvaEntry)
			{
				_blob = blob;
				_pos = pos;
				_rvaEntry = rvaEntry;
			}

			public override void ApplyFixup()
			{
				uint rva = (uint)(_rvaEntry.Blob.RVA + _rvaEntry.Offset);
				_blob.Write(ref _pos, rva);
			}
		}

		#endregion
	}
}
