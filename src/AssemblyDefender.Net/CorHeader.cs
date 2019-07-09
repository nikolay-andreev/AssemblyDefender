using System;
using System.IO;
using System.Runtime.InteropServices;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// The 15th directory entry of the PE header contains the RVA and size of the runtime header in
	/// the image file. The runtime header, which contains all of the runtime-specific data entries and
	/// other information, should reside in a read-only section of the image file. The IL assembler
	/// puts the common language runtime header in the .text section.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct CorHeader
	{
		public const uint Signature = 0x48;

		/// <summary>
		/// Size of the header in bytes.
		/// </summary>
		public uint Cb;

		/// <summary>
		/// Major number of the minimum version of the runtime required to run the program.
		/// </summary>
		public short MajorRuntimeVersion;

		/// <summary>
		/// Minor number of the version of the runtime required to run the program.
		/// </summary>
		public short MinorRuntimeVersion;

		/// <summary>
		/// RVA and size of the metadata.
		/// </summary>
		public DataDirectory Metadata;

		/// <summary>
		/// Binary flags, discussed in the following section. In ILAsm, you can specify this
		/// value explicitly by the directive .corflags 'integer value'.
		/// </summary>
		public CorFlags Flags;

		/// <summary>
		/// Metadata identifier (token) of the entry point for the image file; can be 0 for DLL
		/// images. This field identifies a method belonging to this module or a module
		/// containing the entry point method. In images of version 2.0 and newer, this field
		/// may contain RVA of the embedded native entry point method.
		/// </summary>
		public uint EntryPointToken;

		/// <summary>
		/// RVA and size of managed resources.
		/// </summary>
		public DataDirectory Resources;

		/// <summary>
		/// RVA and size of the hash data for this PE file, used by the loader for binding and versioning.
		/// </summary>
		public DataDirectory StrongNameSignature;

		/// <summary>
		/// RVA and size of the Code Manager table. In the existing releases of the runtime,
		/// this field is reserved and must be set to 0.
		/// </summary>
		public DataDirectory CodeManagerTable;

		/// <summary>
		/// RVA and size in bytes of an array of virtual table (v-table) fixups.
		/// Among current managed compilers, only the VC++ linker and the IL assembler
		/// can produce this array.
		/// </summary>
		public DataDirectory VTableFixups;

		/// <summary>
		/// RVA and size of an array of addresses of jump thunks. Among managed compilers,
		/// only the VC++ of versions pre-8.0 could produce this table, which allows the
		/// export of unmanaged native methods embedded in the managed PE file. In v2.0
		/// of CLR this entry is obsolete and must be set to 0.
		/// </summary>
		public DataDirectory ExportAddressTableJumps;

		/// <summary>
		/// Reserved for precompiled images; set to 0.
		/// </summary>
		public DataDirectory ManagedNativeHeader;

		public static unsafe CorHeader Load(PEImage pe)
		{
			var dd = pe.Directories[DataDirectories.CLIHeader];
			if (dd.IsNull)
			{
				throw new BadImageFormatException(string.Format(SR.MetadataImageNotValid, pe.Location));
			}

			using (var accessor = pe.OpenImageToSectionData(dd.RVA))
			{
				return Read(accessor, pe.Location);
			}
		}

		public static unsafe CorHeader Read(IBinaryAccessor accessor, string location)
		{
			CorHeader corHeader;
			fixed (byte* pBuff = accessor.ReadBytes(sizeof(CorHeader)))
			{
				corHeader = *(CorHeader*)pBuff;
			}

			if (corHeader.Cb != Signature)
			{
				throw new BadImageFormatException(string.Format(SR.MetadataImageNotValid, location));
			}

			return corHeader;
		}
	}
}
