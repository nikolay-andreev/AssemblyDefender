using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The first byte of a signature identifies the type of the signature, which for historical reasons is
	/// called the calling convention of the signature, be it a method signature or some other signature.
	/// </summary>
	public static class SignatureType
	{
		/// <summary>
		/// Mask for method calling convention.
		/// </summary>
		public const int MethodCallConvMask = 0x5;

		/// <summary>
		/// Field. ILAsm has no keyword for this calling convention.
		/// </summary>
		public const int Field = 0x6;

		/// <summary>
		/// Local variables. ILAsm has no keyword for this calling convention.
		/// </summary>
		public const int LocalSig = 0x7;

		/// <summary>
		/// Property. ILAsm has no keyword for this calling convention.
		/// </summary>
		public const int Property = 0x8;

		/// <summary>
		/// Unmanaged calling convention, not currently used by the common language runtime and not
		/// recognized by ILAsm.
		/// </summary>
		public const int Unmanaged = 0x9;

		/// <summary>
		/// Generic method instantiation
		/// </summary>
		public const int GenericInst = 0xa;

		/// <summary>
		/// Used ONLY for 64bit vararg PInvoke calls
		/// </summary>
		public const int NativeVarArgs = 0xb;

		/// <summary>
		/// First invalid calling convention
		/// </summary>
		public const int Max = 0xc;

		/// <summary>
		/// The high bits of the calling convention convey additional info
		/// </summary>
		public const int Mask = 0x0f;

		/// <summary>
		/// Generic method sig with explicit number of type arguments (precedes ordinary parameter count)
		/// </summary>
		public const int Generic = 0x10;

		/// <summary>
		/// Instance method that has an instance pointer (this) as an implicit first argument.
		/// The ILAsm keyword is instance.
		/// </summary>
		public const int HasThis = 0x20;

		/// <summary>
		/// Method call signature. The first explicitly specified parameter is the instance pointer.
		/// The ILAsm keyword is explicit.
		/// </summary>
		public const int ExplicitThis = 0x40;
	}
}
