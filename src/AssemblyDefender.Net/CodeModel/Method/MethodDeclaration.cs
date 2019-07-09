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
	public class MethodDeclaration : MemberNode, IMethod, ICustomAttributeProvider, ISecurityAttributeProvider
	{
		#region Fields

		private const int LoadPInvokeFlag = 1;
		private const int LoadBodyFromStateFlag = 2;
		private string _name;
		private int _sigType;
		private int _flags;
		private int _implFlags;
		private uint _rva;
		private int _ownerTypeRID;
		private MethodReturnType _returnType;
		private PInvoke _pinvoke;
		private MethodParameterCollection _parameters;
		private GenericParameterCollection _genericParameters;
		private MethodOverrideCollection _overrides;
		private CustomAttributeCollection _customAttributes;
		private SecurityAttributeCollection _securityAttributes;
		private IType _resolvedReturnType;
		private IReadOnlyList<IMethod> _resolvedOverrides;
		private int _opFlags;

		#endregion

		#region Ctors

		protected internal MethodDeclaration(Module module, int rid, int typeRID)
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
				_rid = _module.MethodTable.Add(this);
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

		public MethodVisibilityFlags Visibility
		{
			get { return (MethodVisibilityFlags)_flags.GetBits(MethodFlags.VisibilityMask); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.VisibilityMask, (int)value);
				OnChanged();
			}
		}

		public bool IsStatic
		{
			get { return _flags.IsBitsOn(MethodFlags.Static); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.Static, value);
				OnChanged();
			}
		}

		public bool IsFinal
		{
			get { return _flags.IsBitsOn(MethodFlags.Final); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.Final, value);
				OnChanged();
			}
		}

		public bool IsVirtual
		{
			get { return _flags.IsBitsOn(MethodFlags.Virtual); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.Virtual, value);
				OnChanged();
			}
		}

		public bool IsHideBySig
		{
			get { return _flags.IsBitsOn(MethodFlags.HideBySig); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.HideBySig, value);
				OnChanged();
			}
		}

		public bool IsNewSlot
		{
			get { return _flags.IsBitsOn(MethodFlags.NewSlot); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.NewSlot, value);
				OnChanged();
			}
		}

		public bool IsStrict
		{
			get { return _flags.IsBitsOn(MethodFlags.Strict); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.Strict, value);
				OnChanged();
			}
		}

		public bool IsAbstract
		{
			get { return _flags.IsBitsOn(MethodFlags.Abstract); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.Abstract, value);
				OnChanged();
			}
		}

		public bool IsSpecialName
		{
			get { return _flags.IsBitsOn(MethodFlags.SpecialName); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.SpecialName, value);
				OnChanged();
			}
		}

		public bool IsRuntimeSpecialName
		{
			get { return _flags.IsBitsOn(MethodFlags.RTSpecialName); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.RTSpecialName, value);
				OnChanged();
			}
		}

		public bool RequireSecObject
		{
			get { return _flags.IsBitsOn(MethodFlags.RequireSecObject); }
			set
			{
				_flags = _flags.SetBits(MethodFlags.RequireSecObject, value);
				OnChanged();
			}
		}

		#endregion

		#region ImplFlags

		public MethodCodeTypeFlags CodeType
		{
			get { return (MethodCodeTypeFlags)_implFlags.GetBits(MethodImplFlags.CodeTypeMask); }
			set
			{
				_implFlags = _implFlags.SetBits(MethodImplFlags.CodeTypeMask, (ushort)value);
				OnChanged();
			}
		}

		public bool IsManaged
		{
			get { return !_implFlags.IsBitsOn(MethodImplFlags.Unmanaged); }
			set
			{
				_implFlags = _implFlags.SetBits(MethodImplFlags.Unmanaged, !value);
				OnChanged();
			}
		}

		/// <summary>
		/// The code is unmanaged. This flag must be paired with the native flag.
		/// </summary>
		public bool IsUnmanaged
		{
			get { return _implFlags.IsBitsOn(MethodImplFlags.Unmanaged); }
			set
			{
				_implFlags = _implFlags.SetBits(MethodImplFlags.Unmanaged, value);
				OnChanged();
			}
		}

		public bool IsForwardRef
		{
			get { return _implFlags.IsBitsOn(MethodImplFlags.ForwardRef); }
			set
			{
				_implFlags = _implFlags.SetBits(MethodImplFlags.ForwardRef, value);
				OnChanged();
			}
		}

		public bool IsPreserveSig
		{
			get { return _implFlags.IsBitsOn(MethodImplFlags.PreserveSig); }
			set
			{
				_implFlags = _implFlags.SetBits(MethodImplFlags.PreserveSig, value);
				OnChanged();
			}
		}

		public bool IsInternalCall
		{
			get { return _implFlags.IsBitsOn(MethodImplFlags.InternalCall); }
			set
			{
				_implFlags = _implFlags.SetBits(MethodImplFlags.InternalCall, value);
				OnChanged();
			}
		}

		public bool IsSynchronized
		{
			get { return _implFlags.IsBitsOn(MethodImplFlags.Synchronized); }
			set
			{
				_implFlags = _implFlags.SetBits(MethodImplFlags.Synchronized, value);
				OnChanged();
			}
		}

		public bool IsNoInlining
		{
			get { return _implFlags.IsBitsOn(MethodImplFlags.NoInlining); }
			set
			{
				_implFlags = _implFlags.SetBits(MethodImplFlags.NoInlining, value);
				OnChanged();
			}
		}

		#endregion

		public bool HasThis
		{
			get { return _sigType.IsBitsOn(Metadata.SignatureType.HasThis); }
			set
			{
				_sigType = _sigType.SetBits(Metadata.SignatureType.HasThis, value);
				OnChanged();
			}
		}

		public bool ExplicitThis
		{
			get { return _sigType.IsBitsOn(Metadata.SignatureType.ExplicitThis); }
			set
			{
				_sigType = _sigType.SetBits(Metadata.SignatureType.ExplicitThis, value);

				if (value)
					_sigType = _sigType.SetBits(Metadata.SignatureType.HasThis, true);

				OnChanged();
			}
		}

		public bool HasBody
		{
			get { return _rva > 0 || _opFlags.IsBitAtIndexOn(LoadBodyFromStateFlag); }
		}

		public bool IsBodyChanged
		{
			get { return _opFlags.IsBitAtIndexOn(LoadBodyFromStateFlag); }
		}

		/// <summary>
		/// Valid calling conventions are Default and VarArgs.
		/// </summary>
		public MethodCallingConvention CallConv
		{
			get { return (MethodCallingConvention)_sigType.GetBits(Metadata.SignatureType.MethodCallConvMask); }
			set
			{
				_sigType = _sigType.SetBits(Metadata.SignatureType.MethodCallConvMask, (int)value);
				OnChanged();
			}
		}

		public MethodReturnType ReturnType
		{
			get
			{
				if (_returnType == null)
				{
					_returnType = new MethodReturnType(this);
				}

				return _returnType;
			}
		}

		public PInvoke PInvoke
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadPInvokeFlag))
				{
					_opFlags = _opFlags.SetBitAtIndex(LoadPInvokeFlag, false);
					int token = MetadataToken.Get(MetadataTokenType.Method, _rid);
					_pinvoke = PInvoke.Load(this, token);
				}

				return _pinvoke;
			}
			private set
			{
				_pinvoke = value;
				_opFlags = _opFlags.SetBitAtIndex(LoadPInvokeFlag, false);
				OnChanged();
			}
		}

		public MethodParameterCollection Parameters
		{
			get
			{
				if (_parameters == null)
				{
					_parameters = new MethodParameterCollection(this);
				}

				return _parameters;
			}
		}

		public GenericParameterCollection GenericParameters
		{
			get
			{
				if (_genericParameters == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.Method, _rid);
					_genericParameters = new GenericParameterCollection(this, token);
				}

				return _genericParameters;
			}
		}

		public MethodOverrideCollection Overrides
		{
			get
			{
				if (_overrides == null)
				{
					_overrides = new MethodOverrideCollection(this);
				}

				return _overrides;
			}
		}

		public CustomAttributeCollection CustomAttributes
		{
			get
			{
				if (_customAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.Method, _rid);
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
					int token = MetadataToken.Get(MetadataTokenType.Method, _rid);
					_securityAttributes = new SecurityAttributeCollection(this, token);
				}

				return _securityAttributes;
			}
		}

		public override MemberType MemberType
		{
			get { return MemberType.Method; }
		}

		#endregion

		#region Methods

		public TypeDeclaration GetOwnerType()
		{
			return _module.GetType(_ownerTypeRID);
		}

		public void CopyTo(MethodDeclaration copy)
		{
			copy._name = _name;
			copy._sigType = _sigType;
			copy._flags = _flags;
			copy._implFlags = _implFlags;

			if (PInvoke != null)
			{
				PInvoke.CopyTo(copy.CreatePInvoke());
			}

			ReturnType.CopyTo(copy.ReturnType);
			Parameters.CopyTo(copy.Parameters);
			GenericParameters.CopyTo(copy.GenericParameters);
			Overrides.CopyTo(copy.Overrides);
			CustomAttributes.CopyTo(copy.CustomAttributes);
			SecurityAttributes.CopyTo(copy.SecurityAttributes);

			if (MethodBody.IsValid(this))
			{
				var methodBody = MethodBody.Load(this);
				methodBody.Build(copy);
			}
		}

		public PInvoke CreatePInvoke()
		{
			PInvoke = new PInvoke(this, 0);

			return _pinvoke;
		}

		public void DeletePInvoke()
		{
			PInvoke = null;
		}

		public IBinaryAccessor OpenBodyStream()
		{
			// Get data from method rva.
			if (_rva > 0)
			{
				return OpenBodyDataFromImage();
			}

			// Get data from state.
			if (_opFlags.IsBitAtIndexOn(LoadBodyFromStateFlag))
			{
				return OpenBodyDataFromState();
			}

			return null;
		}

		public void SetBody(byte[] data)
		{
			_rva = 0;
			_opFlags = _opFlags.SetBitAtIndex(LoadBodyFromStateFlag, (data != null && data.Length > 0));
			_module.MethodBodyBlob[_rid - 1] = data;

			OnChanged();
		}

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		internal void InvalidatedSignatures()
		{
			_resolvedReturnType = null;
			_resolvedOverrides = null;

			if (_parameters != null)
			{
				foreach (var parameter in _parameters)
				{
					parameter.InvalidatedSignatures();
				}
			}

			if (_genericParameters != null)
			{
				foreach (var genericParameter in _genericParameters)
				{
					genericParameter.InvalidatedSignatures();
				}
			}
		}

		protected internal override void OnDeleted()
		{
			_module.MethodTable.Remove(_rid);
			_module.MethodBodyBlob[_rid - 1] = null;
		}

		protected internal override void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_module.OnChanged();
				_module.MethodTable.Change(_rid);
			}
		}

		protected IBinaryAccessor OpenBodyDataFromImage()
		{
			var image = _module.Image;
			if (image == null)
				return null;

			IBinaryAccessor accessor;
			if (!image.TryOpenImageToRVA(_rva, out accessor))
				return null;

			return accessor;
		}

		protected IBinaryAccessor OpenBodyDataFromState()
		{
			return _module.MethodBodyBlob.Open(_rid - 1);
		}

		protected void Load()
		{
			var image = _module.Image;

			MethodRow row;
			image.GetMethod(_rid, out row);

			_name = image.GetString(row.Name);
			_flags = row.Flags;
			_implFlags = row.ImplFlags;
			_rva = row.RVA;

			using (var accessor = image.OpenBlob(row.Signature))
			{
				LoadSignature(accessor, image);
			}

			if ((_flags & MethodFlags.PInvokeImpl) == MethodFlags.PInvokeImpl)
				_opFlags = _opFlags.SetBitAtIndex(LoadPInvokeFlag, true);
		}

		protected void LoadSignature(IBinaryAccessor accessor, ModuleImage image)
		{
			_sigType = accessor.ReadByte();

			if ((_sigType & Metadata.SignatureType.Generic) == Metadata.SignatureType.Generic)
				accessor.ReadCompressedInteger(); // GenericParameterCount (unused)

			int paramCount = accessor.ReadCompressedInteger();

			int[] rids;
			image.GetParamsByMethod(_rid, out rids);

			int ridIndex = 0;

			_returnType = new MethodReturnType(this);
			_returnType.Load(accessor, rids, ref ridIndex);

			if (paramCount > 0)
			{
				_parameters = new MethodParameterCollection(this);
				_parameters.Load(accessor, paramCount, rids, ridIndex);
			}
		}

		#endregion

		#region IMethod Members

		IMethod IMethod.DeclaringMethod
		{
			get { return this; }
		}

		IType IMethod.Owner
		{
			get { return GetOwnerType(); }
		}

		IType IMethod.ReturnType
		{
			get
			{
				if (_resolvedReturnType == null)
				{
					_resolvedReturnType = AssemblyManager.Resolve(ReturnType.Type, this, true);
				}

				return _resolvedReturnType;
			}
		}

		IReadOnlyList<IMethod> IMethod.Overrides
		{
			get
			{
				if (_resolvedOverrides == null)
				{
					_resolvedOverrides = AssemblyManager.Resolve(Overrides, this, true, true);
				}

				return _resolvedOverrides;
			}
		}

		IReadOnlyList<IMethodSignature> IMethodBase.Overrides
		{
			get { return Overrides; }
		}

		IReadOnlyList<IMethodParameter> IMethod.Parameters
		{
			get { return Parameters; }
		}

		IReadOnlyList<IType> IMethod.GenericArguments
		{
			get { return ReadOnlyList<IType>.Empty; }
		}

		IReadOnlyList<IGenericParameter> IMethod.GenericParameters
		{
			get { return GenericParameters; }
		}

		#endregion

		#region IMethodSignature Members

		int IMethodSignature.VarArgIndex
		{
			get { return -1; }
		}

		int IMethodSignature.GenericParameterCount
		{
			get { return GenericParameters.Count; }
		}

		ITypeSignature IMethodSignature.ReturnType
		{
			get { return ReturnType.Type; }
		}

		ITypeSignature IMethodSignature.Owner
		{
			get { return GetOwnerType(); }
		}

		IMethodSignature IMethodSignature.DeclaringMethod
		{
			get { return this; }
		}

		IReadOnlyList<ITypeSignature> IMethodSignature.Arguments
		{
			get { return Parameters; }
		}

		IReadOnlyList<ITypeSignature> IMethodSignature.GenericArguments
		{
			get { return ReadOnlyList<IType>.Empty; }
		}

		SignatureType ISignature.SignatureType
		{
			get { return SignatureType.Method; }
		}

		#endregion

		#region ICodeNode Members

		EntityType ICodeNode.EntityType
		{
			get { return EntityType.Method; }
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
