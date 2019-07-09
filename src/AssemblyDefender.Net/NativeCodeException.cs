using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.Net
{
	public class NativeCodeException : Exception
	{
		public NativeCodeException(string message)
			: base(message)
		{
		}

		public NativeCodeException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected NativeCodeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
