using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Row in EventMap table.
	/// </summary>
	public struct EventMapRow : IEquatable<EventMapRow>
	{
		/// <summary>
		/// The type declaring the events.
		/// RID in the TypeDef table.
		/// </summary>
		public int Parent;

		/// <summary>
		/// The beginning of the events declared by the type indexed by the Parent entry.
		/// The mechanism of addressing the events in this case is identical to the mechanism
		/// used by TypeDef records to address the Method and Field records belonging to a
		/// certain TypeDef. In the optimized metadata model (the #~ stream), the records in the
		/// Event table are ordered by the declaring type. In the unoptimized model (the #- stream),
		/// the event records are not so ordered, and an intermediate lookup metadata table, EventPtr, is used.
		/// RID in the Event table.
		/// </summary>
		public int EventList;

		public bool Equals(EventMapRow other)
		{
			if (Parent != other.Parent)
				return false;

			if (EventList != other.EventList)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return
				Parent ^
				EventList;
		}
	}
}
