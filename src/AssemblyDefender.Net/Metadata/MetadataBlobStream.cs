using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// #Blob: A blob heap containing internal metadata binary objects, such as default values,
	/// signatures, and so on.
	/// </summary>
	public class MetadataBlobStream
	{
		#region Fields

		private Blob _blob;

		#endregion

		#region Ctors

		public MetadataBlobStream()
		{
			_blob = new Blob(new byte[] { 0 });
		}

		public MetadataBlobStream(byte[] buffer)
		{
			_blob = new Blob(buffer);
		}

		public MetadataBlobStream(Blob blob)
		{
			_blob = blob;
		}

		#endregion

		#region Properties

		public int Length
		{
			get { return _blob.Length; }
		}

		internal Blob Blob
		{
			get { return _blob; }
			set { _blob = value; }
		}

		#endregion

		#region Methods

		public byte[] Get(int position)
		{
			if (position < 0 || position >= _blob.Length)
				throw new ArgumentOutOfRangeException("position");

			if (position == 0)
				return BufferUtils.EmptyArray;

			int length = _blob.ReadCompressedInteger(ref position);
			if (length == 0)
				return BufferUtils.EmptyArray;

			return _blob.ReadBytes(ref position, length);
		}

		public byte[] Get(int position, int offset, int size)
		{
			if (position < 0 || position >= _blob.Length)
				throw new ArgumentOutOfRangeException("position");

			if (position == 0)
				return BufferUtils.EmptyArray;

			int length = _blob.ReadCompressedInteger(ref position);
			if (length < offset + size)
				throw new ArgumentOutOfRangeException("size");

			if (size == 0)
				return BufferUtils.EmptyArray;

			position += offset;

			return _blob.ReadBytes(ref position, size);
		}

		/// <summary>
		/// Read a byte from blob. Position is where blob begin and offset is the byte within the blob.
		/// </summary>
		public byte GetByte(int position, int byteOffset)
		{
			if (position < 0 || position >= _blob.Length)
				throw new ArgumentOutOfRangeException("position");

			if (position == 0)
			{
				if (byteOffset != 0)
					throw new ArgumentOutOfRangeException("blobOffset");

				return 0;
			}

			int length = _blob.ReadCompressedInteger(ref position);
			if (length < byteOffset)
				throw new ArgumentOutOfRangeException("blobOffset");

			position += byteOffset;
			return _blob.ReadByte(ref position);
		}

		public IBinaryAccessor OpenBlob(int position)
		{
			if (position < 0 || position >= _blob.Length)
				throw new ArgumentOutOfRangeException("position");

			int count;
			if (position == 0)
			{
				count = 0;
			}
			else
			{
				count = _blob.ReadCompressedInteger(ref position);
			}

			var blob = new Blob(_blob.GetBuffer(), position, count, false);
			return new BlobAccessor(blob, 0);
		}

		public int Add(byte[] value)
		{
			if (value == null)
				return 0;

			return Add(value, 0, value.Length);
		}

		public int Add(byte[] value, int offset, int count)
		{
			if (count == 0)
				return 0;

			int position = _blob.Length;

			int addPos = position;
			_blob.WriteCompressedInteger(ref addPos, count);
			_blob.Write(ref addPos, value, offset, count);

			return position;
		}

		public void Clear()
		{
			_blob = new Blob(new byte[] { 0 });
		}

		#endregion
	}
}
