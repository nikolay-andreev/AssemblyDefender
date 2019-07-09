using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// The CLI built-in types have corresponding value types defined in the Base Class Library.
	/// They shall be referenced in signatures only using their special encodings.
	/// </summary>
	public enum PrimitiveTypeCode
	{
		/// <summary>
		/// System.Boolean
		/// keyword: bool
		/// </summary>
		Boolean,

		/// <summary>
		/// System.Char
		/// keyword: char
		/// </summary>
		Char,

		/// <summary>
		/// System.SByte
		/// keyword: int8
		/// </summary>
		Int8,

		/// <summary>
		/// System.Int16
		/// keyword: int16
		/// </summary>
		Int16,

		/// <summary>
		/// System.Int32
		/// keyword: int32
		/// </summary>
		Int32,

		/// <summary>
		/// System.Int64
		/// keyword: int64
		/// </summary>
		Int64,

		/// <summary>
		/// System.Byte
		/// keyword: uint8, unsigned int8
		/// </summary>
		UInt8,

		/// <summary>
		/// System.UInt16
		/// keyword: uint16, unsigned int16
		/// </summary>
		UInt16,

		/// <summary>
		/// System.UInt32
		/// keyword: uint32, unsigned int32
		/// </summary>
		UInt32,

		/// <summary>
		/// System.UInt64
		/// keyword: uint64, unsigned int64
		/// </summary>
		UInt64,

		/// <summary>
		/// System.Single
		/// keyword: float32
		/// </summary>
		Float32,

		/// <summary>
		/// System.Double
		/// keyword: float64
		/// </summary>
		Float64,

		/// <summary>
		/// System.IntPtr
		/// keyword: native int
		/// </summary>
		IntPtr,

		/// <summary>
		/// System.UIntPtr
		/// keyword: native int, native unsigned int
		/// </summary>
		UIntPtr,

		/// <summary>
		/// System.Type
		/// </summary>
		Type,

		/// <summary>
		/// System.TypedReference
		/// keyword: typedref
		/// </summary>
		TypedReference,

		/// <summary>
		/// System.Object
		/// keyword: object
		/// </summary>
		Object,

		/// <summary>
		/// System.String
		/// keyword: string
		/// </summary>
		String,

		/// <summary>
		/// keyword: void
		/// </summary>
		Void,

		Undefined,
	}
}
