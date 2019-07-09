using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in Module table.
	/// </summary>
	public struct ModuleRow : IEquatable<ModuleRow>
	{
		/// <summary>
		/// Used only at run time, in edit-and-continue mode.
		/// </summary>
		public ushort Generation;

		/// <summary>
		/// The module name, which is the same as the name of the executable file with its extension but
		/// without a path. The length should not exceed 512 bytes in UTF-8 encoding, counting the zero terminator.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// A globally unique identifier, assigned to the module as it is generated.
		/// Offset in the #GUID stream.
		/// </summary>
		public int Mvid;

		/// <summary>
		/// Used only at run time, in edit-and-continue mode.
		/// Offset in the #GUID stream.
		/// </summary>
		public int EncId;

		/// <summary>
		/// Used only at run time, in edit-and-continue mode.
		/// Offset in the #GUID stream.
		/// </summary>
		public int EncBaseId;

		public bool Equals(ModuleRow other)
		{
			if (Generation != other.Generation)
				return false;

			if (Name != other.Name)
				return false;

			if (Mvid != other.Mvid)
				return false;

			if (EncId != other.EncId)
				return false;

			if (EncBaseId != other.EncBaseId)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Generation ^
				Name ^
				Mvid ^
				EncId ^
				EncBaseId;
		}
	}
}
