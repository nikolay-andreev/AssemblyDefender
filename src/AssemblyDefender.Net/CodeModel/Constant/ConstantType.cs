namespace AssemblyDefender.Net
{
	public enum ConstantType
	{
		/// <summary>
		/// Boolean value, encoded as true or false
		/// </summary>
		Bool,

		/// <summary>
		/// 8-bit integer with the value specified in parentheses.
		/// </summary>
		Int8,

		/// <summary>
		/// 16-bit integer with the value specified in parentheses.
		/// </summary>
		Int16,

		/// <summary>
		/// 32-bit integer with the value specified in parentheses.
		/// </summary>
		Int32,

		/// <summary>
		/// 64-bit integer with the value specified in parentheses.
		/// </summary>
		Int64,

		/// <summary>
		/// 8-bit unsigned integer with the value specified in parentheses.
		/// </summary>
		UInt8,

		/// <summary>
		/// 16-bit unsigned integer with the value specified in parentheses.
		/// </summary>
		UInt16,

		/// <summary>
		/// 32-bit unsigned integer with the value specified in parentheses.
		/// </summary>
		UInt32,

		/// <summary>
		/// 64-bit unsigned integer with the value specified in parentheses.
		/// </summary>
		UInt64,

		/// <summary>
		/// 32-bit floating-point number, with the floating-point number specified in parentheses.
		/// </summary>
		Float32,

		/// <summary>
		/// Int64 is binary representation of double
		/// </summary>
		Float64,

		/// <summary>
		/// 16-bit unsigned integer (Unicode character)
		/// </summary>
		Char,

		/// <summary>
		/// String stored as Unicode
		/// </summary>
		String,

		/// <summary>
		/// String of bytes, stored without conversion.
		/// Can be with one zero byte to make the total byte-count even number.
		/// </summary>
		ByteArray,

		/// <summary>
		/// Null object reference
		/// </summary>
		Nullref,
	}
}
