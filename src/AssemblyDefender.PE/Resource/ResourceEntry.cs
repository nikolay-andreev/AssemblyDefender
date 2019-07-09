using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public abstract class ResourceEntry
	{
		#region Fields

		private int _id;
		private string _name;
		internal ResourceTable _parent;

		#endregion

		#region Ctors

		public ResourceEntry()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Integer that identifies Type, Name, or Language.
		/// </summary>
		public int ID
		{
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>
		/// Type, Name, or Language identifier, depending on level of table.
		/// </summary>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Entry is identified either by Name or ID.
		/// </summary>
		public bool IdentifiedByName
		{
			get { return !string.IsNullOrEmpty(_name); }
		}

		public ResourceTable Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public ResourceEntry Clone()
		{
			return (ResourceEntry)((ICloneable)this).Clone();
		}

		protected void CopyTo(ResourceEntry copy)
		{
			copy._id = _id;
			copy._name = _name;
		}

		#endregion

		#region Static

		internal static ResourceEntry Load(IBinaryAccessor accessor, long basePosition)
		{
			uint nameOrId = accessor.ReadUInt32();
			uint dataOrTable = accessor.ReadUInt32();

			long origPosition = accessor.Position;

			accessor.Position = basePosition + (dataOrTable & 0x7fffffff);

			ResourceEntry entry;
			if ((dataOrTable & 0x80000000) != 0)
			{
				// High bit 1. The lower 31 bits are the address of another resource directory table (the next level down).
				entry = ResourceTableEntry.Load(accessor, basePosition);
			}
			else
			{
				// High bit 0. Address of a Resource Data entry (a leaf).
				entry = ResourceDataEntry.Load(accessor);
			}

			accessor.Position = origPosition;

			if ((nameOrId & 0x80000000) != 0)
			{
				// High bit 1. The address of a string that gives the Type, Name, or Language ID entry, depending on level of table.
				accessor.Position = basePosition + (nameOrId & 0x7fffffff);

				int length = accessor.ReadUInt16();
				entry._name = accessor.ReadString(length, Encoding.Unicode);

				accessor.Position = origPosition;
			}
			else
			{
				// High bit 0. A 32-bit integer that identifies the Type, Name, or Language ID entry.
				entry._id = (int)(nameOrId & 0x7fffffff);
			}

			return entry;
		}

		#endregion
	}
}
