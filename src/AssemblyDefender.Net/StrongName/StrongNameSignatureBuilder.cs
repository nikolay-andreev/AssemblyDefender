using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;

namespace AssemblyDefender.Net
{
	public class StrongNameSignatureBuilder : BuildTask
	{
		#region Fields

		private bool _isSigned;
		private BuildBlob _blob;
		private string _sectionName = PESectionNames.Text;
		private int _blobPriority = 8000;

		#endregion

		#region Properties

		public bool IsSigned
		{
			get { return _isSigned; }
			set { _isSigned = value; }
		}

		public BuildBlob Blob
		{
			get { return _blob; }
		}

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

		#endregion

		#region Methods

		public override void Build()
		{
			if (!_isSigned)
				return;

			int size = 0x80;
			_blob = new BuildBlob(new byte[size]);

			// Add blob
			var section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);

			// Set flags
			var corHeaderBuilder = PE.Tasks.Get<CorHeaderBuilder>(true);
			if (corHeaderBuilder != null)
			{
				corHeaderBuilder.Flags |= CorFlags.StrongNameSigned;
			}
		}

		#endregion
	}
}
