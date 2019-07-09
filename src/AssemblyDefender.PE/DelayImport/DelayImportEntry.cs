using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class DelayImportEntry : ICloneable
	{
		#region Fields

		private uint _funcRVA;
		private string _name;
		private int _ordinal;
		internal DelayImportModuleTable _parent;

		#endregion

		#region Ctors

		public DelayImportEntry()
		{
		}

		public DelayImportEntry(uint funcRVA, string name, int ordinal)
		{
			_funcRVA = funcRVA;
			_name = name;
			_ordinal = ordinal;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The delay-load helper updates these pointers with the real entry points so that the thunks are no longer
		/// in the calling loop.
		/// </summary>
		public uint FuncRVA
		{
			get { return _funcRVA; }
			set { _funcRVA = value; }
		}

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

		public DelayImportModuleTable Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public DelayImportEntry Clone()
		{
			var copy = new DelayImportEntry();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(DelayImportEntry copy)
		{
			copy._funcRVA = _funcRVA;
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
