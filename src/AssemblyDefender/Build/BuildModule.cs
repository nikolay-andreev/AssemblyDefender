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
	public class BuildModule : Module
	{
		#region Fields

		internal const int RowSize = 18;
		private int _stateOffset;
		private ModuleState _state;
		private MainType _mainType;
		private Builder _builder;
		private Assembler _assembler;
		private StateObjectList<DelegateType> _delegateTypes;
		private StateObjectList<ILCryptoInvokeType> _ilCryptoInvokeTypes;

		#endregion

		#region Ctors

		protected internal BuildModule(BuildAssembly assembly, string filePath)
			: base(assembly, filePath)
		{
			var assemblyManager = (BuildAssemblyManager)assembly.AssemblyManager;
			_builder = assemblyManager.Builder;
			_state = assemblyManager.GetState(Location);
			_state.Load(this);
			_stateOffset = _state.GetModuleOffset();
		}

		#endregion

		#region Properties

		public bool StripObfuscationAttribute
		{
			get { return _state.GetTableBit(_stateOffset, 2); }
			set { _state.SetTableBit(_stateOffset, 2, value); }
		}

		public bool NameChanged
		{
			get { return _state.GetTableBit(_stateOffset + 1, 0); }
			set { _state.SetTableBit(_stateOffset + 1, 0, value); }
		}

		public string NewName
		{
			get { return _state.GetTableString(_stateOffset + 2); }
			set { _state.SetTableString(_stateOffset + 2, value); }
		}

		public string MainTypeName
		{
			get { return _state.GetTableString(_stateOffset + 6); }
			set { _state.SetTableString(_stateOffset + 6, value); }
		}

		public string MainTypeNamespace
		{
			get { return _builder.MainTypeNamespace; }
		}

		public Assembler Assembler
		{
			get
			{
				if (_assembler == null)
				{
					_assembler = CreateAssembler();
				}

				return _assembler;
			}
		}

		public Random RandomGenerator
		{
			get { return _builder.Random; }
		}

		public ResourceStorage ResourceStorage
		{
			get { return ((BuildAssembly)_module.Assembly).ResourceStorage; }
		}

		public StateObjectList<DelegateType> DelegateTypes
		{
			get
			{
				if (_delegateTypes == null)
				{
					_delegateTypes = _state.GetOrCreateTableObject<StateObjectList<DelegateType>>(_stateOffset + 10);
				}

				return _delegateTypes;
			}
		}

		public StateObjectList<ILCryptoInvokeType> ILCryptoInvokeTypes
		{
			get
			{
				if (_ilCryptoInvokeTypes == null)
				{
					_ilCryptoInvokeTypes = _state.GetOrCreateTableObject<StateObjectList<ILCryptoInvokeType>>(_stateOffset + 14);
				}

				return _ilCryptoInvokeTypes;
			}
		}

		public MainType MainType
		{
			get { return _mainType; }
		}

		internal ModuleState State
		{
			get { return _state; }
		}

		#endregion

		#region Methods

		internal void CreateMainType()
		{
			_mainType = new MainType(this);
			AddType(_mainType);
		}

		internal void RemoveMainType()
		{
			if (_mainType == null)
				return;

			Types.Remove(_mainType);
			_mainType = null;
		}

		internal void Compile()
		{
			if (_state.IsNew)
			{
				_state.Save();
			}

			if (_isChanged)
			{
				var assembler = Assembler;
				assembler.Build();
				assembler.SaveToFile(_state.BuildFilePath);
			}
			else
			{
				File.Copy(Location, _state.BuildFilePath, true);
			}

			if (IsPrimeModule && _state.SignAssembly)
			{
				_state.SignFiles.Add(_state.OutputFilePath);
			}
		}

		protected override void OnDeleted()
		{
			var assemblyManager = (BuildAssemblyManager)Assembly.AssemblyManager;
			assemblyManager.RemoveState(Location);
		}

		protected override void Close()
		{
			if (_state != null)
			{
				_state.Close();
				_state = null;
			}

			base.Close();
		}

		protected override TypeDeclaration CreateType(int rid, int enclosingTypeRID)
		{
			return new BuildType(this, rid, enclosingTypeRID);
		}

		protected override MethodDeclaration CreateMethod(int rid, int typeRID)
		{
			return new BuildMethod(this, rid, typeRID);
		}

		protected override FieldDeclaration CreateField(int rid, int typeRID)
		{
			return new BuildField(this, rid, typeRID);
		}

		protected override PropertyDeclaration CreateProperty(int rid, int typeRID)
		{
			return new BuildProperty(this, rid, typeRID);
		}

		protected override EventDeclaration CreateEvent(int rid, int typeRID)
		{
			return new BuildEvent(this, rid, typeRID);
		}

		protected override Resource CreateResource(int rid, int notUsed)
		{
			return new BuildResource(this, rid);
		}

		private Assembler CreateAssembler()
		{
			var assembler = new Assembler(this);

			var moduleBuilder = assembler.Tasks.Get<ModuleBuilder>(true);
			moduleBuilder.OptimizeTinyMethodBody = true;

			if (IsPrimeModule)
			{
				var corHeaderBuilder = assembler.Tasks.Get<CorHeaderBuilder>(true);

				var assembly = (BuildAssembly)Assembly;
				if (assembly.IsStrongNameSignedAfterBuild)
				{
					var strongNameBuilder = assembler.Tasks.Get<StrongNameSignatureBuilder>(true);
					strongNameBuilder.IsSigned = true;
					corHeaderBuilder.Flags |= CorFlags.StrongNameSigned;
				}
				else
				{
					corHeaderBuilder.Flags &= ~CorFlags.StrongNameSigned;
				}
			}

			assembler.Tasks.Add(new ResourceStorageBuilder(), 1800);

			return assembler;
		}

		#endregion
	}
}
