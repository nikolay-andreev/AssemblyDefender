using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class TypeReference : TypeSignature
	{
		#region Fields

		private string _name;
		private string _namespace;
		private Signature _owner;
		private bool? _isValueType;

		#endregion

		#region Ctors

		private TypeReference()
		{
		}

		public TypeReference(string name)
		{
			_name = name.NullIfEmpty();
		}

		public TypeReference(string name, bool isValueType)
		{
			_name = name.NullIfEmpty();
			_isValueType = isValueType;
		}

		public TypeReference(string name, string ns)
		{
			_name = name.NullIfEmpty();
			_namespace = ns.NullIfEmpty();
		}

		public TypeReference(string name, string ns, bool isValueType)
			: this(name, ns, null, isValueType)
		{
			_name = name.NullIfEmpty();
			_namespace = ns.NullIfEmpty();
			_isValueType = isValueType;
		}

		public TypeReference(string name, string ns, Signature owner)
			: this(name, ns, owner, null)
		{
		}

		public TypeReference(string name, Signature owner, bool isValueType)
			: this(name, null, owner, isValueType)
		{
		}

		public TypeReference(string name, string ns, Signature owner, bool? isValueType)
		{
			_name = name.NullIfEmpty();
			_namespace = ns.NullIfEmpty();
			_owner = owner;
			_isValueType = isValueType;
		}

		#endregion

		#region Properties

		public override string Name
		{
			get { return _name; }
		}

		public override string Namespace
		{
			get { return _namespace; }
		}

		public override TypeReference EnclosingType
		{
			get { return _owner as TypeReference; }
		}

		public override Signature ResolutionScope
		{
			get
			{
				var owner = _owner;
				while (owner is TypeReference)
				{
					owner = ((TypeReference)owner).Owner;
				}

				return owner;
			}
		}

		public override Signature Owner
		{
			get { return _owner; }
		}

		public bool? IsValueType
		{
			get { return _isValueType; }
			set { _isValueType = value; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.DeclaringType; }
		}

		#endregion

		#region Methods

		public override bool GetSize(Module module, out int size)
		{
			var typeCode = this.GetTypeCode(module);
			switch (typeCode)
			{
				case PrimitiveTypeCode.Undefined:
					return ResolveSize(module, out size);

				case PrimitiveTypeCode.Boolean:
				case PrimitiveTypeCode.Int8:
				case PrimitiveTypeCode.UInt8:
					size = 1;
					return true;

				case PrimitiveTypeCode.Int16:
				case PrimitiveTypeCode.UInt16:
				case PrimitiveTypeCode.Char:
					size = 2;
					return true;

				case PrimitiveTypeCode.Int32:
				case PrimitiveTypeCode.UInt32:
				case PrimitiveTypeCode.Float32:
				case PrimitiveTypeCode.IntPtr:
				case PrimitiveTypeCode.UIntPtr:
					size = 4;
					return true;

				case PrimitiveTypeCode.Int64:
				case PrimitiveTypeCode.UInt64:
				case PrimitiveTypeCode.Float64:
					size = 8;
					return true;

				default:
					size = 0;
					return false;
			}
		}

		public bool? GetIsValueType(Module module)
		{
			if (!_isValueType.HasValue)
			{
				var type = this.Resolve(module);
				if (type != null)
				{
					_isValueType = type.IsValueType();
				}
			}

			return _isValueType;
		}

		private bool ResolveSize(Module module, out int size)
		{
			size = 0;

			var type = (TypeDeclaration)this.Resolve(module);
			if (type == null)
				return false;

			if (type.ClassSize.HasValue)
			{
				size = type.ClassSize.Value;
				return true;
			}

			// Calculate field size. Value type only.
			if (!_isValueType.HasValue)
				_isValueType = type.IsValueType();

			if (!_isValueType.Value)
				return false;

			foreach (var field in type.Fields)
			{
				int fieldSize;
				if (!field.FieldType.GetSize(type.Module, out fieldSize))
					return false;

				size += fieldSize;
			}

			return true;
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _owner);
		}

		internal void SetOwner(Signature owner)
		{
			_owner = owner;
		}

		#endregion

		#region Static

		public static TypeReference Parse(string value)
		{
			return (new ReflectionSignatureParser(value)).ParseTypeRef();
		}

		public static TypeReference Get(TypeSignature typeSig)
		{
			while (typeSig.ElementType != null)
			{
				typeSig = typeSig.ElementType;
			}

			if (typeSig.ElementCode == TypeElementCode.GenericType)
				return ((GenericTypeReference)typeSig).DeclaringType;
			else
				return typeSig as TypeReference;
		}

		public static TypeReference GetPrimitiveType(PrimitiveTypeCode typeCode, Assembly assembly)
		{
			var typeRef = assembly.PrimitiveTypeSignatures[(int)typeCode];
			if (typeRef != null)
				return typeRef;

			typeRef = new TypeReference();

			var primitiveTypeInfo = CodeModelUtils.PrimitiveTypes[(int)typeCode];
			typeRef._name = primitiveTypeInfo.Name;
			typeRef._namespace = "System";
			typeRef._isValueType = primitiveTypeInfo.IsValueType;

			if (assembly.Name != "mscorlib")
			{
				typeRef._owner = AssemblyReference.GetMscorlib(assembly);
			}

			assembly.Module.AddSignature(ref typeRef);
			assembly.PrimitiveTypeSignatures[(int)typeCode] = typeRef;

			return typeRef;
		}

		internal static TypeReference GetPrimitiveType(int elementType, Assembly assembly)
		{
			return GetPrimitiveType(CodeModelUtils.GetTypeCode(elementType), assembly);
		}

		internal static TypeReference LoadTypeDefOrRef(Module module, int token, bool? isValueType = null)
		{
			switch (MetadataToken.GetType(token))
			{
				case MetadataTokenType.TypeRef:
					return LoadTypeRef(module, MetadataToken.GetRID(token), isValueType);

				case MetadataTokenType.TypeDef:
					return LoadTypeDef(module, MetadataToken.GetRID(token), isValueType);

				default:
					throw new InvalidDataException(string.Format("Invalid token {0}. Expected TypeDefOrRef.", MetadataToken.GetType(token)));
			}
		}

		internal static TypeReference LoadTypeRef(Module module, int rid, bool? isValueType = null)
		{
			var image = module.Image;

			var typeRef = image.TypeRefSignatures[rid - 1];
			if (typeRef != null)
			{
				if (isValueType.HasValue)
					typeRef._isValueType = isValueType;

				return typeRef;
			}

			TypeRefRow row;
			image.GetTypeRef(rid, out row);

			int resolutionScopeToken = MetadataToken.DecompressResolutionScope(row.ResolutionScope);

			typeRef = new TypeReference();
			typeRef._name = image.GetString(row.Name);
			typeRef._namespace = image.GetString(row.Namespace);
			typeRef._owner = LoadResolutionScope(module, resolutionScopeToken);
			typeRef._isValueType = isValueType;

			module.AddSignature(ref typeRef);
			image.TypeRefSignatures[rid - 1] = typeRef;

			return typeRef;
		}

		private static Signature LoadResolutionScope(Module module, int token)
		{
			int rid = MetadataToken.GetRID(token);
			if (rid == 0)
				return null;

			switch (MetadataToken.GetType(token))
			{
				case MetadataTokenType.TypeRef:
					return LoadTypeRef(module, rid);

				case MetadataTokenType.Assembly: // Current assembly.
				case MetadataTokenType.Module: // Current module.
					return null;

				case MetadataTokenType.AssemblyRef:
					return AssemblyReference.LoadRef(module, rid);

				case MetadataTokenType.ModuleRef:
					return ModuleReference.LoadRef(module, rid);

				default:
					throw new AssemblyLoadException(string.Format(SR.AssemblyLoadError, module.Location));
			}
		}

		internal static TypeReference LoadTypeDef(Module module, int rid, bool? isValueType = null)
		{
			var image = module.Image;

			var typeRef = image.TypeSignatures[rid - 1];
			if (typeRef != null)
			{
				if (isValueType.HasValue)
					typeRef._isValueType = isValueType;

				return typeRef;
			}

			TypeDefRow row;
			image.GetTypeDef(rid, out row);

			typeRef = new TypeReference();
			typeRef._name = image.GetString(row.Name);
			typeRef._namespace = image.GetString(row.Namespace);

			// Owner
			int enclosingRID;
			if (image.GetEnclosingTypeByNested(rid, out enclosingRID))
				typeRef._owner = LoadTypeDef(module, enclosingRID);
			else
				typeRef._owner = ModuleReference.LoadDef(module);

			module.AddSignature(ref typeRef);
			image.TypeSignatures[rid - 1] = typeRef;

			// Is value type.
			if (isValueType.HasValue)
			{
				typeRef._isValueType = isValueType.Value;
			}
			else
			{
				int extendsToken = MetadataToken.DecompressTypeDefOrRef(row.Extends);
				if (!MetadataToken.IsNull(extendsToken))
				{
					var baseType = TypeSignature.Load(module, extendsToken);
					typeRef._isValueType = GetIsValueType(typeRef, baseType, module);
				}
			}

			return typeRef;
		}

		internal static TypeReference LoadClass(IBinaryAccessor accessor, Module module)
		{
			int token = MetadataToken.DecompressTypeDefOrRef(accessor.ReadCompressedInteger());

			var type = LoadTypeDefOrRef(module, token, false);

			return type;
		}

		internal static TypeReference LoadValueType(IBinaryAccessor accessor, Module module)
		{
			int token = MetadataToken.DecompressTypeDefOrRef(accessor.ReadCompressedInteger());

			var type = LoadTypeDefOrRef(module, token, true);

			return type;
		}

		internal static TypeReference LoadExportedType(Module module, int rid)
		{
			var image = module.Image;

			var typeRef = image.ExportedTypeSignatures[rid - 1];
			if (typeRef != null)
				return typeRef;

			ExportedTypeRow row;
			image.GetExportedType(rid, out row);

			typeRef = new TypeReference();
			typeRef._name = image.GetString(row.TypeName);
			typeRef._namespace = image.GetString(row.TypeNamespace);
			typeRef._owner = LoadExportedTypeImplementation(module, MetadataToken.DecompressImplementation(row.Implementation));
			typeRef._isValueType = false;

			module.AddSignature(ref typeRef);
			image.ExportedTypeSignatures[rid - 1] = typeRef;

			return typeRef;
		}

		private static Signature LoadExportedTypeImplementation(Module module, int token)
		{
			int rid = MetadataToken.GetRID(token);
			if (rid == 0)
				return null;

			switch (MetadataToken.GetType(token))
			{
				case MetadataTokenType.File:
					return ModuleReference.LoadFile(module, rid);

				case MetadataTokenType.AssemblyRef:
					return AssemblyReference.LoadRef(module, rid);

				case MetadataTokenType.ExportedType:
					return LoadExportedType(module, rid);

				default:
					throw new AssemblyLoadException(string.Format(SR.AssemblyLoadError, module.Location));
			}
		}

		private static bool? GetIsValueType(TypeReference typeRef, TypeSignature baseTypeSig, Module module)
		{
			if (baseTypeSig == null)
				return false;

			var baseTypeRef = baseTypeSig.GetLastElementType() as TypeReference;
			if (baseTypeRef == null)
				return false;

			if (!SignatureUtils.EqualsValueTypeOrEnum(baseTypeRef, module.Assembly))
				return false;

			if (SignatureUtils.EqualsValueTypeOrEnum(typeRef, module.Assembly))
				return false;

			return true;
		}

		#endregion
	}
}
