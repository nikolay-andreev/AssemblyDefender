using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace AssemblyDefender.Common
{
	public delegate bool TryConvertCallback<TSource, TTarget>(TSource source, out TTarget target);

	public static class ConvertUtils
	{
		#region Change Type

		public static T ChangeType<T>(object value)
		{
			return (T)ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
		}

		public static T ChangeType<T>(object value, IFormatProvider provider)
		{
			return (T)ChangeType(value, typeof(T), provider);
		}

		public static object ChangeType(object value, Type conversionType)
		{
			return ChangeType(value, conversionType, CultureInfo.InvariantCulture);
		}

		public static object ChangeType(object value, Type conversionType, IFormatProvider provider)
		{
			if (typeof(Nullable<>).IsAssignableFrom(conversionType))
				conversionType = Nullable.GetUnderlyingType(conversionType);

			return Convert.ChangeType(value, conversionType, provider);
		}

		public static T ChangeTypeOrDefault<T>(object value, T defaultValue)
		{
			return (T)ChangeTypeOrDefault(value, typeof(T), defaultValue, CultureInfo.InvariantCulture);
		}

		public static T ChangeTypeOrDefault<T>(object value, T defaultValue, IFormatProvider provider)
		{
			return (T)ChangeTypeOrDefault(value, typeof(T), defaultValue, provider);
		}

		public static object ChangeTypeOrDefault(object value, Type conversionType, object defaultValue)
		{
			return ChangeTypeOrDefault(value, conversionType, defaultValue, CultureInfo.InvariantCulture);
		}

		public static object ChangeTypeOrDefault(object value, Type conversionType, object defaultValue, IFormatProvider provider)
		{
			object result = null;
			try
			{
				result = ChangeType(value, conversionType, provider);
				if (result == null)
					result = defaultValue;
			}
			catch (Exception)
			{
				result = defaultValue;
			}

			return result;
		}

		#endregion

		#region Int16

		public static short ToInt16(string value)
		{
			return ToInt16(value, CultureInfo.InvariantCulture);
		}

		public static short ToInt16(string value, IFormatProvider provider)
		{
			return Convert.ToInt16(value, provider);
		}

		public static short ToInt16(byte[] value)
		{
			return BitConverter.ToInt16(value, 0);
		}

		public static short ToInt16(object value)
		{
			return Convert.ToInt16(value, CultureInfo.InvariantCulture);
		}

		public static short HexToInt16(string value)
		{
			return short.Parse(RemoveHexPrefix(value), NumberStyles.HexNumber);
		}

		#endregion

		#region Int32

		public static int ToInt32(string value)
		{
			return ToInt32(value, CultureInfo.InvariantCulture);
		}

		public static int ToInt32(string value, IFormatProvider provider)
		{
			return Convert.ToInt32(value, provider);
		}

		public static int ToInt32(byte[] value)
		{
			return BitConverter.ToInt32(value, 0);
		}

		public static int ToInt32(object value)
		{
			return Convert.ToInt32(value, CultureInfo.InvariantCulture);
		}

		public static int HexToInt32(string value)
		{
			return int.Parse(RemoveHexPrefix(value), NumberStyles.HexNumber);
		}

		#endregion

		#region Int64

		public static long ToInt64(string value)
		{
			return ToInt64(value, CultureInfo.InvariantCulture);
		}

		public static long ToInt64(string value, IFormatProvider provider)
		{
			return Convert.ToInt64(value, provider);
		}

		public static long ToInt64(byte[] value)
		{
			return BitConverter.ToInt64(value, 0);
		}

		public static long ToInt64(object value)
		{
			if (value == null)
				return 0;

			return Convert.ToInt64(value, CultureInfo.InvariantCulture);
		}

		public static long HexToInt64(string value)
		{
			return long.Parse(RemoveHexPrefix(value), NumberStyles.HexNumber);
		}

		#endregion

		#region UInt16

		public static ushort ToUInt16(string value)
		{
			return ToUInt16(value, CultureInfo.InvariantCulture);
		}

		public static ushort ToUInt16(string value, IFormatProvider provider)
		{
			return Convert.ToUInt16(value, provider);
		}

		public static ushort ToUInt16(byte[] value)
		{
			return BitConverter.ToUInt16(value, 0);
		}

		public static ushort ToUInt16(object value)
		{
			return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
		}

		public static ushort HexToUInt16(string value)
		{
			return ushort.Parse(RemoveHexPrefix(value), NumberStyles.HexNumber);
		}

		#endregion

		#region UInt32

		public static uint ToUInt32(string value)
		{
			return ToUInt32(value, CultureInfo.InvariantCulture);
		}

		public static uint ToUInt32(string value, IFormatProvider provider)
		{
			return Convert.ToUInt32(value, provider);
		}

		public static uint ToUInt32(byte[] value)
		{
			return BitConverter.ToUInt32(value, 0);
		}

		public static uint ToUInt32(object value)
		{
			return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
		}

		public static uint HexToUInt32(string value)
		{
			return uint.Parse(RemoveHexPrefix(value), NumberStyles.HexNumber);
		}

		#endregion

		#region UInt64

		public static ulong ToUInt64(string value)
		{
			return ToUInt64(value, CultureInfo.InvariantCulture);
		}

		public static ulong ToUInt64(string value, IFormatProvider provider)
		{
			return Convert.ToUInt64(value, provider);
		}

		public static ulong ToUInt64(byte[] value)
		{
			return BitConverter.ToUInt64(value, 0);
		}

		public static ulong ToUInt64(object value)
		{
			return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
		}

		public static ulong HexToUInt64(string value)
		{
			return ulong.Parse(RemoveHexPrefix(value), NumberStyles.HexNumber);
		}

		#endregion

		#region Single

		public static float ToSingle(string value)
		{
			return ToSingle(value, CultureInfo.InvariantCulture);
		}

		public static float ToSingle(string value, IFormatProvider provider)
		{
			return float.Parse(value, provider);
		}

		public static float ToSingle(byte[] value)
		{
			return BitConverter.ToSingle(value, 0);
		}

		public static float ToSingle(object value)
		{
			if (value == null)
				return 0;

			return Convert.ToSingle(value, CultureInfo.InvariantCulture);
		}

		#endregion

		#region Double

		public static double ToDouble(string value)
		{
			return ToDouble(value, CultureInfo.InvariantCulture);
		}

		public static double ToDouble(string value, IFormatProvider provider)
		{
			return double.Parse(value, provider);
		}

		public static double ToDouble(byte[] value)
		{
			return BitConverter.ToDouble(value, 0);
		}

		public static double ToDouble(object value)
		{
			if (value == null)
				return 0;

			return Convert.ToDouble(value, CultureInfo.InvariantCulture);
		}

		#endregion

		#region Decimal

		public static decimal ToDecimal(byte[] buffer, int offset = 0)
		{
			int lo =
				(
					(buffer[offset]) |
					(buffer[offset + 1] << 0x8) |
					(buffer[offset + 2] << 0x10) |
					(buffer[offset + 3] << 0x18)
				);

			int mid =
				(
					(buffer[offset + 4]) |
					(buffer[offset + 5] << 0x8) |
					(buffer[offset + 6] << 0x10) |
					(buffer[offset + 7] << 0x18)
				);

			int hi =
				(
					(buffer[offset + 8]) |
					(buffer[offset + 9] << 0x8) |
					(buffer[offset + 10] << 0x10) |
					(buffer[offset + 11] << 0x18)
				);

			int flags =
				(
					(buffer[offset + 12]) |
					(buffer[offset + 13] << 0x8) |
					(buffer[offset + 14] << 0x10) |
					(buffer[offset + 15] << 0x18)
				);

			return new decimal(new int[] { lo, mid, hi, flags });
		}

		#endregion

		#region Boolean

		public static bool ToBoolean(byte[] value)
		{
			return BitConverter.ToBoolean(value, 0);
		}

		#endregion

		#region String

		public static string ToHexString(this short value)
		{
			return value.ToString("X");
		}

		public static string ToHexString(this ushort value)
		{
			return value.ToString("X");
		}

		public static string ToHexString(this int value)
		{
			return value.ToString("X");
		}

		public static string ToHexString(this uint value)
		{
			return value.ToString("X");
		}

		public static string ToHexString(this long value)
		{
			return value.ToString("X");
		}

		public static string ToHexString(this ulong value)
		{
			return value.ToString("X");
		}

		public static string ToHexString(byte[] data)
		{
			return ToHexString(data, 0, data.Length);
		}

		public static string ToHexString(byte[] data, int index, int length)
		{
			char[] chars = new char[(length - index) * 2];
			int j = 0;
			for (int i = 0; i < data.Length; i++)
			{
				byte b = data[i];
				chars[j++] = GetHexChar(b / 0x10);
				chars[j++] = GetHexChar(b % 0x10);
			}

			return new string(chars);
		}

		public static char GetHexChar(int i)
		{
			if (i < 0xa)
			{
				return (char)(i + 0x30);
			}
			else
			{
				return (char)((i - 0xA) + 0x41);
			}
		}

		public static string ToBinString(this short value)
		{
			return Convert.ToString(value, 2);
		}

		public static string ToBinString(this ushort value)
		{
			return Convert.ToString(value, 2);
		}

		public static string ToBinString(this int value)
		{
			return Convert.ToString(value, 2);
		}

		public static string ToBinString(this uint value)
		{
			return Convert.ToString(value, 2);
		}

		public static string ToBinString(this long value)
		{
			return Convert.ToString(value, 2);
		}

		public static string ToBinString(this ulong value)
		{
			return Convert.ToString((long)value, 2);
		}

		public static string ToString(byte[] data)
		{
			return Encoding.Unicode.GetString(data);
		}

		public static string ToUTF8String(byte[] value)
		{
			return Encoding.UTF8.GetString(value);
		}

		public static string RemoveHexPrefix(string value)
		{
			if (string.IsNullOrEmpty(value))
				return value;

			if (value.StartsWith("0x") || value.StartsWith("0X"))
				value = value.Substring(2);

			return value;
		}

		#endregion

		#region Bytes

		/// <summary>
		/// <para>Returns a byte array from a string representing a hexidecimal number.</para>
		/// </summary>
		/// <param name="hexidecimalNumber">
		/// <para>The string containing a valid hexidecimal number.</para>
		/// </param>
		/// <returns><para>The byte array representing the hexidecimal.</para></returns>
		public static byte[] HexToByteArray(string hexidecimalString)
		{
			return HexToByteArray(hexidecimalString, true);
		}

		/// <summary>
		/// <para>Returns a byte array from a string representing a hexidecimal number.</para>
		/// </summary>
		/// <param name="hexidecimalNumber">The string containing a valid hexidecimal number.</param>
		/// <param name="canHaveHexPrefix">True if string can begin with 0x or 0X.</param>
		/// <returns><para>The byte array representing the hexidecimal.</para></returns>
		public static byte[] HexToByteArray(string hexidecimalString, bool canHaveHexPrefix)
		{
			if (string.IsNullOrEmpty(hexidecimalString))
				return BufferUtils.EmptyArray;

			if (hexidecimalString.Length % 2 != 0)
			{
				throw new FormatException("Invalid hex string");
			}

			var builder = new StringBuilder(hexidecimalString.ToUpper(CultureInfo.CurrentCulture));

			if (canHaveHexPrefix)
			{
				if (builder[0] == '0' && (builder[1] == 'x' || builder[1] == 'X'))
				{
					builder.Remove(0, 2);
				}
			}

			byte[] hexBytes = new byte[builder.Length / 2];
			try
			{
				for (int i = 0; i < hexBytes.Length; i++)
				{
					int stringIndex = i * 2;
					hexBytes[i] = Convert.ToByte(builder.ToString(stringIndex, 2), 16);
				}
			}
			catch (FormatException ex)
			{
				throw new FormatException("Invalid hex string", ex);
			}

			return hexBytes;
		}

		public static byte ToByte(char value)
		{
			return (byte)value;
		}

		public static byte ToByte(byte[] value)
		{
			if (value == null || value.Length == 0)
				return 0;

			return value[0];
		}

		public static byte ToByte(object value)
		{
			return ToByte(CultureInfo.InvariantCulture);
		}

		public static byte ToByte(object value, IFormatProvider provider)
		{
			return Convert.ToByte(value, provider);
		}

		public static sbyte ToSByte(byte[] value)
		{
			if (value == null || value.Length == 0)
				return 0;

			return (sbyte)value[0];
		}

		public static sbyte ToSByte(object value)
		{
			return ToSByte(CultureInfo.InvariantCulture);
		}

		public static sbyte ToSByte(object value, IFormatProvider provider)
		{
			return Convert.ToSByte(value, provider);
		}

		public static byte[] ToUTF8Bytes(string value)
		{
			return Encoding.UTF8.GetBytes(value);
		}

		#endregion

		#region Char

		public static char ToChar(byte[] value)
		{
			return BitConverter.ToChar(value, 0);
		}

		#endregion

		#region Object

		public static string ToHexString(object value)
		{
			var formattable = value as IFormattable;
			if (formattable != null)
			{
				return formattable.ToString("X", CultureInfo.InvariantCulture);
			}
			else
			{
				return string.Empty;
			}
		}

		#endregion

		#region DateTime

		private static DateTime OriginDateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));

		/// <summary>
		/// time_t is an int representing the number of seconds since Midnight UTC 1 Jan 1970 on the Gregorian Calendar.
		/// </summary>
		/// <param name="time_t"></param>
		/// <returns></returns>
		public static DateTime ToDateTime(int time_t)
		{
			DateTime convertedDateTime = OriginDateTime + new TimeSpan(time_t * TimeSpan.TicksPerSecond);
			if (TimeZone.CurrentTimeZone.IsDaylightSavingTime(convertedDateTime))
			{
				var daylightTime = TimeZone.CurrentTimeZone.GetDaylightChanges(convertedDateTime.Year);
				convertedDateTime = convertedDateTime + daylightTime.Delta;
			}

			return convertedDateTime;
		}

		/// <summary>
		/// time_t is an int representing the number of seconds since Midnight UTC 1 Jan 1970 on the Gregorian Calendar.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public static int To_time_t(this DateTime time)
		{
			var convertedDateTime = time;
			if (TimeZone.CurrentTimeZone.IsDaylightSavingTime(convertedDateTime))
			{
				var daylightTime = TimeZone.CurrentTimeZone.GetDaylightChanges(convertedDateTime.Year);
				convertedDateTime = convertedDateTime - daylightTime.Delta;
			}

			long diff = convertedDateTime.Ticks - OriginDateTime.Ticks;
			return (int)(diff / TimeSpan.TicksPerSecond);
		}

		#endregion

		#region Base32

		private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

		/// <summary>
		/// Converts an array of bytes to a Base32 string.
		/// </summary>
		public static string ToBase32String(byte[] bytes)
		{
			return ToBase32String(bytes, 0, bytes.Length, Base32Alphabet);
		}

		/// <summary>
		/// Converts an array of bytes to a Base32 string.
		/// </summary>
		public static string ToBase32String(byte[] bytes, string alphabet)
		{
			return ToBase32String(bytes, 0, bytes.Length, alphabet);
		}

		/// <summary>
		/// Converts an array of bytes to a Base32 string.
		/// </summary>
		public static string ToBase32String(byte[] bytes, int offset, int count)
		{
			return ToBase32String(bytes, offset, count, Base32Alphabet);
		}

		/// <summary>
		/// Converts an array of bytes to a Base32 string.
		/// </summary>
		public static string ToBase32String(byte[] bytes, int offset, int count, string alphabet)
		{
			int charCount = Math.Max((int)Math.Ceiling(count * 8 / 5.0), 1);
			char[] charArray = new char[charCount];
			int charIndex = 0;
			int alphabetIndex;
			int byteIndex = offset;
			int hi = 5;

			while (byteIndex < count)
			{
				// Do we need to use the next byte?
				if (hi > 8)
				{
					// Get the last piece from the current byte, shift it to the right
					// and increment the byte counter
					alphabetIndex = (byte)(bytes[byteIndex++] >> (hi - 5));
					if (byteIndex != count)
					{
						// if we are not at the end, get the first piece from
						// the next byte, clear it and shift it to the left
						alphabetIndex = (byte)(((byte)(bytes[byteIndex] << (16 - hi)) >> 3) | alphabetIndex);
					}

					hi -= 3;
				}
				else if (hi == 8)
				{
					alphabetIndex = (byte)(bytes[byteIndex++] >> 3);
					hi -= 3;
				}
				else
				{
					// Simply get the stuff from the current byte
					alphabetIndex = (byte)((byte)(bytes[byteIndex] << (8 - hi)) >> 3);
					hi += 5;
				}

				charArray[charIndex++] = alphabet[alphabetIndex];
			}

			return new string(charArray);
		}

		/// <summary>
		/// Converts a Base32 string into an array of bytes.
		/// </summary>
		public static byte[] FromBase32String(string str)
		{
			return FromBase32String(str, Base32Alphabet);
		}

		/// <summary>
		/// Converts a Base32 string into an array of bytes.
		/// </summary>
		public static byte[] FromBase32String(string str, string alphabet)
		{
			int stringLength = str.Length;
			int byteCount = stringLength * 5 / 8;
			byte[] bytes = new byte[byteCount];

			if (stringLength < 3)
			{
				bytes[0] = (byte)(alphabet.IndexOf(str[0]) | alphabet.IndexOf(str[1]) << 5);
				return bytes;
			}

			int bit_buffer = (alphabet.IndexOf(str[0]) | alphabet.IndexOf(str[1]) << 5);
			int bits_in_buffer = 10;
			int currentCharIndex = 2;

			for (int i = 0; i < byteCount; i++)
			{
				bytes[i] = (byte)bit_buffer;
				bit_buffer >>= 8;
				bits_in_buffer -= 8;
				while (bits_in_buffer < 8 && currentCharIndex < stringLength)
				{
					bit_buffer |= alphabet.IndexOf(str[currentCharIndex++]) << bits_in_buffer;
					bits_in_buffer += 5;
				}
			}

			return bytes;
		}

		#endregion
	}
}
