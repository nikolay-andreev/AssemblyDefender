using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in AssemblyRef table.
	/// </summary>
	public struct AssemblyRefRow : IEquatable<AssemblyRefRow>
	{
		/// <summary>
		/// The major version of the assembly.
		/// </summary>
		public int MajorVersion;

		/// <summary>
		/// The minor version of the assembly.
		/// </summary>
		public int MinorVersion;

		/// <summary>
		/// The build number of the assembly.
		/// </summary>
		public int BuildNumber;

		/// <summary>
		/// The revision number of the assembly.
		/// </summary>
		public int RevisionNumber;

		/// <summary>
		/// Assembly reference flags, which indicate whether the assembly reference holds a full unhashed
		/// public key or a "surrogate" (public key token).
		/// </summary>
		public int Flags;

		/// <summary>
		/// A binary object representing the public encryption key for a strong-named assembly or a
		/// token of this key. A key token is an 8-byte representation of a hashed public key,
		/// and it has nothing to do with metadata tokens.
		/// Offset in the #Blob stream
		/// </summary>
		public int PublicKeyOrToken;

		/// <summary>
		/// The name of the referenced assembly, which must be nonempty and must not contain a path
		/// or a filename extension.
		/// Offset in the #Strings stream
		/// </summary>
		public int Name;

		/// <summary>
		/// The culture name.
		/// Offset in the #Strings stream
		/// </summary>
		public int Locale;

		/// <summary>
		/// A binary object representing a hash of the metadata of the referenced assembly's prime module.
		/// This value is ignored by the loader, so it can safely be omitted.
		/// Offset in the #Blob stream
		/// </summary>
		public int HashValue;

		public bool Equals(AssemblyRefRow other)
		{
			if (MajorVersion != other.MajorVersion)
				return false;

			if (MinorVersion != other.MinorVersion)
				return false;

			if (BuildNumber != other.BuildNumber)
				return false;

			if (RevisionNumber != other.RevisionNumber)
				return false;

			if (Flags != other.Flags)
				return false;

			if (PublicKeyOrToken != other.PublicKeyOrToken)
				return false;

			if (Name != other.Name)
				return false;

			if (Locale != other.Locale)
				return false;

			if (HashValue != other.HashValue)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				MajorVersion ^
				MinorVersion ^
				BuildNumber ^
				RevisionNumber ^
				Flags ^
				PublicKeyOrToken ^
				Name ^
				Locale ^
				HashValue;
		}
	}
}
