using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Describes the constraints on a generic type parameter of a generic type or method.
	/// </summary>
	public static class GenericParamFlags
	{
		/// <summary>
		/// There are no special flags.
		/// </summary>
		public const int None = 0;

		/// <summary>
		/// The generic type parameter is covariant. A covariant type parameter can appear
		/// as the result type of a method, the type of a read-only field, a declared
		/// base type, or an implemented interface.
		/// </summary>
		public const int Covariant = 0x1;

		/// <summary>
		/// The generic type parameter is contravariant. A contravariant type parameter
		/// can appear as a parameter type in method signatures.
		/// </summary>
		public const int Contravariant = 0x2;

		/// <summary>
		/// Selects the combination of all variance flags. This value is the result of
		/// using logical OR to combine the following flags:
		/// System.Reflection.GenericParameterAttributes.Contravariant and
		/// System.Reflection.GenericParameterAttributes.Covariant.
		/// </summary>
		public const int VarianceMask = 0x3;

		/// <summary>
		/// A type can be substituted for the generic type parameter only if it is a reference type.
		/// </summary>
		public const int ReferenceTypeConstraint = 0x4;

		/// <summary>
		/// A type can be substituted for the generic type parameter only if it is a
		/// value type and is not nullable.
		/// </summary>
		public const int NotNullableValueTypeConstraint = 0x8;

		/// <summary>
		/// A type can be substituted for the generic type parameter only if it has a
		/// parameterless constructor.
		/// </summary>
		public const int DefaultConstructorConstraint = 0x10;

		/// <summary>
		/// Selects the combination of all special constraint flags. This value is the
		/// result of using logical OR to combine the following flags:
		/// System.Reflection.GenericParameterAttributes.DefaultConstructorConstraint,
		/// System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint, and
		/// System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint.
		/// </summary>
		public const int SpecialConstraintMask = 0x1c;
	}
}
