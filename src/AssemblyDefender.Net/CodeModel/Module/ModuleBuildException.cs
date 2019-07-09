using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.Net
{
	public class ModuleBuildException : CodeModelException
	{
		public ModuleBuildException(string message)
			: base(message)
		{
		}

		public ModuleBuildException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected ModuleBuildException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
