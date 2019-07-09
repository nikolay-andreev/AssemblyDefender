using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	public interface IReadOnlyList<out T> : IEnumerable<T>
	{
		T this[int index]
		{
			get;
		}

		int Count
		{
			get;
		}
	}
}
