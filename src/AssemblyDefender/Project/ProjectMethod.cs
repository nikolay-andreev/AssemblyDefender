using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ProjectMethod
	{
		#region Fields

		private int _flags;
		public string _name;

		#endregion

		#region Ctors

		public ProjectMethod()
		{
		}

		internal ProjectMethod(IBinaryAccessor accessor, ProjectReadState state)
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

		public bool ObfuscateControlFlow
		{
			get { return _flags.IsBitAtIndexOn(1); }
			set { _flags = _flags.SetBitAtIndex(1, value); }
		}

		public bool ObfuscateControlFlowChanged
		{
			get { return _flags.IsBitAtIndexOn(2); }
			set { _flags = _flags.SetBitAtIndex(2, value); }
		}

		public bool Rename
		{
			get { return _flags.IsBitAtIndexOn(3); }
			set { _flags = _flags.SetBitAtIndex(3, value); }
		}

		public bool RenameChanged
		{
			get { return _flags.IsBitAtIndexOn(4); }
			set { _flags = _flags.SetBitAtIndex(4, value); }
		}

		public bool EncryptIL
		{
			get { return _flags.IsBitAtIndexOn(5); }
			set { _flags = _flags.SetBitAtIndex(5, value); }
		}

		public bool EncryptILChanged
		{
			get { return _flags.IsBitAtIndexOn(6); }
			set { _flags = _flags.SetBitAtIndex(6, value); }
		}

		public bool ObfuscateStrings
		{
			get { return _flags.IsBitAtIndexOn(7); }
			set { _flags = _flags.SetBitAtIndex(7, value); }
		}

		public bool ObfuscateStringsChanged
		{
			get { return _flags.IsBitAtIndexOn(8); }
			set { _flags = _flags.SetBitAtIndex(8, value); }
		}

		public bool RemoveUnused
		{
			get { return _flags.IsBitAtIndexOn(9); }
			set { _flags = _flags.SetBitAtIndex(9, value); }
		}

		public bool RemoveUnusedChanged
		{
			get { return _flags.IsBitAtIndexOn(10); }
			set { _flags = _flags.SetBitAtIndex(10, value); }
		}

		public bool Devirtualize
		{
			get { return _flags.IsBitAtIndexOn(11); }
			set { _flags = _flags.SetBitAtIndex(11, value); }
		}

		public bool DevirtualizeChanged
		{
			get { return _flags.IsBitAtIndexOn(12); }
			set { _flags = _flags.SetBitAtIndex(12, value); }
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

			if (!state.ObfuscateControlFlow)
			{
				ObfuscateControlFlow = false;
				ObfuscateControlFlowChanged = false;
			}

			if (!state.EncryptIL)
			{
				EncryptIL = false;
				EncryptILChanged = false;
			}

			if (!state.ObfuscateStrings)
			{
				ObfuscateStrings = false;
				ObfuscateStringsChanged = false;
			}

			if (!state.RemoveUnusedMembers)
			{
				RemoveUnused = false;
				RemoveUnusedChanged = false;
			}

			if (!state.DevirtualizeMethods)
			{
				Devirtualize = false;
				DevirtualizeChanged = false;
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
