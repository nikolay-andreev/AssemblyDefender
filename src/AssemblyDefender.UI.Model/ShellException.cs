using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.UI.Model
{
	public class ShellException : Exception
	{
		public ShellException(string message)
			: base(message)
		{
		}

		public ShellException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected ShellException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
