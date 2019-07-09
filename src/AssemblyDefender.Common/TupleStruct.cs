using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common
{
	public struct TupleStruct<T1>
	{
		public T1 Item1;

		public TupleStruct(T1 item1)
		{
			this.Item1 = item1;
		}
	}

	public struct TupleStruct<T1, T2>
	{
		public T1 Item1;
		public T2 Item2;

		public TupleStruct(T1 item1, T2 item2)
		{
			this.Item1 = item1;
			this.Item2 = item2;
		}
	}

	public struct TupleStruct<T1, T2, T3>
	{
		public T1 Item1;
		public T2 Item2;
		public T3 Item3;

		public TupleStruct(T1 item1, T2 item2, T3 item3)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
		}
	}

	public struct TupleStruct<T1, T2, T3, T4>
	{
		public T1 Item1;
		public T2 Item2;
		public T3 Item3;
		public T4 Item4;

		public TupleStruct(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
			this.Item4 = item4;
		}
	}
}
