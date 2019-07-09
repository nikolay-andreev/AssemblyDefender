using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IEventBase
	{
		IMethodSignature AddMethod
		{
			get;
		}

		IMethodSignature RemoveMethod
		{
			get;
		}

		IMethodSignature InvokeMethod
		{
			get;
		}
	}
}
