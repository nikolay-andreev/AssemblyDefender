using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in ClassLayout table.
	/// </summary>
	public struct ClassLayoutRow : IEquatable<ClassLayoutRow>
	{
		/// <summary>
		/// Defined by .pack directive. See <see cref="TypePackNode"/>.
		/// </summary>
		public int PackingSize;

		/// <summary>
		/// Defined by .size directive. See <see cref="TypeSizeNode"/>.
		/// </summary>
		public int ClassSize;

		/// <summary>
		/// An index of the type definition record to which this layout belongs. The ClassLayout table
		/// should not contain any duplicate records with the same Parent entry value.
		/// RID in the TypeDef table.
		/// </summary>
		public int Parent;

		public bool Equals(ClassLayoutRow other)
		{
			if (PackingSize != other.PackingSize)
				return false;

			if (ClassSize != other.ClassSize)
				return false;

			if (Parent != other.Parent)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				PackingSize ^
				ClassSize ^
				Parent;
		}
	}
}
