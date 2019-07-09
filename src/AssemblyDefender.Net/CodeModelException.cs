using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.Net
{
	public class CodeModelException : Exception
	{
		public CodeModelException(string message)
			: base(message)
		{
		}

		public CodeModelException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected CodeModelException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
