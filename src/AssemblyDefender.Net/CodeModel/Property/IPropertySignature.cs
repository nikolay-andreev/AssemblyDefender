using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IPropertySignature : ISignature
	{
		string Name
		{
			get;
		}

		ITypeSignature ReturnType
		{
			get;
		}

		ITypeSignature Owner
		{
			get;
		}

		IReadOnlyList<ITypeSignature> Arguments
		{
			get;
		}
	}
}
