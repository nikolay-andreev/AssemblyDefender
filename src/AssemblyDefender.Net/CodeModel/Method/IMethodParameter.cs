using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IMethodParameter : IMethodParameterBase
	{
		string Name
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether this is an input parameter.
		/// </summary>
		bool IsIn
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether this is an output parameter.
		/// </summary>
		bool IsOut
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether this parameter is optional.
		/// </summary>
		bool IsOptional
		{
			get;
		}

		bool IsLcid
		{
			get;
		}

		/// <summary>
		/// Gets the type of this parameter.
		/// </summary>
		new IType Type
		{
			get;
		}

		IMethod Owner
		{
			get;
		}
	}
}
