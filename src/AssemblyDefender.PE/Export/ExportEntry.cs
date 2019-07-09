using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public abstract class ExportEntry
	{
		#region Fields

		private string _name;
		internal ExportTable _parent;

		#endregion

		#region Ctors

		protected ExportEntry()
		{
		}

		protected ExportEntry(string name)
		{
			_name = name;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Exported symbol name.
		/// </summary>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public ExportTable Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public ExportEntry Clone()
		{
			return (ExportEntry)((ICloneable)this).Clone();
		}

		protected void CopyTo(ExportEntry copy)
		{
			copy._name = _name;
		}

		#endregion
	}
}
