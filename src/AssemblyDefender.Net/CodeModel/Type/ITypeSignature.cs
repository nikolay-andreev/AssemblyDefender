using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface ITypeSignature : ISignature
	{
		bool IsNested
		{
			get;
		}

		string Name
		{
			get;
		}

		string Namespace
		{
			get;
		}

		string FullName
		{
			get;
		}

		ITypeSignature EnclosingType
		{
			get;
		}

		ISignature ResolutionScope
		{
			get;
		}

		ISignature Owner
		{
			get;
		}

		ITypeSignature ElementType
		{
			get;
		}

		ITypeSignature DeclaringType
		{
			get;
		}

		TypeElementCode ElementCode
		{
			get;
		}

		IReadOnlyList<ArrayDimension> ArrayDimensions
		{
			get;
		}

		IReadOnlyList<ITypeSignature> GenericArguments
		{
			get;
		}

		void GetGenericParameter(out bool isMethod, out int position);

		ITypeSignature GetCustomModifier(out CustomModifierType modifierType);

		IMethodSignature GetFunctionPointer();
	}
}
