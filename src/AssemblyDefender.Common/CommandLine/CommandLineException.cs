using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.Common.CommandLine
{
	public class CommandLineException : Exception
	{
		public CommandLineException(string message)
			: base(message)
		{
		}

		public CommandLineException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected CommandLineException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
