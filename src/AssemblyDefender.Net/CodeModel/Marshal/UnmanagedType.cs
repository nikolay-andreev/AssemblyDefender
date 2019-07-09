using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Identifies how to marshal parameters or fields to unmanaged code.
	/// </summary>
	public enum UnmanagedType
	{
		/// <summary>
		/// A 4-byte Boolean value (true != 0, false = 0). This is the Win32 BOOL type.
		/// IL: bool
		/// </summary>
		Bool = 0x2,

		/// <summary>
		/// A 1-byte signed integer. You can use this member to transform a Boolean value into a 1-byte,
		/// C-style bool (true = 1, false = 0).
		/// IL: int8
		/// </summary>
		I1 = 0x3,

		/// <summary>
		/// A 1-byte unsigned integer.
		/// IL: unsigned int8
		/// </summary>
		U1 = 0x4,

		/// <summary>
		/// A 2-byte signed integer.
		/// IL: int16
		/// </summary>
		I2 = 0x5,

		/// <summary>
		/// A 2-byte unsigned integer.
		/// IL: unsigned int16
		/// </summary>
		U2 = 0x6,

		/// <summary>
		/// A 4-byte signed integer.
		/// IL: int32
		/// </summary>
		I4 = 0x7,

		/// <summary>
		/// A 4-byte unsigned integer.
		/// IL: unsigned int32
		/// </summary>
		U4 = 0x8,

		/// <summary>
		/// An 8-byte signed integer.
		/// IL: int64
		/// </summary>
		I8 = 0x9,

		/// <summary>
		/// An 8-byte unsigned integer.
		/// IL: unsigned int64
		/// </summary>
		U8 = 0x0a,

		/// <summary>
		/// A 4-byte floating point number.
		/// IL: float32
		/// </summary>
		R4 = 0x0b,

		/// <summary>
		/// An 8-byte floating point number.
		/// IL: float64
		/// </summary>
		R8 = 0x0c,

		/// <summary>
		/// Used on a System.Decimal to marshal the decimal value as a COM currency type instead of as a Decimal.
		/// IL: currency
		/// </summary>
		Currency = 0x0f,

		/// <summary>
		/// A Unicode character string that is a length-prefixed double byte. You can use this member,
		/// which is the default string in COM, on the System.String data type.
		/// IL: bstr
		/// </summary>
		BStr = 0x13,

		/// <summary>
		/// A single byte, null-terminated ANSI character string. You can use this member on the System.String or
		/// System.Text.StringBuilder data types
		/// IL: lpstr
		/// </summary>
		LPStr = 0x14,

		/// <summary>
		/// A 2-byte, null-terminated Unicode character string.
		/// IL: lpwstr
		/// </summary>
		LPWStr = 0x15,

		/// <summary>
		/// A platform-dependent character string: ANSI on Windows 98 and Unicode on Windows NT and Windows XP.
		/// This value is only supported for platform invoke, and not COM interop, because exporting a string of
		/// type LPTStr is not supported.
		/// IL: lptstr
		/// </summary>
		LPTStr = 0x16,

		/// <summary>
		/// Used for in-line, fixed-length character arrays that appear within a structure.
		/// The character type used with System.Runtime.InteropServices.UnmanagedType.ByValTStr
		/// is determined by the System.Runtime.InteropServices.CharSet argument of the
		/// System.Runtime.InteropServices.StructLayoutAttribute applied to the containing
		/// structure. Always use the System.Runtime.InteropServices.MarshalAsAttribute.SizeConst
		/// field to indicate the size of the array.
		/// IL: fixed sysstring [ {int32} ]
		/// </summary>
		ByValTStr = 0x17,

		/// <summary>
		/// A COM IUnknown pointer. You can use this member on the System.Object data type.
		/// IL: iunknown
		/// </summary>
		IUnknown = 0x19,

		/// <summary>
		/// A COM IDispatch pointer (Object in Microsoft Visual Basic 6.0).
		/// IL: idispatch
		/// </summary>
		IDispatch = 0x1a,

		/// <summary>
		/// A VARIANT, which is used to marshal managed formatted classes and value types.
		/// IL: struct
		/// </summary>
		Struct = 0x1b,

		/// <summary>
		/// A COM interface pointer. The System.Guid of the interface is obtained from
		/// the class metadata. Use this member to specify the exact interface type or
		/// the default interface type if you apply it to a class. This member produces
		/// System.Runtime.InteropServices.UnmanagedType.IUnknown behavior when you apply
		/// it to the System.Object data type.
		/// IL: interface
		/// </summary>
		Interface = 0x1c,

		/// <summary>
		/// A SafeArray is a self-describing array that carries the type, rank, and bounds of the associated array data.
		/// You can use this member with the System.Runtime.InteropServices.MarshalAsAttribute.SafeArraySubType
		/// field to override the default element type.
		/// IL: safearray {VariantType}
		///     | safearray {variantType} , {QString}
		/// </summary>
		SafeArray = 0x1d,

		/// <summary>
		/// When System.Runtime.InteropServices.MarshalAsAttribute.Value is set to ByValArray,
		/// the System.Runtime.InteropServices.MarshalAsAttribute.SizeConst must be set
		/// to indicate the number of elements in the array. The System.Runtime.InteropServices.MarshalAsAttribute.ArraySubType
		/// field can optionally contain the System.Runtime.InteropServices.UnmanagedType
		/// of the array elements when it is necessary to differentiate among string
		/// types. You can only use this System.Runtime.InteropServices.UnmanagedType
		/// on an array that appear as fields in a structure.
		/// IL: fixed array [ {int32} ]
		/// </summary>
		ByValArray = 0x1e,

		/// <summary>
		/// A platform-dependent, signed integer. 4-bytes on 32 bit Windows, 8-bytes on 64 bit Windows.
		/// IL: int
		/// </summary>
		SysInt = 0x1f,

		/// <summary>
		/// A platform-dependent, unsigned integer. 4-bytes on 32 bit Windows, 8-bytes on 64 bit Windows.
		/// IL: unsigned int
		/// </summary>
		SysUInt = 0x20,

		/// <summary>
		/// Allows Visual Basic 2005 to change a string in unmanaged code, and have the results reflected in
		/// managed code. This value is only supported for platform invoke.
		/// IL: byvalstr
		/// </summary>
		VBByRefStr = 0x22,

		/// <summary>
		/// An ANSI character string that is a length prefixed, single byte. You can use this member on the
		/// System.String data type.
		/// IL: ansi bstr
		/// </summary>
		AnsiBStr = 0x23,

		/// <summary>
		/// A length-prefixed, platform-dependent char string. ANSI on Windows 98, Unicode on Windows NT.
		/// You rarely use this BSTR-like member.
		/// IL: tbstr
		/// </summary>
		TBStr = 0x24,

		/// <summary>
		/// A 2-byte, OLE-defined VARIANT_BOOL type (true = -1, false = 0).
		/// IL: variant bool
		/// </summary>
		VariantBool = 0x25,

		/// <summary>
		/// An integer that can be used as a C-style function pointer. You can use this member on a System.Delegate
		/// data type or a type that inherits from a System.Delegate.
		/// IL: method
		/// </summary>
		FunctionPtr = 0x26,

		/// <summary>
		/// A dynamic type that determines the type of an object at run time and marshals the object as that type.
		/// Valid for platform invoke methods only.
		/// IL: as any
		/// </summary>
		AsAny = 0x28,

		/// <summary>
		/// A pointer to the first element of a C-style array. When marshaling from managed
		/// to unmanaged, the length of the array is determined by the length of the
		/// managed array. When marshaling from unmanaged to managed, the length of the
		/// array is determined from the System.Runtime.InteropServices.MarshalAsAttribute.SizeConst
		/// and the System.Runtime.InteropServices.MarshalAsAttribute.SizeParamIndex
		/// fields, optionally followed by the unmanaged type of the elements within
		/// the array when it is necessary to differentiate among string types.
		/// IL: [ ]
		/// | [ {int32} ]
		/// | [ + {int32} ]
		/// | [ {int32} + {int32} ]
		/// </summary>
		LPArray = 0x2a,

		/// <summary>
		/// A pointer to a C-style structure that you use to marshal managed formatted classes.
		/// Valid for platform invoke methods only.
		/// IL: lpstruct
		/// </summary>
		LPStruct = 0x2b,

		/// <summary>
		/// Specifies the custom marshaler class when used with MarshalType or MarshalTypeRef.
		/// The MarshalCookie field can be used to pass additional information to the custom marshaler.
		/// You can use this member on any reference type.
		/// IL: custom ( {QString}, {QString}, {QString}, {QString} )
		/// | custom ( {QString}, {QString} )
		/// </summary>
		CustomMarshaler = 0x2c,

		/// <summary>
		/// This native type associated with an System.Runtime.InteropServices.UnmanagedType.I4
		/// or a System.Runtime.InteropServices.UnmanagedType.U4 causes the parameter
		/// to be exported as a HRESULT in the exported type library.
		/// IL: error
		/// </summary>
		Error = 0x2d,

		/// <summary>
		/// First invalid element type
		/// </summary>
		Max = 0x50,
	}
}
