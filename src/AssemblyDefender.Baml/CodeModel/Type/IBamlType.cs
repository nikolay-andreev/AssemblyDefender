using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.Baml
{
	public interface IBamlType
	{
		BamlTypeKind Kind
		{
			get;
		}

		TypeDeclaration Resolve(Assembly ownerAssembly);
	}
}
