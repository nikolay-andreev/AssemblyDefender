using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in Event table.
	/// </summary>
	public struct EventRow : IEquatable<EventRow>
	{
		/// <summary>
		/// Binary flags of the event characteristics.
		/// 2-byte unsigned integer.
		/// </summary>
		public ushort Flags;

		/// <summary>
		/// The name of the event, which must be a simple name no longer than 1,023 bytes in UTF-8 encoding.
		/// Offset in the #Strings stream.
		/// </summary>
		public int Name;

		/// <summary>
		/// The type associated with the event. The coded token indexes a TypeDef, TypeRef, or
		/// TypeSpec record. The class indexed by this token is either a delegate or a class providing
		/// the necessary functionality similar to that of a delegate.
		/// Coded token of type TypeDefOrRef.
		/// </summary>
		public int EventType;

		public bool Equals(EventRow other)
		{
			if (Flags != other.Flags)
				return false;

			if (Name != other.Name)
				return false;

			if (EventType != other.EventType)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				(int)Flags ^
				Name ^
				EventType;
		}
	}
}
