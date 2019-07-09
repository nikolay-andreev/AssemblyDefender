using System;
using System.Runtime.Serialization;

namespace AssemblyDefender
{
	public class AssemblyDefenderException : Exception
	{
		public AssemblyDefenderException(string message)
			: base(message)
		{
		}

		public AssemblyDefenderException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected AssemblyDefenderException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
