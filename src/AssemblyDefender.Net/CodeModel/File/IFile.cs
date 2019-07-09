using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public interface IFileSignature : ISignature
	{
		string Name
		{
			get;
		}

		bool ContainsMetadata
		{
			get;
		}

		byte[] HashValue
		{
			get;
		}
	}
}
