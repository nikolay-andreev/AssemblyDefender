using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public static class PropertyFlags
	{
		/// <summary>
		/// Specifies that no attributes are associated with a property.
		/// </summary>
		public const int None = 0;

		/// <summary>
		/// Specifies that the property is special, with the name describing how the property is special.
		/// </summary>
		public const int SpecialName = 0x200;

		/// <summary>
		/// Specifies that the metadata internal APIs check the name encoding.
		/// </summary>
		public const int RTSpecialName = 0x400;

		/// <summary>
		/// The property has a default value, which resides in the Constant table, that is,
		/// the Constant table contains a record, the Parent entry of which refers to this property.
		/// No ILAsm keyword.
		/// </summary>
		public const int HasDefault = 0x1000;

		/// <summary>
		/// Specifies a flag reserved for runtime use only.
		/// </summary>
		public const int ReservedMask = 0xf400;
	}
}
