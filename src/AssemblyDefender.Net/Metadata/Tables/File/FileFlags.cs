using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public static class FileFlags
	{
		/// <summary>
		/// The file is a managed PE file
		/// </summary>
		public const int ContainsMetaData = 0x0000;

		/// <summary>
		/// The file is a pure resource file
		/// </summary>
		public const int ContainsNoMetaData = 0x0001;
	}
}
