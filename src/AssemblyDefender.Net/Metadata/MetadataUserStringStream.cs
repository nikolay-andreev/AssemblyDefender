using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// #US: A blob heap containing user-defined strings. This stream contains string constants
	/// defined in the user code. The strings are kept in Unicode (UTF-16) encoding, with an
	/// additional trailing byte set to 1 or 0, indicating whether there are any characters with
	/// codes greater than 0x007F in the string. This trailing byte was added to streamline the
	/// encoding conversion operations on string objects produced from user-defined string
	/// constants. This stream’s most interesting characteristic is that the user strings are never
	/// referenced from any metadata table but can be explicitly addressed by the IL code (with
	/// the ldstr instruction). In addition, being actually a blob heap, the #US heap can store not
	/// only Unicode strings but any binary object, which opens some intriguing possibilities.
	/// The first entry is the empty 'blob' that consists of the single byte 0x00.
	/// </summary>
	public class MetadataUserStringStream
	{
		#region Fields

		private Blob _blob;

		#endregion

		#region Ctors

		public MetadataUserStringStream()
		{
			_blob = new Blob(new byte[] { 0 });
		}

		public MetadataUserStringStream(byte[] buffer)
		{
			_blob = new Blob(buffer);
		}

		public MetadataUserStringStream(Blob blob)
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

		public string Get(int position)
		{
			if (position < 0 || position >= _blob.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}

			if (position == 0)
				return string.Empty;

			int length = _blob.ReadCompressedInteger(ref position);

			// There is an additional terminal byte (so all byte counts are odd, not even).
			// This final byte holds the value 1 if and only if any UTF16 character within the string
			// has any bit set in its top byte, or its low byte is any of the following: 0x01–0x08,
			// 0x0E–0x1F, 0x27, 0x2D, 0x7F. Otherwise, it holds 0. The 1 signifies Unicode characters that
			// require handling beyond that normally provided for 8-bit encoding sets.
			if (length < 1)
				return string.Empty;

			return Encoding.Unicode.GetString(_blob.GetBuffer(), position, length - 1);
		}

		public byte[] GetBytes(int position)
		{
			if (position < 0 || position >= _blob.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}

			if (position == 0)
				return BufferUtils.EmptyArray;

			int length = _blob.ReadCompressedInteger(ref position);

			byte[] value = _blob.ReadBytes(ref position, length);

			return value;
		}

		public int Add(string value)
		{
			int position = _blob.Length;

			int addPos = position;
			if (string.IsNullOrEmpty(value))
			{
				_blob.WriteCompressedInteger(ref addPos, 1);
				_blob.Write(ref addPos, (byte)0); // Additional terminal byte.
			}
			else
			{
				byte[] data = Encoding.Unicode.GetBytes(value);
				_blob.WriteCompressedInteger(ref addPos, data.Length + 1);
				_blob.Write(ref addPos, data, 0, data.Length);
				// TO3DO:
				_blob.Write(ref addPos, (byte)0); // Additional terminal byte.
			}

			return position;
		}

		public void Clear()
		{
			_blob = new Blob(new byte[] { 0 });
		}

		#endregion
	}
}
