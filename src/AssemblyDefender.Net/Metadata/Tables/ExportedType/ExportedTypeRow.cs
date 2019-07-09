using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in ExportedType table.
	/// </summary>
	public struct ExportedTypeRow : IEquatable<ExportedTypeRow>
	{
		/// <summary>
		/// Binary flags indicating whether the exported type is a forwarder (forwarder) and the
		/// accessibility of the exported type.
		/// </summary>
		public int Flags;

		/// <summary>
		/// An uncoded token referring to a record of the TypeDef table of the module where the exported class
		/// is defined. This is the only occasion in the entire metadata model in which a module's metadata contains
		/// an explicit value of a metadata token from another module. This token is used as something of a
		/// hint for the loader and can be omitted without any ill effects. If the token is supplied, the loader
		/// retrieves the specific TypeDef record from the respective module's metadata and checks the full name of
		/// ExportedType against the full name of TypeDef. If the names match, the loader has found the class it was
		/// looking for; if the names do not match or if the token was not supplied in the first place, the loader
		/// finds the needed TypeDef by its full name. My advice: never specify a TypeDefId token explicitly when
		/// programming in ILAsm. This shortcut works only for automatic tools such as the Assembly Linker (AL) and
		/// only under certain circumstances.
		/// </summary>
		public int TypeDefId;

		/// <summary>
		/// Exported type's name; must be nonempty.
		/// Offset in the #Strings stream.
		/// </summary>
		public int TypeName;

		/// <summary>
		/// Exported type's namespace; can be empty.
		/// Offset in the #Strings stream.
		/// </summary>
		public int TypeNamespace;

		/// <summary>
		/// Token of the File record indicating the file of the assembly where the exported class is
		/// defined or the token of another ExportedType, if the current one is nested in another one.
		/// The forwarders have AssemblyRef tokens as Implementation, which, in my humble opinion,
		/// makes the forwarder flag redundant: the forwarding nature of an exported type can be deduced
		/// from its Implementation being an AssemblyRef.
		/// </summary>
		public int Implementation;

		public bool Equals(ExportedTypeRow other)
		{
			if (Flags != other.Flags)
				return false;

			if (TypeDefId != other.TypeDefId)
				return false;

			if (TypeName != other.TypeName)
				return false;

			if (TypeNamespace != other.TypeNamespace)
				return false;

			if (Implementation != other.Implementation)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Flags ^
				TypeDefId ^
				TypeName ^
				TypeNamespace ^
				Implementation;
		}
	}
}
