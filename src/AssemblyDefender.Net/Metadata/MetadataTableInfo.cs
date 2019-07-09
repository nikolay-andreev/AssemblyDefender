using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public struct MetadataTableInfo
	{
		private string _name;
		private short _keyColumnIndex;
		private MetadataColumnInfo[] _columns;

		internal MetadataTableInfo(string name, short keyColumnIndex, MetadataColumnInfo[] columns)
		{
			_name = name;
			_keyColumnIndex = keyColumnIndex;
			_columns = columns;
		}

		public string Name
		{
			get { return _name; }
		}

		public short KeyColumnIndex
		{
			get { return _keyColumnIndex; }
		}

		public MetadataColumnInfo[] Columns
		{
			get { return _columns; }
		}
	}
}
