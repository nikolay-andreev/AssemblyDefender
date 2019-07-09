using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class FileReference : Signature, IFileSignature
	{
		#region Fields

		private string _name;
		private bool _containsMetadata;
		private byte[] _hashValue;

		#endregion

		#region Ctors

		private FileReference()
		{
		}

		public FileReference(string name)
		{
			_name = name.NullIfEmpty();
		}

		public FileReference(string name, bool containsMetadata)
			: this(name)
		{
			_containsMetadata = containsMetadata;
		}

		public FileReference(string name, bool containsMetadata, byte[] hashValue)
			: this(name, containsMetadata)
		{
			_hashValue = hashValue.NullIfEmpty();
		}

		public FileReference(IFileSignature fileSig)
		{
			_name = fileSig.Name;
			_containsMetadata = fileSig.ContainsMetadata;
			_hashValue = fileSig.HashValue;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		public byte[] HashValue
		{
			get { return _hashValue; }
		}

		public bool ContainsMetadata
		{
			get { return _containsMetadata; }
		}

		public override SignatureType SignatureType
		{
			get { return SignatureType.File; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return string.Format("File: {0}", _name);
		}

		#endregion

		#region Static

		internal static FileReference Load(Module module, int rid)
		{
			var image = module.Image;

			var file = image.FileSignatures[rid - 1];
			if (file != null)
				return file;

			FileRow row;
			image.GetFile(rid, out row);

			file = new FileReference();
			file._name = image.GetString(row.Name);
			file._hashValue = image.GetBlob(row.HashValue);
			file._containsMetadata = (row.Flags == FileFlags.ContainsMetaData);

			module.AddSignature(ref file);
			image.FileSignatures[rid - 1] = file;

			return file;
		}

		#endregion
	}
}
