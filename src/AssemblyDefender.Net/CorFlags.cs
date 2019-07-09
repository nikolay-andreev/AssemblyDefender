namespace AssemblyDefender.Net
{
	/// <summary>
	/// The Flags field of the common language runtime header
	/// </summary>
	public enum CorFlags : uint
	{
		None = 0,

		/// <summary>
		/// The image file contains IL code only, with no embedded native unmanaged code except
		/// the start-up stub (which simply executes an indirect jump to the CLR entry point).
		/// Common language runtime-aware operating systems (such as Windows XP and newer) ignore
		/// the start-up stub and invoke the CLR automatically, so for all practical purposes
		/// the file can be considered pure IL.
		/// </summary>
		ILOnly = 0x0000001,

		/// <summary>
		/// The image file can be loaded only into a 32-bit process. This flag is set alone when native
		/// unmanaged code is embedded in the PE file or when the .reloc section contains additional
		/// relocations or is set in combination with _ILONLY when the executable does not contain
		/// additional relocations but is in some way 32-bit specific (for example, invokes an unmanaged
		/// 32-bit specific API or uses 4-byte integers to store pointers).
		/// </summary>
		F32BitsRequired = 0x0000002,

		/// <summary>
		/// This flag is obsolete and should not be set. Setting it—as the IL assembler allows,
		/// using the .corflags directive—will render your module unloadable.
		/// </summary>
		ILLibrary = 0x00000004,

		/// <summary>
		/// The image file is protected with a strong name signature. The strong name signature
		/// includes the public key and the signature hash and is a part of an assembly’s identity,
		/// along with the assembly name, version number, and culture information. This flag is set
		/// when the strong name signing procedure is applied to the image file. No compiler,
		/// including ILAsm, can set this flag explicitly.
		/// </summary>
		StrongNameSigned = 0x0000008,

		/// <summary>
		/// The executable’s entry point is an unmanaged method. The EntryPointToken/EntryPointRVA
		/// field of the CLR header contains the RVA of this native method. This flag was introduced
		/// in version 2.0 of the CLR.
		/// </summary>
		NativeEntrypoint = 0x00000010,

		/// <summary>
		/// The CLR loader and the JIT compiler are required to track debug information about the methods.
		/// This flag is not used.
		/// </summary>
		TrackDebugData = 0x00010000,
	}
}
