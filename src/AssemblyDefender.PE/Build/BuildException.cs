using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.PE
{
	public class BuildException : PEException
	{
		public BuildException(string message)
			: base(message)
		{
		}

		public BuildException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected BuildException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
