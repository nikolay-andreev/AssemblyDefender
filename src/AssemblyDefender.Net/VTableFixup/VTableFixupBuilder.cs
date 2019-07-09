using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class VTableFixupBuilder : BuildTask
	{
		#region Fields

		private VTableFixupTable _table;
		private BuildBlob _blob;
		private BuildBlob _dataBlob;
		private string _sectionName = PESectionNames.Text;
		private string _dataSectionName = PESectionNames.SData;
		private int _blobPriority = 1000;
		private int _dataBlobPriority = 1000;

		#endregion

		#region Properties

		public VTableFixupTable Table
		{
			get { return _table; }
			set { _table = value; }
		}

		public BuildBlob Blob
		{
			get { return _blob; }
		}

		public BuildBlob DataBlob
		{
			get { return _dataBlob; }
		}

		public string SectionName
		{
			get { return _sectionName; }
			set { _sectionName = value; }
		}

		public string DataSectionName
		{
			get { return _dataSectionName; }
			set { _dataSectionName = value; }
		}

		public int BlobPriority
		{
			get { return _blobPriority; }
			set { _blobPriority = value; }
		}

		public int DataBlobPriority
		{
			get { return _dataBlobPriority; }
			set { _dataBlobPriority = value; }
		}

		#endregion

		#region Methods

		public override void Build()
		{
			if (_table == null || _table.Count == 0)
				return;

			_blob = new BuildBlob();
			_dataBlob = new BuildBlob();

			int pos = 0;
			int dataPos = 0;
			for (int i = 0; i < _table.Count; i++)
			{
				var fixup = _table[i];
				if (fixup.Count == 0)
					continue;

				_blob.Write(ref pos, (uint)dataPos); // RVA
				_blob.Write(ref pos, (ushort)fixup.Count); // Count
				_blob.Write(ref pos, (ushort)fixup.Type); // Type

				bool is32Bits = ((fixup.Type & VTableFixupType.SlotSize32Bit) == VTableFixupType.SlotSize32Bit);

				for (int j = 0; j < fixup.Count; j++)
				{
					_dataBlob.Write(ref dataPos, (uint)fixup[j]);

					if (!is32Bits)
					{
						_dataBlob.Write(ref dataPos, (uint)0);
					}
				}
			}

			// Add fixups
			PE.Fixups.Add(new WriteDataRVAFixup(this));

			// Add blobs
			var section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);

			section = PE.GetSection(_dataSectionName);
			section.Blobs.Add(_dataBlob, _dataBlobPriority);
		}

		public int GetFixupOffset(VTableFixupEntry searchFixup)
		{
			int offset = 0;
			foreach (var fixup in _table)
			{
				if (object.ReferenceEquals(fixup, searchFixup))
				{
					return offset;
				}

				int size = ((fixup.Type & VTableFixupType.SlotSize32Bit) == VTableFixupType.SlotSize32Bit) ? 4 : 8;
				offset += (fixup.Count * size);
			}

			throw new InvalidOperationException();
		}

		public VTableFixupTable GetOrCreateTable()
		{
			if (_table == null)
			{
				_table = new VTableFixupTable();
			}

			return _table;
		}

		#endregion

		#region Nested types

		private class WriteDataRVAFixup : BuildFixup
		{
			private VTableFixupBuilder _builder;

			internal WriteDataRVAFixup(VTableFixupBuilder builder)
			{
				_builder = builder;
			}

			public override void ApplyFixup()
			{
				var table = _builder._table;
				var blob = _builder._blob;
				var dataBlob = _builder._dataBlob;

				int pos = 0;
				int dataPos = 0;
				for (int i = 0; i < table.Count; i++)
				{
					var fixup = table[i];
					if (fixup.Count == 0)
						continue;

					blob.Write(ref pos, (uint)(dataBlob.RVA + dataPos));
					pos += 4; // Count + Type

					bool is32Bits = ((fixup.Type & VTableFixupType.SlotSize32Bit) == VTableFixupType.SlotSize32Bit);
					dataPos += fixup.Count * (is32Bits ? 4 : 8);
				}
			}
		}

		#endregion
	}
}
