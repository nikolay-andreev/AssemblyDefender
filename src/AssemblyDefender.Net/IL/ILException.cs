using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.Net.IL
{
	public class ILException : Exception
	{
		public ILException(string message)
			: base(message)
		{
		}

		public ILException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected ILException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
