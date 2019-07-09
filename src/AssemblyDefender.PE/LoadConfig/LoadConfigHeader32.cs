using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// The 32-bit load configuration structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct LoadConfigHeader32
	{
		/// <summary>
		/// Flags that indicate attributes of the file, currently unused.
		/// </summary>
		public int Characteristics;

		/// <summary>
		/// Date and time stamp value. The value is represented in the number of seconds that have elapsed since
		/// midnight (00:00:00), January 1, 1970, Universal Coordinated Time, according to the system clock.
		/// The time stamp can be printed by using the C runtime (CRT) time function.
		/// </summary>
		public int TimeDateStamp;

		/// <summary>
		/// Major version number.
		/// </summary>
		public short MajorVersion;

		/// <summary>
		/// Minor version number.
		/// </summary>
		public short MinorVersion;

		/// <summary>
		/// The global loader flags to clear for this process as the loader starts the process.
		/// </summary>
		public uint GlobalFlagsClear;

		/// <summary>
		/// The global loader flags to set for this process as the loader starts the process.
		/// </summary>
		public uint GlobalFlagsSet;

		/// <summary>
		/// The default timeout value to use for this process�s critical sections that are abandoned.
		/// </summary>
		public int CriticalSectionDefaultTimeout;

		/// <summary>
		/// Memory that must be freed before it is returned to the system, in bytes.
		/// </summary>
		public int DeCommitFreeBlockThreshold;

		/// <summary>
		/// Total amount of free memory, in bytes.
		/// </summary>
		public int DeCommitTotalFreeThreshold;

		/// <summary>
		/// [x86 only] The VA of a list of addresses where the LOCK prefix is used so that they can be replaced
		/// with NOP on single processor machines.
		/// </summary>
		public uint LockPrefixTable;

		/// <summary>
		/// Maximum allocation size, in bytes.
		/// </summary>
		public int MaximumAllocationSize;

		/// <summary>
		/// Maximum virtual memory size, in bytes.
		/// </summary>
		public int VirtualMemoryThreshold;

		/// <summary>
		/// Process heap flags that correspond to the first argument of the HeapCreate function. These flags apply
		/// to the process heap that is created during process startup.
		/// </summary>
		public int ProcessHeapFlags;

		/// <summary>
		/// Setting this field to a non-zero value is equivalent to calling SetProcessAffinityMask with this value
		/// during process startup (.exe only)
		/// </summary>
		public uint ProcessAffinityMask;

		/// <summary>
		/// The service pack version identifier.
		/// </summary>
		public short CSDVersion;

		/// <summary>
		/// Must be zero.
		/// </summary>
		public short Reserved1;

		/// <summary>
		/// Reserved for use by the system.
		/// </summary>
		public uint EditList;

		/// <summary>
		/// A pointer to a cookie that is used by Visual C++ or GS implementation.
		/// </summary>
		public uint SecurityCookie;

		/// <summary>
		/// [x86 only] The VA of the sorted table of RVAs of each valid, unique SE handler in the image.
		/// </summary>
		public uint SEHandlerTable;

		/// <summary>
		/// [x86 only] The count of unique handlers in the table.
		/// </summary>
		public uint SEHandlerCount;
	}
}
