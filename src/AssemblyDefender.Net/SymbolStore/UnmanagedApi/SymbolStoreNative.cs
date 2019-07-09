using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	public static class SymbolStoreNative
	{
		#region CLSID

		/// <summary>
		/// This is the "Master Dispenser", always guaranteed to be the most recent dispenser on the machine.
		/// </summary>
		public const string CLSID_CorSymBinder = "0A29FF9E-7F9C-4437-8B11-F424491E3931";
		public const string CLSID_CorSymReader = "0A3976C5-4529-4ef8-B0B0-42EED37082CD";
		public const string CLSID_CorSymWriter = "0AE2DEB0-F901-478b-BB9F-881EE8066788";

		public static readonly Guid CLSID_CorSymBinderGuid = new Guid(CLSID_CorSymBinder);
		public static readonly Guid CLSID_CorSymReaderGuid = new Guid(CLSID_CorSymReader);
		public static readonly Guid CLSID_CorSymWriterGuid = new Guid(CLSID_CorSymWriter);

		#endregion

		#region Guids for known languages, language vendors, and document types

		public const string LanguageType_C = "63A08714-FC37-11D2-904C-00C04FA302A1";
		public const string LanguageType_CPlusPlus = "3A12D0B7-C26C-11D0-B442-00A0244A1DD2";
		public const string LanguageType_CSharp = "3F5162F8-07C6-11D3-9053-00C04FA302A1";
		public const string LanguageType_Basic = "3A12D0B8-C26C-11D0-B442-00A0244A1DD2";
		public const string LanguageType_Java = "3A12D0B4-C26C-11D0-B442-00A0244A1DD2";
		public const string LanguageType_ILAssembly = "AF046CD3-D0E1-11D2-977C-00A0C9B4D50C";
		public const string LanguageVendor_Microsoft = "994B45C4-E6E9-11D2-903F-00C04FA302A1";
		public const string DocumentType_Text = "5A869D0B-6611-11D3-BD2A-0000F80849BD";
		public const string DocumentType_MC = "EB40CB65-3C1F-4352-9D7B-BA0FC47A9D77";

		#endregion

		#region Guids for known Source Hash Algorithms

		public const string SourceHash_MD5 = "406EA660-64CF-4C82-B6F0-42D48172A799";
		public const string SourceHash_SHA1 = "FF1816EC-AA5E-4D10-87F7-6F4963833460";

		#endregion
	}
}
