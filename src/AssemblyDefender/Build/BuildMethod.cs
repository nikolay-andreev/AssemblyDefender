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
	public class BuildMethod : MethodDeclaration
	{
		#region Fields

		internal const int RowSize = 12;
		private int _stateOffset;
		private ModuleState _state;
		private ILCryptoMethod _ilCrypto;

		#endregion

		#region Ctors

		protected internal BuildMethod(BuildModule module, int rid, int typeRID)
			: base(module, rid, typeRID)
		{
			_state = module.State;
			_stateOffset = _state.GetMethodOffset(rid);
		}

		#endregion

		#region Properties
		
		public bool Rename
		{
			get { return _state.GetTableBit(_stateOffset, 1); }
			set { _state.SetTableBit(_stateOffset, 1, value); }
		}

		public bool ObfuscateControlFlow
		{
			get { return _state.GetTableBit(_stateOffset, 2); }
			set { _state.SetTableBit(_stateOffset, 2, value); }
		}

		public bool EncryptIL
		{
			get { return _state.GetTableBit(_stateOffset, 3); }
			set { _state.SetTableBit(_stateOffset, 3, value); }
		}

		public bool ObfuscateStrings
		{
			get { return _state.GetTableBit(_stateOffset, 4); }
			set { _state.SetTableBit(_stateOffset, 4, value); }
		}

		public bool DevirtualizeMethod
		{
			get { return _state.GetTableBit(_stateOffset, 5); }
			set { _state.SetTableBit(_stateOffset, 5, value); }
		}

		public bool DevirtualizeMethodProcessed
		{
			get { return _state.GetTableBit(_stateOffset, 6); }
			set { _state.SetTableBit(_stateOffset, 6, value); }
		}

		public bool Strip
		{
			get { return _state.GetTableBit(_stateOffset + 1, 0); }
			set { _state.SetTableBit(_stateOffset + 1, 0, value); }
		}

		public bool StripProcessed
		{
			get { return _state.GetTableBit(_stateOffset + 1, 1); }
			set { _state.SetTableBit(_stateOffset + 1, 1, value); }
		}

		public bool StripEncrypted
		{
			get { return _state.GetTableBit(_stateOffset + 1, 2); }
			set { _state.SetTableBit(_stateOffset + 1, 2, value); }
		}

		public bool StripObfuscationAttribute
		{
			get { return _state.GetTableBit(_stateOffset + 1, 5); }
			set { _state.SetTableBit(_stateOffset + 1, 5, value); }
		}

		public bool NameChanged
		{
			get { return _state.GetTableBit(_stateOffset + 1, 6); }
			set { _state.SetTableBit(_stateOffset + 1, 6, value); }
		}

		public string NewName
		{
			get { return _state.GetTableString(_stateOffset + 2); }
			set { _state.SetTableString(_stateOffset + 2, value); }
		}

		public ILCryptoMethod ILCrypto
		{
			get
			{
				if (_ilCrypto == null)
				{
					_ilCrypto = _state.GetTableObject<ILCryptoMethod>(_stateOffset + 6);
				}

				return _ilCrypto;
			}
			private set
			{
				_state.SetTableObject(_stateOffset + 6, value);
				_ilCrypto = value;
			}
		}

		internal ModuleState State
		{
			get { return _state; }
		}

		#endregion

		#region Methods

		public ILCryptoMethod CreateILCrypto()
		{
			ILCrypto = new ILCryptoMethod();

			return _ilCrypto;
		}

		#endregion
	}
}
