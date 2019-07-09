using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class ExportRVAEntry : ExportEntry, ICloneable
	{
		#region Fields

		private uint _exportRVA;

		#endregion

		#region Ctors

		public ExportRVAEntry()
		{
		}

		public ExportRVAEntry(string name, uint exportRVA)
			: base(name)
		{
			_exportRVA = exportRVA;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The address of the exported symbol when loaded into memory, relative to the image base. For example,
		/// the address of an exported function.
		/// </summary>
		public uint ExportRVA
		{
			get { return _exportRVA; }
			set { _exportRVA = value; }
		}

		#endregion

		#region Methods

		public new ExportRVAEntry Clone()
		{
			var copy = new ExportRVAEntry();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(ExportRVAEntry copy)
		{
			copy._exportRVA = _exportRVA;

			base.CopyTo(copy);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}
