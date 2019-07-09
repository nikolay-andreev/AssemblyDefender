using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common
{
	public static class NumericUtils
	{
		#region Int16

		public static short Align(this short value, int align)
		{
			return (short)((value + align - 1) & ~(align - 1));
		}

		#endregion

		#region Int32

		public static int Align(this int value, int align)
		{
			return (int)((value + align - 1) & ~(align - 1));
		}

		#endregion

		#region Int64

		public static long Align(this long value, int align)
		{
			return (int)((value + align - 1) & ~(align - 1));
		}

		#endregion

		#region UInt16

		public static ushort Align(this ushort value, int align)
		{
			return (ushort)((value + align - 1) & ~(align - 1));
		}

		public static ushort Align(this ushort value, ushort align)
		{
			return (ushort)((value + align - 1) & ~(align - 1));
		}

		#endregion

		#region UInt32

		public static uint Align(this uint value, int align)
		{
			return (uint)((value + align - 1) & ~(align - 1));
		}

		public static uint Align(this uint value, uint align)
		{
			return ((value + align - 1) & ~(align - 1));
		}

		#endregion

		#region Int64

		public static ulong Align(this ulong value, int align)
		{
			uint ualign = (uint)align;
			return ((value + ualign - 1) & ~(ualign - 1));
		}

		public static ulong Align(this ulong value, uint align)
		{
			return ((value + align - 1) & ~(align - 1));
		}

		#endregion
	}
}
