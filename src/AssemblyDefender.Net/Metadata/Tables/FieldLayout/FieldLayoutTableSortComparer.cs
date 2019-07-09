using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	public class FieldLayoutTableSortComparer : TableSortComparer
	{
		public FieldLayoutTableSortComparer(int[] fieldArray)
			: base(fieldArray)
		{
		}

		public FieldLayoutTableSortComparer(FieldLayoutTable table)
		{
			var rows = table.GetRows();
			int count = rows.Length;
			_values = new int[count];
			for (int i = 0; i < count; i++)
			{
				_values[i] = rows[i].Field;
			}
		}
	}
}
