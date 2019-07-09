using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Baml
{
	public class BamlImage
	{
		#region Fields

		public static readonly string FileExtension = ".baml";
		public static readonly string DefaultFeatureID = "MSBAML";
		public static readonly VersionPair DefaultVersion = new VersionPair(0, 0x60);

		private string _featureID;
		private VersionPair _readerVersion;
		private VersionPair _updaterVersion;
		private VersionPair _writerVersion;
		private BamlNode _firstNode;

		#endregion

		#region Ctors

		public BamlImage()
		{
			_featureID = DefaultFeatureID;
			_readerVersion = _updaterVersion = _writerVersion = DefaultVersion;
		}

		#endregion

		#region Properties

		public string FeatureID
		{
			get { return _featureID; }
			set { _featureID = value; }
		}

		public VersionPair ReaderVersion
		{
			get { return _readerVersion; }
			set { _readerVersion = value; }
		}

		public VersionPair UpdaterVersion
		{
			get { return _updaterVersion; }
			set { _updaterVersion = value; }
		}

		public VersionPair WriterVersion
		{
			get { return _writerVersion; }
			set { _writerVersion = value; }
		}

		public BamlNode FirstNode
		{
			get { return _firstNode; }
			set { _firstNode = value; }
		}

		#endregion

		#region Methods

		public byte[] Save()
		{
			var builder = new BamlImageBuilder(this);
			var blob = builder.Build();
			return blob.ToArray();
		}

		#endregion

		#region Static

		public static BamlImage LoadFile(string filePath, bool throwOnError = false)
		{
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				return Load(stream, throwOnError);
			}
		}

		public static BamlImage Load(byte[] data, bool throwOnError = false)
		{
			return Load(new BlobAccessor(data), throwOnError);
		}

		public static BamlImage Load(Stream stream, bool throwOnError = false)
		{
			return Load(new StreamAccessor(stream), throwOnError);
		}

		public static BamlImage Load(IBinaryAccessor accessor, bool throwOnError = false)
		{
			var loaded = new BamlImageLoader(accessor);

			try
			{
				return loaded.Load();
			}
			catch (Exception ex)
			{
				if (throwOnError)
				{
					throw new BamlException("Baml format is not valid.", ex);
				}
			}

			return null;
		}

		#endregion
	}
}
