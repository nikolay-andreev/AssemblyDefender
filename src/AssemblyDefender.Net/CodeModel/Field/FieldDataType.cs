using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum FieldDataType
	{
		/// <summary>
		/// Specify read/write data located in section with ContainsInitializedData flag
		/// </summary>
		Data,

		/// <summary>
		/// Specify a read-only data located in section with ContainsCode flag
		/// </summary>
		Cli,

		/// <summary>
		/// Specify a Thread Local Storage (TLS) data.
		/// </summary>
		Tls,
	}
}
