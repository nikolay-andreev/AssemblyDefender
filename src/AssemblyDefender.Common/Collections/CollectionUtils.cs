using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	public static class CollectionUtils
	{
		public static void Shuffle<T>(this List<T> list)
		{
			Shuffle(list, new Random());
		}

		public static void Shuffle<T>(this List<T> list, Random random)
		{
			for (int i = list.Count - 1; i > 0; i--)
			{
				int n = random.Next(i + 1);
				var temp = list[i];
				list[i] = list[n];
				list[n] = temp;
			}
		}

		public static void Shuffle<T>(this T[] array)
		{
			Shuffle(array, new Random());
		}

		public static void Shuffle<T>(this T[] array, Random random)
		{
			for (int i = array.Length - 1; i > 0; i--)
			{
				int n = random.Next(i + 1);
				var temp = array[i];
				array[i] = array[n];
				array[n] = temp;
			}
		}

		public static T FirstOrDefaultOfType<T>(this IEnumerable source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			foreach (object o in source)
			{
				if (o is T)
				{
					return (T)o;
				}
			}

			return default(T);
		}

		public static T Get<K, T>(this Dictionary<K, T> dict, K key, Func<T> acquire)
		{
			T value;
			if (!dict.TryGetValue(key, out value))
			{
				value = acquire();
				dict[key] = value;
			}

			return value;
		}

		public static T SyncGet<K, T>(this Dictionary<K, T> dict, K key, Func<T> acquire)
		{
			T value;
			if (!dict.TryGetValue(key, out value))
			{
				lock (dict)
				{
					if (!dict.TryGetValue(key, out value))
					{
						value = acquire();
						dict[key] = value;
					}
				}
			}

			return value;
		}
	}
}
