using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class CertificateBuilder : BuildTask
	{
		#region Fields

		private string _sectionName = PESectionNames.IData;
		private int _blobPriority = 1000;
		private CertificateTable _table;
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

		public CertificateTable Table
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

			int pos = 0;
			for (int i = 0; i < _table.Count; i++)
			{
				var entry = _table[i];

				if (entry.Data == null)
					continue;

				int alignLength = entry.Data.Length.Align(8);

				int length = 8 + alignLength;
				_blob.Write(ref pos, (int)length);
				_blob.Write(ref pos, (ushort)entry.Revision);
				_blob.Write(ref pos, (ushort)entry.Type);
				_blob.Write(ref pos, entry.Data);

				int alignCount = alignLength - entry.Data.Length;
				if (alignCount > 0)
				{
					_blob.Write(ref pos, 0, alignCount);
				}
			}

			// Set data directories
			PE.Fixups.Add(new SetDataDirectoryFixup(_blob));

			// Add blobs
			var section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);
		}

		#endregion

		#region Nested types

		private class SetDataDirectoryFixup : BuildFixup
		{
			private BuildBlob _blob;

			public SetDataDirectoryFixup(BuildBlob blob)
			{
				_blob = blob;
			}

			public override void ApplyFixup()
			{
				PE.Directories[DataDirectories.CertificateTable] =
					new DataDirectory(_blob.PointerToRawData, _blob.Length);
			}
		}

		#endregion
	}
}
