using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IPropertyBase
	{
		IMethodSignature GetMethod
		{
			get;
		}

		IMethodSignature SetMethod
		{
			get;
		}
	}
}
