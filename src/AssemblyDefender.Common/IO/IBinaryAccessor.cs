using System;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public interface IBinaryAccessor : IDisposable
	{
		bool CanRead { get; }

		bool CanSeek { get; }

		bool CanWrite { get; }

		long Length { get; }

		long Position { get; set; }

		IBinaryAccessor Map(long offset, long length);

		int Read();

		int Read(byte[] buffer, int offset, int count);

		/// <summary>
		/// Reads a Boolean value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		bool ReadBoolean();

		/// <summary>
		/// Reads a byte from the current stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>The next byte read from the stream.</returns>
		byte ReadByte();

		/// <summary>
		/// Reads a signed byte from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A signed byte read from the stream.</returns>
		sbyte ReadSByte();

		/// <summary>
		/// Reads a 2-byte signed integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 2-byte signed integer read from the current stream.</returns>
		short ReadInt16();

		/// <summary>
		/// Reads a 4-byte signed integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 4-byte signed integer read from the current stream.</returns>
		int ReadInt32();

		/// <summary>
		/// Reads an 8-byte signed integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>An 8-byte signed integer read from the stream.</returns>
		long ReadInt64();

		/// <summary>
		/// Reads a 2-byte unsigned integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 2-byte unsigned integer read from the current stream.</returns>
		ushort ReadUInt16();

		/// <summary>
		/// Reads a 4-byte unsigned integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 4-byte unsigned integer read from the current stream.</returns>
		uint ReadUInt32();

		/// <summary>
		/// Reads an 8-byte unsigned integer from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>An 8-byte unsigned integer read from the stream.</returns>
		ulong ReadUInt64();

		/// <summary>
		/// Reads a 4-byte floating point value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 4-byte floating point value read from the current stream.</returns>
		float ReadSingle();

		/// <summary>
		/// Reads an 8-byte floating point value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>An 8-byte floating point value read from the stream..</returns>
		double ReadDouble();

		/// <summary>
		/// Reads a decimal value from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A decimal value read from the stream.</returns>
		decimal ReadDecimal();

		/// <summary>
		/// Reads character from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A character read from the stream.</returns>
		char ReadChar();

		/// <summary>
		/// Reads in a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>A 32-bit integer in compressed format.</returns>
		int Read7BitEncodedInt();

		/// <summary>
		/// Reads a string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <returns>The string being read.</returns>
		string ReadString(int length);

		/// <summary>
		/// Reads a string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="length">The length of the strings to read.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		string ReadString(int length, Encoding encoding);

		/// <summary>
		/// Reads a string from the stream. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		string ReadLengthPrefixedString();

		/// <summary>
		/// Reads a string from the stream. The string is prefixed with the length, encoded as an integer
		/// seven bits at a time.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		string ReadLengthPrefixedString(Encoding encoding);

		/// <summary>
		/// Reads a null-terminated string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <returns>The string being read.</returns>
		string ReadNullTerminatedString();

		/// <summary>
		/// Reads a null-terminated string from the stream.
		/// </summary>
		/// <param name="position">The byte position in the stream at which to begin reading.</param>
		/// <param name="encoding">Encoding of the string to read.</param>
		/// <returns>The string being read.</returns>
		string ReadNullTerminatedString(Encoding encoding);

		void Write(byte[] buffer);

		void Write(byte[] buffer, int offset, int count);

		/// <summary>
		/// Writes a character array to the stream and advances the current position of the stream in accordance
		/// with the Encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The non-surrogate, Unicode character to write.</param>
		void Write(char value);

		/// <summary>
		/// Writes a one-byte Boolean value to the stream, with 0 representing false and 1 representing true.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The Boolean value to write (0 or 1). </param>
		void Write(bool value);

		/// <summary>
		/// Writes a signed byte to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The signed byte to write. </param>
		void Write(sbyte value);

		/// <summary>
		/// Writes a byte to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The byte to write.</param>
		void Write(byte value);

		/// <summary>
		/// Writes a two-byte signed integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The two-byte signed integer to write.</param>
		void Write(short value);

		/// <summary>
		/// Writes a four-byte signed integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The four-byte signed integer to write.</param>
		void Write(int value);

		/// <summary>
		/// Writes an eight-byte signed integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The eight-byte signed integer to write.</param>
		void Write(long value);

		/// <summary>
		/// Writes a two-byte unsigned integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		void Write(ushort value);

		/// <summary>
		/// Writes a four-byte unsigned integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		void Write(uint value);

		/// <summary>
		/// Writes an eight-byte unsigned integer to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		void Write(ulong value);

		/// <summary>
		/// Writes a four-byte floating-point value to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The four-byte floating-point value to write.</param>
		void Write(float value);

		/// <summary>
		/// Writes an eight-byte floating-point value to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The eight-byte floating-point value to write.</param>
		void Write(double value);

		/// <summary>
		/// Writes a decimal value to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The decimal value to write.</param>
		void Write(decimal value);

		/// <summary>
		/// Writes a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The 32-bit integer to be written.</param>
		void Write7BitEncodedInt(int value);

		/// <summary>
		/// Writes a string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		void Write(string value);

		/// <summary>
		/// Writes a string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		void Write(string value, Encoding encoding);

		/// <summary>
		/// Writes a length-prefixed string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		void WriteLengthPrefixedString(string value);

		/// <summary>
		/// Writes a length-prefixed string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		void WriteLengthPrefixedString(string value, Encoding encoding);

		/// <summary>
		/// Writes a null-terminated string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		void WriteNullTerminatedString(string value);

		/// <summary>
		/// Writes a null-terminated string to the stream.
		/// </summary>
		/// <param name="stream">The stream at which to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="encoding">Encoding of the string to write.</param>
		void WriteNullTerminatedString(string value, Encoding encoding);
	}
}
