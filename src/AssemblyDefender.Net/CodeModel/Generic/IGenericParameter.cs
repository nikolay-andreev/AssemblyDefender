using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IGenericParameter : IGenericParameterBase
	{
		string Name
		{
			get;
		}

		GenericParameterVariance Variance
		{
			get;
		}

		bool DefaultConstructorConstraint
		{
			get;
		}

		bool ReferenceTypeConstraint
		{
			get;
		}

		bool ValueTypeConstraint
		{
			get;
		}

		ICodeNode Owner
		{
			get;
		}

		new IReadOnlyList<IType> Constraints
		{
			get;
		}
	}
}
