using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ResourceStorage
	{
		#region Fields

		private bool _hasEncrypt;
		private bool _hasCompress;
		private int _encryptKey;
		private string _resourceName;
		private Blob _blob;
		private BuildAssembly _assembly;

		#endregion

		#region Ctors

		internal ResourceStorage(BuildAssembly assembly)
		{
			_assembly = assembly;

			var random = assembly.RandomGenerator;
			_encryptKey = random.Next(100, int.MaxValue);
			_resourceName = random.NextString(12);
			_blob = new Blob();
		}

		#endregion

		#region Properties

		public Blob this[int blobId]
		{
			get
			{
				int pos = (blobId ^ _encryptKey);
				pos++; // flags
				int size = _blob.Read7BitEncodedInt(ref pos);
				return new Blob(_blob.GetBuffer(), pos, size, true);
			}
		}

		public int NextBlobID
		{
			get { return _encryptKey ^ _blob.Length; }
		}

		public int EncryptKey
		{
			get { return _encryptKey; }
		}

		public int Size
		{
			get { return _blob.Length; }
		}

		public bool HasEncrypt
		{
			get { return _hasEncrypt; }
		}

		public bool HasCompress
		{
			get { return _hasCompress; }
		}

		public string ResourceName
		{
			get { return _resourceName; }
		}

		public Blob Blob
		{
			get { return _blob; }
		}

		#endregion

		#region Methods

		public int Add(byte[] buffer, bool encrypt = false, bool compress = false)
		{
			return Add(buffer, 0, buffer.Length, encrypt, compress);
		}

		public int Add(byte[] buffer, int offset, int count, bool encrypt = false, bool compress = false)
		{
			int blobId = NextBlobID;

			byte flags = 0;

			// Compress
			if (compress && count > 30)
			{
				buffer = CompressionUtils.GZipCompress(buffer, offset, count);
				offset = 0;
				count = buffer.Length;
				flags |= 2;
				_hasCompress = true;
			}

			// Encrypt
			if (encrypt)
			{
				StrongCryptoUtils.Encrypt(buffer, _encryptKey, offset, count);
				flags |= 1;
				_hasEncrypt = true;
			}

			// Write
			int pos = _blob.Length;
			_blob.Write(ref pos, (byte)flags);
			_blob.Write7BitEncodedInt(ref pos, count);
			_blob.Write(ref pos, buffer, offset, count);

			return blobId;
		}

		internal void Build(ModuleBuilder moduleBuilder)
		{
			if (_blob.Length == 0)
				return;

			moduleBuilder.BuildManagedResource(_resourceName, ResourceVisibilityFlags.Public, _blob.GetBuffer(), 0, _blob.Length);
		}

		#endregion
	}
}
