using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IField : ICodeNode, IFieldSignature
	{
		FieldVisibilityFlags Visibility
		{
			get;
		}

		bool IsStatic
		{
			get;
		}

		bool IsInitOnly
		{
			get;
		}

		bool IsLiteral
		{
			get;
		}

		bool IsNotSerialized
		{
			get;
		}

		bool IsSpecialName
		{
			get;
		}

		bool IsRuntimeSpecialName
		{
			get;
		}

		ConstantInfo? DefaultValue
		{
			get;
		}

		new IType FieldType
		{
			get;
		}

		new IType Owner
		{
			get;
		}

		IField DeclaringField
		{
			get;
		}
	}
}
