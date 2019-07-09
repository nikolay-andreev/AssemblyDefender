using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.Net
{
	public class AssemblyLoadException : CodeModelException
	{
		public AssemblyLoadException(string message)
			: base(message)
		{
		}

		public AssemblyLoadException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected AssemblyLoadException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
