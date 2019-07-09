using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ProjectField
	{
		#region Fields

		private int _flags;
		public string _name;

		#endregion

		#region Ctors

		public ProjectField()
		{
		}

		internal ProjectField(IBinaryAccessor accessor, ProjectReadState state)
		{
			Read(accessor, state);
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public bool NameChanged
		{
			get { return _flags.IsBitAtIndexOn(0); }
			set { _flags = _flags.SetBitAtIndex(0, value); }
		}

		public bool Rename
		{
			get { return _flags.IsBitAtIndexOn(1); }
			set { _flags = _flags.SetBitAtIndex(1, value); }
		}

		public bool RenameChanged
		{
			get { return _flags.IsBitAtIndexOn(2); }
			set { _flags = _flags.SetBitAtIndex(2, value); }
		}

		public bool RemoveUnused
		{
			get { return _flags.IsBitAtIndexOn(5); }
			set { _flags = _flags.SetBitAtIndex(5, value); }
		}

		public bool RemoveUnusedChanged
		{
			get { return _flags.IsBitAtIndexOn(6); }
			set { _flags = _flags.SetBitAtIndex(6, value); }
		}

		internal bool IsEmpty
		{
			get { return _flags == 0; }
		}

		#endregion

		#region Methods

		internal void Scavenge(ProjectScavengeState state)
		{
			if (!state.RenameMembers)
			{
				Rename = false;
				RenameChanged = false;
				NameChanged = false;
			}

			if (!state.RemoveUnusedMembers)
			{
				RemoveUnused = false;
				RemoveUnusedChanged = false;
			}
		}

		internal void Read(IBinaryAccessor accessor, ProjectReadState state)
		{
			_flags = accessor.ReadInt32();

			if (NameChanged)
				_name = state.GetString(accessor.Read7BitEncodedInt());
		}

		internal void Write(IBinaryAccessor accessor, ProjectWriteState state)
		{
			accessor.Write((int)_flags);

			if (NameChanged)
				accessor.Write7BitEncodedInt(state.SetString(_name));
		}

		#endregion
	}
}
