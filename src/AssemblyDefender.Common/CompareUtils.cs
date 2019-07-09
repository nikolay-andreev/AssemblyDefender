using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AssemblyDefender.Common
{
	public static class CompareUtils
	{
		public static int Compare(object x, object y, PropertyDescriptor property)
		{
			object xProperty = property.GetValue(x);
			object yProperty = property.GetValue(y);

			return Compare(xProperty, yProperty);
		}

		public static int Compare(object x, object y)
		{
			return Comparer.Default.Compare(x, y);
		}

		public static int Compare<T>(T x, T y)
		{
			return Comparer<T>.Default.Compare(x, y);
		}

		/// <summary>
		/// <para>Determine if two byte arrays are equal.</para>
		/// </summary>
		/// <param name="byte1">
		/// <para>The first byte array to compare.</para>
		/// </param>
		/// <param name="byte2">
		/// <para>The byte array to compare to the first.</para>
		/// </param>
		/// <returns>
		/// <para><see langword="true"/> if the two byte arrays are equal; otherwise <see langword="false"/>.</para>
		/// </returns>
		public static bool Equals(byte[] x, byte[] y, bool nullAndZeroAreEquals = false)
		{
			if (object.ReferenceEquals(x, y))
				return true;

			if (x == null)
				return nullAndZeroAreEquals ? y.Length == 0 : false;

			if (y == null)
				return nullAndZeroAreEquals ? x.Length == 0 : false;

			if (x == null || y == null)
				return false;

			if (x.Length != y.Length)
				return false;

			for (int i = 0; i < x.Length; i++)
			{
				if (x[i] != y[i])
				{
					return false;
				}
			}

			return true;
		}

		public static bool Equals(byte[] x, int xIndex, byte[] y, int yIndex, int count, bool nullAndZeroAreEquals = false)
		{
			if (object.ReferenceEquals(x, y))
				return true;

			if (x == null)
				return nullAndZeroAreEquals ? y.Length == 0 : false;

			if (y == null)
				return nullAndZeroAreEquals ? x.Length == 0 : false;

			if (x.Length < xIndex + count)
				return false;

			if (y.Length < yIndex + count)
				return false;

			for (int i = xIndex, j = yIndex, k = 0; k < count; i++, j++, k++)
			{
				if (x[i] != y[j])
				{
					return false;
				}
			}

			return true;
		}

		public static bool Equals(Version x, Version y, bool nullAndZeroAreEquals = false)
		{
			if (object.ReferenceEquals(x ,y))
				return true;

			if (x == null)
				return nullAndZeroAreEquals ? y.IsZero() : false;

			if (y == null)
				return nullAndZeroAreEquals ? x.IsZero() : false;

			if (x.Major != y.Major)
				return false;

			if (x.Minor != y.Minor)
				return false;

			if (x.Build != y.Build)
				return false;

			if (x.Revision != y.Revision)
				return false;

			return true;
		}

		/// <summary>
		/// Compares two enumerations of elements for equality by calling the Equals method on each pair of elements.
		/// The enumerations must be of equal length, or must both be null, in order to be considered equal.
		/// </summary>
		/// <typeparam name="T">The element type of the collection</typeparam>
		/// <param name="left">An enumeration of elements. The enumeration may be null, but the elements may not.</param>
		/// <param name="right">An enumeration of elements. The enumeration may be null, but the elements may not.</param>
		public static bool Equals<T>(IEnumerable<T> left, IEnumerable<T> right)
		{
			if (left == null)
				return right == null || !right.GetEnumerator().MoveNext();

			var leftEnum = left.GetEnumerator();
			if (right == null)
				return !leftEnum.MoveNext();

			var rightEnum = right.GetEnumerator();
			while (leftEnum.MoveNext())
			{
				if (!rightEnum.MoveNext())
					return false;

				if (!leftEnum.Current.Equals(rightEnum.Current))
					return false;
			}

			return !rightEnum.MoveNext();
		}

		/// <summary>
		/// Compares two enumerations of elements for equality by calling the Equals method on each pair of elements.
		/// The enumerations must be of equal length, or must both be null, in order to be considered equal.
		/// </summary>
		/// <typeparam name="T">The element type of the collection</typeparam>
		/// <param name="left">An enumeration of elements. The enumeration may be null, but the elements may not.</param>
		/// <param name="right">An enumeration of elements. The enumeration may be null, but the elements may not.</param>
		/// <param name="comparer">An object that compares two enumeration elements for equality.</param>
		public static bool Equals<T>(IEnumerable<T> left, IEnumerable<T> right, IEqualityComparer<T> comparer)
		{
			if (left == null)
				return right == null || !right.GetEnumerator().MoveNext();

			var leftEnum = left.GetEnumerator();
			if (right == null)
				return !leftEnum.MoveNext();

			var rightEnum = right.GetEnumerator();
			while (leftEnum.MoveNext())
			{
				if (!rightEnum.MoveNext())
					return false;

				if (!comparer.Equals(leftEnum.Current, rightEnum.Current))
					return false;
			}

			return !rightEnum.MoveNext();
		}
	}
}
