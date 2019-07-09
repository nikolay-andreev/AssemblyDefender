using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public class MetadataException : Exception
	{
		public MetadataException(string message)
			: base(message)
		{
		}

		public MetadataException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected MetadataException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
