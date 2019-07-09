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
	public class BuildProperty : PropertyDeclaration
	{
		#region Fields

		internal const int RowSize = 6;
		private int _stateOffset;
		private ModuleState _state;

		#endregion

		#region Ctors

		protected internal BuildProperty(BuildModule module, int rid, int typeRID)
			: base(module, rid, typeRID)
		{
			_state = module.State;
			_stateOffset = _state.GetPropertyOffset(rid);
		}

		#endregion

		#region Properties

		public bool Rename
		{
			get { return _state.GetTableBit(_stateOffset, 0); }
			set { _state.SetTableBit(_stateOffset, 0, value); }
		}

		public bool Strip
		{
			get { return _state.GetTableBit(_stateOffset, 1); }
			set { _state.SetTableBit(_stateOffset, 1, value); }
		}

		public bool StripProcessed
		{
			get { return _state.GetTableBit(_stateOffset, 2); }
			set { _state.SetTableBit(_stateOffset, 2, value); }
		}

		public bool StripObfuscationAttribute
		{
			get { return _state.GetTableBit(_stateOffset, 4); }
			set { _state.SetTableBit(_stateOffset, 4, value); }
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
