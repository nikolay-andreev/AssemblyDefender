using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class BuildField : FieldDeclaration
	{
		#region Fields

		internal const int RowSize = 6;
		private int _stateOffset;
		private ModuleState _state;

		#endregion

		#region Ctors

		protected internal BuildField(BuildModule module, int rid, int typeRID)
			: base(module, rid, typeRID)
		{
			_state = module.State;
			_stateOffset = _state.GetFieldOffset(rid);
		}

		#endregion

		#region Properties

		public bool Rename
		{
			get { return _state.GetTableBit(_stateOffset, 0); }
			set { _state.SetTableBit(_stateOffset, 0, value); }
		}

		public bool ObfuscateStrings
		{
			get { return _state.GetTableBit(_stateOffset, 1); }
			set { _state.SetTableBit(_stateOffset, 1, value); }
		}

		public bool Strip
		{
			get { return _state.GetTableBit(_stateOffset, 2); }
			set { _state.SetTableBit(_stateOffset, 2, value); }
		}

		public bool StripProcessed
		{
			get { return _state.GetTableBit(_stateOffset, 3); }
			set { _state.SetTableBit(_stateOffset, 3, value); }
		}

		public bool StripObfuscationAttribute
		{
			get { return _state.GetTableBit(_stateOffset, 5); }
			set { _state.SetTableBit(_stateOffset, 5, value); }
		}

		public bool NameChanged
		{
			get { return _state.GetTableBit(_stateOffset + 1, 3); }
			set { _state.SetTableBit(_stateOffset + 1, 3, value); }
		}

		public string NewName
		{
			get { return _state.GetTableString(_stateOffset + 2); }
			set { _state.SetTableString(_stateOffset + 2, value); }
		}

		internal ModuleState State
		{
			get { return _state; }
		}

		#endregion
	}
}
