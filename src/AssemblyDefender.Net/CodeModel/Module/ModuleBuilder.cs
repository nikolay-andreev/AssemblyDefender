using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Build metadata and code from assembly scope.
	/// </summary>
	public class ModuleBuilder : BuildTask
	{
		#region Fields

		private bool _optimizeTinyMethodBody;
		private bool _retargetableAssemblyReferences;
		private Module _module;
		private MetadataScope _metadata;
		private BuildBlobPoint[] _methodBodyPoints;
		private FieldDataPoint[] _fieldDataPoints;
		private BuildBlob _methodBodyBlob;
		private BuildBlob _fieldDataBlob;
		private BuildBlob _fieldCLIDataBlob;
		private BuildBlob _fieldTLSDataBlob;
		private BuildBlob _managedResourceBlob;
		private int[] _typeRIDMap;
		private int[] _methodRIDMap;
		private int[] _fieldRIDMap;
		private int[] _propertyRIDMap;
		private int[] _eventRIDMap;
		private int[] _resourceRIDMap;
		private BuildSignatureComparer _defHashComparer;
		private HashList<AssemblyReference> _assemblyRefHash;
		private HashList<ModuleReference> _moduleRefHash;
		private HashList<FileReference> _fileRefHash;
		private HashList<ITypeSignature> _typeDefHash;
		private HashList<IMethodSignature> _methodDefHash;
		private HashList<IFieldSignature> _fieldDefHash;
		private HashList<TypeReference> _exportedTypeHash;
		private State _state;
		private string _methodBodySectionName = PESectionNames.Text;
		private string _fieldDataSectionName = PESectionNames.SData;
		private string _fieldCLIDataSectionName = PESectionNames.Text;
		private string _fieldTLSDataSectionName = PESectionNames.Tls;
		private string _managedResourceSectionName = PESectionNames.Text;
		private int _methodBodyBlobPriority = 18000;
		private int _fieldDataBlobPriority = 24000;
		private int _fieldCLIDataBlobPriority = 15000;
		private int _fieldTLSDataBlobPriority = 1000;
		private int _managedResourceBlobPriority = 37000;

		#endregion

		#region Ctors

		public ModuleBuilder()
		{
		}

		#endregion

		#region Properties

		public bool OptimizeTinyMethodBody
		{
			get { return _optimizeTinyMethodBody; }
			set { _optimizeTinyMethodBody = value; }
		}

		public bool RetargetableAssemblyReferences
		{
			get { return _retargetableAssemblyReferences; }
			set { _retargetableAssemblyReferences = value; }
		}

		public Module Module
		{
			get { return _module; }
			set { _module = value; }
		}

		public BuildBlobPoint[] MethodBodyPoints
		{
			get { return _methodBodyPoints; }
			set { _methodBodyPoints = value; }
		}

		public FieldDataPoint[] FieldDataPoints
		{
			get { return _fieldDataPoints; }
			set { _fieldDataPoints = value; }
		}

		#region Blobs

		public BuildBlob MethodBodyBlob
		{
			get { return _methodBodyBlob; }
			set { _methodBodyBlob = value; }
		}

		public BuildBlob FieldDataBlob
		{
			get { return _fieldDataBlob; }
			set { _fieldDataBlob = value; }
		}

		public BuildBlob FieldCLIDataBlob
		{
			get { return _fieldCLIDataBlob; }
			set { _fieldCLIDataBlob = value; }
		}

		public BuildBlob FieldTLSDataBlob
		{
			get { return _fieldTLSDataBlob; }
			set { _fieldTLSDataBlob = value; }
		}

		public BuildBlob ManagedResourceBlob
		{
			get { return _managedResourceBlob; }
			set { _managedResourceBlob = value; }
		}

		#endregion

		#region RID Mappings

		public int[] TypeRIDMap
		{
			get { return _typeRIDMap; }
			set { _typeRIDMap = value; }
		}

		public int[] MethodRIDMap
		{
			get { return _methodRIDMap; }
			set { _methodRIDMap = value; }
		}

		public int[] FieldRIDMap
		{
			get { return _fieldRIDMap; }
			set { _fieldRIDMap = value; }
		}

		public int[] PropertyRIDMap
		{
			get { return _propertyRIDMap; }
			set { _propertyRIDMap = value; }
		}

		public int[] EventRIDMap
		{
			get { return _eventRIDMap; }
			set { _eventRIDMap = value; }
		}

		public int[] ResourceRIDMap
		{
			get { return _resourceRIDMap; }
			set { _resourceRIDMap = value; }
		}

		#endregion

		#region Hash

		public HashList<AssemblyReference> AssemblyRefHash
		{
			get { return _assemblyRefHash; }
		}

		public HashList<ModuleReference> ModuleRefHash
		{
			get { return _moduleRefHash; }
		}

		public HashList<FileReference> FileRefHash
		{
			get { return _fileRefHash; }
		}

		public HashList<ITypeSignature> TypeDefHash
		{
			get { return _typeDefHash; }
		}

		public HashList<IMethodSignature> MethodDefHash
		{
			get { return _methodDefHash; }
		}

		public HashList<IFieldSignature> FieldDefHash
		{
			get { return _fieldDefHash; }
		}

		public HashList<TypeReference> ExportedTypeHash
		{
			get { return _exportedTypeHash; }
		}

		#endregion

		#region Sections settings

		public string MethodBodySectionName
		{
			get { return _methodBodySectionName; }
			set { _methodBodySectionName = value; }
		}

		public string FieldDataSectionName
		{
			get { return _fieldDataSectionName; }
			set { _fieldDataSectionName = value; }
		}

		public string FieldCLIDataSectionName
		{
			get { return _fieldCLIDataSectionName; }
			set { _fieldCLIDataSectionName = value; }
		}

		public string FieldTLSDataSectionName
		{
			get { return _fieldTLSDataSectionName; }
			set { _fieldTLSDataSectionName = value; }
		}

		public string ManagedResourceSectionName
		{
			get { return _managedResourceSectionName; }
			set { _managedResourceSectionName = value; }
		}

		public int MethodBodyBlobPriority
		{
			get { return _methodBodyBlobPriority; }
			set { _methodBodyBlobPriority = value; }
		}

		public int FieldDataBlobPriority
		{
			get { return _fieldDataBlobPriority; }
			set { _fieldDataBlobPriority = value; }
		}

		public int FieldCLIDataBlobPriority
		{
			get { return _fieldCLIDataBlobPriority; }
			set { _fieldCLIDataBlobPriority = value; }
		}

		public int FieldTLSDataBlobPriority
		{
			get { return _fieldTLSDataBlobPriority; }
			set { _fieldTLSDataBlobPriority = value; }
		}

		public int ManagedResourceBlobPriority
		{
			get { return _managedResourceBlobPriority; }
			set { _managedResourceBlobPriority = value; }
		}

		#endregion

		#endregion

		#region Methods

		public override void Build()
		{
			if (_module == null)
				return;

			Initialize();

			AddString(CodeModelUtils.GlobalTypeName);

			_metadata.FrameworkVersionMoniker = _module.FrameworkVersionMoniker;

			BuildAssemblyReferences(_module.AssemblyReferences);
			BuildModuleReferences(_module.ModuleReferences);
			BuildFiles(_module.Files);

			if (_module.IsPrimeModule)
			{
				BuildAssembly();
			}

			BuildModule();
			BuildTypes(_module.Types);
			BuildExportedTypes(_module.ExportedTypes);
			BuildResources(_module.Resources);
			SetEntryPointToken();
		}

		public int AddString(string value)
		{
			if (value == null || value.Length == 0)
				return 0;

			int position;
			if (!_state.StringToPosition.TryGetValue(value, out position))
			{
				position = _state.Strings.Add(value);
				_state.StringToPosition.Add(value, position);
			}

			return position;
		}

		public int AddUserString(string value)
		{
			if (value == null)
				value = string.Empty;

			int position;
			if (!_state.UserStringToPosition.TryGetValue(value, out position))
			{
				position = _state.UserStrings.Add(value);
				_state.UserStringToPosition.Add(value, position);
			}

			return position;
		}

		public int AddGuid(Guid value)
		{
			int position;
			if (!_state.GuidToPosition.TryGetValue(value, out position))
			{
				position = _state.Guids.Add(value);
				_state.GuidToPosition.Add(value, position);
			}

			return position;
		}

		public int AddBlob(byte[] value)
		{
			if (value == null)
				return 0;

			return AddBlob(value, 0, value.Length);
		}

		public int AddBlob(byte[] value, int offset, int count)
		{
			if (value == null || count == 0)
				return 0;

			int position;
			if (!_state.BlobToPosition.TryGetValue(value, offset, count, out position))
			{
				position = _state.BlobToPosition.Length;
				int addPos = position;
				_state.BlobToPosition.WriteCompressedInteger(ref addPos, count);
				_state.BlobToPosition.Add(value, offset, count, position);
			}

			return position;
		}

		private void Initialize()
		{
			// Blobs
			_methodBodyBlob = new BuildBlob();
			_fieldDataBlob = new BuildBlob();
			_fieldCLIDataBlob = new BuildBlob();
			_fieldTLSDataBlob = new BuildBlob();
			_managedResourceBlob = new BuildBlob();
			_methodBodyBlob.OffsetAlignment = 4;

			// Hash
			_defHashComparer = new BuildSignatureComparer();
			int typeDefCapacity = _module.Types.Count * 2;
			_typeDefHash = new HashList<ITypeSignature>(typeDefCapacity, _defHashComparer);
			_methodDefHash = new HashList<IMethodSignature>(typeDefCapacity * 3, _defHashComparer);
			_fieldDefHash = new HashList<IFieldSignature>(typeDefCapacity * 3, _defHashComparer);
			_assemblyRefHash = new HashList<AssemblyReference>(30, SignatureComparer.Default);
			_moduleRefHash = new HashList<ModuleReference>(10, SignatureComparer.Default);
			_fileRefHash = new HashList<FileReference>(10, SignatureComparer.Default);
			_exportedTypeHash = new HashList<TypeReference>(10, SignatureComparer.Default);

			// State
			_state = new State();

			_metadata = new MetadataScope();

			// Heaps
			_state.Strings = _metadata.Strings;
			_state.UserStrings = _metadata.UserStrings;
			_state.Guids = _metadata.Guids;
			_state.GuidToPosition.Add(Guid.Empty, 0);

			// Tables
			var tables = _metadata.Tables;
			tables.SetSorted(false);
			tables.SchemaMajorVersion = 2;
			tables.SchemaMinorVersion = 0;
			_state.Tables = tables;
			_state.AssemblyTable = tables.AssemblyTable;
			_state.AssemblyOSTable = tables.AssemblyOSTable;
			_state.AssemblyProcessorTable = tables.AssemblyProcessorTable;
			_state.AssemblyRefTable = tables.AssemblyRefTable;
			_state.AssemblyRefOSTable = tables.AssemblyRefOSTable;
			_state.AssemblyRefProcessorTable = tables.AssemblyRefProcessorTable;
			_state.ClassLayoutTable = tables.ClassLayoutTable;
			_state.ConstantTable = tables.ConstantTable;
			_state.CustomAttributeTable = tables.CustomAttributeTable;
			_state.DeclSecurityTable = tables.DeclSecurityTable;
			_state.ENCLogTable = tables.ENCLogTable;
			_state.ENCMapTable = tables.ENCMapTable;
			_state.EventTable = tables.EventTable;
			_state.EventMapTable = tables.EventMapTable;
			_state.EventPtrTable = tables.EventPtrTable;
			_state.ExportedTypeTable = tables.ExportedTypeTable;
			_state.FieldTable = tables.FieldTable;
			_state.FieldLayoutTable = tables.FieldLayoutTable;
			_state.FieldMarshalTable = tables.FieldMarshalTable;
			_state.FieldPtrTable = tables.FieldPtrTable;
			_state.FieldRVATable = tables.FieldRVATable;
			_state.FileTable = tables.FileTable;
			_state.GenericParamTable = tables.GenericParamTable;
			_state.GenericParamConstraintTable = tables.GenericParamConstraintTable;
			_state.ImplMapTable = tables.ImplMapTable;
			_state.InterfaceImplTable = tables.InterfaceImplTable;
			_state.ManifestResourceTable = tables.ManifestResourceTable;
			_state.MemberRefTable = tables.MemberRefTable;
			_state.MethodTable = tables.MethodTable;
			_state.MethodImplTable = tables.MethodImplTable;
			_state.MethodPtrTable = tables.MethodPtrTable;
			_state.MethodSemanticsTable = tables.MethodSemanticsTable;
			_state.MethodSpecTable = tables.MethodSpecTable;
			_state.ModuleTable = tables.ModuleTable;
			_state.ModuleRefTable = tables.ModuleRefTable;
			_state.NestedClassTable = tables.NestedClassTable;
			_state.ParamTable = tables.ParamTable;
			_state.ParamPtrTable = tables.ParamPtrTable;
			_state.PropertyTable = tables.PropertyTable;
			_state.PropertyMapTable = tables.PropertyMapTable;
			_state.PropertyPtrTable = tables.PropertyPtrTable;
			_state.StandAloneSigTable = tables.StandAloneSigTable;
			_state.TypeDefTable = tables.TypeDefTable;
			_state.TypeRefTable = tables.TypeRefTable;
			_state.TypeSpecTable = tables.TypeSpecTable;

			// Load
			LoadMembers();

			_state.ResourceCount += _module.Resources.Count;

			// RID maps
			_typeRIDMap = new int[_state.TypeCount];
			_methodRIDMap = new int[_state.MethodCount];
			_fieldRIDMap = new int[_state.FieldCount];
			_propertyRIDMap = new int[_state.PropertyCount];
			_eventRIDMap = new int[_state.EventCount];
			_resourceRIDMap = new int[_state.ResourceCount];

			_methodBodyPoints = new BuildBlobPoint[_state.MethodCount];
			_fieldDataPoints = new FieldDataPoint[_state.FieldDataCount];

			// Set metadata
			var metadataBuilder = PE.Tasks.Get<MetadataBuilder>(true);
			metadataBuilder.Metadata = _metadata;
		}

		private void LoadMembers()
		{
			_defHashComparer.CompareByObjectReference = true;

			LoadMembers(_module.Types);

			_defHashComparer.CompareByObjectReference = false;
		}

		private void LoadMembers(TypeDeclarationCollection types)
		{
			_state.TypeCount += types.Count;

			foreach (var type in types)
			{
				_state.MethodCount += type.Methods.Count;
				_state.FieldCount += type.Fields.Count;
				_state.PropertyCount += type.Properties.Count;
				_state.EventCount += type.Events.Count;

				_typeDefHash.Add(type);

				// Methods
				foreach (var method in type.Methods)
				{
					_methodDefHash.Add(method);
				}

				// Fields
				foreach (var field in type.Fields)
				{
					_fieldDefHash.Add(field);

					if (field.HasData)
					{
						_state.FieldDataCount++;
					}
				}

				// Nested types
				LoadMembers(type.NestedTypes);
			}
		}

		internal void FinalizeBuild()
		{
			_metadata.Blobs = new MetadataBlobStream(_state.BlobToPosition.ToArray());

			if (_methodBodyBlob.Length > 0)
			{
				var section = PE.GetSection(_methodBodySectionName);
				section.Blobs.Add(_methodBodyBlob, _methodBodyBlobPriority);
			}

			if (_fieldDataBlob.Length > 0)
			{
				var section = PE.GetSection(_fieldDataSectionName);
				section.Blobs.Add(_fieldDataBlob, _fieldDataBlobPriority);
			}

			if (_fieldCLIDataBlob.Length > 0)
			{
				var section = PE.GetSection(_fieldCLIDataSectionName);
				section.Blobs.Add(_fieldCLIDataBlob, _fieldCLIDataBlobPriority);
			}

			if (_fieldTLSDataBlob.Length > 0)
			{
				var section = PE.GetSection(_fieldTLSDataSectionName);
				section.Blobs.Add(_fieldTLSDataBlob, _fieldTLSDataBlobPriority);

				// Change CLI flags
				var corHeaderBuilder = PE.Tasks.Get<CorHeaderBuilder>(true);
				corHeaderBuilder.Flags &= ~CorFlags.ILOnly;
				corHeaderBuilder.Flags |= CorFlags.F32BitsRequired;
			}

			if (_managedResourceBlob.Length > 0)
			{
				var section = PE.GetSection(_managedResourceSectionName);
				section.Blobs.Add(_managedResourceBlob, _managedResourceBlobPriority);
			}

			_state = null;
		}

		private void SetEntryPointToken()
		{
			var entryPointSig = _module.EntryPoint;
			if (entryPointSig == null)
				return;

			int entryPointToken;
			switch (entryPointSig.SignatureType)
			{
				case SignatureType.Method:
					{
						var methodRef = (MethodReference)entryPointSig;

						int rid = _methodDefHash.IndexOf((MethodReference)entryPointSig) + 1;
						if (rid < 1)
						{
							throw new ModuleBuildException(string.Format(SR.MethodNotFound2, methodRef.ToString()));
						}

						entryPointToken = MetadataToken.Get(MetadataTokenType.Method, rid);
					}
					break;

				case SignatureType.File:
					{
						var fileRef = (FileReference)entryPointSig;

						int rid = BuildFile(fileRef);

						entryPointToken = MetadataToken.Get(MetadataTokenType.File, rid);
					}
					break;

				default:
					throw new InvalidOperationException();
			}

			var corHeaderBuilder = PE.Tasks.Get<CorHeaderBuilder>(true);
			corHeaderBuilder.EntryPointToken = entryPointToken;
		}

		#region Assembly

		private void BuildAssembly()
		{
			var assembly = _module.Assembly;

			var row = new AssemblyRow();
			row.Name = AddString(assembly.Name);
			row.Locale = AddString(assembly.Culture);

			if (assembly.DisableJITcompileOptimizer)
				row.Flags |= AssemblyFlags.DisableJITcompileOptimizer;

			if (assembly.EnableJITcompileTracking)
				row.Flags |= AssemblyFlags.EnableJITcompileTracking;

			row.Flags |= ((int)assembly.ProcessorArchitecture << (int)AssemblyFlags.PA_Shift);

			row.PublicKey = AddBlob(assembly.PublicKey);
			row.HashAlgId = assembly.HashAlgorithm;

			if (assembly.Version != null)
			{
				var version = assembly.Version;
				row.MajorVersion = version.Major;
				row.MinorVersion = version.Minor;
				row.BuildNumber = version.Build;
				row.RevisionNumber = version.Revision;
			}

			int rid = _state.AssemblyTable.Add(ref row);

			int token = MetadataToken.Get(MetadataTokenType.Assembly, rid);

			BuildCustomAttributes(assembly.CustomAttributes, token);
			BuildSecurityAttributes(assembly.SecurityAttributes, token);
		}

		private void BuildAssemblyReferences(AssemblyReferenceCollection assemblyReferences)
		{
			foreach (var assemblyRef in assemblyReferences)
			{
				BuildAssemblyRef(assemblyRef);
			}
		}

		public int BuildAssemblyRef(AssemblyReference assemblyRef)
		{
			int rid = _assemblyRefHash.IndexOf(assemblyRef) + 1;
			if (rid > 0)
				return rid;

			var row = new AssemblyRefRow();
			row.Name = AddString(assemblyRef.Name);
			row.Locale = AddString(assemblyRef.Culture);

			if (_retargetableAssemblyReferences)
				row.Flags |= AssemblyFlags.Retargetable;

			row.Flags |= ((int)assemblyRef.ProcessorArchitecture << (int)AssemblyFlags.PA_Shift);

			row.PublicKeyOrToken = AddBlob(assemblyRef.PublicKeyToken);

			if (assemblyRef.Version != null)
			{
				var version = assemblyRef.Version;
				row.MajorVersion = version.Major;
				row.MinorVersion = version.Minor;
				row.BuildNumber = version.Build;
				row.RevisionNumber = version.Revision;
			}

			_state.AssemblyRefTable.Add(ref row);

			rid = _state.AssemblyRefTable.Count;
			_assemblyRefHash.Add(assemblyRef);

			return rid;
		}

		#endregion

		#region Module

		private void BuildModule()
		{
			var row = new ModuleRow();
			row.Name = AddString(_module.Name);
			row.Mvid = AddGuid(_module.Mvid);
			row.EncId = 0;
			row.EncBaseId = 0;
			row.Generation = 0;

			int rid = _state.ModuleTable.Add(ref row);

			int token = MetadataToken.Get(MetadataTokenType.Module, rid);

			BuildCustomAttributes(_module.CustomAttributes, token);
		}

		private void BuildModuleReferences(ModuleReferenceCollection moduleReferences)
		{
			foreach (var moduleRef in moduleReferences)
			{
				BuildModuleRef(moduleRef);
			}
		}

		public int BuildModuleRef(ModuleReference moduleRef)
		{
			int rid = _moduleRefHash.IndexOf(moduleRef) + 1;
			if (rid > 0)
				return rid;

			var row = new ModuleRefRow();
			row.Name = AddString(moduleRef.Name);

			_state.ModuleRefTable.Add(ref row);

			rid = _state.ModuleRefTable.Count;
			_moduleRefHash.Add(moduleRef);

			return rid;
		}

		#endregion

		#region File

		private void BuildFiles(FileReferenceCollection fileReferences)
		{
			foreach (var fileRef in fileReferences)
			{
				BuildFile(fileRef);
			}
		}

		public int BuildFile(FileReference fileRef)
		{
			int rid = _fileRefHash.IndexOf(fileRef) + 1;
			if (rid > 0)
				return rid;

			var row = new FileRow();
			row.Name = AddString(fileRef.Name);
			row.HashValue = AddBlob(fileRef.HashValue);

			if (fileRef.ContainsMetadata)
				row.Flags = FileFlags.ContainsMetaData;
			else
				row.Flags = FileFlags.ContainsNoMetaData;

			rid = _state.FileTable.Add(ref row);
			_fileRefHash.Add(fileRef);

			return rid;
		}

		public int BuildFile(string fileName)
		{
			var fileRef = new FileReference(fileName, true, null);
			return BuildFile(fileRef);
		}

		#endregion

		#region Type

		private void BuildTypes(TypeDeclarationCollection types)
		{
			foreach (var type in types)
			{
				BuildType(type);
			}
		}

		private void BuildType(TypeDeclaration type)
		{
			var row = new TypeDefRow();
			row.Name = AddString(type.Name);
			row.Namespace = AddString(type.Namespace);
			row.FieldList = _state.FieldTable.Count + 1;
			row.MethodList = _state.MethodTable.Count + 1;

			BuildTypeFlags(type, ref row);

			if (type.BaseType != null)
			{
				row.Extends = MetadataToken.CompressTypeDefOrRef(BuildTypeSig(type.BaseType));
			}

			int rid = _state.TypeDefTable.Add(ref row);

			_typeRIDMap[rid - 1] = type.RID;

			int token = MetadataToken.Get(MetadataTokenType.TypeDef, rid);

			BuildCustomAttributes(type.CustomAttributes, token);
			BuildSecurityAttributes(type.SecurityAttributes, token);

			if (type.PackingSize.HasValue || type.ClassSize.HasValue)
			{
				BuildTypeLayout(type, rid);
			}

			BuildTypeInterfaces(type, rid);
			BuildGenericParameters(type.GenericParameters, token);
			BuildFields(type.Fields, rid);
			BuildMethods(type.Methods, rid);
			BuildProperties(type.Properties, rid);
			BuildEvents(type.Events, rid);
			BuildNestedTypes(type.NestedTypes, rid);
		}

		private void BuildTypeFlags(TypeDeclaration type, ref TypeDefRow row)
		{
			row.Flags |= (int)type.Visibility;
			row.Flags |= (int)type.Layout;
			row.Flags |= (int)type.CharSet;

			if (type.IsInterface)
				row.Flags |= TypeDefFlags.Interface;

			if (type.IsAbstract)
				row.Flags |= TypeDefFlags.Abstract;

			if (type.IsSealed)
				row.Flags |= TypeDefFlags.Sealed;

			if (type.IsSpecialName)
				row.Flags |= TypeDefFlags.SpecialName;

			if (type.IsRuntimeSpecialName)
				row.Flags |= TypeDefFlags.RTSpecialName;

			if (type.IsImportFromCOMTypeLib)
				row.Flags |= TypeDefFlags.Import;

			if (type.IsSerializable)
				row.Flags |= TypeDefFlags.Serializable;

			if (type.IsBeforeFieldInit)
				row.Flags |= TypeDefFlags.BeforeFieldInit;

			if (type.SecurityAttributes.Count > 0)
				row.Flags |= TypeDefFlags.HasSecurity;
		}

		private void BuildTypeLayout(TypeDeclaration type, int typeRID)
		{
			var row = new ClassLayoutRow();
			row.PackingSize = type.PackingSize.HasValue ? type.PackingSize.Value : 0;
			row.ClassSize = type.ClassSize.HasValue ? type.ClassSize.Value : 0;
			row.Parent = typeRID;

			_state.ClassLayoutTable.Add(ref row);
		}

		private void BuildTypeInterfaces(TypeDeclaration type, int typeRID)
		{
			foreach (var typeSig in type.Interfaces)
			{
				var row = new InterfaceImplRow();
				row.Class = typeRID;
				row.Interface = MetadataToken.CompressTypeDefOrRef(BuildTypeSig(typeSig));
				_state.InterfaceImplTable.Add(ref row);
			}
		}

		private void BuildNestedTypes(TypeDeclarationCollection nestedTypes, int enclosingTypeRID)
		{
			foreach (var nestedType in nestedTypes)
			{
				BuildNestedType(nestedType, enclosingTypeRID);
			}
		}

		private void BuildNestedType(TypeDeclaration nestedType, int enclosingTypeRID)
		{
			var row = new NestedClassRow();
			row.EnclosingClass = enclosingTypeRID;
			row.NestedClass = _state.TypeDefTable.Count + 1;
			_state.NestedClassTable.Add(ref row);

			BuildType(nestedType);
		}

		#endregion

		#region TypeSignature

		public int BuildTypeSig(TypeSignature typeSig, bool checkPrimitive = false)
		{
			if (typeSig == null)
				return MetadataToken.Get(MetadataTokenType.TypeRef, 0);

			if (typeSig.ElementCode == TypeElementCode.DeclaringType)
			{
				return BuildTypeSigInTypeDefOrRef((TypeReference)typeSig, checkPrimitive);
			}
			else
			{
				return BuildTypeSigInTypeSpec(typeSig);
			}
		}

		public int BuildTypeSigInTypeDefOrRef(TypeReference typeRef, bool checkPrimitive = false)
		{
			if (checkPrimitive)
			{
				var typeCode = typeRef.GetTypeCode(_module);
				if (typeCode != PrimitiveTypeCode.Undefined && typeCode != PrimitiveTypeCode.Type)
					return BuildTypeSigPrimitive(typeCode);
			}

			if (typeRef.ResolutionScope == null)
			{
				// Local type.
				int typeRID = _typeDefHash.IndexOf(typeRef) + 1;
				if (typeRID > 0)
					return MetadataToken.Get(MetadataTokenType.TypeDef, typeRID);
			}

			// External type.
			return BuildTypeSigInTypeRef(typeRef);
		}

		public int BuildTypeSigInTypeRef(TypeReference typeRef)
		{
			var row = new TypeRefRow();
			row.Name = AddString(typeRef.Name);
			row.Namespace = AddString(typeRef.Namespace);

			int resolutionScopeToken;
			if (typeRef.Owner != null)
			{
				switch (typeRef.Owner.SignatureType)
				{
					case SignatureType.Assembly:
						{
							int ownerRID = BuildAssemblyRef((AssemblyReference)typeRef.Owner);
							resolutionScopeToken = MetadataToken.Get(MetadataTokenType.AssemblyRef, ownerRID);
						}
						break;

					case SignatureType.Module:
						{
							var moduleRef = (ModuleReference)typeRef.Owner;
							if (moduleRef.Name == _module.Name)
							{
								resolutionScopeToken = MetadataToken.Get(MetadataTokenType.Module, 1);
							}
							else
							{
								int ownerRID = BuildModuleRef(moduleRef);
								resolutionScopeToken = MetadataToken.Get(MetadataTokenType.ModuleRef, ownerRID);
							}
						}
						break;

					case SignatureType.Type:
						{
							int ownerRID = BuildTypeSigInTypeRef((TypeReference)typeRef.Owner);
							resolutionScopeToken = MetadataToken.Get(MetadataTokenType.TypeRef, ownerRID);
						}
						break;

					default:
						throw new InvalidOperationException();
				}
			}
			else
			{
				resolutionScopeToken = MetadataToken.Get(MetadataTokenType.Module, 1);
			}

			row.ResolutionScope = MetadataToken.CompressResolutionScope(resolutionScopeToken);

			int rid;
			if (_state.TypeRefRowHash.TryAdd(row, out rid))
			{
				_state.TypeRefTable.Add(ref row);
			}

			rid++;

			return MetadataToken.Get(MetadataTokenType.TypeRef, rid);
		}

		public int BuildTypeSigInTypeSpec(TypeSignature typeRef)
		{
			var row = new TypeSpecRow();

			int pos = 0;
			var blob = new Blob();
			BuildTypeSig(blob, ref pos, typeRef);
			row.Signature = AddBlob(blob.GetBuffer(), 0, blob.Length);

			int rid;
			if (_state.TypeSpecRowHash.TryAdd(row, out rid))
			{
				_state.TypeSpecTable.Add(ref row);
			}

			rid++;

			return MetadataToken.Get(MetadataTokenType.TypeSpec, rid);
		}

		public int BuildTypeSigPrimitive(PrimitiveTypeCode typeCode)
		{
			var row = new TypeSpecRow();

			int pos = 0;
			var blob = new Blob();
			blob.WriteCompressedInteger(ref pos, CodeModelUtils.GetElementType(typeCode));
			row.Signature = AddBlob(blob.GetBuffer(), 0, blob.Length);

			int rid;
			if (_state.TypeSpecRowHash.TryAdd(row, out rid))
			{
				_state.TypeSpecTable.Add(ref row);
			}

			rid++;

			return MetadataToken.Get(MetadataTokenType.TypeSpec, rid);
		}

		private void BuildTypeSig(Blob blob, ref int pos, TypeSignature typeSig)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					BuildArrayType(blob, ref pos, (ArrayType)typeSig);
					break;

				case TypeElementCode.ByRef:
					BuildByRefType(blob, ref pos, (ByRefType)typeSig);
					break;

				case TypeElementCode.CustomModifier:
					BuildCustomModifier(blob, ref pos, (CustomModifier)typeSig);
					break;

				case TypeElementCode.FunctionPointer:
					BuildFunctionPointer(blob, ref pos, (FunctionPointer)typeSig);
					break;

				case TypeElementCode.GenericParameter:
					BuildGenericType(blob, ref pos, (GenericParameterType)typeSig);
					break;

				case TypeElementCode.GenericType:
					BuildGenericTypeRef(blob, ref pos, (GenericTypeReference)typeSig);
					break;

				case TypeElementCode.Pinned:
					BuildPinnedType(blob, ref pos, (PinnedType)typeSig);
					break;

				case TypeElementCode.Pointer:
					BuildPointerType(blob, ref pos, (PointerType)typeSig);
					break;

				case TypeElementCode.DeclaringType:
					BuildTypeRefOrPrimitive(blob, ref pos, (TypeReference)typeSig);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private void BuildArrayType(Blob blob, ref int pos, ArrayType arrayType)
		{
			if (arrayType.ArrayDimensions.Count > 0)
				blob.WriteCompressedInteger(ref pos, ElementType.Array);
			else
				blob.WriteCompressedInteger(ref pos, ElementType.SzArray);

			BuildTypeSig(blob, ref pos, arrayType.ElementType);

			if (arrayType.ArrayDimensions.Count > 0)
				BuildTypeArrayDimensions(blob, ref pos, arrayType.ArrayDimensions);
		}

		private void BuildTypeArrayDimensions(Blob blob, ref int pos, IReadOnlyList<ArrayDimension> dimensions)
		{
			int numberOfSizes = 0;
			int[] sizes = new int[dimensions.Count];

			int numberOfLoBounds = 0;
			int[] loBounds = new int[dimensions.Count];

			bool endOfSizes = false;
			bool endOfLoBounds = false;
			foreach (var dimension in dimensions)
			{
				int? lowerBound = dimension.LowerBound;
				int? upperBound = dimension.UpperBound;

				if (lowerBound.HasValue)
				{
					if (upperBound.HasValue)
					{
						// Both
						if (!endOfSizes)
							sizes[numberOfSizes++] = upperBound.Value - lowerBound.Value + 1;

						if (!endOfLoBounds)
							loBounds[numberOfLoBounds++] = lowerBound.Value;
					}
					else
					{
						// Lower bound
						if (!endOfLoBounds)
							loBounds[numberOfLoBounds++] = lowerBound.Value;

						endOfSizes = true;
					}
				}
				else
				{
					if (upperBound.HasValue)
					{
						// Upper bound
						if (!endOfSizes)
							sizes[numberOfSizes++] = upperBound.Value + 1; // lower bound is 0

						if (!endOfLoBounds)
							loBounds[numberOfLoBounds++] = 0;
					}
					else
					{
						// None
						endOfSizes = true;
						endOfLoBounds = true;
					}
				}
			}

			// Rank
			blob.WriteCompressedInteger(ref pos, dimensions.Count);

			// Sizes
			blob.WriteCompressedInteger(ref pos, numberOfSizes);
			for (int i = 0; i < numberOfSizes; i++)
			{
				blob.WriteCompressedInteger(ref pos, sizes[i]);
			}

			// LoBounds
			blob.WriteCompressedInteger(ref pos, numberOfLoBounds);
			for (int i = 0; i < numberOfLoBounds; i++)
			{
				blob.WriteCompressedInteger(ref pos, loBounds[i]);
			}
		}

		private void BuildByRefType(Blob blob, ref int pos, ByRefType byRefType)
		{
			blob.WriteCompressedInteger(ref pos, ElementType.ByRef);
			BuildTypeSig(blob, ref pos, byRefType.ElementType);
		}

		private void BuildCustomModifier(Blob blob, ref int pos, CustomModifier customModifier)
		{
			switch (customModifier.ModifierType)
			{
				case CustomModifierType.ModReq:
					blob.WriteCompressedInteger(ref pos, ElementType.CModReqD);
					break;

				case CustomModifierType.ModOpt:
					blob.WriteCompressedInteger(ref pos, ElementType.CModOpt);
					break;

				default:
					throw new InvalidOperationException();
			}

			int token = BuildTypeSig(customModifier.Modifier);
			blob.WriteCompressedInteger(ref pos, (int)MetadataToken.CompressTypeDefOrRef(token));

			BuildTypeSig(blob, ref pos, customModifier.ElementType);
		}

		private void BuildFunctionPointer(Blob blob, ref int pos, FunctionPointer functionPointer)
		{
			blob.WriteCompressedInteger(ref pos, ElementType.FnPtr);
			BuildCallSite(blob, ref pos, functionPointer.CallSite);
		}

		private void BuildGenericType(Blob blob, ref int pos, GenericParameterType genericType)
		{
			blob.WriteCompressedInteger(ref pos, genericType.IsMethod ? ElementType.MVar : ElementType.Var);
			blob.WriteCompressedInteger(ref pos, genericType.Position);
		}

		private void BuildGenericTypeRef(Blob blob, ref int pos, GenericTypeReference genericTypeRef)
		{
			if (genericTypeRef.GenericArguments.Count > 0)
			{
				blob.WriteCompressedInteger(ref pos, ElementType.GenericInst);
				BuildTypeRef(blob, ref pos, genericTypeRef.DeclaringType);
				BuildGenericArguments(blob, ref pos, genericTypeRef.GenericArguments);
			}
			else
			{
				BuildTypeRef(blob, ref pos, genericTypeRef.DeclaringType);
			}
		}

		private void BuildPinnedType(Blob blob, ref int pos, PinnedType pinnedType)
		{
			blob.WriteCompressedInteger(ref pos, ElementType.Pinned);
			BuildTypeSig(blob, ref pos, pinnedType.ElementType);
		}

		private void BuildPointerType(Blob blob, ref int pos, PointerType pointerType)
		{
			blob.WriteCompressedInteger(ref pos, ElementType.Ptr);
			BuildTypeSig(blob, ref pos, pointerType.ElementType);
		}

		private void BuildTypeRefOrPrimitive(Blob blob, ref int pos, TypeReference typeRef)
		{
			var typeCode = typeRef.GetTypeCode(_module);
			switch (typeCode)
			{
				case PrimitiveTypeCode.Undefined:
				case PrimitiveTypeCode.Type:
					BuildTypeRef(blob, ref pos, typeRef);
					break;

				default:
					{
						int elementType = CodeModelUtils.GetElementType(typeCode);
						blob.WriteCompressedInteger(ref pos, elementType);
					}
					break;
			}
		}

		private void BuildTypeRef(Blob blob, ref int pos, TypeReference typeRef)
		{
			bool? isValueType = typeRef.GetIsValueType(_module);
			if (isValueType.HasValue && isValueType.Value)
				blob.WriteCompressedInteger(ref pos, ElementType.ValueType);
			else
				blob.WriteCompressedInteger(ref pos, ElementType.Class);

			int token = BuildTypeSigInTypeDefOrRef(typeRef);
			blob.WriteCompressedInteger(ref pos, (int)MetadataToken.CompressTypeDefOrRef(token));
		}

		#endregion

		#region Generic

		private void BuildGenericParameters(GenericParameterCollection genericParameters, int owner)
		{
			for (int i = 0; i < genericParameters.Count; i++)
			{
				BuildGenericParameter(genericParameters[i], owner, i);
			}
		}

		private void BuildGenericParameter(GenericParameter genParam, int owner, int number)
		{
			var row = new GenericParamRow();
			row.Owner = MetadataToken.CompressTypeOrMethodDef(owner);
			row.Name = AddString(genParam.Name);
			row.Number = number;

			{
				// Flags
				switch (genParam.Variance)
				{
					case GenericParameterVariance.Covariant:
						row.Flags |= GenericParamFlags.Covariant;
						break;

					case GenericParameterVariance.Contravariant:
						row.Flags |= GenericParamFlags.Contravariant;
						break;
				}

				if (genParam.DefaultConstructorConstraint)
					row.Flags |= GenericParamFlags.DefaultConstructorConstraint;

				if (genParam.ReferenceTypeConstraint)
					row.Flags |= GenericParamFlags.ReferenceTypeConstraint;

				if (genParam.ValueTypeConstraint)
					row.Flags |= GenericParamFlags.NotNullableValueTypeConstraint;
			}

			int rid = _state.GenericParamTable.Add(ref row);
			int token = MetadataToken.Get(MetadataTokenType.GenericParam, rid);

			BuildGenericParameterConstraints(genParam.Constraints, rid);
			BuildCustomAttributes(genParam.CustomAttributes, token);
		}

		private void BuildGenericParameterConstraints(GenericParameterConstraintCollection constraints, int genParamRID)
		{
			foreach (var constraint in constraints)
			{
				var row = new GenericParamConstraintRow();
				row.Owner = genParamRID;
				row.Constraint = MetadataToken.CompressTypeDefOrRef(BuildTypeSig(constraint));
				_state.GenericParamConstraintTable.Add(ref row);
			}
		}

		private void BuildGenericArguments(Blob blob, ref int pos, IReadOnlyList<TypeSignature> genericArguments)
		{
			blob.WriteCompressedInteger(ref pos, genericArguments.Count);

			foreach (var genericArgument in genericArguments)
			{
				BuildTypeSig(blob, ref pos, genericArgument);
			}
		}

		#endregion

		#region Field

		private void BuildFields(FieldDeclarationCollection fields, int typeRID)
		{
			foreach (var field in fields)
			{
				BuildField(field, typeRID);
			}
		}

		private void BuildField(FieldDeclaration field, int typeRID)
		{
			var row = new FieldRow();
			row.Name = AddString(field.Name);

			BuildFieldFlags(field, ref row);

			{
				// Signature
				int pos = 0;
				var blob = new Blob();
				BuildField(blob, ref pos, field);
				row.Signature = AddBlob(blob.GetBuffer(), 0, blob.Length);
			}

			int rid = _state.FieldTable.Add(ref row);

			_fieldRIDMap[rid - 1] = field.RID;

			int token = MetadataToken.Get(MetadataTokenType.Field, rid);

			if (field.Offset.HasValue)
			{
				BuildFieldLayout(field, rid);
			}

			if (field.MarshalFieldType != null)
			{
				BuildMarshalType(field.MarshalFieldType, token);
			}

			if (field.DefaultValue.HasValue)
			{
				BuildConstant(field.DefaultValue.Value, token);
			}
			else if (field.HasData)
			{
				BuildFieldData(field, rid);
			}

			BuildCustomAttributes(field.CustomAttributes, token);
		}

		private void BuildField(Blob blob, ref int pos, FieldDeclaration field)
		{
			blob.Write(ref pos, (byte)Metadata.SignatureType.Field);
			BuildTypeSig(blob, ref pos, field.FieldType);
		}

		private void BuildFieldFlags(FieldDeclaration field, ref FieldRow row)
		{
			row.Flags |= (ushort)field.Visibility;

			if (field.IsStatic)
				row.Flags |= FieldFlags.Static;

			if (field.IsInitOnly)
				row.Flags |= FieldFlags.InitOnly;

			if (field.IsLiteral)
				row.Flags |= FieldFlags.Literal;

			if (field.IsNotSerialized)
				row.Flags |= FieldFlags.NotSerialized;

			if (field.IsSpecialName)
				row.Flags |= FieldFlags.SpecialName;

			if (field.IsRuntimeSpecialName)
				row.Flags |= FieldFlags.RTSpecialName;

			if (field.MarshalFieldType != null)
				row.Flags |= FieldFlags.HasFieldMarshal;

			if (field.DefaultValue.HasValue)
				row.Flags |= FieldFlags.HasDefault;
			else if (field.HasData)
				row.Flags |= FieldFlags.HasFieldRVA;
		}

		private void BuildFieldLayout(FieldDeclaration field, int fieldRID)
		{
			var row = new FieldLayoutRow();
			row.Field = fieldRID;
			row.OffSet = field.Offset.Value;
			_state.FieldLayoutTable.Add(ref row);
		}

		private void BuildFieldData(FieldDeclaration field, int fieldRID)
		{
			var row = new FieldRVARow();
			row.Field = fieldRID;
			int rid = _state.FieldRVATable.Add(ref row);

			FieldDataType dataType;
			byte[] data = field.GetData(out dataType);

			BuildBlob blob;
			switch (dataType)
			{
				case FieldDataType.Data:
					blob = _fieldDataBlob;
					break;

				case FieldDataType.Cli:
					blob = _fieldCLIDataBlob;
					break;

				case FieldDataType.Tls:
					blob = _fieldTLSDataBlob;
					break;

				default:
					throw new InvalidOperationException();
			}

			blob.Align(8, 0);

			_fieldDataPoints[rid - 1] =
				new FieldDataPoint()
				{
					Type = dataType,
					Offset = blob.Length,
				};

			int pos = blob.Length;
			blob.Write(ref pos, data ?? BufferUtils.EmptyArray);
		}

		#endregion

		#region FieldSignature

		public int BuildFieldSig(FieldReference fieldRef)
		{
			var owner = fieldRef.Owner;
			if (owner != null && owner.ResolutionScope == null)
			{
				int rid = _fieldDefHash.IndexOf(fieldRef) + 1;
				if (rid > 0)
					return MetadataToken.Get(MetadataTokenType.Field, rid);
			}

			return BuildFieldSigInMemberRef(fieldRef);
		}

		public int BuildFieldSigInMemberRef(FieldReference fieldRef)
		{
			var row = new MemberRefRow();
			row.Name = AddString(fieldRef.Name);

			// Owner
			row.Class = MetadataToken.CompressMemberRefParent(BuildFieldSigInMemberRefOwner(fieldRef));

			// Signature
			{
				int pos = 0;
				var blob = new Blob();
				blob.Write(ref pos, (byte)Metadata.SignatureType.Field);
				BuildTypeSig(blob, ref pos, fieldRef.FieldType);

				row.Signature = AddBlob(blob.GetBuffer(), 0, blob.Length);
			}

			int rid;
			if (_state.MemberRefRowHash.TryAdd(row, out rid))
			{
				_state.MemberRefTable.Add(ref row);
			}

			rid++;

			return MetadataToken.Get(MetadataTokenType.MemberRef, rid);
		}

		private int BuildFieldSigInMemberRefOwner(FieldReference fieldRef)
		{
			if (fieldRef.Owner.ElementCode == TypeElementCode.DeclaringType)
			{
				var typeRef = (TypeReference)fieldRef.Owner;

				if (typeRef.Name == CodeModelUtils.GlobalTypeName &&
					typeRef.Namespace == null &&
					typeRef.Owner != null &&
					typeRef.Owner.SignatureType == SignatureType.Module)
				{
					// A ModuleRef token, if the member is defined, in another module of the same assembly,
					// as a global function or variable.
					int ownerRID = BuildModuleRef((ModuleReference)typeRef.Owner);
					return MetadataToken.Get(MetadataTokenType.ModuleRef, ownerRID);
				}
			}

			return BuildTypeSig(fieldRef.Owner, true);
		}

		#endregion

		#region Method

		private void BuildMethods(MethodDeclarationCollection methods, int typeRID)
		{
			foreach (var method in methods)
			{
				BuildMethod(method, typeRID);
			}
		}

		private void BuildMethod(MethodDeclaration method, int typeRID)
		{
			var row = new MethodRow();
			row.Name = AddString(method.Name);
			row.ParamList = _state.ParamTable.Count + 1;

			BuildMethodFlags(method, ref row);
			BuildMethodImplFlags(method, ref row);

			{
				// Signature
				int pos = 0;
				var blob = new Blob();
				BuildMethod(blob, ref pos, method);
				row.Signature = AddBlob(blob.GetBuffer(), 0, blob.Length);
			}

			var rid = _state.MethodTable.Add(ref row);

			_methodRIDMap[rid - 1] = method.RID;

			int token = MetadataToken.Get(MetadataTokenType.Method, rid);

			BuildCustomAttributes(method.CustomAttributes, token);
			BuildSecurityAttributes(method.SecurityAttributes, token);
			BuildMethodReturnType(method.ReturnType, rid);
			BuildMethodParameters(method.Parameters, rid);
			BuildGenericParameters(method.GenericParameters, token);
			BuildMethodOverrides(method.Overrides, typeRID, rid);

			if (method.PInvoke != null)
			{
				BuildPInvoke(method.PInvoke, token);
			}

			if (method.HasBody)
			{
				BuildMethodBody(method, rid);
			}
		}

		private void BuildMethod(Blob blob, ref int pos, MethodDeclaration method)
		{
			byte sigType = (byte)method.CallConv;

			if (method.HasThis)
				sigType |= Metadata.SignatureType.HasThis;

			if (method.ExplicitThis)
				sigType |= Metadata.SignatureType.ExplicitThis;

			if (method.GenericParameters.Count > 0)
				sigType |= Metadata.SignatureType.Generic;

			blob.Write(ref pos, (byte)sigType);

			if (method.GenericParameters.Count > 0)
			{
				blob.WriteCompressedInteger(ref pos, method.GenericParameters.Count);
			}

			blob.WriteCompressedInteger(ref pos, method.Parameters.Count);

			BuildTypeSig(blob, ref pos, method.ReturnType.Type);

			BuildMethodParameters(blob, ref pos, method.Parameters);
		}

		private void BuildMethodFlags(MethodDeclaration method, ref MethodRow row)
		{
			row.Flags |= (ushort)method.Visibility;

			if (method.IsStatic)
				row.Flags |= MethodFlags.Static;

			if (method.IsFinal)
				row.Flags |= MethodFlags.Final;

			if (method.IsVirtual)
				row.Flags |= MethodFlags.Virtual;

			if (method.IsHideBySig)
				row.Flags |= MethodFlags.HideBySig;

			if (method.IsNewSlot)
				row.Flags |= MethodFlags.NewSlot;

			if (method.IsStrict)
				row.Flags |= MethodFlags.Strict;

			if (method.IsAbstract)
				row.Flags |= MethodFlags.Abstract;

			if (method.IsSpecialName)
				row.Flags |= MethodFlags.SpecialName;

			if (method.IsRuntimeSpecialName)
				row.Flags |= MethodFlags.RTSpecialName;

			if (method.RequireSecObject)
				row.Flags |= MethodFlags.RequireSecObject;

			if (method.PInvoke != null)
				row.Flags |= MethodFlags.PInvokeImpl;

			if (method.SecurityAttributes.Count > 0)
				row.Flags |= MethodFlags.HasSecurity;
		}

		private void BuildMethodImplFlags(MethodDeclaration method, ref MethodRow row)
		{
			row.ImplFlags |= (ushort)method.CodeType;

			if (!method.IsManaged)
				row.ImplFlags |= (ushort)MethodImplFlags.Unmanaged;

			if (method.IsForwardRef)
				row.ImplFlags |= MethodImplFlags.ForwardRef;

			if (method.IsPreserveSig)
				row.ImplFlags |= MethodImplFlags.PreserveSig;

			if (method.IsInternalCall)
				row.ImplFlags |= MethodImplFlags.InternalCall;

			if (method.IsSynchronized)
				row.ImplFlags |= MethodImplFlags.Synchronized;

			if (method.IsNoInlining)
				row.ImplFlags |= MethodImplFlags.NoInlining;
		}

		private void BuildMethodReturnType(MethodReturnType returnType, int methodRID)
		{
			if (returnType.MarshalType == null &&
				returnType.CustomAttributes.Count == 0)
				return;

			var row = new ParamRow();
			row.Sequence = 0;
			row.Flags = ParamFlags.ReturnValue;

			if (returnType.MarshalType != null)
				row.Flags |= ParamFlags.HasFieldMarshal;

			int rid = _state.ParamTable.Add(ref row);

			int token = MetadataToken.Get(MetadataTokenType.Param, rid);

			BuildCustomAttributes(returnType.CustomAttributes, token);

			if (returnType.MarshalType != null)
			{
				BuildMarshalType(returnType.MarshalType, token);
			}
		}

		private void BuildMethodParameters(Blob blob, ref int pos, MethodParameterCollection parameters)
		{
			foreach (var parameter in parameters)
			{
				BuildTypeSig(blob, ref pos, parameter.Type);
			}
		}

		private void BuildMethodParameters(MethodParameterCollection parameters, int methodRID)
		{
			for (int i = 0; i < parameters.Count; i++)
			{
				BuildMethodParameter(parameters[i], methodRID, i + 1);
			}
		}

		private void BuildMethodParameter(MethodParameter parameter, int methodRID, int sequence)
		{
			var row = new ParamRow();
			row.Sequence = (ushort)sequence;
			row.Name = AddString(parameter.Name);

			BuildMethodParameterFlags(parameter, ref row);

			int rid = _state.ParamTable.Add(ref row);

			int token = MetadataToken.Get(MetadataTokenType.Param, rid);

			BuildCustomAttributes(parameter.CustomAttributes, token);

			if (parameter.MarshalType != null)
			{
				BuildMarshalType(parameter.MarshalType, token);
			}

			if (parameter.DefaultValue.HasValue)
			{
				BuildConstant(parameter.DefaultValue.Value, token);
			}
		}

		private void BuildMethodParameterFlags(MethodParameter parameter, ref ParamRow row)
		{
			if (parameter.IsIn)
				row.Flags |= ParamFlags.In;

			if (parameter.IsOut)
				row.Flags |= ParamFlags.Out;

			if (parameter.IsOptional)
				row.Flags |= ParamFlags.Optional;

			if (parameter.IsLcid)
				row.Flags |= ParamFlags.Lcid;

			if (parameter.MarshalType != null)
				row.Flags |= ParamFlags.HasFieldMarshal;

			if (parameter.DefaultValue.HasValue)
				row.Flags |= ParamFlags.HasDefault;
		}

		private void BuildMethodOverrides(MethodOverrideCollection methodOverrides, int typeRID, int methodRID)
		{
			foreach (var methodOverride in methodOverrides)
			{
				BuildMethodOverride(methodOverride, typeRID, methodRID);
			}
		}

		private void BuildMethodOverride(MethodSignature methodOverride, int typeRID, int methodRID)
		{
			var row = new MethodImplRow();
			row.Class = typeRID;
			row.MethodBody = MetadataToken.CompressMethodDefOrRef(MetadataToken.Get(MetadataTokenType.Method, methodRID));
			row.MethodDeclaration = MetadataToken.CompressMethodDefOrRef(BuildMethodSig(methodOverride));

			_state.MethodImplTable.Add(ref row);
		}

		private void BuildMethodSemantic(MethodReference methodRef, int owner, int flags)
		{
			var row = new MethodSemanticsRow();
			row.Association = MetadataToken.CompressHasSemantic(owner);
			row.Method = BuildMethodSigInMethodDef(methodRef);
			row.Semantic = (ushort)flags;

			_state.MethodSemanticsTable.Add(ref row);
		}

		#endregion

		#region MethodBody

		private void BuildMethodBody(MethodDeclaration method, int methodRID)
		{
			switch (method.CodeType)
			{
				case MethodCodeTypeFlags.CIL:
					BuildILMethodBody(method, methodRID);
					break;

				case MethodCodeTypeFlags.Native:
					BuildNativeMethodBody(method, methodRID);
					break;
			}
			var methodBody = MethodBody.Load(method);
		}

		private void BuildILMethodBody(MethodDeclaration method, int methodRID)
		{
			var methodBody = MethodBody.Load(method);

			_methodBodyBlob.Align(4, 0);

			int pos = 0;
			var blob = new Blob();
			BuildILMethodBody(blob, ref pos, methodBody);

			pos = _methodBodyBlob.Length;

			_methodBodyPoints[methodRID - 1] =
				new BuildBlobPoint()
				{
					Offset = pos,
					Blob = _methodBodyBlob,
				};

			_methodBodyBlob.Write(ref pos, blob.GetBuffer(), 0, blob.Length);
		}

		private void BuildILMethodBody(Blob blob, ref int pos, MethodBody methodBody)
		{
			int codeSize;
			byte[] codeBlob = BuildILCodeBlob(methodBody, out codeSize);

			if (IsILTinyFormat(methodBody, codeSize))
			{
				// Tiny format
				byte flags;
				if ((codeSize % 2) == 0)
					flags = ILMethodFlags.TinyFormat;
				else
					flags = ILMethodFlags.TinyFormat1;

				flags |= (byte)(codeSize << 2);
				blob.Write(ref pos, (byte)flags);
				blob.Write(ref pos, codeBlob, 0, codeSize);
			}
			else
			{
				// Fat format
				int flags = ILMethodFlags.FatFormat;

				if (methodBody.InitLocals)
					flags |= ILMethodFlags.InitLocals;

				if (methodBody.ExceptionHandlers.Count > 0)
					flags |= ILMethodFlags.MoreSects;

				// Size of this header expressed as the count of 4-byte integers occupied (currently 3)
				flags |= (3 << 12);
				blob.Write(ref pos, (ushort)flags);
				blob.Write(ref pos, (ushort)methodBody.MaxStackSize);
				blob.Write(ref pos, (int)codeSize);
				BuildILLocalVariables(blob, ref pos, methodBody.LocalVariables);
				blob.Write(ref pos, codeBlob, 0, codeSize);
				if (methodBody.ExceptionHandlers.Count > 0)
				{
					BuildILExceptionHandlers(blob, ref pos, methodBody.ExceptionHandlers);
				}
			}
		}

		private bool IsILTinyFormat(MethodBody methodBody, int codeSize)
		{
			if (codeSize >= 0x40)
				return false;

			if (methodBody.LocalVariables.Count > 0)
				return false;

			if (methodBody.ExceptionHandlers.Count > 0)
				return false;

			if (methodBody.MaxStackSize > MethodBody.DefaultMaxStackSize)
				return false;

			if (methodBody.MaxStackSize < MethodBody.DefaultMaxStackSize)
			{
				if (!_optimizeTinyMethodBody)
					return false;

				methodBody.MaxStackSize = MethodBody.DefaultMaxStackSize;
			}

			return true;
		}

		private byte[] BuildILCodeBlob(MethodBody methodBody, out int codeSize)
		{
			var instructions = methodBody.Instructions;
			if (instructions.Count == 0)
			{
				codeSize = 0;
				return BufferUtils.EmptyArray;
			}

			int pos = 0;
			var blob = new Blob();

			foreach (var instruction in instructions)
			{
				var opCode = instruction.OpCode;
				if (opCode.OpByte1 == 0xff)
				{
					// one byte opcode
					blob.Write(ref pos, (byte)opCode.OpByte2);
				}
				else
				{
					// two bytes opcode
					blob.Write(ref pos, (byte)opCode.OpByte1);
					blob.Write(ref pos, (byte)opCode.OpByte2);
				}

				object value = instruction.Value;
				switch (opCode.OperandType)
				{
					case OperandType.InlineBrTarget:
						{
							blob.Write(ref pos, (int)value);
						}
						break;

					case OperandType.InlineField:
						{
							var fieldRef = (FieldReference)value;
							int token = BuildFieldSig(fieldRef);
							blob.Write(ref pos, (int)token);
						}
						break;

					case OperandType.InlineI:
						{
							blob.Write(ref pos, (int)value);
						}
						break;

					case OperandType.InlineI8:
						{
							blob.Write(ref pos, (long)value);
						}
						break;

					case OperandType.InlineMethod:
						{
							var methodSig = (MethodSignature)value;
							int token = BuildMethodSig(methodSig);
							blob.Write(ref pos, (int)token);
						}
						break;

					case OperandType.InlineR:
						{
							blob.Write(ref pos, (double)value);
						}
						break;

					case OperandType.InlineSig:
						{
							var callSite = (CallSite)value;
							int token = BuildMethodSigInStandAloneSig(callSite);
							blob.Write(ref pos, (int)token);
						}
						break;

					case OperandType.InlineString:
						{
							// Token of a userdefined string, whose RID portion is actually an offset in the #US blob stream.
							int rid = AddUserString((string)value);
							int token = MetadataToken.Get(MetadataTokenType.String, rid);
							blob.Write(ref pos, (int)token);
						}
						break;

					case OperandType.InlineSwitch:
						{
							int[] branches = (int[])value;

							blob.Write(ref pos, (int)branches.Length);

							for (int i = 0; i < branches.Length; i++)
								blob.Write(ref pos, (int)branches[i]);
						}
						break;

					case OperandType.InlineTok:
						{
							var signature = (Signature)value;

							int token;
							switch (signature.SignatureType)
							{
								case SignatureType.Type:
									token = BuildTypeSig((TypeSignature)signature);
									break;

								case SignatureType.Method:
									token = BuildMethodSig((MethodSignature)signature);
									break;

								case SignatureType.Field:
									token = BuildFieldSig((FieldReference)signature);
									break;

								default:
									throw new InvalidOperationException();
							}

							blob.Write(ref pos, (int)token);
						}
						break;

					case OperandType.InlineType:
						{
							var typeSig = (TypeSignature)value;
							int token = BuildTypeSig(typeSig, true);
							blob.Write(ref pos, (int)token);
						}
						break;

					case OperandType.InlineVar:
						{
							blob.Write(ref pos, (short)value);
						}
						break;

					case OperandType.ShortInlineBrTarget:
						{
							blob.Write(ref pos, (sbyte)value);
						}
						break;

					case OperandType.ShortInlineI:
						{
							blob.Write(ref pos, (byte)value);
						}
						break;

					case OperandType.ShortInlineR:
						{
							blob.Write(ref pos, (float)value);
						}
						break;

					case OperandType.ShortInlineVar:
						{
							blob.Write(ref pos, (byte)value);
						}
						break;
				}
			}

			codeSize = (int)blob.Length;

			return blob.GetBuffer();
		}

		private void BuildILLocalVariables(Blob blob, ref int pos, List<TypeSignature> localVariables)
		{
			int token = BuildILLocalVariables(localVariables);
			blob.Write(ref pos, (int)token);
		}

		public int BuildILLocalVariables(List<TypeSignature> localVariables)
		{
			if (localVariables.Count == 0)
				return 0;

			var row = new StandAloneSigRow();

			// Signature
			int pos = 0;
			var blob = new Blob();
			BuildILLocalVariableSignature(blob, ref pos, localVariables);
			row.Signature = AddBlob(blob.GetBuffer(), 0, blob.Length);

			int rid;
			if (_state.StandAloneSigRowHash.TryAdd(row, out rid))
			{
				_state.StandAloneSigTable.Add(ref row);
			}

			rid++;

			return MetadataToken.Get(MetadataTokenType.Signature, rid);
		}

		private void BuildILLocalVariableSignature(Blob blob, ref int pos, List<TypeSignature> localVariables)
		{
			blob.Write(ref pos, (byte)Metadata.SignatureType.LocalSig);

			blob.WriteCompressedInteger(ref pos, localVariables.Count);

			foreach (var localVariable in localVariables)
			{
				BuildTypeSig(blob, ref pos, localVariable);
			}
		}

		private void BuildILExceptionHandlers(Blob blob, ref int pos, List<ExceptionHandler> exceptionHandlers)
		{
			// Align method data sections.
			int positionInBlob = _methodBodyBlob.Length + pos;
			int numOfAlignBytes = positionInBlob.Align(4) - positionInBlob;
			if (numOfAlignBytes > 0)
			{
				blob.Write(ref pos, (byte)0, numOfAlignBytes);
			}

			int flags = ILMethodSect.EHTable;

			bool isFat = false;

			// Determine if exception handlers have to be stored in fat format.
			if ((exceptionHandlers.Count * 12) + 4 > Byte.MaxValue)
			{
				isFat = true;
			}
			else
			{
				foreach (var exceptionHandler in exceptionHandlers)
				{
					if (exceptionHandler.TryOffset > UInt16.MaxValue ||
						exceptionHandler.TryLength > Byte.MaxValue ||
						exceptionHandler.HandlerOffset > UInt16.MaxValue ||
						exceptionHandler.HandlerLength > Byte.MaxValue)
					{
						isFat = true;
						break;
					}
				}
			}

			if (isFat)
			{
				flags |= ILMethodSect.FatFormat;

				// Fat format
				int length = (exceptionHandlers.Count * 24) + 4;
				flags |= (length << 8);
				blob.Write(ref pos, (int)flags);

				foreach (var handler in exceptionHandlers)
				{
					BuildILExceptionHandlerFat(blob, ref pos, handler);
				}
			}
			else
			{
				// Tiny format
				blob.Write(ref pos, (byte)flags);

				int length = (exceptionHandlers.Count * 12) + 4;
				blob.Write(ref pos, (byte)length);

				blob.Write(ref pos, (byte)0, 2); // padded

				foreach (var handler in exceptionHandlers)
				{
					BuildILExceptionHandlerTiny(blob, ref pos, handler);
				}
			}
		}

		private void BuildILExceptionHandlerFat(Blob blob, ref int pos, ExceptionHandler handler)
		{
			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					blob.Write(ref pos, (int)ILExceptionFlag.CATCH);
					break;

				case ExceptionHandlerType.Filter:
					blob.Write(ref pos, (int)ILExceptionFlag.FILTER);
					break;

				case ExceptionHandlerType.Finally:
					blob.Write(ref pos, (int)ILExceptionFlag.FINALLY);
					break;

				case ExceptionHandlerType.Fault:
					blob.Write(ref pos, (int)ILExceptionFlag.FAULT);
					break;
			}

			blob.Write(ref pos, (int)handler.TryOffset);
			blob.Write(ref pos, (int)handler.TryLength);
			blob.Write(ref pos, (int)handler.HandlerOffset);
			blob.Write(ref pos, (int)handler.HandlerLength);

			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					{
						int token = BuildTypeSig(handler.CatchType);
						blob.Write(ref pos, (int)token);
					}
					break;

				case ExceptionHandlerType.Filter:
					{
						blob.Write(ref pos, (int)handler.FilterOffset);
					}
					break;

				default:
					{
						blob.Write(ref pos, (int)0); // padded
					}
					break;
			}
		}

		private void BuildILExceptionHandlerTiny(Blob blob, ref int pos, ExceptionHandler handler)
		{
			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					blob.Write(ref pos, (short)ILExceptionFlag.CATCH);
					break;

				case ExceptionHandlerType.Filter:
					blob.Write(ref pos, (short)ILExceptionFlag.FILTER);
					break;

				case ExceptionHandlerType.Finally:
					blob.Write(ref pos, (short)ILExceptionFlag.FINALLY);
					break;

				case ExceptionHandlerType.Fault:
					blob.Write(ref pos, (short)ILExceptionFlag.FAULT);
					break;
			}

			blob.Write(ref pos, (ushort)handler.TryOffset);
			blob.Write(ref pos, (byte)handler.TryLength);
			blob.Write(ref pos, (ushort)handler.HandlerOffset);
			blob.Write(ref pos, (byte)handler.HandlerLength);

			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					{
						int token = BuildTypeSig(handler.CatchType);
						blob.Write(ref pos, (int)token);
					}
					break;

				case ExceptionHandlerType.Filter:
					{
						blob.Write(ref pos, (int)handler.FilterOffset);
					}
					break;

				default:
					{
						blob.Write(ref pos, (int)0); // padded
					}
					break;
			}
		}

		protected virtual void BuildNativeMethodBody(MethodDeclaration method, int methodRID)
		{
			throw new ModuleBuildException(string.Format(SR.NativeMethodBodyNotSupported, method.GetOwnerType().ToString(), method.ToString()));
		}

		#endregion

		#region MethodSignature

		public int BuildMethodSig(MethodSignature methodSig)
		{
			switch (methodSig.Type)
			{
				case MethodSignatureType.CallSite:
					return BuildMethodSigInStandAloneSig((CallSite)methodSig);

				case MethodSignatureType.GenericMethod:
					return BuildMethodSigInMethodSpec((GenericMethodReference)methodSig);

				case MethodSignatureType.DeclaringMethod:
					return BuildMethodSigInMethodRefOrDef((MethodReference)methodSig);

				default:
					throw new InvalidOperationException();
			}
		}

		public int BuildMethodSigInMethodRefOrDef(MethodReference methodRef)
		{
			if (methodRef.CallConv != MethodCallingConvention.VarArgs)
			{
				var owner = methodRef.Owner;
				if (owner != null && owner.ResolutionScope == null)
				{
					int rid = _methodDefHash.IndexOf(methodRef) + 1;
					if (rid > 0)
						return MetadataToken.Get(MetadataTokenType.Method, rid);
				}
			}

			return BuildMethodSigInMemberRef(methodRef);
		}

		public int BuildMethodSigInMethodDef(MethodReference methodRef)
		{
			int rid = _methodDefHash.IndexOf(methodRef) + 1;
			if (rid < 1)
			{
				throw new ModuleBuildException(string.Format(SR.MethodNotFound2, methodRef.ToString()));
			}

			return MetadataToken.Get(MetadataTokenType.Method, rid);
		}

		public int BuildMethodSigInMemberRef(MethodReference methodRef)
		{
			var row = new MemberRefRow();
			row.Name = AddString(methodRef.Name);

			// Owner
			row.Class = MetadataToken.CompressMemberRefParent(BuildMethodSigInMemberRefOwner(methodRef));

			// Signature
			{
				int pos = 0;
				var blob = new Blob();
				BuildCallSite(blob, ref pos, methodRef.CallSite);
				row.Signature = AddBlob(blob.GetBuffer(), 0, blob.Length);
			}

			int rid;
			if (_state.MemberRefRowHash.TryAdd(row, out rid))
			{
				_state.MemberRefTable.Add(ref row);
			}

			rid++;

			return MetadataToken.Get(MetadataTokenType.MemberRef, rid);
		}

		public int BuildMethodSigInMemberRefOwner(MethodReference methodRef)
		{
			var owner = methodRef.Owner;
			if (owner.ElementCode == TypeElementCode.DeclaringType)
			{
				var typeRef = (TypeReference)methodRef.Owner;

				if (typeRef.Name == CodeModelUtils.GlobalTypeName &&
					typeRef.Namespace == null &&
					typeRef.Owner != null &&
					typeRef.Owner.SignatureType == SignatureType.Module)
				{
					// A ModuleRef token, if the member is defined, in another module of the same assembly,
					// as a global function or variable.
					int ownerRID = BuildModuleRef((ModuleReference)typeRef.Owner);
					return MetadataToken.Get(MetadataTokenType.ModuleRef, ownerRID);
				}

				if (owner.ResolutionScope == null)
				{
					int methodRID = _methodDefHash.IndexOf(methodRef) + 1;
					if (methodRID > 0)
					{
						// A MethodDef token, when used to supply a call-site signature for a vararg method that is
						// defined in this module. The Name shall match the Name in the corresponding MethodDef row.
						// The Signature shall match the Signature in the target method definition.
						return MetadataToken.Get(MetadataTokenType.Method, methodRID);
					}
				}
			}

			return BuildTypeSig(methodRef.Owner, true);
		}

		public int BuildMethodSigInMethodSpec(GenericMethodReference genericMethodRef)
		{
			if (genericMethodRef.GenericArguments.Count > 0)
			{
				var row = new MethodSpecRow();

				row.Method = MetadataToken.CompressMethodDefOrRef(BuildMethodSigInMethodRefOrDef(genericMethodRef.DeclaringMethod));

				// Signature
				int pos = 0;
				var blob = new Blob();
				blob.Write(ref pos, (byte)Metadata.SignatureType.GenericInst);
				BuildGenericArguments(blob, ref pos, genericMethodRef.GenericArguments);

				row.Instantiation = AddBlob(blob.GetBuffer(), 0, blob.Length);

				int rid;
				if (_state.MethodSpecRowHash.TryAdd(row, out rid))
				{
					_state.MethodSpecTable.Add(ref row);
				}

				rid++;

				return MetadataToken.Get(MetadataTokenType.MethodSpec, rid);
			}
			else
			{
				return BuildMethodSigInMethodRefOrDef(genericMethodRef.DeclaringMethod);
			}
		}

		public int BuildMethodSigInStandAloneSig(CallSite callSite)
		{
			var row = new StandAloneSigRow();

			int pos = 0;
			var blob = new Blob();
			BuildCallSite(blob, ref pos, callSite);

			row.Signature = AddBlob(blob.GetBuffer(), 0, blob.Length);

			int rid;
			if (_state.StandAloneSigRowHash.TryAdd(row, out rid))
			{
				_state.StandAloneSigTable.Add(ref row);
			}

			rid++;

			return MetadataToken.Get(MetadataTokenType.Signature, rid);
		}

		private void BuildCallSite(Blob blob, ref int pos, CallSite callSite)
		{
			byte callConv = (byte)callSite.CallConv;

			if (callSite.HasThis)
				callConv |= Metadata.SignatureType.HasThis;

			if (callSite.ExplicitThis)
				callConv |= Metadata.SignatureType.ExplicitThis;

			if (callSite.GenericParameterCount > 0)
				callConv |= Metadata.SignatureType.Generic;

			blob.Write(ref pos, (byte)callConv);

			if (callSite.GenericParameterCount > 0)
			{
				blob.WriteCompressedInteger(ref pos, callSite.GenericParameterCount);
			}

			blob.WriteCompressedInteger(ref pos, callSite.Arguments.Count);

			BuildTypeSig(blob, ref pos, callSite.ReturnType);

			BuildMethodArguments(blob, ref pos, callSite.Arguments, callSite.VarArgIndex);
		}

		private void BuildMethodArguments(Blob blob, ref int pos, IReadOnlyList<TypeSignature> arguments, int varArgIndex)
		{
			for (int i = 0; i < arguments.Count; i++)
			{
				if (i == varArgIndex)
				{
					blob.WriteCompressedInteger(ref pos, ElementType.Sentinel);
				}

				BuildTypeSig(blob, ref pos, arguments[i]);
			}
		}

		#endregion

		#region Property

		private void BuildProperties(PropertyDeclarationCollection properties, int typeRID)
		{
			if (properties.Count == 0)
				return;

			var mapRow = new PropertyMapRow();
			mapRow.Parent = typeRID;
			mapRow.PropertyList = _state.PropertyTable.Count + 1;
			_state.PropertyMapTable.Add(ref mapRow);

			foreach (var property in properties)
			{
				BuildProperty(property);
			}
		}

		private void BuildProperty(PropertyDeclaration property)
		{
			var row = new PropertyRow();
			row.Name = AddString(property.Name);

			{
				// Flags
				if (property.IsSpecialName)
					row.Flags |= PropertyFlags.SpecialName;

				if (property.IsRuntimeSpecialName)
					row.Flags |= PropertyFlags.RTSpecialName;

				if (property.DefaultValue.HasValue)
					row.Flags |= PropertyFlags.HasDefault;
			}

			{
				// Signature
				int pos = 0;
				var blob = new Blob();
				BuildProperty(blob, ref pos, property);
				row.Type = AddBlob(blob.GetBuffer(), 0, blob.Length);
			}

			int rid = _state.PropertyTable.Add(ref row);

			_propertyRIDMap[rid - 1] = property.RID;

			int token = MetadataToken.Get(MetadataTokenType.Property, rid);

			BuildCustomAttributes(property.CustomAttributes, token);
			BuildPropertySemantics(property, token);

			if (property.DefaultValue.HasValue)
			{
				BuildConstant(property.DefaultValue.Value, token);
			}
		}

		private void BuildProperty(Blob blob, ref int pos, PropertyDeclaration property)
		{
			byte sigType = Metadata.SignatureType.Property;

			if (property.HasThis)
				sigType |= Metadata.SignatureType.HasThis;

			blob.Write(ref pos, (byte)sigType);

			blob.WriteCompressedInteger(ref pos, property.Parameters.Count);

			BuildTypeSig(blob, ref pos, property.ReturnType);

			BuildPropertyParameters(blob, ref pos, property.Parameters);
		}

		private void BuildPropertyParameters(Blob blob, ref int pos, PropertyParameterCollection parameters)
		{
			foreach (var parameter in parameters)
			{
				BuildTypeSig(blob, ref pos, parameter);
			}
		}

		private void BuildPropertySemantics(PropertyDeclaration property, int owner)
		{
			if (property.GetMethod != null)
			{
				BuildMethodSemantic(property.GetMethod, owner, MethodSemanticFlags.Getter);
			}

			if (property.SetMethod != null)
			{
				BuildMethodSemantic(property.SetMethod, owner, MethodSemanticFlags.Setter);
			}
		}

		#endregion

		#region Event

		private void BuildEvents(EventDeclarationCollection events, int typeRID)
		{
			if (events.Count == 0)
				return;

			var mapRow = new EventMapRow();
			mapRow.Parent = typeRID;
			mapRow.EventList = _state.EventTable.Count + 1;
			_state.EventMapTable.Add(ref mapRow);

			foreach (EventDeclaration eventDecl in events)
			{
				BuildEvent(eventDecl);
			}
		}

		private void BuildEvent(EventDeclaration eventDecl)
		{
			var row = new EventRow();
			row.Name = AddString(eventDecl.Name);

			{
				// Flags
				if (eventDecl.IsSpecialName)
					row.Flags |= EventFlags.SpecialName;

				if (eventDecl.IsRuntimeSpecialName)
					row.Flags |= EventFlags.RTSpecialName;
			}

			if (eventDecl.EventType != null)
			{
				row.EventType = MetadataToken.CompressTypeDefOrRef(BuildTypeSig(eventDecl.EventType));
			}

			int rid = _state.EventTable.Add(ref row);

			_eventRIDMap[rid - 1] = eventDecl.RID;

			int token = MetadataToken.Get(MetadataTokenType.Event, rid);

			BuildEventSemantics(eventDecl, token);
			BuildCustomAttributes(eventDecl.CustomAttributes, token);
		}

		private void BuildEventSemantics(EventDeclaration eventDecl, int owner)
		{
			if (eventDecl.AddMethod != null)
			{
				BuildMethodSemantic(eventDecl.AddMethod, owner, MethodSemanticFlags.AddOn);
			}

			if (eventDecl.RemoveMethod != null)
			{
				BuildMethodSemantic(eventDecl.RemoveMethod, owner, MethodSemanticFlags.RemoveOn);
			}

			if (eventDecl.InvokeMethod != null)
			{
				BuildMethodSemantic(eventDecl.InvokeMethod, owner, MethodSemanticFlags.Fire);
			}
		}

		#endregion

		#region Resource

		private void BuildResources(ResourceCollection resources)
		{
			foreach (var resource in resources)
			{
				BuildManagedResource(resource);
			}
		}

		public void BuildManagedResource(Resource resource)
		{
			var row = new ManifestResourceRow();
			row.Name = AddString(resource.Name);
			row.Flags = (int)resource.Visibility;

			if (resource.Owner != null)
			{
				switch (resource.Owner.SignatureType)
				{
					case SignatureType.Assembly:
						{
							row.Offset = resource.Offset;
							int ownerRID = BuildAssemblyRef((AssemblyReference)resource.Owner);
							row.Implementation = MetadataToken.CompressImplementation(MetadataToken.Get(MetadataTokenType.AssemblyRef, ownerRID));
						}
						break;

					case SignatureType.File:
						{
							row.Offset = resource.Offset;
							int ownerRID = BuildFile((FileReference)resource.Owner);
							row.Implementation = MetadataToken.CompressImplementation(MetadataToken.Get(MetadataTokenType.File, ownerRID));
						}
						break;

					default:
						throw new InvalidOperationException();
				}
			}
			else
			{
				row.Offset = _managedResourceBlob.Length;

				byte[] data = resource.GetData();

				int pos = _managedResourceBlob.Length;
				_managedResourceBlob.Write(ref pos, (int)data.Length);
				_managedResourceBlob.Write(ref pos, data);
				_managedResourceBlob.Align(8, 0);
			}

			int rid = _state.ManifestResourceTable.Add(ref row);

			_resourceRIDMap[rid - 1] = resource.RID;

			int token = MetadataToken.Get(MetadataTokenType.ManifestResource, rid);

			BuildCustomAttributes(resource.CustomAttributes, token);
		}

		public void BuildManagedResource(string name, ResourceVisibilityFlags visibility, byte[] data, int offset, int size)
		{
			var row = new ManifestResourceRow();
			row.Name = AddString(name);
			row.Flags = (int)visibility;
			row.Offset = _managedResourceBlob.Length;

			int pos = _managedResourceBlob.Length;
			_managedResourceBlob.Write(ref pos, (int)size);
			_managedResourceBlob.Write(ref pos, data, offset, size);
			_managedResourceBlob.Align(8, 0);

			_state.ManifestResourceTable.Add(ref row);
		}

		#endregion

		#region ExportedType

		private void BuildExportedTypes(ExportedTypeCollection exportedTypes)
		{
			foreach (var exportedType in exportedTypes)
			{
				BuildExportedType(exportedType);
			}
		}

		private int BuildExportedType(TypeReference exportedType)
		{
			int rid = _exportedTypeHash.IndexOf(exportedType) + 1;
			if (rid > 0)
				return rid;

			var row = new ExportedTypeRow();
			row.TypeName = AddString(exportedType.Name);
			row.TypeNamespace = AddString(exportedType.Namespace);

			if (exportedType.Owner == null)
			{
				throw new ModuleBuildException(string.Format(SR.AssemblyLoadError, _module.Location));
			}

			int implementationToken;
			switch (exportedType.Owner.SignatureType)
			{
				case SignatureType.Assembly:
					{
						row.Flags = TypeDefFlags.Public;
						int ownerRID = BuildAssemblyRef((AssemblyReference)exportedType.Owner);
						implementationToken = MetadataToken.Get(MetadataTokenType.AssemblyRef, ownerRID);
					}
					break;

				case SignatureType.Module:
					{
						row.Flags = TypeDefFlags.Public;
						int ownerRID = BuildFile(((ModuleReference)exportedType.Owner).Name);
						implementationToken = MetadataToken.Get(MetadataTokenType.File, ownerRID);
					}
					break;

				case SignatureType.Type:
					{
						row.Flags = TypeDefFlags.NestedPublic;
						int ownerRID = BuildExportedType((TypeReference)exportedType.Owner);
						implementationToken = MetadataToken.Get(MetadataTokenType.ExportedType, ownerRID);
					}
					break;

				default:
					throw new InvalidOperationException();
			}

			row.Implementation = MetadataToken.CompressImplementation(implementationToken);

			rid = _state.ExportedTypeTable.Add(ref row);
			_exportedTypeHash.Add(exportedType);

			return rid;
		}

		#endregion

		#region CustomAttribute

		private void BuildCustomAttributes(CustomAttributeCollection customAttributes, int parent)
		{
			foreach (var customAttribute in customAttributes)
			{
				BuildCustomAttribute(customAttribute, parent);
			}
		}

		private void BuildCustomAttribute(CustomAttribute customAttribute, int parent)
		{
			var row = new CustomAttributeRow();
			row.Parent = MetadataToken.CompressHasCustomAttribute(parent);
			row.Type = MetadataToken.CompressCustomAttributeType(BuildMethodSig(customAttribute.Constructor));

			// Signature
			if (customAttribute.FailedToLoad)
			{
				row.Value = AddBlob(customAttribute.GetRawData());
			}
			else
			{
				int pos = 0;
				var blob = new Blob();
				BuildCustomAttribute(blob, ref pos, customAttribute);
				row.Value = AddBlob(blob.GetBuffer(), 0, blob.Length);
			}

			_state.CustomAttributeTable.Add(ref row);
		}

		private void BuildCustomAttribute(Blob blob, ref int pos, CustomAttribute customAttribute)
		{
			// The encoded blob begins with the prolog, which is always the 2-byte value 0x0001.
			// This is actually the version of the custom attribute blob encoding scheme, which hasn’t changed
			// since its introduction, so the prolog is the same for all existing versions of the runtime.
			blob.Write(ref pos, (short)0x0001);

			// Indexed arguments
			foreach (var ctorArgument in customAttribute.CtorArguments)
			{
				BuildCustomAttributeCtorArgument(blob, ref pos, ctorArgument);
			}

			// Named arguments
			blob.Write(ref pos, (short)customAttribute.NamedArguments.Count);
			foreach (var namedArgument in customAttribute.NamedArguments)
			{
				BuildCustomAttributeNamedArgument(blob, ref pos, namedArgument);
			}
		}

		private void BuildCustomAttributeCtorArgument(Blob blob, ref int pos, CustomAttributeTypedArgument argument)
		{
			var value = argument.Value;
			bool isArray = false;
			int arrayLength = 0;

			if (value != null)
			{
				var valueType = value.GetType();
				if (valueType.IsArray)
				{
					isArray = true;
					arrayLength = ((Array)value).Length;
					BuildCustomAttributeBlobArray(blob, ref pos, arrayLength);
				}
			}

			var type = argument.Type;
			var typeCode = type.GetTypeCode(_module);

			BuildCustomAttributeValue(blob, ref pos, argument.Value, typeCode, isArray);
		}

		private void BuildCustomAttributeNamedArgument(Blob blob, ref int pos, CustomAttributeNamedArgument argument)
		{
			switch (argument.Type)
			{
				case CustomAttributeNamedArgumentType.Field:
					blob.Write(ref pos, (byte)ElementType.Field);
					break;

				case CustomAttributeNamedArgumentType.Property:
					blob.Write(ref pos, (byte)ElementType.Property);
					break;

				default:
					throw new InvalidOperationException();
			}

			BuildCustomAttributeValue(blob, ref pos, argument.TypedValue, argument.Name);
		}

		private void BuildCustomAttributeValue(Blob blob, ref int pos, CustomAttributeTypedArgument typedValue, string name)
		{
			var value = typedValue.Value;
			var type = typedValue.Type;
			var typeCode = type.GetTypeCode(_module);

			if (typeCode != PrimitiveTypeCode.Undefined)
			{
				bool isArray = false;
				int arrayLength = 0;

				if (value != null)
				{
					var valueType = value.GetType();
					if (valueType.IsArray)
					{
						isArray = true;
						arrayLength = ((Array)value).Length;
						blob.Write(ref pos, (byte)ElementType.SzArray);
					}
				}

				BuildCustomAttributeElementType(blob, ref pos, typeCode);

				if (name != null)
				{
					BuildCustomAttributeBlobString(blob, ref pos, name);
				}

				if (isArray)
				{
					BuildCustomAttributeBlobArray(blob, ref pos, arrayLength);
				}

				BuildCustomAttributeValue(blob, ref pos, value, typeCode, isArray);
			}
			else
			{
				var enumTypedValue = (CustomAttributeTypedArgument)value;
				var enumValue = enumTypedValue.Value;
				var enumType = enumTypedValue.Type;

				bool isArray = false;
				int arrayLength = 0;

				if (enumValue != null)
				{
					var valueType = enumValue.GetType();
					if (valueType.IsArray)
					{
						isArray = true;
						arrayLength = ((Array)enumValue).Length;
						blob.Write(ref pos, (byte)ElementType.SzArray);
					}
				}

				blob.Write(ref pos, (byte)ElementType.Enum);
				BuildCustomAttributeBlobString(blob, ref pos, type.ToReflectionString());

				if (name != null)
				{
					BuildCustomAttributeBlobString(blob, ref pos, name);
				}

				if (isArray)
				{
					BuildCustomAttributeBlobArray(blob, ref pos, arrayLength);
				}

				var enumTypeCode = enumType.GetTypeCode(_module);
				BuildCustomAttributeValue(blob, ref pos, enumValue, enumTypeCode, isArray);
			}
		}

		private void BuildCustomAttributeValue(Blob blob, ref int pos, object value, PrimitiveTypeCode typeCode, bool isArray)
		{
			switch (typeCode)
			{
				case PrimitiveTypeCode.Boolean:
					BuildCustomAttributeValueBool(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.Char:
					BuildCustomAttributeValueChar(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.Int8:
					BuildCustomAttributeValueInt8(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.Int16:
					BuildCustomAttributeValueInt16(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.Int32:
					BuildCustomAttributeValueInt32(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.Int64:
					BuildCustomAttributeValueInt64(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.UInt8:
					BuildCustomAttributeValueUInt8(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.UInt16:
					BuildCustomAttributeValueUInt16(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.UInt32:
					BuildCustomAttributeValueUInt32(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.UInt64:
					BuildCustomAttributeValueUInt64(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.Float32:
					BuildCustomAttributeValueFloat32(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.Float64:
					BuildCustomAttributeValueFloat64(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.Type:
					BuildCustomAttributeValueType(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.Object:
					BuildCustomAttributeValueObject(blob, ref pos, value, isArray);
					break;

				case PrimitiveTypeCode.String:
					BuildCustomAttributeValueString(blob, ref pos, value, isArray);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private void BuildCustomAttributeValueBool(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				bool[] array = (bool[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (byte)(array[i] ? 1 : 0));
				}
			}
			else
			{
				blob.Write(ref pos, (byte)((bool)value ? 1 : 0));
			}
		}

		private void BuildCustomAttributeValueChar(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				short[] array = (short[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (short)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (short)value);
			}
		}

		private void BuildCustomAttributeValueInt8(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				sbyte[] array = (sbyte[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (sbyte)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (sbyte)value);
			}
		}

		private void BuildCustomAttributeValueInt16(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				short[] array = (short[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (short)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (short)value);
			}
		}

		private void BuildCustomAttributeValueInt32(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				int[] array = (int[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (int)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (int)value);
			}
		}

		private void BuildCustomAttributeValueInt64(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				long[] array = (long[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (long)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (long)value);
			}
		}

		private void BuildCustomAttributeValueUInt8(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				byte[] array = (byte[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (byte)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (byte)value);
			}
		}

		private void BuildCustomAttributeValueUInt16(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				ushort[] array = (ushort[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (ushort)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (ushort)value);
			}
		}

		private void BuildCustomAttributeValueUInt32(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				uint[] array = (uint[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (uint)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (uint)value);
			}
		}

		private void BuildCustomAttributeValueUInt64(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				ulong[] array = (ulong[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (ulong)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (ulong)value);
			}
		}

		private void BuildCustomAttributeValueFloat32(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				float[] array = (float[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (float)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (float)value);
			}
		}

		private void BuildCustomAttributeValueFloat64(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				double[] array = (double[])value;
				for (int i = 0; i < array.Length; i++)
				{
					blob.Write(ref pos, (double)array[i]);
				}
			}
			else
			{
				blob.Write(ref pos, (double)value);
			}
		}

		private void BuildCustomAttributeValueType(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				var array = (TypeSignature[])value;
				for (int i = 0; i < array.Length; i++)
				{
					BuildCustomAttributeValueType(blob, ref pos, array[i]);
				}
			}
			else
			{
				BuildCustomAttributeValueType(blob, ref pos, (TypeSignature)value);
			}
		}

		private void BuildCustomAttributeValueType(Blob blob, ref int pos, TypeSignature value)
		{
			string typeName = value != null ? value.ToReflectionString() : null;
			BuildCustomAttributeBlobString(blob, ref pos, typeName);
		}

		private void BuildCustomAttributeValueObject(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				var array = (CustomAttributeTypedArgument[])value;
				for (int i = 0; i < array.Length; i++)
				{
					BuildCustomAttributeValue(blob, ref pos, (CustomAttributeTypedArgument)array[i], null);
				}
			}
			else
			{
				BuildCustomAttributeValue(blob, ref pos, (CustomAttributeTypedArgument)value, null);
			}
		}

		private void BuildCustomAttributeValueString(Blob blob, ref int pos, object value, bool isArray)
		{
			if (isArray)
			{
				string[] array = (string[])value;
				for (int i = 0; i < array.Length; i++)
				{
					BuildCustomAttributeBlobString(blob, ref pos, (string)array[i]);
				}
			}
			else
			{
				BuildCustomAttributeBlobString(blob, ref pos, (string)value);
			}
		}

		private void BuildCustomAttributeElementType(Blob blob, ref int pos, PrimitiveTypeCode typeCode)
		{
			int elementType = CodeModelUtils.GetElementType(typeCode);
			if (elementType == ElementType.Object)
			{
				elementType = ElementType.Boxed;
			}

			blob.Write(ref pos, (byte)elementType);
		}

		private void BuildCustomAttributeBlobString(Blob blob, ref int pos, string value)
		{
			if (value == null)
			{
				blob.Write(ref pos, (byte)0xff);
			}
			else if (value == string.Empty)
			{
				blob.Write(ref pos, (byte)0);
			}
			else
			{
				byte[] valueBytes = Encoding.UTF8.GetBytes(value);
				blob.WriteCompressedInteger(ref pos, valueBytes.Length);
				blob.Write(ref pos, valueBytes);
			}
		}

		private void BuildCustomAttributeBlobArray(Blob blob, ref int pos, int length)
		{
			blob.Write(ref pos, (uint)(length > 0 ? (uint)length : 0xffffffff));
		}

		#endregion

		#region SecurityAttribute

		private void BuildSecurityAttributes(SecurityAttributeCollection securityAttributes, int parent)
		{
			var sets = new List<SecurityAttributeSet>();
			var setByAction = new Dictionary<SecurityAction, SecurityAttributeSet>();

			foreach (var securityAttribute in securityAttributes)
			{
				if (!string.IsNullOrEmpty(securityAttribute.Xml))
				{
					var set = new SecurityAttributeSet();
					set.Action = securityAttribute.Action;
					set.Xml = securityAttribute.Xml;
					sets.Add(set);
				}
				else
				{
					SecurityAttributeSet set;
					if (!setByAction.TryGetValue(securityAttribute.Action, out set))
					{
						set = new SecurityAttributeSet();
						set.Action = securityAttribute.Action;
						set.Attributes = new List<SecurityAttribute>();
						sets.Add(set);
						setByAction.Add(securityAttribute.Action, set);
					}

					set.Attributes.Add(securityAttribute);
				}
			}

			foreach (var set in sets)
			{
				BuildSecurityAttributes(set, parent);
			}
		}

		private void BuildSecurityAttributes(SecurityAttributeSet securityAttributeSet, int parent)
		{
			var row = new DeclSecurityRow();
			row.Parent = MetadataToken.CompressHasDeclSecurity(parent);
			row.Action = securityAttributeSet.Action;

			if (!string.IsNullOrEmpty(securityAttributeSet.Xml))
			{
				byte[] xmlData = Encoding.Unicode.GetBytes(securityAttributeSet.Xml);
				row.PermissionSet = AddBlob(xmlData);
			}
			else
			{
				int pos = 0;
				var blob = new Blob();
				BuildSecurityAttributes(blob, ref pos, securityAttributeSet.Attributes);
				row.PermissionSet = AddBlob(blob.GetBuffer(), 0, blob.Length);
			}

			_state.DeclSecurityTable.Add(ref row);
		}

		private void BuildSecurityAttributes(Blob blob, ref int pos, List<SecurityAttribute> securityAttributes)
		{
			// An XML text cannot begin with a dot, so the system identifies the type of encoding (XML or binary)
			// by the very first byte.
			blob.Write(ref pos, (byte)0x2E); // '.' character

			blob.WriteCompressedInteger(ref pos, securityAttributes.Count);

			foreach (var securityAttribute in securityAttributes)
			{
				BuildSecurityAttribute(blob, ref pos, securityAttribute);
			}
		}

		private void BuildSecurityAttribute(Blob blob, ref int pos, SecurityAttribute securityAttribute)
		{
			if (securityAttribute.FailedToLoad)
			{
				blob.Write(ref pos, securityAttribute.GetRawData());
			}
			else
			{
				string typeName = securityAttribute.Type.ToReflectionString();
				blob.WriteCompressedInteger(ref pos, typeName.Length);
				blob.Write(ref pos, typeName, Encoding.UTF8);

				// Signature
				int sigPos = 0;
				var sigBlob = new Blob();
				BuildSecurityAttributeArguments(sigBlob, ref sigPos, securityAttribute.NamedArguments);

				blob.WriteCompressedInteger(ref pos, sigBlob.Length);
				blob.Write(ref pos, sigBlob.GetBuffer(), 0, sigBlob.Length);
			}
		}

		private void BuildSecurityAttributeArguments(Blob blob, ref int pos, CustomAttributeNamedArgumentCollection arguments)
		{
			blob.WriteCompressedInteger(ref pos, arguments.Count);
			foreach (var argument in arguments)
			{
				BuildCustomAttributeNamedArgument(blob, ref pos, argument);
			}
		}

		#endregion

		#region Marshal

		private void BuildMarshalType(MarshalType marshalType, int parent)
		{
			var row = new FieldMarshalRow();
			row.Parent = MetadataToken.CompressHasFieldMarshal(parent);

			int pos = 0;
			var blob = new Blob();
			BuildMarshalType(blob, ref pos, marshalType);
			row.NativeType = AddBlob(blob.GetBuffer(), 0, blob.Length);

			_state.FieldMarshalTable.Add(ref row);
		}

		private void BuildMarshalType(Blob blob, ref int pos, MarshalType marshalType)
		{
			switch (marshalType.UnmanagedType)
			{
				case UnmanagedType.LPArray:
					BuildLPArrayMarshalType(blob, ref pos, (LPArrayMarshalType)marshalType);
					break;

				case UnmanagedType.ByValArray:
					BuildByValArrayMarshalType(blob, ref pos, (ByValArrayMarshalType)marshalType);
					break;

				case UnmanagedType.ByValTStr:
					BuildByValTStrMarshalType(blob, ref pos, (ByValTStrMarshalType)marshalType);
					break;

				case UnmanagedType.SafeArray:
					BuildSafeArrayMarshalType(blob, ref pos, (SafeArrayMarshalType)marshalType);
					break;

				case UnmanagedType.CustomMarshaler:
					BuildCustomMarshalType(blob, ref pos, (CustomMarshalType)marshalType);
					break;

				default:
					BuildPrimitiveMarshalType(blob, ref pos, (PrimitiveMarshalType)marshalType);
					break;
			}
		}

		private void BuildLPArrayMarshalType(Blob blob, ref int pos, LPArrayMarshalType marshalType)
		{
			blob.Write(ref pos, (byte)UnmanagedType.LPArray);
			blob.Write(ref pos, (byte)marshalType.ArraySubType);

			if (marshalType.ArraySubType != UnmanagedType.Max)
			{
				if (marshalType.IIdParameterIndex.HasValue)
				{
					blob.WriteCompressedInteger(ref pos, marshalType.IIdParameterIndex.Value);
				}

				if (marshalType.LengthParamIndex.HasValue)
				{
					blob.WriteCompressedInteger(ref pos, marshalType.LengthParamIndex.Value);
				}

				if (marshalType.ArrayLength.HasValue)
				{
					blob.WriteCompressedInteger(ref pos, marshalType.ArrayLength.Value);
				}
			}
		}

		private void BuildByValArrayMarshalType(Blob blob, ref int pos, ByValArrayMarshalType marshalType)
		{
			blob.Write(ref pos, (byte)UnmanagedType.ByValArray);
			blob.WriteCompressedInteger(ref pos, marshalType.Length);
		}

		private void BuildByValTStrMarshalType(Blob blob, ref int pos, ByValTStrMarshalType marshalType)
		{
			blob.Write(ref pos, (byte)UnmanagedType.ByValTStr);
			blob.WriteCompressedInteger(ref pos, marshalType.Length);
		}

		private void BuildSafeArrayMarshalType(Blob blob, ref int pos, SafeArrayMarshalType marshalType)
		{
			blob.Write(ref pos, (byte)UnmanagedType.SafeArray);
			blob.WriteCompressedInteger(ref pos, (int)marshalType.VariantType);

			if (marshalType.UserDefinedSubType == null)
				return;

			blob.WriteCompressedInteger(ref pos, marshalType.UserDefinedSubType.Length);

			if (marshalType.UserDefinedSubType != string.Empty)
			{
				blob.Write(ref pos, marshalType.UserDefinedSubType, Encoding.UTF8);
			}
		}

		private void BuildCustomMarshalType(Blob blob, ref int pos, CustomMarshalType marshalType)
		{
			blob.Write(ref pos, (byte)UnmanagedType.CustomMarshaler);

			// GuidString
			if (!string.IsNullOrEmpty(marshalType.GuidString))
			{
				blob.WriteCompressedInteger(ref pos, marshalType.GuidString.Length);
				blob.Write(ref pos, marshalType.GuidString, Encoding.UTF8);
			}
			else
			{
				blob.WriteCompressedInteger(ref pos, 0);
			}

			// UnmanagedTypeString
			if (!string.IsNullOrEmpty(marshalType.UnmanagedTypeString))
			{
				blob.WriteCompressedInteger(ref pos, marshalType.UnmanagedTypeString.Length);
				blob.Write(ref pos, marshalType.UnmanagedTypeString, Encoding.UTF8);
			}
			else
			{
				blob.WriteCompressedInteger(ref pos, 0);
			}

			// TypeName
			if (!string.IsNullOrEmpty(marshalType.TypeName))
			{
				blob.WriteCompressedInteger(ref pos, marshalType.TypeName.Length);
				blob.Write(ref pos, (string)marshalType.TypeName, Encoding.UTF8);
			}
			else
			{
				blob.WriteCompressedInteger(ref pos, 0);
			}

			// Cookie
			if (!string.IsNullOrEmpty(marshalType.Cookie))
			{
				blob.WriteCompressedInteger(ref pos, marshalType.Cookie.Length);
				blob.Write(ref pos, marshalType.Cookie, Encoding.UTF8);
			}
			else
			{
				blob.WriteCompressedInteger(ref pos, 0);
			}
		}

		private void BuildPrimitiveMarshalType(Blob blob, ref int pos, PrimitiveMarshalType marshalType)
		{
			blob.Write(ref pos, (byte)marshalType.UnmanagedType);

			if (marshalType.IIdParameterIndex.HasValue)
			{
				blob.WriteCompressedInteger(ref pos, marshalType.IIdParameterIndex.Value);
			}
		}

		#endregion

		#region Constant

		private void BuildConstant(ConstantInfo constantInfo, int parent)
		{
			byte[] data;
			ConstantTableType type;

			switch (constantInfo.Type)
			{
				case ConstantType.Bool:
					data = BitConverter.GetBytes((bool)constantInfo.Value);
					type = ConstantTableType.Boolean;
					break;

				case ConstantType.Int8:
					data = new byte[] { (byte)(sbyte)constantInfo.Value };
					type = ConstantTableType.I1;
					break;

				case ConstantType.Int16:
					data = BitConverter.GetBytes((short)constantInfo.Value);
					type = ConstantTableType.I2;
					break;

				case ConstantType.Int32:
					data = BitConverter.GetBytes((int)constantInfo.Value);
					type = ConstantTableType.I4;
					break;

				case ConstantType.Int64:
					data = BitConverter.GetBytes((long)constantInfo.Value);
					type = ConstantTableType.I8;
					break;

				case ConstantType.UInt8:
					data = new byte[] { (byte)constantInfo.Value };
					type = ConstantTableType.U1;
					break;

				case ConstantType.UInt16:
					data = BitConverter.GetBytes((ushort)constantInfo.Value);
					type = ConstantTableType.U2;
					break;

				case ConstantType.UInt32:
					data = BitConverter.GetBytes((uint)constantInfo.Value);
					type = ConstantTableType.U4;
					break;

				case ConstantType.UInt64:
					data = BitConverter.GetBytes((ulong)constantInfo.Value);
					type = ConstantTableType.U8;
					break;

				case ConstantType.Float32:
					data = BitConverter.GetBytes((float)constantInfo.Value);
					type = ConstantTableType.R4;
					break;

				case ConstantType.Float64:
					data = BitConverter.GetBytes((double)constantInfo.Value);
					type = ConstantTableType.R8;
					break;

				case ConstantType.Char:
					data = BitConverter.GetBytes((short)constantInfo.Value);
					type = ConstantTableType.Char;
					break;

				case ConstantType.String:
					data = Encoding.Unicode.GetBytes((string)constantInfo.Value ?? "");
					type = ConstantTableType.String;
					break;

				case ConstantType.ByteArray:
					data = (byte[])constantInfo.Value ?? BufferUtils.EmptyArray;
					type = 0;
					break;

				case ConstantType.Nullref:
					data = BitConverter.GetBytes((int)0);
					type = ConstantTableType.Class;
					break;

				default:
					throw new InvalidOperationException();
			}

			var row = new ConstantRow();
			row.Parent = MetadataToken.CompressHasConstant(parent);
			row.Type = type;
			row.Value = AddBlob(data);

			_state.ConstantTable.Add(ref row);
		}

		#endregion

		#region PInvoke

		private void BuildPInvoke(PInvoke pinvoke, int owner)
		{
			var row = new ImplMapRow();
			row.MemberForwarded = MetadataToken.CompressMemberForwarded(owner);
			row.ImportName = AddString(pinvoke.ImportName);
			row.ImportScope = BuildModuleRef(pinvoke.ImportScope);

			BuildPInvokeFlags(pinvoke, ref row);

			_state.ImplMapTable.Add(ref row);
		}

		private void BuildPInvokeFlags(PInvoke pinvoke, ref ImplMapRow row)
		{
			row.MappingFlags |= (ushort)pinvoke.CallConv;
			row.MappingFlags |= (ushort)pinvoke.CharSet;

			if (pinvoke.BestFitMapping.HasValue)
			{
				if (pinvoke.BestFitMapping.Value)
					row.MappingFlags |= ImplMapFlags.BestFitOn;
				else
					row.MappingFlags |= ImplMapFlags.BestFitOff;
			}

			if (pinvoke.ThrowOnUnmappableChar.HasValue)
			{
				if (pinvoke.ThrowOnUnmappableChar.Value)
					row.MappingFlags |= ImplMapFlags.CharMapErrorOn;
				else
					row.MappingFlags |= ImplMapFlags.CharMapErrorOff;
			}

			if (pinvoke.ExactSpelling)
				row.MappingFlags |= ImplMapFlags.NoMangle;

			if (pinvoke.SetLastError)
				row.MappingFlags |= ImplMapFlags.LastError;
		}

		#endregion

		#region Others

		public int BuildSig(Signature signature)
		{
			switch (signature.SignatureType)
			{
				case SignatureType.Assembly:
					return BuildAssemblyRef((AssemblyReference)signature);

				case SignatureType.Module:
					return BuildModuleRef((ModuleReference)signature);

				case SignatureType.File:
					return BuildFile((FileReference)signature);

				case SignatureType.Type:
					return BuildTypeSig((TypeSignature)signature);

				case SignatureType.Method:
					return BuildMethodSig((MethodSignature)signature);

				case SignatureType.Field:
					return BuildFieldSig((FieldReference)signature);

				default:
					throw new InvalidOperationException();
			}
		}

		#endregion

		#endregion

		#region Nested types

		public struct FieldDataPoint
		{
			public FieldDataType Type;
			public int Offset;
		}

		private class State
		{
			#region Heaps

			internal MetadataStringStream Strings;
			internal MetadataUserStringStream UserStrings;
			internal MetadataGuidStream Guids;
			internal Dictionary<string, int> StringToPosition = new Dictionary<string, int>();
			internal Dictionary<string, int> UserStringToPosition = new Dictionary<string, int>();
			internal Dictionary<Guid, int> GuidToPosition = new Dictionary<Guid, int>();
			internal DictionaryBlob<int> BlobToPosition = new DictionaryBlob<int>(0, new byte[] { 0 });

			#endregion

			#region Tables

			internal MetadataTableStream Tables;
			internal AssemblyTable AssemblyTable;
			internal AssemblyOSTable AssemblyOSTable;
			internal AssemblyProcessorTable AssemblyProcessorTable;
			internal AssemblyRefTable AssemblyRefTable;
			internal AssemblyRefOSTable AssemblyRefOSTable;
			internal AssemblyRefProcessorTable AssemblyRefProcessorTable;
			internal ClassLayoutTable ClassLayoutTable;
			internal ConstantTable ConstantTable;
			internal CustomAttributeTable CustomAttributeTable;
			internal DeclSecurityTable DeclSecurityTable;
			internal ENCLogTable ENCLogTable;
			internal ENCMapTable ENCMapTable;
			internal EventTable EventTable;
			internal EventMapTable EventMapTable;
			internal EventPtrTable EventPtrTable;
			internal ExportedTypeTable ExportedTypeTable;
			internal FieldTable FieldTable;
			internal FieldLayoutTable FieldLayoutTable;
			internal FieldMarshalTable FieldMarshalTable;
			internal FieldPtrTable FieldPtrTable;
			internal FieldRVATable FieldRVATable;
			internal FileTable FileTable;
			internal GenericParamTable GenericParamTable;
			internal GenericParamConstraintTable GenericParamConstraintTable;
			internal ImplMapTable ImplMapTable;
			internal InterfaceImplTable InterfaceImplTable;
			internal ManifestResourceTable ManifestResourceTable;
			internal MemberRefTable MemberRefTable;
			internal MethodTable MethodTable;
			internal MethodImplTable MethodImplTable;
			internal MethodPtrTable MethodPtrTable;
			internal MethodSemanticsTable MethodSemanticsTable;
			internal MethodSpecTable MethodSpecTable;
			internal ModuleTable ModuleTable;
			internal ModuleRefTable ModuleRefTable;
			internal NestedClassTable NestedClassTable;
			internal ParamTable ParamTable;
			internal ParamPtrTable ParamPtrTable;
			internal PropertyTable PropertyTable;
			internal PropertyMapTable PropertyMapTable;
			internal PropertyPtrTable PropertyPtrTable;
			internal StandAloneSigTable StandAloneSigTable;
			internal TypeDefTable TypeDefTable;
			internal TypeRefTable TypeRefTable;
			internal TypeSpecTable TypeSpecTable;

			#endregion

			#region Node count

			internal int TypeCount;
			internal int MethodCount;
			internal int FieldCount;
			internal int FieldDataCount;
			internal int PropertyCount;
			internal int EventCount;
			internal int ResourceCount;

			#endregion

			#region Row hash

			internal HashList<TypeRefRow> TypeRefRowHash = new HashList<TypeRefRow>();
			internal HashList<TypeSpecRow> TypeSpecRowHash = new HashList<TypeSpecRow>();
			internal HashList<MethodSpecRow> MethodSpecRowHash = new HashList<MethodSpecRow>();
			internal HashList<MemberRefRow> MemberRefRowHash = new HashList<MemberRefRow>();
			internal HashList<StandAloneSigRow> StandAloneSigRowHash = new HashList<StandAloneSigRow>();

			#endregion
		}

		private class SecurityAttributeSet
		{
			internal SecurityAction Action;
			internal string Xml;
			internal List<SecurityAttribute> Attributes;
		}

		private class BuildSignatureComparer : IEqualityComparer<ISignature>
		{
			internal bool CompareByObjectReference;

			public bool Equals(ISignature x, ISignature y)
			{
				if (CompareByObjectReference)
					return object.ReferenceEquals(x, y);
				else
					return SignatureComparer.IgnoreTypeOwner.Equals(x, y);
			}

			public int GetHashCode(ISignature obj)
			{
				return SignatureComparer.IgnoreTypeOwner.GetHashCode(obj);
			}
		}

		#endregion
	}
}
