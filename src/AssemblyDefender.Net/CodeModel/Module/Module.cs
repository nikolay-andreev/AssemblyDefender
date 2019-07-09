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
	public class Module : CodeNode, IModule, ICustomAttributeProvider
	{
		#region Fields

		private const int LoadEntryPointFlag = 1;
		private string _name;
		private Guid _mvid;
		private string _location;
		private string _frameworkVersionMoniker;
		private Signature _entryPoint;
		private AssemblyReferenceCollection _assemblyReferences;
		private ModuleReferenceCollection _moduleReferences;
		private FileReferenceCollection _files;
		private TypeDeclarationCollection _types;
		private ExportedTypeCollection _exportedTypes;
		private ResourceCollection _resources;
		private ResourceTable _unmanagedResources;
		private CustomAttributeCollection _customAttributes;
		private Assembly _assembly;
		private ModuleImage _image;
		private AssemblyManager _assemblyManager;
		private HashList<ISignature> _signatures;
		private BlobSet _methodBodyBlob;
		private BlobSet _fieldDataBlob;
		private BlobSet _resourceBlob;
		private int _opFlags;

		#endregion

		#region Ctors

		protected internal Module(Assembly assembly, string location)
		{
			_assembly = assembly;
			_location = location;

			Initialize();
		}

		#endregion

		#region Properties

		public bool IsPrimeModule
		{
			get { return object.ReferenceEquals(_assembly.Module, this); }
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

		public Guid Mvid
		{
			get { return _mvid; }
			set
			{
				_mvid = value;
				OnChanged();
			}
		}

		/// <summary>
		/// Gets the location of the file that contains the manifest.
		/// </summary>
		public string Location
		{
			get { return _location; }
		}

		public string FrameworkVersionMoniker
		{
			get { return _frameworkVersionMoniker; }
			set
			{
				_frameworkVersionMoniker = value;
				OnChanged();
			}
		}

		/// <summary>
		/// Method or File signature
		/// </summary>
		public Signature EntryPoint
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadEntryPointFlag))
				{
					_entryPoint = LoadEntryPoint();
				}

				return _entryPoint;
			}
			set
			{
				_entryPoint = value;
				_opFlags = _opFlags.SetBitAtIndex(LoadEntryPointFlag, false);
				OnChanged();
			}
		}

		public new Assembly Assembly
		{
			get { return _assembly; }
		}

		public AssemblyReferenceCollection AssemblyReferences
		{
			get
			{
				if (_assemblyReferences == null)
				{
					_assemblyReferences = new AssemblyReferenceCollection(this);
				}

				return _assemblyReferences;
			}
		}

		public ModuleReferenceCollection ModuleReferences
		{
			get
			{
				if (_moduleReferences == null)
				{
					_moduleReferences = new ModuleReferenceCollection(this);
				}

				return _moduleReferences;
			}
		}

		public FileReferenceCollection Files
		{
			get
			{
				if (_files == null)
				{
					_files = new FileReferenceCollection(this);
				}

				return _files;
			}
		}

		public TypeDeclarationCollection Types
		{
			get
			{
				if (_types == null)
				{
					_types = new TypeDeclarationCollection(this);
				}

				return _types;
			}
		}

		public ExportedTypeCollection ExportedTypes
		{
			get
			{
				if (_exportedTypes == null)
				{
					_exportedTypes = new ExportedTypeCollection(this);
				}

				return _exportedTypes;
			}
		}

		public ResourceCollection Resources
		{
			get
			{
				if (_resources == null)
				{
					_resources = new ResourceCollection(this);
				}

				return _resources;
			}
		}

		public ResourceTable UnmanagedResources
		{
			get
			{
				if (_unmanagedResources == null)
				{
					if (IsNew)
					{
						_unmanagedResources = new ResourceTable();
					}
					else
					{
						_unmanagedResources = PE.ResourceTable.TryLoad(_image.PE) ?? new ResourceTable();
					}
				}

				return _unmanagedResources;
			}
		}

		public CustomAttributeCollection CustomAttributes
		{
			get
			{
				if (_customAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.Module, 1);
					_customAttributes = new CustomAttributeCollection(this, token);
				}

				return _customAttributes;
			}
		}

		public ModuleImage Image
		{
			get { return _image; }
		}

		internal BlobSet MethodBodyBlob
		{
			get
			{
				if (_methodBodyBlob == null)
				{
					_methodBodyBlob = new BlobSet();
				}

				return _methodBodyBlob;
			}
		}

		internal BlobSet FieldDataBlob
		{
			get
			{
				if (_fieldDataBlob == null)
				{
					_fieldDataBlob = new BlobSet();
				}

				return _fieldDataBlob;
			}
		}

		internal BlobSet ResourceBlob
		{
			get
			{
				if (_resourceBlob == null)
				{
					_resourceBlob = new BlobSet();
				}

				return _resourceBlob;
			}
		}

		#endregion

		#region Methods

		public void CopyTo(Module copy)
		{
			copy._name = _name;
			copy._mvid = _mvid;
			copy._frameworkVersionMoniker = _frameworkVersionMoniker;
			copy._entryPoint = EntryPoint;
			AssemblyReferences.CopyTo(copy.AssemblyReferences);
			ModuleReferences.CopyTo(copy.ModuleReferences);
			Files.CopyTo(copy.Files);
			Types.CopyTo(copy.Types);
			ExportedTypes.CopyTo(copy.ExportedTypes);
			Resources.CopyTo(copy.Resources);
			CustomAttributes.CopyTo(copy.CustomAttributes);

			if (_unmanagedResources != null)
			{
				copy._unmanagedResources = _unmanagedResources.Clone();
			}
		}

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		internal T GetSignature<T>(int signatureID)
			where T : ISignature
		{
			if (signatureID == 0)
				return default(T);

			return (T)_signatures[signatureID - 1];
		}

		internal int AddSignature<T>(ref T signature)
			where T : ISignature
		{
			if (signature == null)
				return 0;

			int index;
			if (!_signatures.TryAdd(signature, out index))
			{
				signature = (T)_signatures[index];
			}

			return index + 1;
		}

		internal void InvalidatedSignatures()
		{
			// Types
			if (_typeTable != null)
			{
				foreach (var type in _typeTable.EnumLiveNodes())
				{
					type.InvalidatedSignatures();
				}
			}

			// Methods
			if (_methodTable != null)
			{
				foreach (var method in _methodTable.EnumLiveNodes())
				{
					method.InvalidatedSignatures();
				}
			}

			// Fields
			if (_fieldTable != null)
			{
				foreach (var field in _fieldTable.EnumLiveNodes())
				{
					field.InvalidatedSignatures();
				}
			}

			// Properties
			if (_propertyTable != null)
			{
				foreach (var property in _propertyTable.EnumLiveNodes())
				{
					property.InvalidatedSignatures();
				}
			}

			// Events
			if (_eventTable != null)
			{
				foreach (var e in _eventTable.EnumLiveNodes())
				{
					e.InvalidatedSignatures();
				}
			}
		}

		protected internal virtual void OnDeleted()
		{
		}

		protected internal virtual void Close()
		{
			if (_image != null)
			{
				_image.Close();
				_image = null;
			}
		}

		protected void AddType(TypeDeclaration type)
		{
			Types.Add(type);
		}

		private void Initialize()
		{
			_parent = _assembly;
			_module = this;
			_assemblyManager = _assembly.AssemblyManager;
			_signatures = new HashList<ISignature>();

			if (!string.IsNullOrEmpty(_location))
			{
				LoadImage();
				Load();
			}
			else
			{
				IsNew = true;
				_mvid = Guid.NewGuid();
				_frameworkVersionMoniker = MicrosoftNetFramework.VersionMonikerLatest;
			}
		}

		private void LoadImage()
		{
			PEImage pe = null;
			try
			{
				pe = PEImage.LoadFile(_location, _assemblyManager.FileLoadMode);

				CorHeader corHeader;
				var metadata = MemoryMappedMetadata.Load(pe, out corHeader);

				_image = new ModuleImage(this, pe, metadata, corHeader);
			}
			catch (Exception)
			{
				if (pe != null)
				{
					pe.Dispose();
					pe = null;
				}

				throw;
			}
		}

		private void Load()
		{
			_frameworkVersionMoniker = _image.FrameworkVersionMoniker;

			ModuleRow row;
			_image.GetModule(1, out row);

			_name = _image.GetString(row.Name);

			_mvid = _image.GetGuid(row.Mvid);

			if (_image != null)
			{
				_opFlags = _opFlags.SetBitAtIndex(LoadEntryPointFlag, true);
			}
		}

		private Signature LoadEntryPoint()
		{
			_opFlags = _opFlags.SetBitAtIndex(LoadEntryPointFlag, false);

			int token = (int)_image.CorHeader.EntryPointToken;
			if (token == 0)
				return null;

			int rid = MetadataToken.GetRID(token);
			if (rid == 0)
				return null;

			switch (MetadataToken.GetType(token))
			{
				case MetadataTokenType.Method:
					return MethodReference.LoadMethodDef(this, rid);

				case MetadataTokenType.File:
					return FileReference.Load(this, rid);
			}

			return null;
		}

		#region Type

		private MemberTable<TypeDeclaration> _typeTable;

		internal MemberTable<TypeDeclaration> TypeTable
		{
			get
			{
				if (_typeTable == null)
				{
					_typeTable = new MemberTable<TypeDeclaration>(
						_image != null ? _image.GetTypeDefCount() : 0,
						(_assemblyManager.CacheMode & AssemblyCacheMode.Type) == AssemblyCacheMode.Type,
						CreateType);
				}

				return _typeTable;
			}
		}

		public TypeDeclaration GetType(int rid, bool throwIfMissing = false)
		{
			var type = GetType(rid);
			if (type == null)
			{
				if (throwIfMissing)
				{
					throw new CodeModelException(string.Format(SR.TypeResolveError, rid));
				}

				return null;
			}

			return type;
		}

		internal TypeDeclaration GetType(int rid)
		{
			int enclosingTypeRID;
			if (_image != null)
				_image.GetEnclosingTypeByNested(rid, out enclosingTypeRID);
			else
				enclosingTypeRID = 0;

			return TypeTable.Get(rid, enclosingTypeRID);
		}

		protected internal virtual TypeDeclaration CreateType(int rid, int enclosingTypeRID)
		{
			return new TypeDeclaration(this, rid, enclosingTypeRID);
		}

		#endregion

		#region Method

		private MemberTable<MethodDeclaration> _methodTable;

		internal MemberTable<MethodDeclaration> MethodTable
		{
			get
			{
				if (_methodTable == null)
				{
					_methodTable = new MemberTable<MethodDeclaration>(
						_image != null ? _image.GetMethodCount() : 0,
						(_assemblyManager.CacheMode & AssemblyCacheMode.Method) == AssemblyCacheMode.Method,
						CreateMethod);
				}

				return _methodTable;
			}
		}

		public MethodDeclaration GetMethod(int rid, bool throwIfMissing = false)
		{
			int typeRID = _image != null ? _image.GetTypeByMethod(rid) : 0;
			var method = MethodTable.Get(rid, typeRID);
			if (method == null)
			{
				if (throwIfMissing)
				{
					throw new CodeModelException(string.Format(SR.MethodResolveError, rid));
				}

				return null;
			}

			return method;
		}

		protected internal virtual MethodDeclaration CreateMethod(int rid, int typeRID)
		{
			return new MethodDeclaration(this, rid, typeRID);
		}

		#endregion

		#region Field

		private MemberTable<FieldDeclaration> _fieldTable;

		internal MemberTable<FieldDeclaration> FieldTable
		{
			get
			{
				if (_fieldTable == null)
				{
					_fieldTable = new MemberTable<FieldDeclaration>(
						_image != null ? _image.GetFieldCount() : 0,
						(_assemblyManager.CacheMode & AssemblyCacheMode.Field) == AssemblyCacheMode.Field,
						CreateField);
				}

				return _fieldTable;
			}
		}

		public FieldDeclaration GetField(int rid, bool throwIfMissing = false)
		{
			int typeRID = _image != null ? _image.GetTypeByField(rid) : 0;
			var field = FieldTable.Get(rid, typeRID);
			if (field == null)
			{
				if (throwIfMissing)
				{
					throw new CodeModelException(string.Format(SR.FieldResolveError, rid));
				}

				return null;
			}

			return field;
		}

		protected internal virtual FieldDeclaration CreateField(int rid, int typeRID)
		{
			return new FieldDeclaration(this, rid, typeRID);
		}

		#endregion

		#region Property

		private MemberTable<PropertyDeclaration> _propertyTable;

		internal MemberTable<PropertyDeclaration> PropertyTable
		{
			get
			{
				if (_propertyTable == null)
				{
					_propertyTable = new MemberTable<PropertyDeclaration>(
						_image != null ? _image.GetPropertyCount() : 0,
						(_assemblyManager.CacheMode & AssemblyCacheMode.Property) == AssemblyCacheMode.Property,
						CreateProperty);
				}

				return _propertyTable;
			}
		}

		public PropertyDeclaration GetProperty(int rid, bool throwIfMissing = false)
		{
			int typeRID = _image != null ? _image.GetTypeByProperty(rid) : 0;
			var property = PropertyTable.Get(rid, typeRID);
			if (property == null)
			{
				if (throwIfMissing)
				{
					throw new CodeModelException(string.Format(SR.PropertyResolveError, rid));
				}

				return null;
			}

			return property;
		}

		protected internal virtual PropertyDeclaration CreateProperty(int rid, int typeRID)
		{
			return new PropertyDeclaration(this, rid, typeRID);
		}

		#endregion

		#region Event

		private MemberTable<EventDeclaration> _eventTable;

		internal MemberTable<EventDeclaration> EventTable
		{
			get
			{
				if (_eventTable == null)
				{
					_eventTable = new MemberTable<EventDeclaration>(
						_image != null ? _image.GetEventCount() : 0,
						(_assemblyManager.CacheMode & AssemblyCacheMode.Event) == AssemblyCacheMode.Event,
						CreateEvent);
				}

				return _eventTable;
			}
		}

		public EventDeclaration GetEvent(int rid, bool throwIfMissing = false)
		{
			int typeRID = _image != null ? _image.GetTypeByEvent(rid) : 0;
			var eventDecl = EventTable.Get(rid, typeRID);
			if (eventDecl == null)
			{
				if (throwIfMissing)
				{
					throw new CodeModelException(string.Format(SR.EventResolveError, rid));
				}

				return null;
			}

			return eventDecl;
		}

		protected internal virtual EventDeclaration CreateEvent(int rid, int typeRID)
		{
			return new EventDeclaration(this, rid, typeRID);
		}

		#endregion

		#region Resource

		private MemberTable<Resource> _resourceTable;

		internal MemberTable<Resource> ResourceTable
		{
			get
			{
				if (_resourceTable == null)
				{
					_resourceTable = new MemberTable<Resource>(
						_image != null ? _image.GetManifestResourceCount() : 0,
						(_assemblyManager.CacheMode & AssemblyCacheMode.Resource) == AssemblyCacheMode.Resource,
						CreateResource);
				}

				return _resourceTable;
			}
		}

		public Resource GetResource(int rid, bool throwIfMissing = false)
		{
			var resource = ResourceTable.Get(rid, 0);
			if (resource == null)
			{
				if (throwIfMissing)
				{
					throw new CodeModelException(string.Format(SR.ResourceResolveError, rid));
				}

				return null;
			}

			return resource;
		}

		protected internal virtual Resource CreateResource(int rid, int notUsed)
		{
			return new Resource(this, rid);
		}

		#endregion

		#region CustomAttribute

		private MemberTable<CustomAttribute> _customAttributeTable;

		internal MemberTable<CustomAttribute> CustomAttributeTable
		{
			get
			{
				if (_customAttributeTable == null)
				{
					_customAttributeTable = new MemberTable<CustomAttribute>(
						_image != null ? _image.GetCustomAttributeCount() : 0,
						(_assemblyManager.CacheMode & AssemblyCacheMode.CustomAttribute) == AssemblyCacheMode.CustomAttribute,
						CreateCustomAttribute);
				}

				return _customAttributeTable;
			}
		}

		protected internal virtual CustomAttribute CreateCustomAttribute(int rid, int notUsed)
		{
			return new CustomAttribute(this, rid);
		}

		#endregion

		#region SecurityAttribute

		private MemberTable<SecurityAttribute> _securityAttributeTable;

		internal MemberTable<SecurityAttribute> SecurityAttributeTable
		{
			get
			{
				if (_securityAttributeTable == null)
				{
					_securityAttributeTable = new MemberTable<SecurityAttribute>(
						_image != null ? _image.GetSecurityAttributeCount() : 0,
						(_assemblyManager.CacheMode & AssemblyCacheMode.SecurityAttribute) == AssemblyCacheMode.SecurityAttribute,
						CreateSecurityAttribute);
				}

				return _securityAttributeTable;
			}
		}

		protected internal virtual SecurityAttribute CreateSecurityAttribute(int rid, int notUsed)
		{
			return new SecurityAttribute(this, rid);
		}

		#endregion

		#endregion

		#region IModule Members

		SignatureType ISignature.SignatureType
		{
			get { return SignatureType.Module; }
		}

		IReadOnlyList<IType> IModule.Types
		{
			get { return Types; }
		}

		EntityType ICodeNode.EntityType
		{
			get { return EntityType.Module; }
		}

		IAssembly ICodeNode.Assembly
		{
			get { return _assembly; }
		}

		IModule ICodeNode.Module
		{
			get { return this; }
		}

		bool ICodeNode.HasGenericContext
		{
			get { return false; }
		}

		IType ICodeNode.GetGenericArgument(bool isMethod, int position)
		{
			return null;
		}

		#endregion
	}
}
