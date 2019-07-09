using System;
using System.Runtime.Serialization;

namespace AssemblyDefender.Common
{
	public class ArgumentNullOrEmptyException : ArgumentException
	{
		public ArgumentNullOrEmptyException()
			: this(null, null, null)
		{
		}

		public ArgumentNullOrEmptyException(string paramName)
			: this(paramName, null, null)
		{
		}

		public ArgumentNullOrEmptyException(string paramName, string message)
			: this(paramName, message, null)
		{
		}

		public ArgumentNullOrEmptyException(string paramName, string message, Exception exception)
			: base(paramName, message ?? SR.ArgumentNullOrEmpty, exception)
		{
		}

		protected ArgumentNullOrEmptyException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
