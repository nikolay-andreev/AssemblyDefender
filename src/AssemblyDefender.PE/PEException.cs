using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.PE
{
	public class PEException : Exception
	{
		public PEException(string message)
			: base(message)
		{
		}

		public PEException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected PEException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
