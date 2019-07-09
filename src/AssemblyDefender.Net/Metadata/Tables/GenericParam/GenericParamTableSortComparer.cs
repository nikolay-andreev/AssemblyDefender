using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	public class GenericParamTableSortComparer : TableSortComparer
	{
		private int[] _numberArray;

		public GenericParamTableSortComparer(int[] ownerArray, int[] numberArray)
			: base(ownerArray)
		{
			_numberArray = numberArray;
		}

		public GenericParamTableSortComparer(GenericParamTable table)
		{
			var rows = table.GetRows();
			int count = rows.Length;
			_values = new int[count];
			_numberArray = new int[count];
			for (int i = 0; i < count; i++)
			{
				_values[i] = rows[i].Owner;
				_numberArray[i] = rows[i].Number;
			}
		}

		public override int Compare(int x, int y)
		{
			// Owner
			var owner1 = _values[x];
			var owner2 = _values[y];

			if (owner1 < owner2)
				return -1;

			if (owner1 > owner2)
				return 1;

			// Number
			var number1 = _numberArray[x];
			var number2 = _numberArray[y];

			if (number1 < number2)
				return -1;

			if (number1 > number2)
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
