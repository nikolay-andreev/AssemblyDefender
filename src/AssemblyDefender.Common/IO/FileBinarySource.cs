using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public class FileBinarySource : IBinarySource
	{
		private bool _writable;
		private string _filePath;

		public FileBinarySource(string filePath)
			: this(filePath, false)
		{
		}

		public FileBinarySource(string filePath, bool writable)
		{
			_filePath = filePath;
			_writable = writable;
		}

		public bool CanWrite
		{
			get { return _writable; }
		}

		public string Location
		{
			get { return _filePath; }
		}

		public IBinaryAccessor Open()
		{
			return new StreamAccessor(OpenStream());
		}

		public Stream OpenStream()
		{
			if (_writable)
				return new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			else
				return new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}

		public void Dispose()
		{
		}
	}
}
