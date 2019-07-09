using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Defines an Assembly, which is a reusable, versionable, and self-describing building block of a
	/// common language runtime application.
	/// </summary>
	public class Assembly : CodeNode, IAssembly, ICustomAttributeProvider, ISecurityAttributeProvider
	{
		#region Fields

		private string _name;
		private string _culture;
		private bool _disableJITcompileOptimizer;
		private bool _enableJITcompileTracking;
		private ProcessorArchitecture _processorArchitecture;
		private HashAlgorithm _hashAlgorithm;
		private Version _version;
		private byte[] _publicKey;
		private byte[] _publicKeyToken;
		private DotNetFramework _framework = MicrosoftNetFramework.VersionLatest;
		private ModuleCollection _modules;
		private CustomAttributeCollection _customAttributes;
		private SecurityAttributeCollection _securityAttributes;
		private IReadOnlyList<IType> _resolvedExportedTypes;
		private AssemblyManager _assemblyManager;
		internal TypeReference[] PrimitiveTypeSignatures = new TypeReference[(int)PrimitiveTypeCode.Undefined];

		#endregion

		#region Ctors

		protected Assembly()
		{
		}

		public Assembly(AssemblyManager assemblyManager)
			: this(assemblyManager, null)
		{
		}

		protected internal Assembly(AssemblyManager assemblyManager, string location)
		{
			if (assemblyManager == null)
				throw new ArgumentNullException("assemblyManager");

			_assemblyManager = assemblyManager;
			_module = CreateModule(location);

			if (_module.Image != null)
			{
				Load();
			}
			else
			{
				IsNew = true;
			}
		}

		#endregion

		#region Properties

		public bool IsStrongNameSigned
		{
			get { return _publicKey != null; }
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value.NullIfEmpty();
				OnChanged();
			}
		}

		public string Culture
		{
			get { return _culture; }
			set
			{
				_culture = value.NullIfEmpty();
				OnChanged();
			}
		}

		/// <summary>
		/// From DebuggableAttribute.
		/// </summary>
		public bool DisableJITcompileOptimizer
		{
			get { return _disableJITcompileOptimizer; }
			set
			{
				_disableJITcompileOptimizer = value;
				OnChanged();
			}
		}

		/// <summary>
		/// From DebuggableAttribute.
		/// </summary>
		public bool EnableJITcompileTracking
		{
			get { return _enableJITcompileTracking; }
			set
			{
				_enableJITcompileTracking = value;
				OnChanged();
			}
		}

		/// <summary>
		/// Gets or sets a value that identifies the processor and bits-per-word of the platform targeted by an executable.
		/// </summary>
		public ProcessorArchitecture ProcessorArchitecture
		{
			get { return _processorArchitecture; }
			set
			{
				_processorArchitecture = value;
				OnChanged();
			}
		}

		public HashAlgorithm HashAlgorithm
		{
			get { return _hashAlgorithm; }
			set
			{
				_hashAlgorithm = value;
				OnChanged();
			}
		}

		public Version Version
		{
			get { return _version; }
			set
			{
				_version = value;
				OnChanged();
			}
		}

		public byte[] PublicKey
		{
			get { return _publicKey; }
			set
			{
				_publicKey = value.NullIfEmpty();
				_publicKeyToken = null;
				OnChanged();
			}
		}

		public byte[] PublicKeyToken
		{
			get
			{
				if (_publicKeyToken == null && _publicKey != null && _publicKey.Length > 0)
				{
					try
					{
						_publicKeyToken = StrongNameUtils.CreateTokenFromPublicKey(_publicKey);
					}
					catch (InvalidOperationException)
					{
						_publicKeyToken = BufferUtils.EmptyArray;
					}
				}

				return _publicKeyToken;
			}
		}

		/// <summary>
		/// Gets the location of the file that contains the manifest.
		/// </summary>
		public string Location
		{
			get { return _module.Location; }
		}

		/// <summary>
		/// Method or File signature
		/// </summary>
		public Signature EntryPoint
		{
			get { return _module.EntryPoint; }
			set { _module.EntryPoint = value; }
		}

		public DotNetFramework Framework
		{
			get { return _framework; }
			set { _framework = value ?? MicrosoftNetFramework.VersionLatest; }
		}

		public AssemblyReferenceCollection AssemblyReferences
		{
			get { return _module.AssemblyReferences; }
		}

		public TypeDeclarationCollection Types
		{
			get { return _module.Types; }
		}

		public ExportedTypeCollection ExportedTypes
		{
			get { return _module.ExportedTypes; }
		}

		public FileReferenceCollection Files
		{
			get { return _module.Files; }
		}

		public ResourceCollection Resources
		{
			get { return _module.Resources; }
		}

		public ResourceTable UnmanagedResources
		{
			get { return _module.UnmanagedResources; }
		}

		public ModuleCollection Modules
		{
			get
			{
				if (_modules == null)
				{
					_modules = new ModuleCollection(this);
				}

				return _modules;
			}
		}

		public CustomAttributeCollection CustomAttributes
		{
			get
			{
				if (_customAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.Assembly, 1);
					_customAttributes = new CustomAttributeCollection(this, token);
				}

				return _customAttributes;
			}
		}

		public SecurityAttributeCollection SecurityAttributes
		{
			get
			{
				if (_securityAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.Assembly, 1);
					_securityAttributes = new SecurityAttributeCollection(this, token);
				}

				return _securityAttributes;
			}
		}

		public new AssemblyManager AssemblyManager
		{
			get { return _assemblyManager; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		public void CopyTo(Assembly copy)
		{
			copy._name = _name;
			copy._culture = _culture;
			copy._disableJITcompileOptimizer = _disableJITcompileOptimizer;
			copy._enableJITcompileTracking = _enableJITcompileTracking;
			copy._processorArchitecture = _processorArchitecture;
			copy._hashAlgorithm = _hashAlgorithm;
			copy._version = _version;
			copy._publicKey = _publicKey;
			copy._publicKeyToken = _publicKeyToken;
			copy._framework = _framework;
			CustomAttributes.CopyTo(copy.CustomAttributes);
			SecurityAttributes.CopyTo(copy.SecurityAttributes);

			_module.CopyTo(copy._module);

			for (int i = 1; i < Modules.Count; i++)
			{
				Modules[i].CopyTo(copy.Modules.Add());
			}
		}

		public void InvalidatedSignatures()
		{
			_resolvedExportedTypes = null;

			foreach (var modue in _modules)
			{
				modue.InvalidatedSignatures();
			}
		}

		protected internal override void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
			}
		}

		protected internal virtual Module CreateModule(string location)
		{
			return new Module(this, location);
		}

		protected internal virtual void Close()
		{
			if (_modules != null)
			{
				foreach (var module in _modules)
				{
					module.Close();
				}
			}
			else
			{
				_module.Close();
			}
		}

		private void Load()
		{
			var image = _module.Image;

			if (image.GetAssemblyCount() == 0)
			{
				throw new AssemblyLoadException(string.Format(SR.AssemblyLoadError, _module.Location));
			}

			AssemblyRow row;
			image.GetAssembly(1, out row);

			_name = image.GetString(row.Name);
			_culture = image.GetString(row.Locale);
			_processorArchitecture = (ProcessorArchitecture)((int)(row.Flags & AssemblyFlags.PA_Mask) >> (int)AssemblyFlags.PA_Shift);
			_version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);
			_publicKey = image.GetBlob(row.PublicKey);
			_hashAlgorithm = row.HashAlgId;
			_framework = GetFramework();
		}

		private DotNetFramework GetFramework()
		{
			var mscorlib = GetReferencedMscorlib();
			if (mscorlib != null)
			{
				return DotNetFramework.GetByMscorlib(mscorlib);
			}
			else
			{
				return DotNetFramework.Get(FrameworkType.MicrosoftNet, _module.FrameworkVersionMoniker);
			}
		}

		private IAssemblySignature GetReferencedMscorlib()
		{
			if (_name == "mscorlib" &&
				IsStrongNameSigned &&
				_version != null)
			{
				return this;
			}

			AssemblyReference latestMscorlib = null;
			foreach (var assemblyRef in AssemblyReferences)
			{
				if (assemblyRef.Name == "mscorlib" &&
					assemblyRef.IsStrongNameSigned &&
					assemblyRef.Version != null)
				{
					if (latestMscorlib == null || latestMscorlib.Version < assemblyRef.Version)
					{
						latestMscorlib = assemblyRef;
					}
				}
			}

			return latestMscorlib;
		}

		private void LoadExportedTypes()
		{
			var exportedTypes = _module.ExportedTypes;
			var array = new IType[exportedTypes.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = exportedTypes[i].Resolve(_module, true);
			}

			_resolvedExportedTypes = new ReadOnlyList<IType>(array);
		}

		#endregion

		#region IAssembly Members

		IReadOnlyList<IModule> IAssembly.Modules
		{
			get { return Modules; }
		}

		IReadOnlyList<IType> IAssembly.Types
		{
			get { return _module.Types; }
		}

		SignatureType ISignature.SignatureType
		{
			get { return SignatureType.Assembly; }
		}

		EntityType ICodeNode.EntityType
		{
			get { return EntityType.Assembly; }
		}

		IReadOnlyList<IType> IAssembly.ExportedTypes
		{
			get
			{
				if (_resolvedExportedTypes == null)
				{
					LoadExportedTypes();
				}

				return _resolvedExportedTypes;
			}
		}

		#endregion

		#region ICodeNode Members

		bool ICodeNode.HasGenericContext
		{
			get { return false; }
		}

		IAssembly ICodeNode.Assembly
		{
			get { return this; }
		}

		IModule ICodeNode.Module
		{
			get { return _module; }
		}

		IType ICodeNode.GetGenericArgument(bool isMethod, int position)
		{
			return null;
		}

		#endregion

		#region Static

		public static bool IsAssemblyFile(string filePath)
		{
			using (var source = new FileBinarySource(filePath))
			{
				return IsAssembly(source);
			}
		}

		public static bool IsAssembly(IBinarySource source)
		{
			try
			{
				var pe = new PEImage(source, false);
				var metadata = MemoryMappedMetadata.Load(pe);
				return metadata.GetTableRowCount(Metadata.MetadataTableType.Assembly) > 0;
			}
			catch (Exception)
			{
				return false;
			}
		}

		#endregion
	}
}
