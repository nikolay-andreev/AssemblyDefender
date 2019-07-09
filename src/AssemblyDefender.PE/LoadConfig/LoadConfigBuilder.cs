using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class LoadConfigBuilder : BuildTask
	{
		#region Fields

		private string _sectionName = PESectionNames.Text;
		private int _blobPriority = 1000;
		private LoadConfigInfo _info;
		private BuildBlob _blob;

		#endregion

		#region Properties

		public string SectionName
		{
			get { return _sectionName; }
			set { _sectionName = value; }
		}

		public int BlobPriority
		{
			get { return _blobPriority; }
			set { _blobPriority = value; }
		}

		public LoadConfigInfo Info
		{
			get { return _info; }
			set { _info = value; }
		}

		public BuildBlob Blob
		{
			get { return _blob; }
			set { _blob = value; }
		}

		#endregion

		#region Methods

		public override void Build()
		{
			if (_info == null)
				return;

			var _blob = new BuildBlob();

			int pos = 0;

			if (PE.Is32Bits)
			{
				_blob.Write(ref pos, (int)_info.Characteristics);
				_blob.Write(ref pos, (int)_info.TimeDateStamp.To_time_t());
				_blob.Write(ref pos, (short)_info.MajorVersion);
				_blob.Write(ref pos, (short)_info.MinorVersion);
				_blob.Write(ref pos, (uint)_info.GlobalFlagsClear);
				_blob.Write(ref pos, (uint)_info.GlobalFlagsSet);
				_blob.Write(ref pos, (int)_info.CriticalSectionDefaultTimeout);
				_blob.Write(ref pos, (int)_info.DeCommitFreeBlockThreshold);
				_blob.Write(ref pos, (int)_info.DeCommitTotalFreeThreshold);
				_blob.Write(ref pos, (uint)_info.LockPrefixTable);
				_blob.Write(ref pos, (int)_info.MaximumAllocationSize);
				_blob.Write(ref pos, (int)_info.VirtualMemoryThreshold);
				_blob.Write(ref pos, (int)_info.ProcessHeapFlags);
				_blob.Write(ref pos, (uint)_info.ProcessAffinityMask);
				_blob.Write(ref pos, (short)_info.CSDVersion);
				_blob.Write(ref pos, (short)_info.Reserved1);
				_blob.Write(ref pos, (uint)_info.EditList);
				_blob.Write(ref pos, (uint)_info.SecurityCookie);
				_blob.Write(ref pos, (uint)_info.SEHandlerTable);
				_blob.Write(ref pos, (uint)_info.SEHandlerCount);
			}
			else
			{
				_blob.Write(ref pos, (int)_info.Characteristics);
				_blob.Write(ref pos, (int)_info.TimeDateStamp.To_time_t());
				_blob.Write(ref pos, (short)_info.MajorVersion);
				_blob.Write(ref pos, (short)_info.MinorVersion);
				_blob.Write(ref pos, (uint)_info.GlobalFlagsClear);
				_blob.Write(ref pos, (uint)_info.GlobalFlagsSet);
				_blob.Write(ref pos, (int)_info.CriticalSectionDefaultTimeout);
				_blob.Write(ref pos, (long)_info.DeCommitFreeBlockThreshold);
				_blob.Write(ref pos, (long)_info.DeCommitTotalFreeThreshold);
				_blob.Write(ref pos, (ulong)_info.LockPrefixTable);
				_blob.Write(ref pos, (long)_info.MaximumAllocationSize);
				_blob.Write(ref pos, (long)_info.VirtualMemoryThreshold);
				_blob.Write(ref pos, (ulong)_info.ProcessAffinityMask);
				_blob.Write(ref pos, (int)_info.ProcessHeapFlags);
				_blob.Write(ref pos, (short)_info.CSDVersion);
				_blob.Write(ref pos, (short)_info.Reserved1);
				_blob.Write(ref pos, (ulong)_info.EditList);
				_blob.Write(ref pos, (ulong)_info.SecurityCookie);
				_blob.Write(ref pos, (ulong)_info.SEHandlerTable);
				_blob.Write(ref pos, (ulong)_info.SEHandlerCount);
			}

			// Set data directories
			// For compatibility with Windows XP and earlier versions of Windows,
			// the size must be 64 for x86 images.
			PE.Fixups.Add(
				new PEBuilder.SetDataDirectoryFromBlobRVAFixup(
					DataDirectories.LoadConfigTable, _blob, 0, 64));

			// Add _blobs
			BuildSection section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);
		}

		#endregion
	}
}
