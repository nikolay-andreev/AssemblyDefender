using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Specifies MethodSemantics attributes.
	/// </summary>
	public static class MethodSemanticFlags
	{
		/// <summary>
		/// The method sets a value of a property.
		/// </summary>
		public const int Setter = 0x0001;

		/// <summary>
		/// The method retrieves a value of a property.
		/// </summary>
		public const int Getter = 0x0002;

		/// <summary>
		/// The method has another meaning for a property or an event.
		/// </summary>
		public const int Other = 0x0004;

		/// <summary>
		/// The method subscribes to an event.
		/// </summary>
		public const int AddOn = 0x0008;

		/// <summary>
		/// The method removes the subscription to an event.
		/// </summary>
		public const int RemoveOn = 0x0010;

		/// <summary>
		/// The method fires an event.
		/// </summary>
		public const int Fire = 0x0020;
	}
}
