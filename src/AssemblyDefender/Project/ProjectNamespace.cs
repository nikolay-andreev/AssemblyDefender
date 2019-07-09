using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ProjectNamespace
	{
		#region Fields

		private int _flags;

		#endregion

		#region Ctors

		public ProjectNamespace()
		{
		}

		internal ProjectNamespace(IBinaryAccessor accessor, ProjectReadState state)
		{
			Read(accessor, state);
		}

		#endregion

		#region Properties

		public bool ObfuscateControlFlow
		{
			get { return _flags.IsBitAtIndexOn(0); }
			set { _flags = _flags.SetBitAtIndex(0, value); }
		}

		public bool ObfuscateControlFlowChanged
		{
			get { return _flags.IsBitAtIndexOn(1); }
			set { _flags = _flags.SetBitAtIndex(1, value); }
		}

		public bool RenameMembers
		{
			get { return _flags.IsBitAtIndexOn(2); }
			set { _flags = _flags.SetBitAtIndex(2, value); }
		}

		public bool RenameMembersChanged
		{
			get { return _flags.IsBitAtIndexOn(3); }
			set { _flags = _flags.SetBitAtIndex(3, value); }
		}

		public bool RenamePublicTypes
		{
			get { return _flags.IsBitAtIndexOn(4); }
			set { _flags = _flags.SetBitAtIndex(4, value); }
		}

		public bool RenamePublicTypesChanged
		{
			get { return _flags.IsBitAtIndexOn(5); }
			set { _flags = _flags.SetBitAtIndex(5, value); }
		}

		public bool RenamePublicMethods
		{
			get { return _flags.IsBitAtIndexOn(6); }
			set { _flags = _flags.SetBitAtIndex(6, value); }
		}

		public bool RenamePublicMethodsChanged
		{
			get { return _flags.IsBitAtIndexOn(7); }
			set { _flags = _flags.SetBitAtIndex(7, value); }
		}

		public bool RenamePublicFields
		{
			get { return _flags.IsBitAtIndexOn(8); }
			set { _flags = _flags.SetBitAtIndex(8, value); }
		}

		public bool RenamePublicFieldsChanged
		{
			get { return _flags.IsBitAtIndexOn(9); }
			set { _flags = _flags.SetBitAtIndex(9, value); }
		}

		public bool RenamePublicProperties
		{
			get { return _flags.IsBitAtIndexOn(10); }
			set { _flags = _flags.SetBitAtIndex(10, value); }
		}

		public bool RenamePublicPropertiesChanged
		{
			get { return _flags.IsBitAtIndexOn(11); }
			set { _flags = _flags.SetBitAtIndex(11, value); }
		}

		public bool RenamePublicEvents
		{
			get { return _flags.IsBitAtIndexOn(12); }
			set { _flags = _flags.SetBitAtIndex(12, value); }
		}

		public bool RenamePublicEventsChanged
		{
			get { return _flags.IsBitAtIndexOn(13); }
			set { _flags = _flags.SetBitAtIndex(13, value); }
		}

		public bool EncryptIL
		{
			get { return _flags.IsBitAtIndexOn(14); }
			set { _flags = _flags.SetBitAtIndex(14, value); }
		}

		public bool EncryptILChanged
		{
			get { return _flags.IsBitAtIndexOn(15); }
			set { _flags = _flags.SetBitAtIndex(15, value); }
		}

		public bool ObfuscateStrings
		{
			get { return _flags.IsBitAtIndexOn(16); }
			set { _flags = _flags.SetBitAtIndex(16, value); }
		}

		public bool ObfuscateStringsChanged
		{
			get { return _flags.IsBitAtIndexOn(17); }
			set { _flags = _flags.SetBitAtIndex(17, value); }
		}

		public bool RemoveUnusedMembers
		{
			get { return _flags.IsBitAtIndexOn(18); }
			set { _flags = _flags.SetBitAtIndex(18, value); }
		}

		public bool RemoveUnusedMembersChanged
		{
			get { return _flags.IsBitAtIndexOn(19); }
			set { _flags = _flags.SetBitAtIndex(19, value); }
		}

		public bool SealTypes
		{
			get { return _flags.IsBitAtIndexOn(20); }
			set { _flags = _flags.SetBitAtIndex(20, value); }
		}

		public bool SealTypesChanged
		{
			get { return _flags.IsBitAtIndexOn(21); }
			set { _flags = _flags.SetBitAtIndex(21, value); }
		}

		public bool DevirtualizeMethods
		{
			get { return _flags.IsBitAtIndexOn(22); }
			set { _flags = _flags.SetBitAtIndex(22, value); }
		}

		public bool DevirtualizeMethodsChanged
		{
			get { return _flags.IsBitAtIndexOn(23); }
			set { _flags = _flags.SetBitAtIndex(23, value); }
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
				RenameMembers = false;
				RenameMembersChanged = false;
				RenamePublicTypes = false;
				RenamePublicTypesChanged = false;
				RenamePublicMethods = false;
				RenamePublicMethodsChanged = false;
				RenamePublicFields = false;
				RenamePublicFieldsChanged = false;
				RenamePublicProperties = false;
				RenamePublicPropertiesChanged = false;
				RenamePublicEvents = false;
				RenamePublicEventsChanged = false;
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
				RemoveUnusedMembers = false;
				RemoveUnusedMembersChanged = false;
			}

			if (!state.SealTypes)
			{
				SealTypes = false;
				SealTypesChanged = false;
			}

			if (!state.DevirtualizeMethods)
			{
				DevirtualizeMethods = false;
				DevirtualizeMethodsChanged = false;
			}
		}

		internal void Read(IBinaryAccessor accessor, ProjectReadState state)
		{
			_flags = accessor.ReadInt32();
		}

		internal void Write(IBinaryAccessor accessor, ProjectWriteState state)
		{
			accessor.Write((int)_flags);
		}

		#endregion
	}
}
