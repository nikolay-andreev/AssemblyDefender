using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public static class MetadataConstants
	{
		public const uint CorHeaderSignature = 0x48;
		public const uint MetadataHeaderSignature = 0x424a5342;
		public const int CodedTokenCount = 13;
		public const int TableCount = 45;
		public const int SortedTableCount = 14;
		public static readonly string StreamTable = "#~";
		public static readonly string StreamTableUnoptimized = "#-";
		public static readonly string StreamStrings = "#Strings";
		public static readonly string StreamUserStrings = "#US";
		public static readonly string StreamGuid = "#GUID";
		public static readonly string StreamBlob = "#Blob";
	}
}
