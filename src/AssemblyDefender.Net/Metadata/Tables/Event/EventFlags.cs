using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Event attributes.
	/// </summary>
	public static class EventFlags
	{
		/// <summary>
		/// Specifies that the event has no attributes.
		/// </summary>
		public const int None = 0;

		/// <summary>
		/// Specifies that the event is special in a way described by the name.
		/// </summary>
		public const int SpecialName = 0x0200;

		/// <summary>
		/// Specifies that the common language runtime should check name encoding.
		/// </summary>
		public const int RTSpecialName = 0x0400;
	}
}
