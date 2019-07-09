using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class FieldDeclaration : MemberNode, IField, ICustomAttributeProvider
	{
		#region Fields

		private const int LoadMarshalFieldTypeFlag = 1;
		private const int LoadDefaultValueFlag = 2;
		private const int LoadDataFromImageFlag = 3;
		private const int LoadDataFromStateFlag = 4;
		private const int LoadFieldLayoutFlag = 5;
		private string _name;
		private int? _offset;
		private int _flags;
		private int _ownerTypeRID;
		private TypeSignature _fieldType;
		private MarshalType _marshalFieldType;
		private ConstantInfo? _defaultValue;
		private CustomAttributeCollection _customAttributes;
		private FieldDataType _dataType;
		private IType _resolvedFieldType;
		private int _opFlags;

		#endregion

		#region Ctors

		protected internal FieldDeclaration(Module module, int rid, int typeRID)
			: base(module)
		{
			_rid = rid;
			_ownerTypeRID = typeRID;

			if (_rid > 0)
			{
				Load();
			}
			else
			{
				IsNew = true;
				_rid = _module.FieldTable.Add(this);
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

		public int? Offset
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadFieldLayoutFlag))
				{
					LaodFieldLayout();
				}

				return _offset;
			}
			set
			{
				_offset = value;
				_opFlags = _opFlags.SetBitAtIndex(LoadFieldLayoutFlag, false);
				OnChanged();
			}
		}

		#region Flags

		public FieldVisibilityFlags Visibility
		{
			get { return (FieldVisibilityFlags)_flags.GetBits(FieldFlags.VisibilityMask); }
			set
			{
				_flags = _flags.SetBits(FieldFlags.VisibilityMask, (int)value);
				OnChanged();
			}
		}

		/// <summary>
		/// The field is static, shared by all instances of the type. Global fields must be static.
		/// keyword: static
		/// </summary>
		public bool IsStatic
		{
			get { return _flags.IsBitsOn(FieldFlags.Static); }
			set
			{
				_flags = _flags.SetBits(FieldFlags.Static, value);
				OnChanged();
			}
		}

		/// <summary>
		/// The field can be initialized only and cannot be written to later. Initialization takes place
		/// in an instance constructor (.ctor) for instance fields and in a class constructor (.cctor)
		/// for static fields. This flag is not enforced by the CLR, it exists for the compilers’ reference only.
		/// keyword: initonly
		/// </summary>
		public bool IsInitOnly
		{
			get { return _flags.IsBitsOn(FieldFlags.InitOnly); }
			set
			{
				_flags = _flags.SetBits(FieldFlags.InitOnly, value);
				OnChanged();
			}
		}

		/// <summary>
		/// The field is a compile-time constant. The loader does not lay out this field and does not
		/// create an internal handle for it. The field cannot be directly addressed from IL and can be
		/// used only as a Reflection reference to retrieve an associated metadata-held constant.
		/// If you try to access a literal field directly—for example, through the ldsfld instruction —
		/// the JIT compiler throws a MissingField exception and aborts the task.
		/// keyword: literal
		/// </summary>
		public bool IsLiteral
		{
			get { return _flags.IsBitsOn(FieldFlags.Literal); }
			set
			{
				_flags = _flags.SetBits(FieldFlags.Literal, value);
				OnChanged();
			}
		}

		/// <summary>
		/// The field is not serialized when the owner is remoted. This flag has meaning only for
		/// instance fields of the serializable types.
		/// keyword: notserialized
		/// </summary>
		public bool IsNotSerialized
		{
			get { return _flags.IsBitsOn(FieldFlags.NotSerialized); }
			set
			{
				_flags = _flags.SetBits(FieldFlags.NotSerialized, value);
				OnChanged();
			}
		}

		public bool IsSpecialName
		{
			get { return _flags.IsBitsOn(FieldFlags.SpecialName); }
			set
			{
				_flags = _flags.SetBits(FieldFlags.SpecialName, value);
				OnChanged();
			}
		}

		public bool IsRuntimeSpecialName
		{
			get { return _flags.IsBitsOn(FieldFlags.RTSpecialName); }
			set
			{
				_flags = _flags.SetBits(FieldFlags.RTSpecialName, value);
				OnChanged();
			}
		}

		#endregion

		public bool HasData
		{
			get
			{
				return
					_opFlags.IsBitAtIndexOn(LoadDataFromImageFlag) ||
					_opFlags.IsBitAtIndexOn(LoadDataFromStateFlag);
			}
		}

		public TypeSignature FieldType
		{
			get { return _fieldType; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("FieldType");

				_fieldType = value;
				_module.AddSignature(ref _fieldType);

				OnChanged();
			}
		}

		public MarshalType MarshalFieldType
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadMarshalFieldTypeFlag))
				{
					_opFlags = _opFlags.SetBitAtIndex(LoadMarshalFieldTypeFlag, false);
					int token = MetadataToken.Get(MetadataTokenType.Field, _rid);
					_marshalFieldType = MarshalType.Load(this, token);
				}

				return _marshalFieldType;
			}
			private set
			{
				_marshalFieldType = value;
				_opFlags = _opFlags.SetBitAtIndex(LoadMarshalFieldTypeFlag, false);
				OnChanged();
			}
		}

		public ConstantInfo? DefaultValue
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadDefaultValueFlag))
				{
					_opFlags = _opFlags.SetBitAtIndex(LoadDefaultValueFlag, false);
					int token = MetadataToken.Get(MetadataTokenType.Field, _rid);
					_defaultValue = ConstantInfo.Load(_module, token);
				}

				return _defaultValue;
			}
			set
			{
				_opFlags = _opFlags.SetBitAtIndex(LoadDefaultValueFlag, false);
				_defaultValue = value;
				OnChanged();
			}
		}

		public CustomAttributeCollection CustomAttributes
		{
			get
			{
				if (_customAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.Field, _rid);
					_customAttributes = new CustomAttributeCollection(this, token);
				}

				return _customAttributes;
			}
		}

		public override MemberType MemberType
		{
			get { return MemberType.Field; }
		}

		#endregion

		#region Methods

		public TypeDeclaration GetOwnerType()
		{
			return _module.GetType(_ownerTypeRID);
		}

		public void CopyTo(FieldDeclaration copy)
		{
			copy._name = _name;
			copy._flags = _flags;
			copy._fieldType = _fieldType;
			copy._offset = Offset;
			copy._defaultValue = DefaultValue;

			if (MarshalFieldType != null)
			{
				copy.CreateMarshalFieldType(MarshalFieldType.UnmanagedType);
				MarshalFieldType.CopyTo(copy.MarshalFieldType);
			}

			CustomAttributes.CopyTo(copy.CustomAttributes);

			{
				FieldDataType dataType;
				byte[] data = GetData(out dataType);
				copy.SetData(data, dataType);
			}
		}

		public MarshalType CreateMarshalFieldType(UnmanagedType type)
		{
			MarshalFieldType = MarshalType.CreateNew(this, type);

			return _marshalFieldType;
		}

		public void DeleteMarshalFieldType()
		{
			MarshalFieldType = null;
		}

		public byte[] GetData(out FieldDataType type)
		{
			if (_opFlags.IsBitAtIndexOn(LoadDataFromImageFlag))
			{
				// Get data from field rva.
				return GetDataFromImage(out type);
			}

			// Get data from state.
			if (_opFlags.IsBitAtIndexOn(LoadDataFromStateFlag))
			{
				return GetDataFromState(out type);
			}

			type = FieldDataType.Data;
			return null;
		}

		public void SetData(byte[] data, FieldDataType type)
		{
			_opFlags = _opFlags.SetBitAtIndex(LoadDataFromImageFlag, false);
			_opFlags = _opFlags.SetBitAtIndex(LoadDataFromStateFlag, (data != null && data.Length > 0));
			_dataType = type;
			_module.FieldDataBlob[_rid - 1] = data;

			OnChanged();
		}

		private byte[] GetDataFromImage(out FieldDataType type)
		{
			type = FieldDataType.Data;

			var image = _module.Image;
			if (image == null)
				return null;

			int dataRID;
			if (!image.GetFieldRVAByField(_rid, out dataRID))
				return null;

			uint rva;
			int dataSize;
			image.GetFieldRVA(dataRID, out rva, out dataSize);

			PESection section;
			long position;
			if (!image.PE.ResolvePositionToSectionData(rva, out position, out section))
				return null;

			if ((section.Characteristics & SectionCharacteristics.ContainsCode) == SectionCharacteristics.ContainsCode)
			{
				type = FieldDataType.Cli;
			}
			else if (image.TLS.Contains(rva))
			{
				type = FieldDataType.Tls;
			}
			else
			{
				type = FieldDataType.Data;
			}

			using (var accessor = image.OpenImage(position))
			{
				return accessor.ReadBytes(dataSize);
			}
		}

		private byte[] GetDataFromState(out FieldDataType type)
		{
			type = _dataType;
			return _module.FieldDataBlob[_rid - 1];
		}

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		internal void InvalidatedSignatures()
		{
			_resolvedFieldType = null;
		}

		protected internal override void OnDeleted()
		{
			_module.FieldTable.Remove(_rid);
			_module.FieldDataBlob[_rid - 1] = null;
		}

		protected internal override void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_module.FieldTable.Change(_rid);
				_module.OnChanged();
			}
		}

		protected void Load()
		{
			var image = _module.Image;

			FieldRow row;
			image.GetField(_rid, out row);

			_name = image.GetString(row.Name);

			_flags = row.Flags;

			using (var accessor = image.OpenBlob(row.Signature))
			{
				byte sigType = accessor.ReadByte();
				if (sigType != Metadata.SignatureType.Field)
				{
					throw new CodeModelException(string.Format(SR.AssemblyLoadError, _module.Location));
				}

				_fieldType = TypeSignature.Load(accessor, _module);
			}

			if ((_flags & FieldFlags.HasFieldMarshal) == FieldFlags.HasFieldMarshal)
				_opFlags = _opFlags.SetBitAtIndex(LoadMarshalFieldTypeFlag, true);

			if ((_flags & FieldFlags.HasDefault) == FieldFlags.HasDefault)
				_opFlags = _opFlags.SetBitAtIndex(LoadDefaultValueFlag, true);

			if ((_flags & FieldFlags.HasFieldRVA) == FieldFlags.HasFieldRVA)
				_opFlags = _opFlags.SetBitAtIndex(LoadDataFromImageFlag, true);

			_opFlags = _opFlags.SetBitAtIndex(LoadFieldLayoutFlag, true);
		}

		protected void LaodFieldLayout()
		{
			_opFlags = _opFlags.SetBitAtIndex(LoadFieldLayoutFlag, false);

			int offset;
			if (_module.Image.GetFieldLayoutOffsetByField(_rid, out offset))
				_offset = offset;
		}

		#endregion

		#region IField Members

		IType IField.FieldType
		{
			get
			{
				if (_resolvedFieldType == null)
				{
					_resolvedFieldType = AssemblyManager.Resolve(_fieldType, this, true);
				}

				return _resolvedFieldType;
			}
		}

		IType IField.Owner
		{
			get { return GetOwnerType(); }
		}

		IField IField.DeclaringField
		{
			get { return this; }
		}

		#endregion

		#region IFieldSignature Members

		ITypeSignature IFieldSignature.FieldType
		{
			get { return _fieldType; }
		}

		ITypeSignature IFieldSignature.Owner
		{
			get { return GetOwnerType(); }
		}

		SignatureType ISignature.SignatureType
		{
			get { return SignatureType.Field; }
		}

		#endregion

		#region ICodeNode Members

		EntityType ICodeNode.EntityType
		{
			get { return EntityType.Field; }
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
