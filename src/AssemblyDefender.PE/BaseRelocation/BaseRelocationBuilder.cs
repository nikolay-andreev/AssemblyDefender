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
	public class BaseRelocationBuilder : BuildTask
	{
		#region Fields

		private const int PageSize = 0x1000;
		private string _sectionName = PESectionNames.Reloc;
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
			if (_table == null || _table.Count == 0)
				return;

			var blocks = CreateBlocks();

			int size = 0;
			foreach (var block in blocks)
			{
				size += (8 + (block.Entries.Count * 2).Align(4));
			}

			_blob = new BuildBlob(new byte[size]);

			// Build relocation blob fixup
			PE.Fixups.Add(new BuildRelocationFixup(_blob, blocks));

			// Set data directories
			PE.Fixups.Add(
				new PEBuilder.SetDataDirectoryFromBlobRVAFixup(
					DataDirectories.BaseRelocationTable, _blob));

			// Add blobs
			var section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);

			// Set flags
			PE.Characteristics &= ~ImageCharacteristics.RELOCS_STRIPPED;
		}

		public BuildTable GetOrCreateTable()
		{
			if (_table == null)
			{
				_table = new BuildTable();
			}

			return _table;
		}

		private List<BuildBlock> CreateBlocks()
		{
			// Group entries by blob
			var entriesByBlob = new Dictionary<BuildBlob, List<BuildEntry>>();
			foreach (var entry in _table)
			{
				List<BuildEntry> entries;
				if (!entriesByBlob.TryGetValue(entry.Blob, out entries))
				{
					entries = new List<BuildEntry>();
					entriesByBlob.Add(entry.Blob, entries);
				}

				entries.Add(entry);
			}

			// Create blocks
			var blocks = new List<BuildBlock>();

			foreach (var kvp in entriesByBlob)
			{
				var entries = kvp.Value;

				// Sort entries by offset
				entries.Sort(delegate(BuildEntry entry1, BuildEntry entry2)
				{
					return Comparer<int>.Default.Compare(entry1.Offset, entry2.Offset);
				});

				BuildBlock block = null;

				foreach (var entry in entries)
				{
					if (block == null || (block.Offset + PageSize) <= entry.Offset)
					{
						block = new BuildBlock();
						block.Blob = entry.Blob;
						block.Offset = ((entry.Offset / PageSize) * PageSize);

						blocks.Add(block);
					}

					entry.Offset -= block.Offset;
					block.Entries.Add(entry);
				}
			}

			return blocks;
		}

		#endregion

		#region Nested types

		public class BuildTable : IEnumerable<BuildEntry>
		{
			#region Fields

			private List<BuildEntry> _list = new List<BuildEntry>();

			#endregion

			#region Properties

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

			public void Add(BuildBlob blob, int offset, BaseRelocationType type)
			{
				if (blob == null)
					throw new ArgumentNullException("blob");

				_list.Add(
					new BuildEntry()
					{
						Blob = blob,
						Offset = offset,
						Type = type,
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

		public class BuildEntry
		{
			private BuildBlob _blob;
			private int _offset;
			private BaseRelocationType _type;

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

			public BaseRelocationType Type
			{
				get { return _type; }
				set { _type = value; }
			}
		}

		private class BuildBlock
		{
			public int Offset;
			public BuildBlob Blob;
			public List<BuildEntry> Entries = new List<BuildEntry>();
		}

		private class BuildRelocationFixup : BuildFixup
		{
			private BuildBlob _blob;
			private List<BuildBlock> _blocks;

			public BuildRelocationFixup(BuildBlob blob, List<BuildBlock> blocks)
			{
				_blob = blob;
				_blocks = blocks;
			}

			public override void ApplyFixup()
			{
				// Sort blocks by RVA
				_blocks.Sort(delegate(BuildBlock block1, BuildBlock block2)
				{
					return Comparer<uint>.Default.Compare(block1.Blob.RVA, block2.Blob.RVA);
				});

				int pos = 0;
				foreach (var block in _blocks)
				{
					uint pageRVA = block.Blob.RVA + (uint)block.Offset;
					_blob.Write(ref pos, (uint)pageRVA); // PageRVA

					int entrySize = (block.Entries.Count * 2);
					int alignedEntrySize = entrySize.Align(4);
					int blockSize = 8 + alignedEntrySize;
					_blob.Write(ref pos, (int)blockSize); // BlockSize

					foreach (var entry in block.Entries)
					{
						ushort value = (ushort)(((uint)entry.Offset & 0xFFF) | ((uint)entry.Type << 12));
						_blob.Write(ref pos, (ushort)value);
					}

					int alignCount = alignedEntrySize - entrySize;
					if (alignCount > 0)
					{
						_blob.Write(ref pos, 0, alignCount);
					}
				}
			}
		}

		#endregion
	}
}
