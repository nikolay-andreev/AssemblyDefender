using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class ResourceTableEntry : ResourceEntry, ICloneable
	{
		#region Fields

		private ResourceTable _table;

		#endregion

		#region Ctors

		public ResourceTableEntry()
		{
		}

		#endregion

		#region Properties

		public ResourceTable Table
		{
			get
			{
				if (_table == null)
				{
					_table = new ResourceTable();
					_table._parent = this;
				}

				return _table;
			}
			set
			{
				if (value != null)
				{
					_table = value;
					_table._parent = this;
				}
				else
				{
					_table = null;
				}
			}
		}

		#endregion

		#region Methods

		public new ResourceTableEntry Clone()
		{
			var copy = new ResourceTableEntry();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(ResourceTableEntry copy)
		{
			if (_table != null)
			{
				copy._table = _table.Clone();
				copy._table._parent = copy;
			}

			base.CopyTo(copy);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		#region Static

		internal new static ResourceTableEntry Load(IBinaryAccessor accessor, long basePosition)
		{
			var entry = new ResourceTableEntry();
			entry._table = ResourceTable.Load(accessor, basePosition);
			entry._table._parent = entry;

			return entry;
		}

		#endregion
	}
}
