using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IModule : ICodeNode, IModuleSignature
	{
		string Location
		{
			get;
		}

		bool IsPrimeModule
		{
			get;
		}

		IReadOnlyList<IType> Types
		{
			get;
		}
	}
}
