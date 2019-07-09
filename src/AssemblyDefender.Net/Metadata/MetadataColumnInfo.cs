using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public struct MetadataColumnInfo
	{
		#region Fields

		private string _name;
		private int _type;

		#endregion

		#region Ctors

		internal MetadataColumnInfo(string name, int type)
		{
			_name = name;
			_type = type;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		public int Type
		{
			get { return _type; }
		}

		public bool IsTableIndex
		{
			get { return _type >= 0 && _type <= 44; }
		}

		public bool IsCodedToken
		{
			get { return _type >= 64 && _type <= 76; }
		}

		#endregion
	}
}
