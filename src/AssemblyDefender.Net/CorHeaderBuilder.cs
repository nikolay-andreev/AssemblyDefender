using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;

namespace AssemblyDefender.Net
{
	public class CorHeaderBuilder : BuildTask
	{
		#region Fields

		private CorFlags _flags = CorFlags.ILOnly;
		private int _entryPointToken;
		private int _majorRuntimeVersion = 2;
		private int _minorRuntimeVersion = 5;
		private BuildBlob _blob;
		private string _sectionName = PESectionNames.Text;
		private int _blobPriority = 4000;

		#endregion

		#region Properties

		public CorFlags Flags
		{
			get { return _flags; }
			set { _flags = value; }
		}

		public int EntryPointToken
		{
			get { return _entryPointToken; }
			set { _entryPointToken = value; }
		}

		public int MajorRuntimeVersion
		{
			get { return _majorRuntimeVersion; }
			set { _majorRuntimeVersion = value; }
		}

		public int MinorRuntimeVersion
		{
			get { return _minorRuntimeVersion; }
			set { _minorRuntimeVersion = value; }
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
			var metadataBuilder = PE.Tasks.Get<MetadataBuilder>();
			if (metadataBuilder == null)
				return;

			var metadataBlob = metadataBuilder.Blob;
			if (metadataBlob == null)
				return;

			_blob = new BuildBlob(new byte[72]);

			// Add fixup
			PE.Fixups.Add(new CLIFixup(this));

			// Set data directories
			PE.Fixups.Add(
				new PEBuilder.SetDataDirectoryFromBlobRVAFixup(
					DataDirectories.CLIHeader, _blob));

			// Add blob
			var section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);
		}

		#endregion

		#region Nested types

		private class CLIFixup : BuildFixup
		{
			private CorHeaderBuilder _builder;

			internal CLIFixup(CorHeaderBuilder builder)
			{
				_builder = builder;
			}

			public override void ApplyFixup()
			{
				var blob = _builder._blob;

				int pos = 0;
				blob.Write(ref pos, (uint)MetadataConstants.CorHeaderSignature);
				blob.Write(ref pos, (short)_builder._majorRuntimeVersion);
				blob.Write(ref pos, (short)_builder._minorRuntimeVersion);

				// Metadata
				var metadataBuilder = PE.Tasks.Get<MetadataBuilder>(true);
				var metadataBlob = metadataBuilder.Blob;
				blob.Write(ref pos, new DataDirectory(metadataBlob.RVA, metadataBlob.Length));

				// Flags
				blob.Write(ref pos, (uint)_builder._flags);

				// EntryPointToken
				blob.Write(ref pos, (int)_builder._entryPointToken);

				// Resources
				var moduleBuilder = PE.Tasks.Get<ModuleBuilder>(true);
				var managedResourceBlob = moduleBuilder.ManagedResourceBlob;
				if (managedResourceBlob != null)
					blob.Write(ref pos, new DataDirectory(managedResourceBlob.RVA, managedResourceBlob.Length));
				else
					blob.Write(ref pos, (ulong)0);

				// StrongNameSignature
				var strongNameSignatureBuilder = PE.Tasks.Get<StrongNameSignatureBuilder>(true);
				var strongNameSignatureBlob = strongNameSignatureBuilder.Blob;
				if (strongNameSignatureBlob != null)
					blob.Write(ref pos, new DataDirectory(strongNameSignatureBlob.RVA, strongNameSignatureBlob.Length));
				else
					blob.Write(ref pos, (ulong)0);

				// CodeManagerTable
				blob.Write(ref pos, (ulong)0);

				// VTableFixups
				var vTableFixupBuilder = PE.Tasks.Get<VTableFixupBuilder>(true);
				var vTableFixupBlob = vTableFixupBuilder.Blob;
				if (vTableFixupBlob != null)
					blob.Write(ref pos, new DataDirectory(vTableFixupBlob.RVA, vTableFixupBlob.Length));
				else
					blob.Write(ref pos, (ulong)0);

				// ExportAddressTableJumps
				blob.Write(ref pos, (ulong)0);

				// ManagedNativeHeader
				blob.Write(ref pos, (ulong)0);
			}
		}

		#endregion
	}
}
