using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class TLSBuilder : BuildTask
	{
		#region Fields

		private string _sectionName = PESectionNames.SData;
		private int _blobPriority = 1000;
		private BuildBlob _blob;
		private BuildBlob _dataBlob;

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

		public BuildBlob Blob
		{
			get { return _blob; }
			set { _blob = value; }
		}

		public BuildBlob DataBlob
		{
			get { return _dataBlob; }
			set { _dataBlob = value; }
		}

		#endregion

		#region Methods

		public unsafe override void Build()
		{
			if (_dataBlob == null || _dataBlob.Length == 0)
				return;

			var relocBuilder = PE.Tasks.Get<BaseRelocationBuilder>();
			if (relocBuilder == null)
				return;

			var relocTable = relocBuilder.GetOrCreateTable();
			_blob = new BuildBlob();

			int pos = 0;
			if (PE.Is32Bits)
			{
				// Header
				PE.Fixups.Add(new WriteVAFixup(_blob, pos, _dataBlob, 0));
				relocTable.Add(_blob, pos, BaseRelocationType.HIGHLOW);
				_blob.Write(ref pos, (uint)0); // StartAddressOfRawData

				PE.Fixups.Add(new WriteVAFixup(_blob, pos, _dataBlob, _dataBlob.Length));
				relocTable.Add(_blob, pos, BaseRelocationType.HIGHLOW);
				_blob.Write(ref pos, (uint)0);  // EndAddressOfRawData

				PE.Fixups.Add(new WriteVAFixup(_blob, pos, _dataBlob, sizeof(TLSHeader32) + 4));
				relocTable.Add(_blob, pos, BaseRelocationType.HIGHLOW);
				_blob.Write(ref pos, (uint)0); // AddressOfIndex

				PE.Fixups.Add(new WriteVAFixup(_blob, pos, _dataBlob, sizeof(TLSHeader32)));
				relocTable.Add(_blob, pos, BaseRelocationType.HIGHLOW);
				_blob.Write(ref pos, (uint)0); // AddressOfCallBacks

				_blob.Write(ref pos, (int)0); // SizeOfZeroFill
				_blob.Write(ref pos, (uint)0); // Characteristics

				// CallBacks
				_blob.Write(ref pos, (uint)0);

				// Index
				_blob.Write(ref pos, (uint)0xCCCCCCCC); // Does't really matter, the OS will fill it in
			}
			else
			{
				PE.Fixups.Add(new WriteVAFixup(_blob, pos, _dataBlob, 0));
				relocTable.Add(_blob, pos, BaseRelocationType.HIGHLOW);
				_blob.Write(ref pos, (ulong)0); // StartAddressOfRawData

				PE.Fixups.Add(new WriteVAFixup(_blob, pos, _dataBlob, _dataBlob.Length));
				relocTable.Add(_blob, pos, BaseRelocationType.HIGHLOW);
				_blob.Write(ref pos, (ulong)0);  // EndAddressOfRawData

				PE.Fixups.Add(new WriteVAFixup(_blob, pos, _dataBlob, sizeof(TLSHeader64) + 8));
				relocTable.Add(_blob, pos, BaseRelocationType.HIGHLOW);
				_blob.Write(ref pos, (ulong)0); // AddressOfIndex

				PE.Fixups.Add(new WriteVAFixup(_blob, pos, _dataBlob, sizeof(TLSHeader64)));
				relocTable.Add(_blob, pos, BaseRelocationType.HIGHLOW);
				_blob.Write(ref pos, (ulong)0); // AddressOfCallBacks

				_blob.Write(ref pos, (int)0); // SizeOfZeroFill
				_blob.Write(ref pos, (uint)0); // Characteristics

				// CallBacks
				_blob.Write(ref pos, (ulong)0);

				// Index
				_blob.Write(ref pos, (ulong)0xCCCCCCCC); // Does't really matter, the OS will fill it in
			}

			// Set data directories
			PE.Fixups.Add(new SetDataDirectoryFixup(_blob));

			// Add _blobs
			BuildSection section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);
		}

		#endregion

		#region Nested types

		private class WriteVAFixup : BuildFixup
		{
			private BuildBlob _blob;
			private int _pos;
			private BuildBlob _rvaBlob;
			private int _offset;

			public WriteVAFixup(BuildBlob blob, int pos, BuildBlob rvaBlob)
				: this(blob, pos, rvaBlob, 0)
			{
			}

			public WriteVAFixup(BuildBlob blob, int pos, BuildBlob rvaBlob, int offset)
			{
				_blob = blob;
				_pos = pos;
				_rvaBlob = rvaBlob;
				_offset = offset;
			}

			public override void ApplyFixup()
			{
				if (PE.Is32Bits)
				{
					_blob.Write(ref _pos, (uint)(PE.ImageBase + _rvaBlob.RVA + (uint)_offset));
				}
				else
				{
					_blob.Write(ref _pos, (ulong)(PE.ImageBase + _rvaBlob.RVA + (uint)_offset));
				}
			}
		}

		private class SetDataDirectoryFixup : BuildFixup
		{
			private BuildBlob _blob;

			public SetDataDirectoryFixup(BuildBlob blob)
			{
				_blob = blob;
			}

			public unsafe override void ApplyFixup()
			{
				if (PE.Is32Bits)
				{
					PE.Directories[DataDirectories.TlsTable] =
						new DataDirectory(_blob.RVA, sizeof(TLSHeader32));
				}
				else
				{
					PE.Directories[DataDirectories.TlsTable] =
						new DataDirectory(_blob.RVA, sizeof(TLSHeader64));
				}
			}
		}

		#endregion
	}
}
