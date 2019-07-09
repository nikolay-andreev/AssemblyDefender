using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class BuildAssembly : Assembly
	{
		#region Fields

		internal const int RowSize = 56;
		private ModuleState _state;
		private Builder _builder;
		private ResourceStorage _resourceStorage;
		private IReadOnlyList<Assembly> _satelliteAssemblies;

		#endregion

		#region Ctors

		internal BuildAssembly(BuildAssemblyManager assemblyManager, string filePath)
			: base(assemblyManager, filePath)
		{
			var module = (BuildModule)_module;
			_state = module.State;
			_builder = assemblyManager.Builder;
		}

		#endregion

		#region Properties

		public bool ObfuscateControlFlow
		{
			get { return _state.GetTableBit(0, 0); }
			set { _state.SetTableBit(0, 0, value); }
		}

		public bool RenameMembers
		{
			get { return _state.GetTableBit(0, 2); }
			set { _state.SetTableBit(0, 2, value); }
		}

		public bool Rename
		{
			get { return _state.GetTableBit(0, 3); }
			set { _state.SetTableBit(0, 3, value); }
		}

		public bool EncryptIL
		{
			get { return _state.GetTableBit(0, 6); }
			set { _state.SetTableBit(0, 6, value); }
		}

		public bool ObfuscateResources
		{
			get { return _state.GetTableBit(1, 0); }
			set { _state.SetTableBit(1, 0, value); }
		}

		public bool ObfuscateStrings
		{
			get { return _state.GetTableBit(1, 1); }
			set { _state.SetTableBit(1, 1, value); }
		}

		public bool RemoveUnusedMembers
		{
			get { return _state.GetTableBit(1, 2); }
			set { _state.SetTableBit(1, 2, value); }
		}

		public bool SealTypes
		{
			get { return _state.GetTableBit(1, 3); }
			set { _state.SetTableBit(1, 3, value); }
		}

		public bool DevirtualizeMethods
		{
			get { return _state.GetTableBit(1, 4); }
			set { _state.SetTableBit(1, 4, value); }
		}

		public bool SuppressILdasm
		{
			get { return _state.GetTableBit(2, 0); }
			set { _state.SetTableBit(2, 0, value); }
		}

		public bool IgnoreObfuscationAttribute
		{
			get { return _state.GetTableBit(2, 1); }
			set { _state.SetTableBit(2, 1, value); }
		}

		public bool StripObfuscationAttribute
		{
			get { return _state.GetTableBit(2, 2); }
			set { _state.SetTableBit(2, 2, value); }
		}

		public bool StripObfuscationAttributeExists
		{
			get { return _state.GetTableBit(2, 3); }
			set { _state.SetTableBit(2, 3, value); }
		}

		public bool IsStrongNameSignedAfterBuild
		{
			get { return _state.GetTableBit(3, 0); }
			set { _state.SetTableBit(3, 0, value); }
		}

		public bool HasWpfResource
		{
			get { return _state.GetTableBit(3, 1); }
			set { _state.SetTableBit(3, 1, value); }
		}

		public string OutputPath
		{
			get { return _state.OutputPath; }
			set { _state.OutputPath = value; }
		}

		public string StrongNameKeyFilePath
		{
			get { return _state.StrongNameKeyFilePath; }
			set { _state.StrongNameKeyFilePath = value; }
		}

		public string StrongNameKeyPassword
		{
			get { return _state.StrongNameKeyPassword; }
			set { _state.StrongNameKeyPassword = value; }
		}

		public string MainTypeNamespace
		{
			get { return _builder.MainTypeNamespace; }
		}

		public Random RandomGenerator
		{
			get { return _builder.Random; }
		}

		public ResourceStorage ResourceStorage
		{
			get
			{
				if (_resourceStorage == null)
				{
					_resourceStorage = new ResourceStorage(this);
				}

				return _resourceStorage;
			}
		}

		public IReadOnlyList<Assembly> SatelliteAssemblies
		{
			get
			{
				if (_satelliteAssemblies == null)
				{
					LoadSatelliteAssemblies();
				}

				return _satelliteAssemblies;
			}
		}

		internal ModuleState State
		{
			get { return _state; }
		}

		#region Name

		public bool NameChanged
		{
			get { return _state.GetTableBit(4, 0); }
			set { _state.SetTableBit(4, 0, value); }
		}

		public string NewName
		{
			get { return _state.GetTableString(8); }
			set { _state.SetTableString(8, value); }
		}

		#endregion

		#region Culture

		public bool CultureChanged
		{
			get { return _state.GetTableBit(4, 1); }
			set { _state.SetTableBit(4, 1, value); }
		}

		public string NewCulture
		{
			get { return _state.GetTableString(12); }
			set { _state.SetTableString(12, value); }
		}

		#endregion

		#region Version

		public bool VersionChanged
		{
			get { return _state.GetTableBit(4, 2); }
			set { _state.SetTableBit(4, 2, value); }
		}

		public Version NewVersion
		{
			get
			{
				return new Version(
					(int)(ushort)_state.GetTableInt16(16),
					(int)(ushort)_state.GetTableInt16(18),
					(int)(ushort)_state.GetTableInt16(20),
					(int)(ushort)_state.GetTableInt16(22));
			}
			set
			{
				if (value != null)
				{
					_state.SetTableInt16(16, (short)value.Major);
					_state.SetTableInt16(18, (short)value.Minor);
					_state.SetTableInt16(20, (short)value.Build);
					_state.SetTableInt16(22, (short)value.Revision);
				}
				else
				{
					_state.SetTableInt64(16, 0);
				}
			}
		}

		#endregion

		#region PublicKey

		public bool PublicKeyChanged
		{
			get { return _state.GetTableBit(4, 3); }
			set { _state.SetTableBit(4, 3, value); }
		}

		public byte[] NewPublicKey
		{
			get { return _state.GetTableBlob(24); }
			set { _state.SetTableBlob(24, value); }
		}

		public byte[] NewPublicKeyToken
		{
			get { return _state.GetTableBlob(28); }
			set { _state.SetTableBlob(28, value); }
		}

		#endregion

		#region Title

		public bool TitleChanged
		{
			get { return _state.GetTableBit(4, 4); }
			set { _state.SetTableBit(4, 4, value); }
		}

		public string NewTitle
		{
			get { return _state.GetTableString(32); }
			set { _state.SetTableString(32, value); }
		}

		#endregion

		#region Description

		public bool DescriptionChanged
		{
			get { return _state.GetTableBit(4, 5); }
			set { _state.SetTableBit(4, 5, value); }
		}

		public string NewDescription
		{
			get { return _state.GetTableString(36); }
			set { _state.SetTableString(36, value); }
		}

		#endregion

		#region Company

		public bool CompanyChanged
		{
			get { return _state.GetTableBit(4, 6); }
			set { _state.SetTableBit(4, 6, value); }
		}

		public string NewCompany
		{
			get { return _state.GetTableString(40); }
			set { _state.SetTableString(40, value); }
		}

		#endregion

		#region Product

		public bool ProductChanged
		{
			get { return _state.GetTableBit(4, 7); }
			set { _state.SetTableBit(4, 7, value); }
		}

		public string NewProduct
		{
			get { return _state.GetTableString(44); }
			set { _state.SetTableString(44, value); }
		}

		#endregion

		#region Copyright

		public bool CopyrightChanged
		{
			get { return _state.GetTableBit(5, 0); }
			set { _state.SetTableBit(5, 0, value); }
		}

		public string NewCopyright
		{
			get { return _state.GetTableString(48); }
			set { _state.SetTableString(48, value); }
		}

		#endregion

		#region Trademark

		public bool TrademarkChanged
		{
			get { return _state.GetTableBit(5, 1); }
			set { _state.SetTableBit(5, 1, value); }
		}

		public string NewTrademark
		{
			get { return _state.GetTableString(52); }
			set { _state.SetTableString(52, value); }
		}

		#endregion

		#endregion

		#region Methods

		public BuildResource GetWpfResource()
		{
			foreach (BuildResource resource in Resources)
			{
				if (resource.IsWpf)
					return resource;
			}

			return null;
		}

		public void Compile()
		{
			foreach (BuildModule module in Modules)
			{
				module.Compile();
			}

			foreach (var satelliteAssembly in SatelliteAssemblies)
			{
				CompileSatelliteAssembly(satelliteAssembly);
			}
		}

		public void SaveState()
		{
			foreach (BuildModule module in Modules)
			{
				module.State.Save();
			}
		}

		protected override Module CreateModule(string location)
		{
			return new BuildModule(this, location);
		}

		private void CompileSatelliteAssembly(Assembly assembly)
		{
			string outputPath = Path.Combine(_state.OutputPath, assembly.Culture);
			string outputFilePath = Path.Combine(outputPath, assembly.Module.Name);

			DirectoryUtils.CreateDirectoryIfMissing(outputPath);

			var assembler = new Assembler(assembly.Module);

			var corHeaderBuilder = assembler.Tasks.Get<CorHeaderBuilder>(true);
			if (IsStrongNameSignedAfterBuild)
			{
				var strongNameBuilder = assembler.Tasks.Get<StrongNameSignatureBuilder>(true);
				strongNameBuilder.IsSigned = true;
				corHeaderBuilder.Flags |= CorFlags.StrongNameSigned;
				_state.SignFiles.Add(outputFilePath);
			}
			else
			{
				corHeaderBuilder.Flags &= ~CorFlags.StrongNameSigned;
			}

			assembler.Build();

			assembler.SaveToFile(outputFilePath);
		}

		private void LoadSatelliteAssemblies()
		{
			var assemblies = new List<Assembly>();
			var cultures = _builder.Cultures;
			string folder = Path.GetDirectoryName(Location);
			string resourceFileName = string.Format("{0}.resources.dll", Name);

			foreach (string subFolder in Directory.GetDirectories(folder))
			{
				string culture = Path.GetFileName(subFolder);
				if (!cultures.Contains(culture))
					continue;

				string path = Path.Combine(subFolder, resourceFileName);
				if (!File.Exists(path))
					continue;

				var assembly = AssemblyManager.LoadAssembly(path);
				if (assembly != null && !string.IsNullOrEmpty(assembly.Culture))
				{
					assemblies.Add(assembly);
				}
			}

			_satelliteAssemblies = new ReadOnlyList<Assembly>(assemblies.ToArray());
		}

		#endregion
	}
}
