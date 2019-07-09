using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common
{
	/// <summary>
	///
	/// </summary>
	/// <example>
	/// OR truth table
	/// 0 | 0 = 0
	/// 1 | 0 = 1
	/// 0 | 1 = 1
	/// 1 | 1 = 1
	/// AND truth table
	/// 0 & 0 = 0
	/// 1 & 0 = 0
	/// 0 & 1 = 0
	/// 1 & 1 = 1
	/// XOR truth table
	/// 0 ^ 0 = 0
	/// 1 ^ 0 = 1
	/// 0 ^ 1 = 1
	/// 1 ^ 1 = 0
	/// </example>
	public static class BitUtils
	{
		#region Byte

		public static bool IsBitAtIndexOn(this byte data, int index)
		{
			return (data & (1 << index)) != 0;
		}

		public static byte SetBitAtIndex(this byte data, int index, bool value)
		{
			byte mask = (byte)(1 << index);
			if (value)
				return (byte)(data | mask);
			else
				return (byte)(data & ~mask);
		}

		#endregion

		#region Int16

		public static bool IsBitAtIndexOn(this short data, int index)
		{
			return (data & (1 << index)) != 0;
		}

		public static short SetBitAtIndex(this short data, int index, bool value)
		{
			short mask = (short)(1 << index);
			if (value)
				return (short)(data | mask);
			else
				return (short)(data & ~mask);
		}

		#endregion

		#region Int32

		public static int GetBits(this int data, int mask)
		{
			return data & mask;
		}

		public static bool IsBitsOn(this int data, int mask)
		{
			return (data & mask) == mask;
		}

		public static bool IsAnyBitOn(this int data, int mask)
		{
			return (data & mask) != 0;
		}

		public static bool IsBitAtIndexOn(this int data, int index)
		{
			return (data & (1 << index)) != 0;
		}

		public static int SetBitAtIndex(this int data, int index, bool value)
		{
			int mask = 1 << index;
			if (value)
				return data | mask;
			else
				return data & ~mask;
		}

		public static int SetBits(this int data, int mask, int value)
		{
			return ((data & ~mask) | value);
		}

		public static int SetBits(this int data, int mask, bool value)
		{
			if (value)
				return data | mask; // Set on
			else
				return data & ~mask; // Set off
		}

		public static int ToggleBits(this int data)
		{
			return data ^= unchecked((int)uint.MaxValue);
		}

		public static int ToggleBits(this int data, int mask)
		{
			return data ^= mask;
		}

		public static string ToBitsString(this int data)
		{
			var builder = new StringBuilder(0x20);

			for (int i = 0; i < 0x20; i++)
			{
				if ((data & 0x80000000L) != 0L)
					builder.Append("1");
				else
					builder.Append("0");

				data = data << 1;
			}

			return builder.ToString();
		}

		#endregion

		#region Int64

		public static long GetBits(this long data, long mask)
		{
			return data & mask;
		}

		public static bool IsBitsOn(this long data, long mask)
		{
			return (data & mask) == mask;
		}

		public static bool IsAnyBitOn(this long data, long mask)
		{
			return (data & mask) != 0;
		}

		public static bool IsBitAtIndexOn(this long data, int index)
		{
			return (data & (1 << index)) != 0;
		}

		public static long SetBitAtIndex(this long data, int index, bool value)
		{
			long mask = 1 << index;
			if (value)
				return data | mask;
			else
				return data & ~mask;
		}

		public static long SetBits(this long data, long mask, long value)
		{
			return ((data & ~mask) | value);
		}

		public static long SetBits(this long data, long mask, bool value)
		{
			if (value)
				return data | mask; // Set on
			else
				return data & ~mask; // Set off
		}

		public static long ToggleBits(this long data)
		{
			return data ^= unchecked((long)ulong.MaxValue);
		}

		public static long ToggleBits(this long data, long mask)
		{
			return data ^= mask;
		}

		#endregion
	}
}
