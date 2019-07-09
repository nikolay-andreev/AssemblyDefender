using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class TypeDeclaration : MemberNode, IType, ICustomAttributeProvider, ISecurityAttributeProvider
	{
		#region Fields

		private const int LoadBaseTypeFlag = 1;
		private const int LoadLayoutFlag = 2;
		private string _name;
		private string _namespace;
		private int _flags;
		private int? _packingSize;
		private int? _classSize;
		private int _enclosingTypeRID;
		private TypeSignature _baseType;
		private TypeInterfaceCollection _interfaces;
		private GenericParameterCollection _genericParameters;
		private TypeDeclarationCollection _nestedTypes;
		private MethodDeclarationCollection _methods;
		private FieldDeclarationCollection _fields;
		private PropertyDeclarationCollection _properties;
		private EventDeclarationCollection _events;
		private CustomAttributeCollection _customAttributes;
		private SecurityAttributeCollection _securityAttributes;
		private IType _resolvedBaseType;
		private IReadOnlyList<IType> _resolvedInterfaces;
		private int _opFlags;

		#endregion

		#region Ctors

		protected internal TypeDeclaration(Module module, int rid, int enclosingTypeRID)
			: base(module)
		{
			_rid = rid;
			_enclosingTypeRID = enclosingTypeRID;

			if (_rid > 0)
			{
				Load();
			}
			else
			{
				IsNew = true;
				_rid = _module.TypeTable.Add(this);
			}
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value.NullIfEmpty();
				OnChanged();
			}
		}

		public string Namespace
		{
			get { return _namespace; }
			set
			{
				_namespace = value.NullIfEmpty();
				OnChanged();
			}
		}

		public string FullName
		{
			get { return CodeModelUtils.GetTypeName(_name, _namespace); }
		}

		public bool IsNested
		{
			get { return _enclosingTypeRID > 0; }
		}

		#region Flags

		public TypeVisibilityFlags Visibility
		{
			get { return (TypeVisibilityFlags)_flags.GetBits(TypeDefFlags.VisibilityMask); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.VisibilityMask, (int)value);
				OnChanged();
			}
		}

		public TypeLayoutFlags Layout
		{
			get { return (TypeLayoutFlags)_flags.GetBits(TypeDefFlags.LayoutMask); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.LayoutMask, (int)value);
				OnChanged();
			}
		}

		public TypeCharSetFlags CharSet
		{
			get { return (TypeCharSetFlags)_flags.GetBits(TypeDefFlags.CharSetMask); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.CharSetMask, (int)value);
				OnChanged();
			}
		}

		public bool IsInterface
		{
			get { return _flags.IsBitsOn(TypeDefFlags.Interface); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.Interface, value);
				OnChanged();
			}
		}

		public bool IsAbstract
		{
			get { return _flags.IsBitsOn(TypeDefFlags.Abstract); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.Abstract, value);
				OnChanged();
			}
		}

		public bool IsSealed
		{
			get { return _flags.IsBitsOn(TypeDefFlags.Sealed); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.Sealed, value);
				OnChanged();
			}
		}

		public bool IsSpecialName
		{
			get { return _flags.IsBitsOn(TypeDefFlags.SpecialName); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.SpecialName, value);
				OnChanged();
			}
		}

		public bool IsRuntimeSpecialName
		{
			get { return _flags.IsBitsOn(TypeDefFlags.RTSpecialName); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.RTSpecialName, value);
				OnChanged();
			}
		}

		public bool IsImportFromCOMTypeLib
		{
			get { return _flags.IsBitsOn(TypeDefFlags.Import); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.Import, value);
				OnChanged();
			}
		}

		public bool IsSerializable
		{
			get { return _flags.IsBitsOn(TypeDefFlags.Serializable); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.Serializable, value);
				OnChanged();
			}
		}

		public bool IsBeforeFieldInit
		{
			get { return _flags.IsBitsOn(TypeDefFlags.BeforeFieldInit); }
			set
			{
				_flags = _flags.SetBits(TypeDefFlags.BeforeFieldInit, value);
				OnChanged();
			}
		}

		#endregion

		public int? PackingSize
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadLayoutFlag))
				{
					LaodLayout();
				}

				return _packingSize;
			}
			set
			{
				_packingSize = value;
				_opFlags = _opFlags.SetBitAtIndex(LoadLayoutFlag, false);
				OnChanged();
			}
		}

		public int? ClassSize
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadLayoutFlag))
				{
					LaodLayout();
				}

				return _classSize;
			}
			set
			{
				_classSize = value;
				_opFlags = _opFlags.SetBitAtIndex(LoadLayoutFlag, false);
				OnChanged();
			}
		}

		public TypeSignature BaseType
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadBaseTypeFlag))
				{
					LaodBaseType();
				}

				return _baseType;
			}
			set
			{
				_baseType = value;
				_module.AddSignature(ref _baseType);
				_opFlags = _opFlags.SetBitAtIndex(LoadBaseTypeFlag, false);
				OnChanged();
			}
		}

		public TypeInterfaceCollection Interfaces
		{
			get
			{
				if (_interfaces == null)
				{
					_interfaces = new TypeInterfaceCollection(this);
				}

				return _interfaces;
			}
		}

		public GenericParameterCollection GenericParameters
		{
			get
			{
				if (_genericParameters == null)
				{
					int owner = MetadataToken.Get(MetadataTokenType.TypeDef, _rid);
					_genericParameters = new GenericParameterCollection(this, owner);
				}

				return _genericParameters;
			}
		}

		public TypeDeclarationCollection NestedTypes
		{
			get
			{
				if (_nestedTypes == null)
				{
					_nestedTypes = new TypeDeclarationCollection(this);
				}

				return _nestedTypes;
			}
		}

		public MethodDeclarationCollection Methods
		{
			get
			{
				if (_methods == null)
				{
					_methods = new MethodDeclarationCollection(this);
				}

				return _methods;
			}
		}

		public FieldDeclarationCollection Fields
		{
			get
			{
				if (_fields == null)
				{
					_fields = new FieldDeclarationCollection(this);
				}

				return _fields;
			}
		}

		public PropertyDeclarationCollection Properties
		{
			get
			{
				if (_properties == null)
				{
					_properties = new PropertyDeclarationCollection(this);
				}

				return _properties;
			}
		}

		public EventDeclarationCollection Events
		{
			get
			{
				if (_events == null)
				{
					_events = new EventDeclarationCollection(this);
				}

				return _events;
			}
		}

		public CustomAttributeCollection CustomAttributes
		{
			get
			{
				if (_customAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.TypeDef, _rid);
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
					int token = MetadataToken.Get(MetadataTokenType.TypeDef, _rid);
					_securityAttributes = new SecurityAttributeCollection(this, token);
				}

				return _securityAttributes;
			}
		}

		public override MemberType MemberType
		{
			get { return MemberType.Type; }
		}

		#endregion

		#region Methods

		public TypeDeclaration GetEnclosingType()
		{
			return _enclosingTypeRID > 0 ? _module.GetType(_enclosingTypeRID, true) : null;
		}

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		public void CopyTo(TypeDeclaration copy)
		{
			copy._name = _name;
			copy._namespace = _namespace;
			copy._flags = _flags;
			copy._packingSize = PackingSize;
			copy._classSize = ClassSize;
			copy._baseType = BaseType;
			Interfaces.CopyTo(copy.Interfaces);
			GenericParameters.CopyTo(copy.GenericParameters);
			Methods.CopyTo(copy.Methods);
			Fields.CopyTo(copy.Fields);
			Properties.CopyTo(copy.Properties);
			Events.CopyTo(copy.Events);
			CustomAttributes.CopyTo(copy.CustomAttributes);
			SecurityAttributes.CopyTo(copy.SecurityAttributes);
			NestedTypes.CopyTo(copy.NestedTypes);
		}

		internal void InvalidatedSignatures()
		{
			_resolvedBaseType = null;
			_resolvedInterfaces = null;
		}

		protected internal override void OnDeleted()
		{
			Methods.Clear();
			Fields.Clear();
			Properties.Clear();
			Events.Clear();
			NestedTypes.Clear();

			_module.TypeTable.Remove(_rid);
		}

		protected internal override void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_module.OnChanged();
				_module.TypeTable.Change(_rid);
			}
		}

		protected void Load()
		{
			var image = _module.Image;

			TypeDefRow row;
			image.GetTypeDef(_rid, out row);

			_name = image.GetString(row.Name);
			_namespace = image.GetString(row.Namespace);
			_flags = row.Flags;

			_opFlags = _opFlags.SetBitAtIndex(LoadBaseTypeFlag, true);
			_opFlags = _opFlags.SetBitAtIndex(LoadLayoutFlag, true);
		}

		protected void LaodLayout()
		{
			_opFlags = _opFlags.SetBitAtIndex(LoadLayoutFlag, false);

			var image = _module.Image;

			int layoutRID;
			if (!image.GetClassLayoutByParent(_rid, out layoutRID))
				return;

			ClassLayoutRow layoutRow;
			image.GetClassLayout(layoutRID, out layoutRow);

			_packingSize = layoutRow.PackingSize;
			_classSize = layoutRow.ClassSize;
		}

		protected void LaodBaseType()
		{
			_opFlags = _opFlags.SetBitAtIndex(LoadBaseTypeFlag, false);

			var image = _module.Image;

			int extendsToken = MetadataToken.DecompressTypeDefOrRef(image.GetTypeDefExtends(_rid));
			if (MetadataToken.IsNull(extendsToken))
				return;

			_baseType = TypeSignature.Load(_module, extendsToken);
		}

		protected void AddMethod(MethodDeclaration method)
		{
			Methods.Add(method);
		}

		protected void AddField(FieldDeclaration field)
		{
			Fields.Add(field);
		}

		protected void AddProperty(PropertyDeclaration property)
		{
			Properties.Add(property);
		}

		protected void AddEvent(EventDeclaration e)
		{
			Events.Add(e);
		}

		protected void AddNestedType(TypeDeclaration type)
		{
			NestedTypes.Add(type);
		}

		#endregion

		#region IType Members

		IType IType.ElementType
		{
			get { return null; }
		}

		IType IType.DeclaringType
		{
			get { return this; }
		}

		IType IType.BaseType
		{
			get
			{
				if (_resolvedBaseType == null && BaseType != null)
				{
					_resolvedBaseType = AssemblyManager.Resolve(BaseType, this, true);
				}

				return _resolvedBaseType;
			}
		}

		ITypeSignature ITypeBase.BaseType
		{
			get { return BaseType; }
		}

		IType IType.EnclosingType
		{
			get { return GetEnclosingType(); }
		}

		IReadOnlyList<IType> IType.GenericArguments
		{
			get { return ReadOnlyList<IType>.Empty; }
		}

		IReadOnlyList<IGenericParameter> IType.GenericParameters
		{
			get { return GenericParameters; }
		}

		IReadOnlyList<IType> IType.Interfaces
		{
			get
			{
				if (_resolvedInterfaces == null)
				{
					_resolvedInterfaces = AssemblyManager.Resolve(Interfaces, this, true);
				}

				return _resolvedInterfaces;
			}
		}

		IReadOnlyList<ITypeSignature> ITypeBase.Interfaces
		{
			get { return Interfaces; }
		}

		IReadOnlyList<IMethod> IType.Methods
		{
			get { return Methods; }
		}

		IReadOnlyList<IField> IType.Fields
		{
			get { return Fields; }
		}

		IReadOnlyList<IProperty> IType.Properties
		{
			get { return Properties; }
		}

		IReadOnlyList<IEvent> IType.Events
		{
			get { return Events; }
		}

		IReadOnlyList<IType> IType.NestedTypes
		{
			get { return NestedTypes; }
		}

		IType IType.GetCustomModifier(out CustomModifierType modifierType)
		{
			modifierType = CustomModifierType.ModOpt;
			return null;
		}

		ICallSite IType.GetFunctionPointer()
		{
			return null;
		}

		#endregion

		#region ITypeSignature Members

		SignatureType ISignature.SignatureType
		{
			get { return SignatureType.Type; }
		}

		ITypeSignature ITypeSignature.EnclosingType
		{
			get { return GetEnclosingType(); }
		}

		ISignature ITypeSignature.ResolutionScope
		{
			get
			{
				if (_module.IsPrimeModule)
					return _module.Assembly;
				else
					return _module;
			}
		}

		ISignature ITypeSignature.Owner
		{
			get
			{
				if (_enclosingTypeRID > 0)
					return GetEnclosingType();
				else if (_module.IsPrimeModule)
					return _module.Assembly;
				else
					return _module;
			}
		}

		ITypeSignature ITypeSignature.ElementType
		{
			get { return null; }
		}

		ITypeSignature ITypeSignature.DeclaringType
		{
			get { return this; }
		}

		TypeElementCode ITypeSignature.ElementCode
		{
			get { return TypeElementCode.DeclaringType; }
		}

		IReadOnlyList<ArrayDimension> ITypeSignature.ArrayDimensions
		{
			get { return ReadOnlyList<ArrayDimension>.Empty; }
		}

		IReadOnlyList<ITypeSignature> ITypeSignature.GenericArguments
		{
			get { return ReadOnlyList<ITypeSignature>.Empty; }
		}

		ITypeSignature ITypeSignature.GetCustomModifier(out CustomModifierType modifierType)
		{
			modifierType = CustomModifierType.ModOpt;
			return null;
		}

		IMethodSignature ITypeSignature.GetFunctionPointer()
		{
			return null;
		}

		void ITypeSignature.GetGenericParameter(out bool isMethod, out int position)
		{
			isMethod = false;
			position = -1;
		}

		#endregion

		#region ICodeNode Members

		EntityType ICodeNode.EntityType
		{
			get { return EntityType.Type; }
		}

		IAssembly ICodeNode.Assembly
		{
			get { return Assembly; }
		}

		IModule ICodeNode.Module
		{
			get { return _module; }
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
