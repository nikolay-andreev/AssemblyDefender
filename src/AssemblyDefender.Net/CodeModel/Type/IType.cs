using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IType : ICodeNode, ITypeSignature, ITypeBase
	{
		bool IsInterface
		{
			get;
		}

		bool IsAbstract
		{
			get;
		}

		bool IsSealed
		{
			get;
		}

		int? PackingSize
		{
			get;
		}

		int? ClassSize
		{
			get;
		}

		TypeVisibilityFlags Visibility
		{
			get;
		}

		TypeLayoutFlags Layout
		{
			get;
		}

		TypeCharSetFlags CharSet
		{
			get;
		}

		new IType ElementType
		{
			get;
		}

		new IType DeclaringType
		{
			get;
		}

		new IType BaseType
		{
			get;
		}

		new IType EnclosingType
		{
			get;
		}

		IReadOnlyList<IGenericParameter> GenericParameters
		{
			get;
		}

		new IReadOnlyList<IType> Interfaces
		{
			get;
		}

		new IReadOnlyList<IType> GenericArguments
		{
			get;
		}

		IReadOnlyList<IMethod> Methods
		{
			get;
		}

		IReadOnlyList<IField> Fields
		{
			get;
		}

		IReadOnlyList<IProperty> Properties
		{
			get;
		}

		IReadOnlyList<IEvent> Events
		{
			get;
		}

		IReadOnlyList<IType> NestedTypes
		{
			get;
		}

		new IType GetCustomModifier(out CustomModifierType modifierType);

		new ICallSite GetFunctionPointer();
	}
}
