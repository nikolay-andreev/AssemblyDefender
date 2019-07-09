using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public interface ISignature
	{
		SignatureType SignatureType
		{
			get;
		}
	}
}
