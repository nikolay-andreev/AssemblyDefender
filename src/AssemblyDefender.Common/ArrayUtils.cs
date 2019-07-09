using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common
{
	public static class ArrayUtils
	{
		public static bool BinarySearch<T, TKey>(
			T[] array, int index, int length, TKey key,
			Func<T, TKey, int> comparison,
			out int resultIndex)
		{
			int num = index;
			int num2 = (index + length) - 1;
			while (num <= num2)
			{
				int num3 = num + ((num2 - num) >> 1);
				int num4 = comparison(array[num3], key);
				if (num4 == 0)
				{
					resultIndex = num3;
					return true;
				}

				if (num4 < 0)
					num = num3 + 1;
				else
					num2 = num3 - 1;
			}

			resultIndex = ~num;
			return false;
		}

		public static bool BinarySearchSmallRange<T, TKey>(
			T[] array, int index, int length, TKey key,
			Func<T, TKey, int> comparison,
			out int fromIndex, out int toIndex)
		{
			if (!BinarySearch<T, TKey>(array, index, length, key, comparison, out fromIndex))
			{
				toIndex = fromIndex;
				return false;
			}

			toIndex = fromIndex + 1;

			// Binary search has located random index of sequential values. Check prev and next items.
			while (fromIndex > index)
			{
				if (0 != comparison(array[fromIndex - 1], key))
					break;

				fromIndex--;
			}

			int count = index + length;
			while (toIndex < count)
			{
				if (0 != comparison(array[toIndex], key))
					break;

				toIndex++;
			}

			return true;
		}

		public static T[] NewCopy<T>(this T[] array)
		{
			var newArray = new T[array.Length];
			Array.Copy(array, newArray, array.Length);

			return newArray;
		}
	}
}
