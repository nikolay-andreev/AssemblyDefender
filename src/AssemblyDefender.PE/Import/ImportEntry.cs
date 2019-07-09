using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// Represents imported method.
	/// </summary>
	public class ImportEntry : ICloneable
	{
		#region Fields

		private string _name;
		private int _ordinal;
		internal ImportModuleTable _parent;

		#endregion

		#region Ctors

		public ImportEntry()
		{
		}

		public ImportEntry(string name, int ordinal)
		{
			_name = name;
			_ordinal = ordinal;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name to import.
		/// </summary>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// An index into the export name pointer table. A match is attempted first with this value. If it fails,
		/// a binary search is performed on the DLL’s export name pointer table.
		/// This value is 1 based and use 0 for N/A.
		/// </summary>
		public int Ordinal
		{
			get { return _ordinal; }
			set { _ordinal = value; }
		}

		public ImportModuleTable Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public ImportEntry Clone()
		{
			var copy = new ImportEntry();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(ImportEntry copy)
		{
			copy._name = _name;
			copy._ordinal = _ordinal;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}
