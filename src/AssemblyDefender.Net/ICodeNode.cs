using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public interface ICodeNode : ISignature
	{
		EntityType EntityType
		{
			get;
		}

		IAssembly Assembly
		{
			get;
		}

		IModule Module
		{
			get;
		}

		AssemblyManager AssemblyManager
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating if node has type or method generic arguments.
		/// </summary>
		bool HasGenericContext
		{
			get;
		}

		IType GetGenericArgument(bool isMethod, int position);
	}
}
