using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	///
	/// </summary>
	public class LoadConfigInfo : ICloneable
	{
		#region Fields

		private int _characteristics;
		private DateTime _timeDateStamp;
		private short _majorVersion;
		private short _minorVersion;
		private uint _globalFlagsClear;
		private uint _globalFlagsSet;
		private int _criticalSectionDefaultTimeout;
		private long _deCommitFreeBlockThreshold;
		private long _deCommitTotalFreeThreshold;
		private ulong _lockPrefixTable;
		private long _maximumAllocationSize;
		private long _virtualMemoryThreshold;
		private ulong _processAffinityMask;
		private int _processHeapFlags;
		private short _CSDVersion;
		private short _reserved1;
		private ulong _editList;
		private ulong _securityCookie;
		private ulong _SEHandlerTable;
		private ulong _SEHandlerCount;

		#endregion

		#region Ctors

		public LoadConfigInfo()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Flags that indicate attributes of the file, currently unused.
		/// </summary>
		public int Characteristics
		{
			get { return _characteristics; }
			set { _characteristics = value; }
		}

		/// <summary>
		/// Date and time stamp value. The value is represented in the number of seconds that have elapsed since
		/// midnight (00:00:00), January 1, 1970, Universal Coordinated Time, according to the system clock.
		/// The time stamp can be printed by using the C runtime (CRT) time function.
		/// </summary>
		public DateTime TimeDateStamp
		{
			get { return _timeDateStamp; }
		}

		/// <summary>
		/// Major version number.
		/// </summary>
		public short MajorVersion
		{
			get { return _majorVersion; }
			set { _majorVersion = value; }
		}

		/// <summary>
		/// Minor version number.
		/// </summary>
		public short MinorVersion
		{
			get { return _minorVersion; }
			set { _minorVersion = value; }
		}

		/// <summary>
		/// The global loader flags to clear for this process as the loader starts the process.
		/// </summary>
		public uint GlobalFlagsClear
		{
			get { return _globalFlagsClear; }
			set { _globalFlagsClear = value; }
		}

		/// <summary>
		/// The global loader flags to set for this process as the loader starts the process.
		/// </summary>
		public uint GlobalFlagsSet
		{
			get { return _globalFlagsSet; }
			set { _globalFlagsSet = value; }
		}

		/// <summary>
		/// The default timeout value to use for this process�s critical sections that are abandoned.
		/// </summary>
		public int CriticalSectionDefaultTimeout
		{
			get { return _criticalSectionDefaultTimeout; }
			set { _criticalSectionDefaultTimeout = value; }
		}

		/// <summary>
		/// Memory that must be freed before it is returned to the system, in bytes.
		/// </summary>
		public long DeCommitFreeBlockThreshold
		{
			get { return _deCommitFreeBlockThreshold; }
			set { _deCommitFreeBlockThreshold = value; }
		}

		/// <summary>
		/// Total amount of free memory, in bytes.
		/// </summary>
		public long DeCommitTotalFreeThreshold
		{
			get { return _deCommitTotalFreeThreshold; }
			set { _deCommitTotalFreeThreshold = value; }
		}

		/// <summary>
		/// [x86 only] The VA of a list of addresses where the LOCK prefix is used so that they can be replaced
		/// with NOP on single processor machines.
		/// </summary>
		public ulong LockPrefixTable
		{
			get { return _lockPrefixTable; }
			set { _lockPrefixTable = value; }
		}

		/// <summary>
		/// Maximum allocation size, in bytes.
		/// </summary>
		public long MaximumAllocationSize
		{
			get { return _maximumAllocationSize; }
			set { _maximumAllocationSize = value; }
		}

		/// <summary>
		/// Maximum virtual memory size, in bytes.
		/// </summary>
		public long VirtualMemoryThreshold
		{
			get { return _virtualMemoryThreshold; }
			set { _virtualMemoryThreshold = value; }
		}

		/// <summary>
		/// Setting this field to a non-zero value is equivalent to calling SetProcessAffinityMask with this value
		/// during process startup (.exe only)
		/// </summary>
		public ulong ProcessAffinityMask
		{
			get { return _processAffinityMask; }
			set { _processAffinityMask = value; }
		}

		/// <summary>
		/// Process heap flags that correspond to the first argument of the HeapCreate function. These flags apply
		/// to the process heap that is created during process startup.
		/// </summary>
		public int ProcessHeapFlags
		{
			get { return _processHeapFlags; }
			set { _processHeapFlags = value; }
		}

		/// <summary>
		/// The service pack version identifier.
		/// </summary>
		public short CSDVersion
		{
			get { return _CSDVersion; }
			set { _CSDVersion = value; }
		}

		/// <summary>
		/// Must be zero.
		/// </summary>
		public short Reserved1
		{
			get { return _reserved1; }
			set { _reserved1 = value; }
		}

		/// <summary>
		/// Reserved for use by the system.
		/// </summary>
		public ulong EditList
		{
			get { return _editList; }
			set { _editList = value; }
		}

		/// <summary>
		/// A pointer to a cookie that is used by Visual C++ or GS implementation.
		/// </summary>
		public ulong SecurityCookie
		{
			get { return _securityCookie; }
			set { _securityCookie = value; }
		}

		/// <summary>
		/// [x86 only] The VA of the sorted table of RVAs of each valid, unique SE handler in the image.
		/// </summary>
		public ulong SEHandlerTable
		{
			get { return _SEHandlerTable; }
			set { _SEHandlerTable = value; }
		}

		/// <summary>
		/// [x86 only] The count of unique handlers in the table.
		/// </summary>
		public ulong SEHandlerCount
		{
			get { return _SEHandlerCount; }
			set { _SEHandlerCount = value; }
		}

		#endregion

		#region Methods

		public LoadConfigInfo Clone()
		{
			var copy = new LoadConfigInfo();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(LoadConfigInfo copy)
		{
			copy._characteristics = _characteristics;
			copy._timeDateStamp = _timeDateStamp;
			copy._majorVersion = _majorVersion;
			copy._minorVersion = _minorVersion;
			copy._globalFlagsClear = _globalFlagsClear;
			copy._globalFlagsSet = _globalFlagsSet;
			copy._criticalSectionDefaultTimeout = _criticalSectionDefaultTimeout;
			copy._deCommitFreeBlockThreshold = _deCommitFreeBlockThreshold;
			copy._deCommitTotalFreeThreshold = _deCommitTotalFreeThreshold;
			copy._lockPrefixTable = _lockPrefixTable;
			copy._maximumAllocationSize = _maximumAllocationSize;
			copy._virtualMemoryThreshold = _virtualMemoryThreshold;
			copy._processAffinityMask = _processAffinityMask;
			copy._processHeapFlags = _processHeapFlags;
			copy._CSDVersion = _CSDVersion;
			copy._reserved1 = _reserved1;
			copy._editList = _editList;
			copy._securityCookie = _securityCookie;
			copy._SEHandlerTable = _SEHandlerTable;
			copy._SEHandlerCount = _SEHandlerCount;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		#region Static

		public static LoadConfigInfo TryLoad(PEImage image)
		{
			try
			{
				return Load(image);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static unsafe LoadConfigInfo Load(PEImage image)
		{
			if (image == null)
				return null;

			var dd = image.Directories[DataDirectories.LoadConfigTable];
			if (dd.IsNull)
				return null;

			var info = new LoadConfigInfo();

			using (var accessor = image.OpenImageToSectionData(dd.RVA))
			{
				if (image.Is32Bits)
				{
					fixed (byte* pBuff = accessor.ReadBytes(sizeof(LoadConfigHeader32)))
					{
						var header = *(LoadConfigHeader32*)pBuff;
						info._characteristics = header.Characteristics;
						info._timeDateStamp = ConvertUtils.ToDateTime(header.TimeDateStamp);
						info._majorVersion = header.MajorVersion;
						info._minorVersion = header.MinorVersion;
						info._globalFlagsClear = header.GlobalFlagsClear;
						info._globalFlagsSet = header.GlobalFlagsSet;
						info._criticalSectionDefaultTimeout = header.CriticalSectionDefaultTimeout;
						info._deCommitFreeBlockThreshold = header.DeCommitFreeBlockThreshold;
						info._deCommitTotalFreeThreshold = header.DeCommitTotalFreeThreshold;
						info._lockPrefixTable = header.LockPrefixTable;
						info._maximumAllocationSize = header.MaximumAllocationSize;
						info._virtualMemoryThreshold = header.VirtualMemoryThreshold;
						info._processAffinityMask = header.ProcessAffinityMask;
						info._processHeapFlags = header.ProcessHeapFlags;
						info._CSDVersion = header.CSDVersion;
						info._reserved1 = header.Reserved1;
						info._editList = header.EditList;
						info._securityCookie = header.SecurityCookie;
						info._SEHandlerTable = header.SEHandlerTable;
						info._SEHandlerCount = header.SEHandlerCount;
					}
				}
				else
				{
					fixed (byte* pBuff = accessor.ReadBytes(sizeof(LoadConfigHeader64)))
					{
						var header = *(LoadConfigHeader64*)pBuff;
						info._characteristics = header.Characteristics;
						info._timeDateStamp = ConvertUtils.ToDateTime(header.TimeDateStamp);
						info._majorVersion = header.MajorVersion;
						info._minorVersion = header.MinorVersion;
						info._globalFlagsClear = header.GlobalFlagsClear;
						info._globalFlagsSet = header.GlobalFlagsSet;
						info._criticalSectionDefaultTimeout = header.CriticalSectionDefaultTimeout;
						info._deCommitFreeBlockThreshold = header.DeCommitFreeBlockThreshold;
						info._deCommitTotalFreeThreshold = header.DeCommitTotalFreeThreshold;
						info._lockPrefixTable = header.LockPrefixTable;
						info._maximumAllocationSize = header.MaximumAllocationSize;
						info._virtualMemoryThreshold = header.VirtualMemoryThreshold;
						info._processAffinityMask = header.ProcessAffinityMask;
						info._processHeapFlags = header.ProcessHeapFlags;
						info._CSDVersion = header.CSDVersion;
						info._reserved1 = header.Reserved1;
						info._editList = header.EditList;
						info._securityCookie = header.SecurityCookie;
						info._SEHandlerTable = header.SEHandlerTable;
						info._SEHandlerCount = header.SEHandlerCount;
					}
				}
			}

			return info;
		}

		#endregion
	}
}
