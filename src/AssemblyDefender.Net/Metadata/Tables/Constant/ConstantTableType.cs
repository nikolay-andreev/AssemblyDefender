using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The type of the constant — one of the ELEMENT_TYPE_* codes.
	/// </summary>
	public enum ConstantTableType : int
	{
		/// <summary>
		/// 1-byte Boolean, true = 1, false = 0. bool
		/// </summary>
		Boolean = 0x02,

		/// <summary>
		/// 2-byte Unicode character. char
		/// </summary>
		Char = 0x03,

		/// <summary>
		/// Signed 1-byte integer. int8
		/// </summary>
		I1 = 0x04,

		/// <summary>
		/// Unsigned 1-byte integer. uint8
		/// </summary>
		U1 = 0x05,

		/// <summary>
		/// Signed 2-byte integer. int16
		/// </summary>
		I2 = 0x06,

		/// <summary>
		/// Unsigned 2-byte integer. uint16
		/// </summary>
		U2 = 0x07,

		/// <summary>
		/// Signed 4-byte integer. int32
		/// </summary>
		I4 = 0x08,

		/// <summary>
		/// Unsigned 4-byte integer. uint32
		/// </summary>
		U4 = 0x09,

		/// <summary>
		/// Signed 8-byte integer. int64
		/// </summary>
		I8 = 0x0a,

		/// <summary>
		/// Unsigned 8-byte integer. uint64
		/// </summary>
		U8 = 0x0b,

		/// <summary>
		/// 4-byte floating point. float32
		/// </summary>
		R4 = 0x0c,

		/// <summary>
		/// 8-byte floating point. float64
		/// </summary>
		R8 = 0x0d,

		/// <summary>
		/// Unicode string. {quoted_string}, bytearray
		/// </summary>
		String = 0x0e,

		/// <summary>
		/// Null object reference. The value of the constant of this type
		/// must be a 4-byte integer containing 0. nullref
		/// </summary>
		Class = 0x12,
	}
}
