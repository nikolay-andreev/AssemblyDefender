using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum UnmanagedVariantType
	{
		/// <summary>
		/// Indicates that a value was not specified.
		/// </summary>
		VT_EMPTY = 0x00,

		/// <summary>
		/// Indicates a null value, similar to a null value in SQL.
		/// IL: null
		/// </summary>
		VT_NULL = 0x01,

		/// <summary>
		/// Indicates a short integer.
		/// IL: int16
		/// </summary>
		VT_I2 = 0x02,

		/// <summary>
		/// Indicates a long integer.
		/// IL: int32
		/// </summary>
		VT_I4 = 0x03,

		/// <summary>
		/// Indicates a float value.
		/// IL: float32
		/// </summary>
		VT_R4 = 0x04,

		/// <summary>
		/// Indicates a double value.
		/// IL: float64
		/// </summary>
		VT_R8 = 0x05,

		/// <summary>
		/// Indicates a currency value.
		/// IL: currency
		/// </summary>
		VT_CY = 0x06,

		/// <summary>
		/// Indicates a DATE value.
		/// IL: date
		/// </summary>
		VT_DATE = 0x07,

		/// <summary>
		/// Indicates a BSTR string.
		/// IL: bstr
		/// </summary>
		VT_BSTR = 0x08,

		/// <summary>
		/// Indicates an IDispatch pointer.
		/// IL: idispatch
		/// </summary>
		VT_DISPATCH = 0x09,

		/// <summary>
		/// Indicates an SCODE.
		/// IL: error
		/// </summary>
		VT_ERROR = 0x0a,

		/// <summary>
		/// Indicates a Boolean value.
		/// IL: bool
		/// </summary>
		VT_BOOL = 0x0b,

		/// <summary>
		/// Indicates a VARIANT far pointer.
		/// IL: variant
		/// </summary>
		VT_VARIANT = 0x0c,

		/// <summary>
		/// Indicates an IUnknown pointer.
		/// IL: iunknown
		/// </summary>
		VT_UNKNOWN = 0x0d,

		/// <summary>
		/// Indicates a decimal value.
		/// IL: decimal
		/// </summary>
		VT_DECIMAL = 0x0e,

		/// <summary>
		/// Indicates a char value.
		/// IL: int8
		/// </summary>
		VT_I1 = 0x10,

		/// <summary>
		/// Indicates a byte.
		/// IL: unsigned int8
		/// </summary>
		VT_UI1 = 0x11,

		/// <summary>
		/// Indicates an unsigned short.
		/// IL: unsigned int16
		/// </summary>
		VT_UI2 = 0x12,

		/// <summary>
		/// Indicates an unsigned long.
		/// IL: unsigned int32
		/// </summary>
		VT_UI4 = 0x13,

		/// <summary>
		/// Indicates a 64-bit integer.
		/// IL: int64
		/// </summary>
		VT_I8 = 0x14,

		/// <summary>
		/// Indicates an 64-bit unsigned integer.
		/// IL: unsigned int64
		/// </summary>
		VT_UI8 = 0x15,

		/// <summary>
		/// Indicates an integer value.
		/// IL: int
		/// </summary>
		VT_INT = 0x16,

		/// <summary>
		/// Indicates an unsigned integer value.
		/// IL: unsigned int
		/// </summary>
		VT_UINT = 0x17,

		/// <summary>
		/// Indicates a C style void.
		/// IL: void
		/// </summary>
		VT_VOID = 0x18,

		/// <summary>
		/// Indicates an HRESULT.
		/// IL: hresult
		/// </summary>
		VT_HRESULT = 0x19,

		/// <summary>
		/// Indicates a pointer type.
		/// IL: *
		/// </summary>
		VT_PTR = 0x1a,

		/// <summary>
		/// Indicates a SAFEARRAY. Not valid in a VARIANT.
		/// IL: safearray
		/// </summary>
		VT_SAFEARRAY = 0x1b,

		/// <summary>
		/// Indicates a C style array.
		/// IL: carray
		/// </summary>
		VT_CARRAY = 0x1c,

		/// <summary>
		/// Indicates a user defined type.
		/// IL: userdefined
		/// </summary>
		VT_USERDEFINED = 0x1d,

		/// <summary>
		/// Indicates a null-terminated string.
		/// IL: lpstr
		/// </summary>
		VT_LPSTR = 0x1e,

		/// <summary>
		/// Indicates a wide string terminated by null.
		/// IL: lpwstr
		/// </summary>
		VT_LPWSTR = 0x1f,

		/// <summary>
		/// Indicates a user defined type.
		/// IL: record
		/// </summary>
		VT_RECORD = 0x24,

		/// <summary>
		/// Indicates a FILETIME value.
		/// IL: filetime
		/// </summary>
		VT_FILETIME = 0x40,

		/// <summary>
		/// Indicates length prefixed bytes.
		/// IL: blob
		/// </summary>
		VT_BLOB = 0x41,

		/// <summary>
		/// Indicates that the name of a stream follows.
		/// IL: stream
		/// </summary>
		VT_STREAM = 0x42,

		/// <summary>
		/// Indicates that the name of a storage follows.
		/// IL: storage
		/// </summary>
		VT_STORAGE = 0x43,

		/// <summary>
		/// Indicates that a stream contains an object.
		/// IL: streamed_object
		/// </summary>
		VT_STREAMED_OBJECT = 0x44,

		/// <summary>
		/// Indicates that a storage contains an object.
		/// IL: stored_object
		/// </summary>
		VT_STORED_OBJECT = 0x45,

		/// <summary>
		/// Indicates that a blob contains an object.
		/// IL: blob_object
		/// </summary>
		VT_BLOB_OBJECT = 0x46,

		/// <summary>
		/// Indicates the clipboard format.
		/// IL: cf
		/// </summary>
		VT_CF = 0x47,

		/// <summary>
		/// Indicates a class ID.
		/// IL: clsid
		/// </summary>
		VT_CLSID = 0x48,

		/// <summary>
		/// Indicates a simple, counted array.
		/// IL: {VariantType} vector
		/// </summary>
		VT_VECTOR = 0x1000,

		/// <summary>
		/// Indicates a SAFEARRAY pointer.
		/// IL: {VariantType} []
		/// </summary>
		VT_ARRAY = 0x2000,

		/// <summary>
		/// Indicates that a value is a reference.
		/// IL: {VariantType} &
		/// </summary>
		VT_BYREF = 0x4000,
	}
}
