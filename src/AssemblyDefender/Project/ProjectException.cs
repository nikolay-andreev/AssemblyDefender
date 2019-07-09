using System;
using System.Runtime.Serialization;

namespace AssemblyDefender
{
	public class ProjectException : AssemblyDefenderException
	{
		public ProjectException(string message)
			: base(message)
		{
		}

		public ProjectException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected ProjectException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
