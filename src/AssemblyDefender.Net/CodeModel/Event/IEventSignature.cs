using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public interface IEventSignature : ISignature
	{
		string Name
		{
			get;
		}

		ITypeSignature EventType
		{
			get;
		}

		ITypeSignature Owner
		{
			get;
		}
	}
}
