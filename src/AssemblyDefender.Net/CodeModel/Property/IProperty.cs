using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IProperty : ICodeNode, IPropertySignature, IPropertyBase
	{
		new IType ReturnType
		{
			get;
		}

		new IType Owner
		{
			get;
		}

		new IMethod GetMethod
		{
			get;
		}

		new IMethod SetMethod
		{
			get;
		}

		IProperty DeclaringProperty
		{
			get;
		}

		IReadOnlyList<IType> Parameters
		{
			get;
		}
	}
}
