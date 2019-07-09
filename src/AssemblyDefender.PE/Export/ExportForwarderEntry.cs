using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class ExportForwarderEntry : ExportEntry, ICloneable
	{
		#region Fields

		private string _forwarder;

		#endregion

		#region Ctors

		public ExportForwarderEntry()
		{
		}

		public ExportForwarderEntry(string name, string forwarder)
			: base(name)
		{
			_forwarder = forwarder;
		}

		#endregion

		#region Properties

		/// <summary>
		/// This string gives the DLL name and the name of the export (for example, “MYDLL.expfunc”) or the DLL name
		/// and the ordinal number of the export (for example, “MYDLL.#27”).
		///
		/// A forwarder RVA exports a definition from some other image, making it appear as if it were being exported
		/// by the current image. Thus, the symbol is simultaneously imported and exported.
		///
		/// For example, in Kernel32.dll in Windows XP, the export named “HeapAlloc” is forwarded to the string
		/// "NTDLL.RtlAllocateHeap." This allows applications to use the Windows XP–specific module Ntdll.dll without
		/// actually containing import references to it. The application’s import table refers only to Kernel32.dll.
		/// Therefore, the application is not specific to Windows XP and can run on any Win32 system.
		/// </summary>
		public string Forwarder
		{
			get { return _forwarder; }
			set { _forwarder = value; }
		}

		#endregion

		#region Methods

		public new ExportForwarderEntry Clone()
		{
			var copy = new ExportForwarderEntry();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(ExportForwarderEntry copy)
		{
			copy._forwarder = _forwarder;

			base.CopyTo(copy);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}
