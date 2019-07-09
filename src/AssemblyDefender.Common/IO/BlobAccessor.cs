using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public class BlobAccessor : IBinaryAccessor
	{
		#region Fields

		private int _position;
		private Blob _blob;

		#endregion

		#region Ctors

		public BlobAccessor()
			: this(new Blob(), 0)
		{
		}

		public BlobAccessor(byte[] buffer)
			: this(new Blob(buffer), 0)
		{
		}

		public BlobAccessor(byte[] buffer, int offset, int count)
			: this(new Blob(buffer, offset, count), 0)
		{
		}

		public BlobAccessor(Blob blob)
			: this(blob, 0)
		{
		}

		public BlobAccessor(Blob blob, int position)
		{
			if (blob == null)
				throw new ArgumentNullException("blob");

			_blob = blob;
			_position = position;
		}

		#endregion

		#region Properties

		public long Length
		{
			get { return _blob.Length; }
		}

		public long Position
		{
			get { return _position; }
			set { _position = (int)value; }
		}

		public bool CanRead
		{
			get { return true; }
		}

		public bool CanWrite
		{
			get { return _blob.Writable; }
		}

		public bool CanSeek
		{
			get { return true; }
		}

		public Blob Blob
		{
			get { return _blob; }
		}

		#endregion

		#region Methods

		public IBinaryAccessor Map(long offset, long length)
		{
			if (offset + length > _blob.Length)
				throw new ArgumentOutOfRangeException("length");

			var blob = new Blob(_blob.GetBuffer(), _blob.Origin + (int)offset, (int)length, false);
			return new BlobAccessor(blob, 0);
		}

		public void Dispose()
		{
			_blob = null;
			_position = -1;
		}

		#endregion

		#region Reading

		public int Read()
		{
			return _blob.Read(ref _position);
		}

		public int Read(byte[] buffer, int offset, int count)
		{
			return _blob.Read(ref _position, buffer, offset, count);
		}

		/// <summary>
		/// Reads a Boolean value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		public bool ReadBoolean()
		{
			return _blob.ReadBoolean(ref _position);
		}

		/// <summary>
		/// Reads a byte from the current stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>The next byte read from the stream.</returns>
		public byte ReadByte()
		{
			return _blob.ReadByte(ref _position);
		}

		/// <summary>
		/// Reads a signed byte from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A signed byte read from the stream.</returns>
		public sbyte ReadSByte()
		{
			return _blob.ReadSByte(ref _position);
		}

		/// <summary>
		/// Reads a 2-byte signed integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 2-byte signed integer read from the current stream.</returns>
		public short ReadInt16()
		{
			return _blob.ReadInt16(ref _position);
		}

		/// <summary>
		/// Reads a 4-byte signed integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 4-byte signed integer read from the current stream.</returns>
		public int ReadInt32()
		{
			return _blob.ReadInt32(ref _position);
		}

		/// <summary>
		/// Reads an 8-byte signed integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>An 8-byte signed integer read from the stream.</returns>
		public long ReadInt64()
		{
			return _blob.ReadInt64(ref _position);
		}

		/// <summary>
		/// Reads a 2-byte unsigned integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 2-byte unsigned integer read from the current stream.</returns>
		public ushort ReadUInt16()
		{
			return _blob.ReadUInt16(ref _position);
		}

		/// <summary>
		/// Reads a 4-byte unsigned integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 4-byte unsigned integer read from the current stream.</returns>
		public uint ReadUInt32()
		{
			return _blob.ReadUInt32(ref _position);
		}

		/// <summary>
		/// Reads an 8-byte unsigned integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>An 8-byte unsigned integer read from the stream.</returns>
		public ulong ReadUInt64()
		{
			return _blob.ReadUInt64(ref _position);
		}

		/// <summary>
		/// Reads a 4-byte floating point value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 4-byte floating point value read from the current stream.</returns>
		public float ReadSingle()
		{
			return _blob.ReadSingle(ref _position);
		}

		/// <summary>
		/// Reads an 8-byte floating point value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>An 8-byte floating point value read from the stream..</returns>
		public double ReadDouble()
		{
			return _blob.ReadDouble(ref _position);
		}

		/// <summary>
		/// Reads a decimal value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A decimal value read from the stream.</returns>
		public decimal ReadDecimal()
		{
			return _blob.ReadDecimal(ref _position);
		}

		/// <summary>
		/// Reads character from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A character read from the stream.</returns>
		public char ReadChar()
		{
			return _blob.ReadChar(ref _position);
		}

		/// <summary>
		/// Reads in a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 32-bit integer in compressed format.</returns>
		public int Read7BitEncodedInt()
		{
			return _blob.Read7BitEncodedInt(ref _position);
		}

		/// <summary>
		/// Reads a string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadString(int length)
		{
			return _blob.ReadString(ref _position, length);
		}

		/// <summary>
		/// Reads a string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadString(int length, Encoding encoding)
		{
			return _blob.ReadString(ref _position, length, encoding);
		}

		/// <summary>
		/// Reads a string from the stream. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		public string ReadLengthPrefixedString()
		{
			return _blob.ReadLengthPrefixedString(ref _position);
		}

		/// <summary>
		/// Reads a string from the stream. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadLengthPrefixedString(Encoding encoding)
		{
			return _blob.ReadLengthPrefixedString(ref _position, encoding);
		}

		/// <summary>
		/// Reads a null-terminated string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		public string ReadNullTerminatedString()
		{
			return _blob.ReadNullTerminatedString(ref _position);
		}

		/// <summary>
		/// Reads a null-terminated string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		public string ReadNullTerminatedString(Encoding encoding)
		{
			return _blob.ReadNullTerminatedString(ref _position, encoding);
		}

		#endregion

		#region Writing

		public void Write(byte value)
		{
			_blob.Write(ref _position, value);
		}

		public void Write(byte[] buffer)
		{
			_blob.Write(ref _position, buffer);
		}

		public void Write(byte[] buffer, int offset, int count)
		{
			_blob.Write(ref _position, buffer, offset, count);
		}

		/// <summary>
		/// Writes a character array to the stream and advances the current position of the stream in accordance
		/// with the Encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The non-surrogate, Unicode character to write.</param>
		public void Write(char value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a one-byte Boolean value to the stream, with 0 representing false and 1 representing true.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The Boolean value to write (0 or 1). </param>
		public void Write(bool value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a signed byte to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The signed byte to write. </param>
		public void Write(sbyte value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a two-byte signed integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The two-byte signed integer to write.</param>
		public void Write(short value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a four-byte signed integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The four-byte signed integer to write.</param>
		public void Write(int value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes an eight-byte signed integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The eight-byte signed integer to write.</param>
		public void Write(long value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a two-byte unsigned integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		public void Write(ushort value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a four-byte unsigned integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		public void Write(uint value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes an eight-byte unsigned integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		public void Write(ulong value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a four-byte floating-point value to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The four-byte floating-point value to write.</param>
		public void Write(float value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes an eight-byte floating-point value to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The eight-byte floating-point value to write.</param>
		public void Write(double value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a decimal value to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The decimal value to write.</param>
		public void Write(decimal value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The 32-bit integer to be written.</param>
		public void Write7BitEncodedInt(int value)
		{
			_blob.Write7BitEncodedInt(ref _position, value);
		}

		/// <summary>
		/// Writes a string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		public void Write(string value)
		{
			_blob.Write(ref _position, value);
		}

		/// <summary>
		/// Writes a string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void Write(string value, Encoding encoding)
		{
			_blob.Write(ref _position, value, encoding);
		}

		/// <summary>
		/// Writes a length-prefixed string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteLengthPrefixedString(string value)
		{
			_blob.WriteLengthPrefixedString(ref _position, value);
		}

		/// <summary>
		/// Writes a length-prefixed string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteLengthPrefixedString(string value, Encoding encoding)
		{
			_blob.WriteLengthPrefixedString(ref _position, value, encoding);
		}

		/// <summary>
		/// Writes a null-terminated string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteNullTerminatedString(string value)
		{
			_blob.WriteNullTerminatedString(ref _position, value);
		}

		/// <summary>
		/// Writes a null-terminated string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		public void WriteNullTerminatedString(string value, Encoding encoding)
		{
			_blob.WriteNullTerminatedString(ref _position, value, encoding);
		}

		#endregion
	}
}
