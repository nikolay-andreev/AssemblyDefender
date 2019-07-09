using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public static class DataDirectories
	{
		/// <summary>
		/// [1] Export Directory table address and size: The Export Directory table contains information
		/// about four other tables, which hold data describing unmanaged exports of the PE
		/// file. Among managed compilers, only the VC++ linker and ILAsm are capable of exposing
		/// the managed methods exported by a managed PE file as unmanaged exports, to be consumed
		/// by an unmanaged caller.
		/// </summary>
		public const int ExportTable = 0;

		/// <summary>
		/// [2] Import table address and size: This table contains data on unmanaged imports consumed
		/// by the PE file. Among managed compilers, only the VC++ linker make nontrivial
		/// use of this table, importing the unmanaged external functions used in the unmanaged
		/// native code that is embedded within the current, managed PE file. Other compilers,
		/// including the IL assembler, do not embed the unmanaged native code in the managed PE
		/// files, so Import tables of the files produced by these compilers contain a single entry, that
		/// of the CLR entry function.
		/// </summary>
		public const int ImportTable = 1;

		/// <summary>
		/// [3] Resource table address and size: This table contains unmanaged resources embedded
		/// in the PE file; managed resources aren’t part of this data.
		/// </summary>
		public const int ResourceTable = 2;

		/// <summary>
		/// [4] Exception table address and size: This table contains information on unmanaged
		/// exceptions only.
		/// </summary>
		public const int ExceptionTable = 3;

		/// <summary>
		/// [5] Certificate table address and size: The address entry points to a table of attribute certificates
		/// (used for the file authentication), which are not loaded into memory as part of the
		/// image file. As such, the first field of this entry is a file pointer rather than an RVA. Each
		/// entry of the table contains a 4-byte file pointer to the respective attribute certificate and
		/// the 4-byte size of it.
		/// </summary>
		public const int CertificateTable = 4;

		/// <summary>
		/// [6] Base Relocation table address and size.
		/// </summary>
		public const int BaseRelocationTable = 5;

		/// <summary>
		/// [7] Debug data address and size: A managed PE file does not carry embedded debug data;
		/// the debug data is emitted into a PDB file, so this data directory either is all zero or points
		/// to single 30-byte debug directory entry of type 2 (IMAGE_DEBUG_TYPE_CODEVIEW),
		/// which in turn points to a CodeView-style header, containing path to the PDB file. IL
		/// assembler and C# and VB .NET compilers emit this data into the .text section.
		/// </summary>
		public const int Debug = 6;

		/// <summary>
		/// [8] Architecture data address and size: Architecture-specific data. This data directory is not
		/// used (set to all zeros) for I386, IA64, or AMD64 architecture.
		/// </summary>
		public const int Copyright = 7;

		/// <summary>
		/// [9] Global pointer: RVA of the value to be stored in the global pointer register. The size
		/// must be set to 0. This data directory is set to all zeros if the target architecture (for example,
		/// I386 or AMD64) does not use the concept of a global pointer.
		/// </summary>
		public const int GlobalPointer = 8;

		/// <summary>
		/// [10] TLS table address and size: Among managed compilers, only the VC++ linker and the IL
		/// assembler are able to produce the code that would use the thread local storage data.
		/// </summary>
		public const int TlsTable = 9;

		/// <summary>
		/// [11] Load Configuration table address and size: Data specific to Windows NT family of
		/// operating system (for example, the GlobalFlag value).
		/// </summary>
		public const int LoadConfigTable = 10;

		/// <summary>
		/// [12] Bound Import table address and size: This table is an array of bound import descriptors,
		/// each of which describes a DLL this image was bound up with at the time of the
		/// image creation. The descriptors also carry the time stamps of the bindings, and if the
		/// bindings are up-to-date, the OS loader uses these bindings as a "shortcut" for API import.
		/// Otherwise, the loader ignores the bindings and resolves the imported APIs through the
		/// Import tables.
		/// </summary>
		public const int BoundImport = 11;

		/// <summary>
		/// [13] Import Address table address and size: The Import Address table (IAT) is referenced
		/// from the Import Directory table (data directory 1).
		/// </summary>
		public const int IAT = 12;

		/// <summary>
		/// [14] Delay import descriptor address and size: Contains an array of 32-byte ImgDelayDescr
		/// structures, each structure describing a delay-load import. Delay-load imports are DLLs
		/// described as implicit imports but loaded as explicit imports (via calls to the LoadLibrary
		/// API). The load of delay-load DLLs is executed on demand—on the first call into such a
		/// DLL. This differs from the implicit imports, which are loaded eagerly when the importing
		/// executable is initialized.
		/// </summary>
		public const int DelayImportDescriptor = 13;

		/// <summary>
		/// [15] Common language runtime header address and size.
		/// </summary>
		public const int CLIHeader = 14;

		/// <summary>
		/// [16] Reserved: Set to all zeros.
		/// </summary>
		public const int Reserved = 15;
	}
}
