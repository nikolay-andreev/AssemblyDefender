using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface ICallSite : IMethodSignature
	{
		new IType ReturnType
		{
			get;
		}

		new IReadOnlyList<IType> Arguments
		{
			get;
		}

		AssemblyManager AssemblyManager
		{
			get;
		}
	}
}
