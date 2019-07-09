using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ProjectType
	{
		#region Fields

		private int _flags;
		private int _flags2;
		private string _name;
		private string _namespace;

		#endregion

		#region Ctors

		public ProjectType()
		{
		}

		internal ProjectType(IBinaryAccessor accessor, ProjectReadState state)
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

		public string Namespace
		{
			get { return _namespace; }
			set { _namespace = value; }
		}

		public bool NamespaceChanged
		{
			get { return _flags.IsBitAtIndexOn(1); }
			set { _flags = _flags.SetBitAtIndex(1, value); }
		}

		public bool ObfuscateControlFlow
		{
			get { return _flags.IsBitAtIndexOn(2); }
			set { _flags = _flags.SetBitAtIndex(2, value); }
		}

		public bool ObfuscateControlFlowChanged
		{
			get { return _flags.IsBitAtIndexOn(3); }
			set { _flags = _flags.SetBitAtIndex(3, value); }
		}

		public bool Rename
		{
			get { return _flags.IsBitAtIndexOn(4); }
			set { _flags = _flags.SetBitAtIndex(4, value); }
		}

		public bool RenameChanged
		{
			get { return _flags.IsBitAtIndexOn(5); }
			set { _flags = _flags.SetBitAtIndex(5, value); }
		}

		public bool RenameMembers
		{
			get { return _flags.IsBitAtIndexOn(6); }
			set { _flags = _flags.SetBitAtIndex(6, value); }
		}

		public bool RenameMembersChanged
		{
			get { return _flags.IsBitAtIndexOn(7); }
			set { _flags = _flags.SetBitAtIndex(7, value); }
		}

		public bool RenamePublicTypes
		{
			get { return _flags.IsBitAtIndexOn(8); }
			set { _flags = _flags.SetBitAtIndex(8, value); }
		}

		public bool RenamePublicTypesChanged
		{
			get { return _flags.IsBitAtIndexOn(9); }
			set { _flags = _flags.SetBitAtIndex(9, value); }
		}

		public bool RenamePublicMethods
		{
			get { return _flags.IsBitAtIndexOn(10); }
			set { _flags = _flags.SetBitAtIndex(10, value); }
		}

		public bool RenamePublicMethodsChanged
		{
			get { return _flags.IsBitAtIndexOn(11); }
			set { _flags = _flags.SetBitAtIndex(11, value); }
		}

		public bool RenamePublicFields
		{
			get { return _flags.IsBitAtIndexOn(12); }
			set { _flags = _flags.SetBitAtIndex(12, value); }
		}

		public bool RenamePublicFieldsChanged
		{
			get { return _flags.IsBitAtIndexOn(13); }
			set { _flags = _flags.SetBitAtIndex(13, value); }
		}

		public bool RenamePublicProperties
		{
			get { return _flags.IsBitAtIndexOn(14); }
			set { _flags = _flags.SetBitAtIndex(14, value); }
		}

		public bool RenamePublicPropertiesChanged
		{
			get { return _flags.IsBitAtIndexOn(15); }
			set { _flags = _flags.SetBitAtIndex(15, value); }
		}

		public bool RenamePublicEvents
		{
			get { return _flags.IsBitAtIndexOn(16); }
			set { _flags = _flags.SetBitAtIndex(16, value); }
		}

		public bool RenamePublicEventsChanged
		{
			get { return _flags.IsBitAtIndexOn(17); }
			set { _flags = _flags.SetBitAtIndex(17, value); }
		}

		public bool EncryptIL
		{
			get { return _flags2.IsBitAtIndexOn(0); }
			set { _flags2 = _flags2.SetBitAtIndex(0, value); }
		}

		public bool EncryptILChanged
		{
			get { return _flags2.IsBitAtIndexOn(1); }
			set { _flags2 = _flags2.SetBitAtIndex(1, value); }
		}

		public bool ObfuscateStrings
		{
			get { return _flags2.IsBitAtIndexOn(2); }
			set { _flags2 = _flags2.SetBitAtIndex(2, value); }
		}

		public bool ObfuscateStringsChanged
		{
			get { return _flags2.IsBitAtIndexOn(3); }
			set { _flags2 = _flags2.SetBitAtIndex(3, value); }
		}

		public bool RemoveUnused
		{
			get { return _flags2.IsBitAtIndexOn(4); }
			set { _flags2 = _flags2.SetBitAtIndex(4, value); }
		}

		public bool RemoveUnusedChanged
		{
			get { return _flags2.IsBitAtIndexOn(5); }
			set { _flags2 = _flags2.SetBitAtIndex(5, value); }
		}

		public bool RemoveUnusedMembers
		{
			get { return _flags2.IsBitAtIndexOn(6); }
			set { _flags2 = _flags2.SetBitAtIndex(6, value); }
		}

		public bool RemoveUnusedMembersChanged
		{
			get { return _flags2.IsBitAtIndexOn(7); }
			set { _flags2 = _flags2.SetBitAtIndex(7, value); }
		}

		public bool Seal
		{
			get { return _flags2.IsBitAtIndexOn(8); }
			set { _flags2 = _flags2.SetBitAtIndex(8, value); }
		}

		public bool SealChanged
		{
			get { return _flags2.IsBitAtIndexOn(9); }
			set { _flags2 = _flags2.SetBitAtIndex(9, value); }
		}

		public bool SealTypes
		{
			get { return _flags2.IsBitAtIndexOn(10); }
			set { _flags2 = _flags2.SetBitAtIndex(10, value); }
		}

		public bool SealTypesChanged
		{
			get { return _flags2.IsBitAtIndexOn(11); }
			set { _flags2 = _flags2.SetBitAtIndex(11, value); }
		}

		public bool DevirtualizeMethods
		{
			get { return _flags2.IsBitAtIndexOn(12); }
			set { _flags2 = _flags2.SetBitAtIndex(12, value); }
		}

		public bool DevirtualizeMethodsChanged
		{
			get { return _flags2.IsBitAtIndexOn(13); }
			set { _flags2 = _flags2.SetBitAtIndex(13, value); }
		}

		internal bool IsEmpty
		{
			get { return _flags == 0 && _flags2 == 0; }
		}

		#endregion

		#region Methods

		internal void Scavenge(ProjectScavengeState state)
		{
			if (!state.RenameMembers)
			{
				Rename = false;
				RenameChanged = false;
				RenameMembers = false;
				RenameMembersChanged = false;
				NameChanged = false;
				NamespaceChanged = false;
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
				RemoveUnused = false;
				RemoveUnusedChanged = false;
				RemoveUnusedMembers = false;
				RemoveUnusedMembersChanged = false;
			}

			if (!state.SealTypes)
			{
				Seal = false;
				SealChanged = false;
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
			_flags2 = accessor.ReadInt32();

			if (NameChanged)
				_name = state.GetString(accessor.Read7BitEncodedInt());

			if (NamespaceChanged)
				_namespace = state.GetString(accessor.Read7BitEncodedInt());
		}

		internal void Write(IBinaryAccessor accessor, ProjectWriteState state)
		{
			accessor.Write((int)_flags);
			accessor.Write((int)_flags2);

			if (NameChanged)
				accessor.Write7BitEncodedInt(state.SetString(_name));

			if (NamespaceChanged)
				accessor.Write7BitEncodedInt(state.SetString(_namespace));
		}

		#endregion
	}
}
