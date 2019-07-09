using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IAssembly : ICodeNode, IAssemblySignature
	{
		DotNetFramework Framework
		{
			get;
		}

		IReadOnlyList<IModule> Modules
		{
			get;
		}

		IReadOnlyList<IType> Types
		{
			get;
		}

		IReadOnlyList<IType> ExportedTypes
		{
			get;
		}
	}
}
