using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.Baml
{
	public interface IBamlAssembly
	{
		BamlAssemblyKind Kind
		{
			get;
		}

		Assembly Resolve(Assembly ownerAssembly);

		AssemblyReference ToReference(Assembly ownerAssembly);
	}
}
