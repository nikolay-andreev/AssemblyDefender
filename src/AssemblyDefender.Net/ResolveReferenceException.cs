using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace AssemblyDefender.Net
{
	public class ResolveReferenceException : Exception
	{
		public ResolveReferenceException(string message)
			: base(message)
		{
		}

		public ResolveReferenceException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected ResolveReferenceException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
