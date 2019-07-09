using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public class BlobBinarySource : IBinarySource
	{
		private string _location;
		private Blob _blob;

		public BlobBinarySource()
			: this(new Blob())
		{
		}

		public BlobBinarySource(byte[] data)
			: this(new Blob(data), null)
		{
		}

		public BlobBinarySource(byte[] data, string location)
			: this(new Blob(data), location)
		{
		}

		public BlobBinarySource(Blob blob)
			: this(blob, null)
		{
		}

		public BlobBinarySource(Blob blob, string location)
		{
			if (blob == null)
				throw new ArgumentNullException("blob");

			_blob = blob;
			_location = location;
		}

		public bool CanWrite
		{
			get { return _blob.Writable; }
		}

		public string Location
		{
			get { return _location; }
		}

		public IBinaryAccessor Open()
		{
			return new BlobAccessor(_blob, 0);
		}

		public Stream OpenStream()
		{
			return new BlobStream(_blob, 0);
		}

		public void Dispose()
		{
			_blob = null;
		}
	}
}
