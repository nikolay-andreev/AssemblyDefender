using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public interface IBinarySource : IDisposable
	{
		bool CanWrite { get; }

		/// <summary>
		/// Gets the location of data.
		/// </summary>
		string Location { get; }

		/// <summary>
		/// Open a binary accessor.
		/// </summary>
		IBinaryAccessor Open();

		/// <summary>
		/// Open a stream.
		/// </summary>
		Stream OpenStream();
	}
}
