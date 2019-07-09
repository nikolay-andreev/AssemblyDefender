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
	public class MethodReturnType : CodeNode, ICustomAttributeProvider
	{
		#region Fields

		private const int LoadMarshalTypeFlag = 1;
		private int _rid;
		private MethodDeclaration _method;
		private TypeSignature _type;
		private PrimitiveTypeCode? _typeCode;
		private MarshalType _marshalType;
		private CustomAttributeCollection _customAttributes;
		private int _opFlags;

		#endregion

		#region Ctors

		internal MethodReturnType(MethodDeclaration method)
			: base(method)
		{
			_method = method;
			_type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, Assembly);
			_typeCode = PrimitiveTypeCode.Void;

			if (method.IsNew)
			{
				IsNew = true;
			}
		}

		#endregion

		#region Properties

		public MethodDeclaration Method
		{
			get { return _method; }
		}

		public TypeSignature Type
		{
			get { return _type; }
			set
			{
				_type = value;
				_typeCode = null;

				if (_type == null)
				{
					_type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, Assembly);
					_typeCode = PrimitiveTypeCode.Void;
				}

				_module.AddSignature(ref _type);

				OnChanged();
			}
		}

		public PrimitiveTypeCode? TypeCode
		{
			get
			{
				if (!_typeCode.HasValue)
				{
					_typeCode = Type.GetTypeCode(_module);
				}

				return _typeCode;
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

		public void CopyTo(MethodReturnType copy)
		{
			copy._type = _type;

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

		internal void Load(IBinaryAccessor accessor, int[] paramRIDs, ref int ridIndex)
		{
			var image = _module.Image;

			_type = TypeSignature.Load(accessor, _module);
			_typeCode = null;

			if (ridIndex < paramRIDs.Length)
			{
				int paramRID = paramRIDs[ridIndex];

				ParamRow paramRow;
				image.GetParam(paramRID, out paramRow);

				// Sequence zero is for return type. It's optional and is used when return type has marshal or custom attributes.
				if (paramRow.Sequence == 0)
				{
					_rid = paramRID;

					if ((paramRow.Flags & ParamFlags.HasFieldMarshal) == ParamFlags.HasFieldMarshal)
						_opFlags = _opFlags.SetBitAtIndex(LoadMarshalTypeFlag, true);

					ridIndex++;
				}
			}
		}

		#endregion
	}
}
