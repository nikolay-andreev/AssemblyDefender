using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in Assembly table.
	/// </summary>
	public struct AssemblyRow : IEquatable<AssemblyRow>
	{
		/// <summary>
		/// The ID of the hash algorithm used in this assembly to hash the files. The value must be
		/// one of the CALG_* values defined in the header file Wincrypt.h. The default hash algorithm
		/// is CALG_SHA (a.k.a. CALG_SHA1) (0x8004). Ecma International/ISO specifications consider
		/// this algorithm to be standard, offering the best widely available technology for file hashing.
		/// </summary>
		public HashAlgorithm HashAlgId;

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
		/// Assembly flags.
		/// </summary>
		public int Flags;

		/// <summary>
		/// A binary object representing a public encryption key for a strong-named assembly.
		/// Offset in the #Blob stream
		/// </summary>
		public int PublicKey;

		/// <summary>
		/// A binary object representing a public encryption key for a strong-named assembly.
		/// Offset in the #Strings stream
		/// </summary>
		public int Name;

		/// <summary>
		/// The culture (formerly known as locale) name, such as
		/// en-US (American English) or fr-CA (Canadian French), identifying the culture of localized
		/// managed resources of this assembly. The culture name must match one of hundreds of
		/// culture names "known" to the runtime through the .NET Framework class library, but this
		/// validity rule is rather meaningless: to use a culture, the specific language support must be
		/// installed on the target machine. If the language support is not installed, it doesn't matter
		/// whether the culture is "known" to the runtime.
		/// Offset in the #Strings stream
		/// </summary>
		public int Locale;

		public bool Equals(AssemblyRow other)
		{
			if (HashAlgId != other.HashAlgId)
				return false;

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

			if (PublicKey != other.PublicKey)
				return false;

			if (Name != other.Name)
				return false;

			if (Locale != other.Locale)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)HashAlgId ^
				MajorVersion ^
				MinorVersion ^
				BuildNumber ^
				RevisionNumber ^
				Flags ^
				PublicKey ^
				Name ^
				Locale;
		}
	}
}
