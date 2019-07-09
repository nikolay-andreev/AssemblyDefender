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
	public class MethodParameter : CodeNode, IMethodParameter, ICustomAttributeProvider
	{
		#region Fields

		private const int LoadMarshalTypeFlag = 1;
		private const int LoadDefaultValueFlag = 2;
		private int _rid;
		private string _name;
		private int _flags;
		private TypeSignature _type;
		private MarshalType _marshalType;
		private ConstantInfo? _defaultValue;
		private CustomAttributeCollection _customAttributes;
		private MethodDeclaration _method;
		private IType _resolvedType;
		private int _opFlags;

		#endregion

		#region Ctors

		internal MethodParameter(CodeNode parent, MethodDeclaration method, bool isNew)
			: base(parent)
		{
			_method = method;

			if (isNew)
			{
				IsNew = true;
			}
		}

		#endregion

		#region Properties

		public int RID
		{
			get { return _rid; }
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

		public bool IsIn
		{
			get { return _flags.IsBitsOn(ParamFlags.In); }
			set
			{
				_flags = _flags.SetBits(ParamFlags.In, value);
				OnChanged();
			}
		}

		public bool IsOut
		{
			get { return _flags.IsBitsOn(ParamFlags.Out); }
			set
			{
				_flags = _flags.SetBits(ParamFlags.Out, value);
				OnChanged();
			}
		}

		public bool IsOptional
		{
			get { return _flags.IsBitsOn(ParamFlags.Optional); }
			set
			{
				_flags = _flags.SetBits(ParamFlags.Optional, value);
				OnChanged();
			}
		}

		public bool IsLcid
		{
			get { return _flags.IsBitsOn(ParamFlags.Lcid); }
			set
			{
				_flags = _flags.SetBits(ParamFlags.Lcid, value);
				OnChanged();
			}
		}

		public TypeSignature Type
		{
			get { return _type; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("Type");

				_type = value;
				_module.AddSignature(ref _type);

				OnChanged();
			}
		}

		public MarshalType MarshalType
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadMarshalTypeFlag))
				{
					_opFlags = _opFlags.SetBitAtIndex(LoadMarshalTypeFlag, false);
					int token = MetadataToken.Get(MetadataTokenType.Param, _rid);
					_marshalType = MarshalType.Load(this, token);
				}

				return _marshalType;
			}
			private set
			{
				_marshalType = value;
				_opFlags = _opFlags.SetBitAtIndex(LoadMarshalTypeFlag, false);
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
					int token = MetadataToken.Get(MetadataTokenType.Param, _rid);
					_customAttributes = new CustomAttributeCollection(this, token);
				}

				return _customAttributes;
			}
		}

		#endregion

		#region Methods

		public void CopyTo(MethodParameter copy)
		{
			copy._name = _name;
			copy._flags = _flags;
			copy._type = _type;
			copy._defaultValue = DefaultValue;

			if (MarshalType != null)
			{
				MarshalType.CopyTo(copy.CreateMarshalType(MarshalType.UnmanagedType));
			}

			CustomAttributes.CopyTo(copy.CustomAttributes);
		}

		public MarshalType CreateMarshalType(UnmanagedType type)
		{
			MarshalType = MarshalType.CreateNew(this, type);

			return _marshalType;
		}

		public void DeleteMarshalType()
		{
			MarshalType = null;
		}

		internal void InvalidatedSignatures()
		{
			_resolvedType = null;
		}

		internal void Load(IBinaryAccessor accessor, int paramRID)
		{
			_type = TypeSignature.Load(accessor, _module);

			if (paramRID > 0)
			{
				var image = _module.Image;

				ParamRow row;
				image.GetParam(paramRID, out row);

				_rid = paramRID;
				_name = image.GetString(row.Name);
				_flags = row.Flags;

				if ((_flags & ParamFlags.HasFieldMarshal) == ParamFlags.HasFieldMarshal)
					_opFlags = _opFlags.SetBitAtIndex(LoadMarshalTypeFlag, true);

				if ((_flags & ParamFlags.HasDefault) == ParamFlags.HasDefault)
					_opFlags = _opFlags.SetBitAtIndex(LoadDefaultValueFlag, true);
			}
		}

		#endregion

		#region IMethodParameter Members

		IType IMethodParameter.Type
		{
			get
			{
				if (_resolvedType == null)
				{
					_resolvedType = AssemblyManager.Resolve(_type, _method, true);
				}

				return _resolvedType;
			}
		}

		ITypeSignature IMethodParameterBase.Type
		{
			get { return _type; }
		}

		IMethod IMethodParameter.Owner
		{
			get { return _method; }
		}

		#endregion
	}
}
