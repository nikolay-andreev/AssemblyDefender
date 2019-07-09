using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IMethodSignature : ISignature
	{
		string Name
		{
			get;
		}

		bool IsStatic
		{
			get;
		}

		bool HasThis
		{
			get;
		}

		bool ExplicitThis
		{
			get;
		}

		int VarArgIndex
		{
			get;
		}

		int GenericParameterCount
		{
			get;
		}

		MethodCallingConvention CallConv
		{
			get;
		}

		ITypeSignature ReturnType
		{
			get;
		}

		ITypeSignature Owner
		{
			get;
		}

		IReadOnlyList<ITypeSignature> Arguments
		{
			get;
		}

		IReadOnlyList<ITypeSignature> GenericArguments
		{
			get;
		}

		IMethodSignature DeclaringMethod
		{
			get;
		}
	}
}
