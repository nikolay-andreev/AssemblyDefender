using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.Baml
{
	[Serializable]
	public class BamlException : Exception
	{
		public BamlException(string message)
			: base(message)
		{
		}

		public BamlException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected BamlException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
