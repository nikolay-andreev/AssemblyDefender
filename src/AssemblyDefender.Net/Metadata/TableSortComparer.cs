using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public class TableSortComparer : IComparer<int>
	{
		protected int[] _values;

		protected TableSortComparer()
		{
		}

		public TableSortComparer(int[] values)
		{
			_values = values;
		}

		public virtual int Compare(int x, int y)
		{
			// Value
			var value1 = _values[x];
			var value2 = _values[y];

			if (value1 < value2)
				return -1;

			if (value1 > value2)
				return 1;

			// Index
			if (x < y)
				return -1;

			if (x > y)
				return 1;

			return 0;
		}
	}
}
