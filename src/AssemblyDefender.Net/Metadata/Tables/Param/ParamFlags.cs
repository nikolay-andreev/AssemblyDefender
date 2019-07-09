using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Defines the attributes that can be associated with a parameter. These are defined in CorHdr.h.
	/// </summary>
	public static class ParamFlags
	{
		/// <summary>
		/// Specifies that there is no parameter attribute.
		/// </summary>
		public const int None = 0;

		/// <summary>
		/// Specifies that the parameter is an input parameter.
		/// </summary>
		public const int In = 0x1;

		/// <summary>
		/// Specifies that the parameter is an output parameter.
		/// </summary>
		public const int Out = 0x2;

		/// <summary>
		/// Specifies that the parameter is a locale identifier (lcid).
		/// </summary>
		public const int Lcid = 0x4;

		/// <summary>
		/// Specifies that the parameter is a return value.
		/// </summary>
		public const int ReturnValue = 0x8;

		/// <summary>
		/// Specifies that the parameter is optional.
		/// </summary>
		public const int Optional = 0x10;

		/// <summary>
		/// Specifies that the parameter has a default value.
		/// </summary>
		public const int HasDefault = 0x1000;

		/// <summary>
		/// Specifies that the parameter has field marshaling information.
		/// </summary>
		public const int HasFieldMarshal = 0x2000;

		/// <summary>
		/// Specifies that the parameter is reserved.
		/// </summary>
		public const int ReservedMask = 0xf000;
	}
}
