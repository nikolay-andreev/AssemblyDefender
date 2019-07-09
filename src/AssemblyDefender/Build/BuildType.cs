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
	public class BuildType : TypeDeclaration
	{
		#region Fields

		internal const int RowSize = 10;
		private int _stateOffset;
		private ModuleState _state;

		#endregion

		#region Ctors

		protected internal BuildType(BuildModule module, int rid, int enclosingTypeRID)
			: base(module, rid, enclosingTypeRID)
		{
			_state = module.State;
			_stateOffset = _state.GetTypeOffset(rid);
		}

		#endregion

		#region Properties

		public virtual bool IsMainType
		{
			get { return false; }
		}

		public bool Rename
		{
			get { return _state.GetTableBit(_stateOffset, 1); }
			set { _state.SetTableBit(_stateOffset, 1, value); }
		}

		public bool SealType
		{
			get { return _state.GetTableBit(_stateOffset, 2); }
			set { _state.SetTableBit(_stateOffset, 2, value); }
		}

		public bool SealTypeProcessed
		{
			get { return _state.GetTableBit(_stateOffset, 3); }
			set { _state.SetTableBit(_stateOffset, 3, value); }
		}

		public bool Strip
		{
			get { return _state.GetTableBit(_stateOffset, 4); }
			set { _state.SetTableBit(_stateOffset, 4, value); }
		}

		public bool StripProcessed
		{
			get { return _state.GetTableBit(_stateOffset, 5); }
			set { _state.SetTableBit(_stateOffset, 5, value); }
		}

		public bool StripObfuscationAttribute
		{
			get { return _state.GetTableBit(_stateOffset + 1, 1); }
			set { _state.SetTableBit(_stateOffset + 1, 1, value); }
		}

		public bool NameChanged
		{
			get { return _state.GetTableBit(_stateOffset + 1, 4); }
			set { _state.SetTableBit(_stateOffset + 1, 4, value); }
		}

		public string NewName
		{
			get { return _state.GetTableString(_stateOffset + 2); }
			set { _state.SetTableString(_stateOffset + 2, value); }
		}

		public bool NamespaceChanged
		{
			get { return _state.GetTableBit(_stateOffset + 1, 5); }
			set { _state.SetTableBit(_stateOffset + 1, 5, value); }
		}

		public string NewNamespace
		{
			get { return _state.GetTableString(_stateOffset + 6); }
			set { _state.SetTableString(_stateOffset + 6, value); }
		}

		internal ModuleState State
		{
			get { return _state; }
		}

		#endregion
	}
}
