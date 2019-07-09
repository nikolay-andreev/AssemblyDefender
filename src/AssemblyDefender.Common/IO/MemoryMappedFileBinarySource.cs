using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace AssemblyDefender.Common.IO
{
	public class MemoryMappedFileBinarySource : IBinarySource
	{
		#region Fields

		private long _length;
		private bool _disposeFile = true;
		private string _filePath;
		private MemoryMappedFile _file;
		private MemoryMappedViewAccessor _accessor;

		#endregion

		#region Ctors

		public MemoryMappedFileBinarySource(string filePath, bool writable)
			: this(filePath, 0L, 0L, writable)
		{
		}

		public MemoryMappedFileBinarySource(string filePath, long offset, long length, bool writable)
		{
			_filePath = filePath;

			var access = writable ? MemoryMappedFileAccess.ReadWrite : MemoryMappedFileAccess.Read;

			using (var stream = new FileStream(filePath,
				FileMode.Open,
				writable ? FileAccess.ReadWrite : FileAccess.Read,
				FileShare.ReadWrite))
			{
				_length = stream.Length;

				if (length > 0)
					_length = length;

				_file = MemoryMappedFile.CreateFromFile(
					stream,
					null,
					_length,
					access,
					null,
					HandleInheritability.None,
					false);
			}

			_accessor = _file.CreateViewAccessor(offset, _length, access);
		}

		public MemoryMappedFileBinarySource(MemoryMappedViewAccessor accessor, int length, string filePath)
		{
			if (accessor == null)
				throw new ArgumentNullException("accessor");

			_accessor = accessor;
			_length = length;
			_filePath = filePath;
			_disposeFile = false;
		}

		#endregion

		#region Properties

		public bool CanWrite
		{
			get { return _accessor.CanWrite; }
		}

		public string Location
		{
			get { return _filePath; }
		}

		public bool DisposeFile
		{
			get { return _disposeFile; }
			set { _disposeFile = value; }
		}

		#endregion

		#region Methods

		public IBinaryAccessor Open()
		{
			return new MemoryMappedFileAccessor(_accessor, 0L, _length);
		}

		public Stream OpenStream()
		{
			return new MemoryMappedFileStream(_accessor, 0L, _length);
		}

		public void Dispose()
		{
			if (_disposeFile)
			{
				if (_accessor != null)
				{
					_accessor.Dispose();
				}

				if (_file != null)
				{
					_file.Dispose();
				}
			}

			_accessor = null;
			_file = null;
		}

		#endregion
	}
}
