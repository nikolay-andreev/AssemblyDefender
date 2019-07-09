using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in File table.
	/// </summary>
	public struct FileRow : IEquatable<FileRow>
	{
		/// <summary>
		/// Binary flags characterizing the file. This entry is mostly reserved
		/// for future use; the only flag currently defined is ContainsNoMetaData (0x00000001). This flag
		/// indicates that the file in question is not a managed PE file but rather a pure resource file.
		/// </summary>
		public int Flags;

		/// <summary>
		/// The filename, subject to the same rules as the names in Module and ModuleRef.
		/// This is the only occurrence of data duplication in the metadata model:
		/// the File name matches the name used in the ModuleRef with which this File
		/// record is paired. However, since the names in both records are not physical strings but
		/// rather offsets in the string heap, the string data might not actually be duplicated; instead,
		/// both records might reference the same string in the heap. This doesn't mean there is no
		/// data duplication: the offsets are definitely duplicated.
		/// Оffset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// The blob representing the hash of the file, used to
		/// authenticate the files in a multifile assembly. Even in a strong-named assembly, the strong
		/// name signature resides only in the prime module and covers only the prime module.
		/// Nonprime modules in an assembly are authenticated by their hash values.
		/// Оffset in the #Blob stream.
		/// </summary>
		public int HashValue;

		public bool Equals(FileRow other)
		{
			if (Flags != other.Flags)
				return false;

			if (Name != other.Name)
				return false;

			if (HashValue != other.HashValue)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Flags ^
				Name ^
				HashValue;
		}
	}
}
