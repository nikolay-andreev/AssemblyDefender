using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	public class ByteArrayEqualityComparer : EqualityComparer<byte[]>
	{
		public static readonly ByteArrayEqualityComparer Instance = new ByteArrayEqualityComparer();

		public override bool Equals(byte[] x, byte[] y)
		{
			if (x == null || y == null)
				return x == y;

			if (x == y)
				return true;

			if (x.Length != y.Length)
				return false;

			for (int i = 0; i < x.Length; i++)
			{
				if (x[i] != y[i])
					return false;
			}

			return true;
		}

		public bool Equals(byte[] x, int xOffset, byte[] y, int yOffset, int length)
		{
			if (x == null || y == null)
				return x == y;

			if (x == y)
				return true;

			for (int i = 0; i < length; i++)
			{
				if (x[xOffset + i] != y[yOffset + i])
					return false;
			}

			return true;
		}

		public override int GetHashCode(byte[] obj)
		{
			int hash = 0;
			for (int i = 0; i < obj.Length; i++)
			{
				hash = (hash * 37) ^ obj[i];
			}

			return hash;
		}

		public int GetHashCode(byte[] obj, int offset, int length)
		{
			int hash = 0;
			for (int i = offset; i < length; i++)
			{
				hash = (hash * 37) ^ obj[i];
			}

			return hash;
		}
	}
}
