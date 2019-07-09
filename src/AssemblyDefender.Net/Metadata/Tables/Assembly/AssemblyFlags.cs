using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Assembly flags indicating whether the assembly is strong
	/// named (set automatically by the metadata emission API if PublicKey is present), whether
	/// the JIT tracking and/or optimization is enabled (set automatically on assembly load), and
	/// whether the assembly can be retargeted at run time to an assembly of a different version.
	/// JIT tracking is the mapping of IL instruction offsets to addresses of native code produced
	/// by the JIT compiler; this mapping is used during the debugging of the managed code.
	/// </summary>
	public static class AssemblyFlags
	{
		/// <summary>
		/// The assembly is side-by-side compatible.
		/// </summary>
		public const int SideBySideCompatible = 0x0000;

		/// <summary>
		/// The assembly reference holds the full (unhashed) public key.
		/// </summary>
		public const int PublicKey = 0x0001;

		/// <summary>
		/// The implementation of this assembly used at runtime is not expected to match the version seen at compile time.
		/// </summary>
		public const int Retargetable = 0x0100;

		/// <summary>
		/// From DebuggableAttribute.
		/// </summary>
		public const int DisableJITcompileOptimizer = 0x4000;

		/// <summary>
		/// From DebuggableAttribute.
		/// </summary>
		public const int EnableJITcompileTracking = 0x8000;

		#region Processor Architecture

		/// <summary>
		/// Processor Architecture unspecified.
		/// </summary>
		public const int PA_None = 0x0000;

		/// <summary>
		/// Processor Architecture: neutral (PE32)
		/// </summary>
		public const int PA_MSIL = 0x0010;

		/// <summary>
		/// Processor Architecture: x86 (PE32)
		/// </summary>
		public const int PA_x86 = 0x0020;

		/// <summary>
		/// Processor Architecture: Itanium (PE32+)
		/// </summary>
		public const int PA_IA64 = 0x0030;

		/// <summary>
		/// Processor Architecture: AMD X64 (PE32+)
		/// </summary>
		public const int PA_AMD64 = 0x0040;

		/// <summary>
		/// Propagate PA flags to AssemblyRef record.
		/// </summary>
		public const int PA_Specified = 0x0080;

		/// <summary>
		/// Bits describing the processor architecture.
		/// </summary>
		public const int PA_Mask = 0x0070;

		/// <summary>
		/// Bits describing the PA incl. Specified.
		/// </summary>
		public const int PA_FullMask = 0x00F0;

		/// <summary>
		/// NOT A FLAG, shift count in PA flags <--> index conversion
		/// </summary>
		public const int PA_Shift = 0x0004;

		#endregion
	}
}
