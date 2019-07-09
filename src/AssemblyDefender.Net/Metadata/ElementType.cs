using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Element types used extensively in metadata signature.
	/// </summary>
	public static class ElementType
	{
		/// <summary>
		/// Marks end of a list.
		/// </summary>
		public const int End = 0x00;

		/// <summary>
		/// Empty type.
		/// IL: void
		/// .NET: void
		/// </summary>
		public const int Void = 0x01;

		/// <summary>
		/// Single-byte value, true = 1, false = 0.
		/// IL: bool
		/// .NET: System.Boolean
		/// </summary>
		public const int Boolean = 0x02;

		/// <summary>
		/// 2-byte unsigned integer, representing a Unicode character.
		/// IL: char
		/// .NET: System.Char
		/// </summary>
		public const int Char = 0x03;

		/// <summary>
		/// Signed 1-byte integer, the same as char in C/C++.
		/// IL: int8
		/// .NET: System.SByte
		/// </summary>
		public const int I1 = 0x04;

		/// <summary>
		/// Unsigned 1-byte integer.
		/// IL: unsigned int8
		/// .NET: System.Byte
		/// </summary>
		public const int U1 = 0x05;

		/// <summary>
		/// Signed 2-byte integer.
		/// IL: int16
		/// .NET: System.Int16
		/// </summary>
		public const int I2 = 0x06;

		/// <summary>
		/// Unsigned 2-byte integer.
		/// IL: unsigned int16
		/// .NET: System.UInt16
		/// </summary>
		public const int U2 = 0x07;

		/// <summary>
		/// Signed 4-byte integer.
		/// IL: int32
		/// .NET: System.Int32
		/// </summary>
		public const int I4 = 0x08;

		/// <summary>
		/// Unsigned 4-byte integer.
		/// IL: unsigned int32
		/// .NET: System.UInt32
		/// </summary>
		public const int U4 = 0x09;

		/// <summary>
		/// Signed 8-byte integer.
		/// IL: int64
		/// .NET: System.Int64
		/// </summary>
		public const int I8 = 0x0a;

		/// <summary>
		/// Unsigned 8-byte integer.
		/// IL: unsigned int64
		/// .NET: System.UInt64
		/// </summary>
		public const int U8 = 0x0b;

		/// <summary>
		/// 4-byte floating point.
		/// IL: float32
		/// .NET: System.Single
		/// </summary>
		public const int R4 = 0x0c;

		/// <summary>
		/// 8-byte floating point
		/// IL: float64
		/// .NET: System.Double
		/// </summary>
		public const int R8 = 0x0d;

		/// <summary>
		/// String literal
		/// IL: string
		/// .NET: System.String
		/// </summary>
		public const int String = 0x0e;

		/// <summary>
		/// Unmanaged pointer to {type}
		/// IL: {type}*
		/// </summary>
		public const int Ptr = 0x0f;

		/// <summary>
		/// Managed pointer to {type}
		/// IL: {type}&
		/// </summary>
		public const int ByRef = 0x10;

		/// <summary>
		/// (Unboxed) user defined value type.
		/// </summary>
		public const int ValueType = 0x11;

		/// <summary>
		/// User defined reference type.
		/// </summary>
		public const int Class = 0x12;

		/// <summary>
		/// Generic parameter in a type definition, accessed by index from 0.
		/// </summary>
		public const int Var = 0x13;

		/// <summary>
		/// Array of {type}
		/// IL: {type}[{bounds} [,{bounds}*] ]
		/// </summary>
		public const int Array = 0x14;

		/// <summary>
		/// {type} {type-arg-count} {type-1} ... {type-n} */
		/// </summary>
		public const int GenericInst = 0x15;

		/// <summary>
		/// Typed reference, carrying both a reference to a type and information identifying the referenced type.
		/// IL: typedref
		/// .NET: System.TypedReference
		/// </summary>
		public const int TypedByRef = 0x16;

		/// <summary>
		/// Pointer-size integer; size dependent on the target platform, which explains the use of the keyword native.
		/// IL: native int
		/// .NET: System.IntPtr
		/// </summary>
		public const int I = 0x18;

		/// <summary>
		/// Pointer-size unsigned integer.
		/// IL: native unsigned int
		/// .NET: System.UIntPtr
		/// </summary>
		public const int U = 0x19;

		/// <summary>
		/// Method pointer followed by full method signature
		/// </summary>
		public const int FnPtr = 0x1b;

		/// <summary>
		/// IL: object
		/// .NET: System.Object
		/// </summary>
		public const int Object = 0x1c;

		/// <summary>
		/// Single-dim array (vector) with 0 lower bound
		/// IL: {type}[]
		/// </summary>
		public const int SzArray = 0x1d;

		/// <summary>
		/// Generic parameter in a method definition, accessed by index from 0.
		/// </summary>
		public const int MVar = 0x1e;

		/// <summary>
		/// Required modifier: followed by a TypeDef or TypeRef token
		/// </summary>
		public const int CModReqD = 0x1f;

		/// <summary>
		/// Optional modifier: followed by a TypeDef or TypeRef token
		/// </summary>
		public const int CModOpt = 0x20;

		/// <summary>
		/// Implemented within the CLI
		/// </summary>
		public const int Internal = 0x21;

		/// <summary>
		/// Implemented within the CLI
		/// </summary>
		public const int Max = 0x22;

		/// <summary>
		/// Type aliasing
		/// </summary>
		public const int TypeDef = 0x23;

		/// <summary>
		/// OR'd with following element types
		/// </summary>
		public const int Modifier = 0x40;

		/// <summary>
		/// Sentinel for varargs method signature
		/// </summary>
		public const int Sentinel = 0x41;

		/// <summary>
		/// Denotes a local variable that points at a pinned object i.e. Marks a local variable as unmovable
		/// by the garbage collector.
		/// </summary>
		public const int Pinned = 0x45;

		// Custom attribute constants
		public const int Type = 0x50;
		public const int Boxed = 0x51;
		public const int Field = 0x53;
		public const int Property = 0x54;
		public const int Enum = 0x55;
	}
}
