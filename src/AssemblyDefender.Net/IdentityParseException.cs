using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.Net
{
	public class IdentityParseException : Exception
	{
		public IdentityParseException(string message)
			: base(message)
		{
		}

		public IdentityParseException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected IdentityParseException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
