using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in InterfaceImpl table.
	/// </summary>
	public struct InterfaceImplRow : IEquatable<InterfaceImplRow>
	{
		/// <summary>
		/// An index in the TypeDef table, indicating the implementing type.
		/// RID in the TypeDef table.
		/// </summary>
		public int Class;

		/// <summary>
		/// token of the implemented type, which can reside in the TypeDef, TypeRef, or TypeSpec table.
		/// The TypeSpec table can be referenced only if the implemented interface is an instantiation
		/// of a generic interface. The implemented type must be marked as an interface.
		/// Coded token of type TypeDefOrRef.
		/// </summary>
		public int Interface;

		public bool Equals(InterfaceImplRow other)
		{
			if (Class != other.Class)
				return false;

			if (Interface != other.Interface)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Class ^
				Interface;
		}
	}
}
