using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum ExceptionHandlerType
	{
		/// <summary>
		/// A typed exception clause.
		/// </summary>
		/// <example>
		/// .try
		/// {
		/// }
		/// catch [mscorlib]System.Exception
		/// {
		/// }
		/// </example>
		Catch,

		/// <summary>
		/// An exception filter and handler clause.
		/// </summary>
		/// <example>
		/// .try
		/// {
		/// }
		/// filter
		/// {
		///     ldc.i4.1
		///     endfilter
		/// }
		/// {
		///     filter handler
		/// }
		/// </example>
		Filter,

		/// <summary>
		/// A finally clause.
		/// </summary>
		/// <example>
		/// .try
		/// {
		/// }
		/// finally
		/// {
		///     endfinally
		/// }
		/// </example>
		Finally,

		/// <summary>
		/// Fault clause (finally that is called on exception only).
		/// </summary>
		/// <example>
		/// .try
		/// {
		/// }
		/// fault
		/// {
		///     endfault
		/// }
		/// </example>
		Fault,
	}
}
