using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.Baml
{
	public interface IBamlProperty
	{
		BamlPropertyKind Kind
		{
			get;
		}

		/// <summary>
		/// Resolve property or field
		/// </summary>
		MemberNode Resolve(Assembly ownerAssembly);
	}
}
