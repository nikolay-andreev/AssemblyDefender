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
	public class PropertyDeclaration : MemberNode, IProperty, ICustomAttributeProvider
	{
		#region Fields

		private const int LoadDefaultValueFlag = 2;
		private const int LoadGetSemanticFlag = 3;
		private const int LoadSetSemanticFlag = 4;
		private string _name;
		private int _flags;
		private bool _hasThis;
		private int _ownerTypeRID;
		private TypeSignature _returnType;
		private PropertyParameterCollection _parameters;
		private MethodReference _getMethod;
		private MethodReference _setMethod;
		private ConstantInfo? _defaultValue;
		private CustomAttributeCollection _customAttributes;
		private IType _resolvedReturnType;
		private IMethod _resolvedGetMethod;
		private IMethod _resolvedSetMethod;
		private IReadOnlyList<IType> _resolvedParameters;
		private int _opFlags;

		#endregion

		#region Ctors

		protected internal PropertyDeclaration(Module module, int rid, int typeRID)
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
				_rid = _module.PropertyTable.Add(this);
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

		#region Flags

		public bool IsSpecialName
		{
			get { return _flags.IsBitsOn(PropertyFlags.SpecialName); }
			set
			{
				_flags = _flags.SetBits(PropertyFlags.SpecialName, value);
				OnChanged();
			}
		}

		public bool IsRuntimeSpecialName
		{
			get { return _flags.IsBitsOn(PropertyFlags.RTSpecialName); }
			set
			{
				_flags = _flags.SetBits(PropertyFlags.RTSpecialName, value);
				OnChanged();
			}
		}

		#endregion

		public bool IsStatic
		{
			get { return !_hasThis; }
			set
			{
				_hasThis = !value;
				OnChanged();
			}
		}

		public bool HasThis
		{
			get { return _hasThis; }
			set
			{
				_hasThis = value;
				OnChanged();
			}
		}

		public TypeSignature ReturnType
		{
			get { return _returnType; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("ReturnType");

				_returnType = value;
				_module.AddSignature(ref _returnType);

				OnChanged();
			}
		}

		public PropertyParameterCollection Parameters
		{
			get
			{
				if (_parameters == null)
				{
					_parameters = new PropertyParameterCollection(this);
				}

				return _parameters;
			}
		}

		public MethodReference GetMethod
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadGetSemanticFlag))
				{
					LoadSemantics();
				}

				return _getMethod;
			}
			set
			{
				_getMethod = value;
				_module.AddSignature(ref _getMethod);
				_opFlags = _opFlags.SetBitAtIndex(LoadGetSemanticFlag, false);
				OnChanged();
			}
		}

		public MethodReference SetMethod
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadSetSemanticFlag))
				{
					LoadSemantics();
				}

				return _setMethod;
			}
			set
			{
				_setMethod = value;
				_module.AddSignature(ref _setMethod);
				_opFlags = _opFlags.SetBitAtIndex(LoadSetSemanticFlag, false);
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
					int token = MetadataToken.Get(MetadataTokenType.Param, _rid);
					_defaultValue = ConstantInfo.Load(_module, token);
				}

				return _defaultValue;
			}
			set
			{
				_defaultValue = value;
				_opFlags = _opFlags.SetBitAtIndex(LoadDefaultValueFlag, false);
				OnChanged();
			}
		}

		public CustomAttributeCollection CustomAttributes
		{
			get
			{
				if (_customAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.Property, _rid);
					_customAttributes = new CustomAttributeCollection(this, token);
				}

				return _customAttributes;
			}
		}

		public override MemberType MemberType
		{
			get { return MemberType.Property; }
		}

		#endregion

		#region Methods

		public TypeDeclaration GetOwnerType()
		{
			return _module.GetType(_ownerTypeRID);
		}

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		public void CopyTo(PropertyDeclaration copy)
		{
			copy._name = _name;
			copy._flags = _flags;
			copy._hasThis = _hasThis;
			copy._returnType = _returnType;
			copy._getMethod = GetMethod;
			copy._setMethod = SetMethod;
			copy._defaultValue = DefaultValue;
			Parameters.CopyTo(copy.Parameters);
			CustomAttributes.CopyTo(copy.CustomAttributes);
		}

		internal void InvalidatedSignatures()
		{
			_resolvedReturnType = null;
			_resolvedGetMethod = null;
			_resolvedSetMethod = null;
			_resolvedParameters = null;
		}

		protected internal override void OnDeleted()
		{
			_module.PropertyTable.Remove(_rid);
		}

		protected internal override void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_module.OnChanged();
				_module.PropertyTable.Change(_rid);
			}
		}

		protected void Load()
		{
			var image = _module.Image;

			PropertyRow row;
			image.GetProperty(_rid, out row);

			_name = image.GetString(row.Name);

			_flags = row.Flags;

			using (var accessor = image.OpenBlob(row.Type))
			{
				LoadSignature(accessor, image);
			}

			if ((_flags & PropertyFlags.HasDefault) == PropertyFlags.HasDefault)
				_opFlags = _opFlags.SetBitAtIndex(LoadDefaultValueFlag, true);

			_opFlags = _opFlags.SetBitAtIndex(LoadGetSemanticFlag, true);
			_opFlags = _opFlags.SetBitAtIndex(LoadSetSemanticFlag, true);
		}

		protected void LoadSignature(IBinaryAccessor accessor, ModuleImage image)
		{
			byte sigType = accessor.ReadByte();
			if ((sigType & Metadata.SignatureType.Property) != Metadata.SignatureType.Property)
			{
				throw new CodeModelException(string.Format(SR.AssemblyLoadError, _module.Location));
			}

			_hasThis = ((sigType & Metadata.SignatureType.HasThis) == Metadata.SignatureType.HasThis);

			int paramCount = accessor.ReadCompressedInteger();

			_returnType = TypeSignature.Load(accessor, _module);

			_parameters = new PropertyParameterCollection(this);
			_parameters.Load(accessor, paramCount);
		}

		protected void LoadSemantics()
		{
			var image = _module.Image;

			int token = MetadataToken.Get(MetadataTokenType.Property, _rid);

			int[] rids;
			image.GetMethodSemanticsByAssociation(MetadataToken.CompressHasSemantic(token), out rids);

			for (int i = 0; i < rids.Length; i++)
			{
				MethodSemanticsRow row;
				image.GetMethodSemantics(rids[i], out row);

				var methodRef = MethodReference.LoadMethodDef(_module, row.Method);
				switch (row.Semantic)
				{
					case MethodSemanticFlags.Getter:
						if (_opFlags.IsBitAtIndexOn(LoadGetSemanticFlag))
						{
							_getMethod = methodRef;
						}
						break;

					case MethodSemanticFlags.Setter:
						if (_opFlags.IsBitAtIndexOn(LoadSetSemanticFlag))
						{
							_setMethod = methodRef;
						}
						break;
				}
			}

			_opFlags = _opFlags.SetBitAtIndex(LoadGetSemanticFlag, false);
			_opFlags = _opFlags.SetBitAtIndex(LoadSetSemanticFlag, false);
		}

		#endregion

		#region IProperty Members

		IType IProperty.ReturnType
		{
			get
			{
				if (_resolvedReturnType == null)
				{
					_resolvedReturnType = AssemblyManager.Resolve(_returnType, this, true);
				}

				return _resolvedReturnType;
			}
		}

		IType IProperty.Owner
		{
			get { return GetOwnerType(); }
		}

		IMethod IProperty.GetMethod
		{
			get
			{
				if (_resolvedGetMethod == null && GetMethod != null)
				{
					_resolvedGetMethod = AssemblyManager.Resolve(GetMethod, this, false, true);
				}

				return _resolvedGetMethod;
			}
		}

		IMethod IProperty.SetMethod
		{
			get
			{
				if (_resolvedSetMethod == null && SetMethod != null)
				{
					_resolvedSetMethod = AssemblyManager.Resolve(SetMethod, this, false, true);
				}

				return _resolvedSetMethod;
			}
		}

		IProperty IProperty.DeclaringProperty
		{
			get { return this; }
		}

		IReadOnlyList<IType> IProperty.Parameters
		{
			get
			{
				if (_resolvedParameters == null)
				{
					_resolvedParameters = AssemblyManager.Resolve(Parameters, this, true);
				}

				return _resolvedParameters;
			}
		}

		IMethodSignature IPropertyBase.GetMethod
		{
			get { return GetMethod; }
		}

		IMethodSignature IPropertyBase.SetMethod
		{
			get { return SetMethod; }
		}

		#endregion

		#region IPropertySignature Members

		ITypeSignature IPropertySignature.ReturnType
		{
			get { return _returnType; }
		}

		ITypeSignature IPropertySignature.Owner
		{
			get { return GetOwnerType(); }
		}

		IReadOnlyList<ITypeSignature> IPropertySignature.Arguments
		{
			get { return Parameters; }
		}

		SignatureType ISignature.SignatureType
		{
			get { return SignatureType.Property; }
		}

		#endregion

		#region ICodeNode Members

		EntityType ICodeNode.EntityType
		{
			get { return EntityType.Property; }
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
