using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public abstract class TypeSignature : Signature, ITypeSignature
	{
		#region Ctors

		protected TypeSignature()
		{
		}

		#endregion

		#region Properties

		public bool IsNested
		{
			get { return EnclosingType != null; }
		}

		public virtual string Name
		{
			get { return null; }
		}

		public virtual string Namespace
		{
			get { return null; }
		}

		public string FullName
		{
			get { return CodeModelUtils.GetTypeName(Name, Namespace); }
		}

		public virtual TypeReference EnclosingType
		{
			get { return null; }
		}

		public virtual TypeReference DeclaringType
		{
			get { return null; }
		}

		public virtual Signature ResolutionScope
		{
			get { return null; }
		}

		public virtual Signature Owner
		{
			get { return null; }
		}

		public virtual TypeSignature ElementType
		{
			get { return null; }
		}

		public virtual IReadOnlyList<ArrayDimension> ArrayDimensions
		{
			get { return ReadOnlyList<ArrayDimension>.Empty; }
		}

		public virtual IReadOnlyList<TypeSignature> GenericArguments
		{
			get { return ReadOnlyList<TypeSignature>.Empty; }
		}

		public abstract TypeElementCode ElementCode
		{
			get;
		}

		public override SignatureType SignatureType
		{
			get { return SignatureType.Type; }
		}

		#endregion

		#region Methods

		public TypeSignature GetElementType(TypeElementCode type)
		{
			var typeSig = this;
			while (typeSig != null && typeSig.ElementCode != type)
			{
				typeSig = typeSig.ElementType;
			}

			return typeSig;
		}

		public TypeSignature GetLastElementType()
		{
			var typeSig = this;
			while (typeSig.ElementType != null)
			{
				typeSig = typeSig.ElementType;
			}

			return typeSig;
		}

		public TypeReference GetDeclaringElementType()
		{
			var typeSig = GetLastElementType();
			if (typeSig.ElementCode == TypeElementCode.GenericType)
			{
				return typeSig.DeclaringType;
			}
			else
			{
				return typeSig as TypeReference;
			}
		}

		public virtual void GetGenericParameter(out bool isMethod, out int position)
		{
			isMethod = false;
			position = -1;
		}

		public virtual TypeSignature GetCustomModifier(out CustomModifierType modifierType)
		{
			modifierType = CustomModifierType.ModOpt;
			return null;
		}

		public virtual CallSite GetFunctionPointer()
		{
			return null;
		}

		public abstract bool GetSize(Module module, out int size);

		public override string ToString()
		{
			return this.ToString(null, SignaturePrintingFlags.None);
		}

		#endregion

		#region ITypeSignature Members

		ITypeSignature ITypeSignature.EnclosingType
		{
			get { return EnclosingType; }
		}

		ISignature ITypeSignature.ResolutionScope
		{
			get { return ResolutionScope; }
		}

		ISignature ITypeSignature.Owner
		{
			get { return Owner; }
		}

		ITypeSignature ITypeSignature.ElementType
		{
			get { return ElementType; }
		}

		ITypeSignature ITypeSignature.DeclaringType
		{
			get { return DeclaringType; }
		}

		IReadOnlyList<ITypeSignature> ITypeSignature.GenericArguments
		{
			get { return GenericArguments; }
		}

		ITypeSignature ITypeSignature.GetCustomModifier(out CustomModifierType modifierType)
		{
			return GetCustomModifier(out modifierType);
		}

		IMethodSignature ITypeSignature.GetFunctionPointer()
		{
			return GetFunctionPointer();
		}

		#endregion

		#region Static

		public static TypeSignature Parse(string value, bool throwOnError = false)
		{
			var typeSig = (new ReflectionSignatureParser(value)).ParseType();
			if (typeSig == null)
			{
				if (throwOnError)
				{
					throw new IdentityParseException(string.Format(SR.TypeIdentityParseError, value ?? ""));
				}

				return null;
			}

			return typeSig;
		}

		internal static bool IsTypeSpecSignature(Module module, int rid)
		{
			var image = module.Image;

			int blobID = image.GetTypeSpec(rid);

			using (var accessor = image.OpenBlob(blobID))
			{
				byte elementType = accessor.ReadByte();
				if (elementType == Metadata.ElementType.TypeDef)
					return false;
				else
					return true;
			}
		}

		internal static TypeSignature Load(Module module, int token)
		{
			switch (MetadataToken.GetType(token))
			{
				case MetadataTokenType.TypeRef:
					return TypeReference.LoadTypeRef(module, MetadataToken.GetRID(token));

				case MetadataTokenType.TypeDef:
					return TypeReference.LoadTypeDef(module, MetadataToken.GetRID(token));

				case MetadataTokenType.TypeSpec:
					return LoadTypeSpec(module, MetadataToken.GetRID(token));

				default:
					throw new InvalidDataException(string.Format("Invalid token {0}. Expected TypeDefOrRef.", MetadataToken.GetType(token)));
			}
		}

		internal static TypeSignature LoadTypeSpec(Module module, int rid)
		{
			var image = module.Image;

			var typeSig = image.TypeSpecSignatures[rid - 1];
			if (typeSig != null)
				return typeSig;

			int blobID = image.GetTypeSpec(rid);

			using (var accessor = image.OpenBlob(blobID))
			{
				// Note that a TypeSpecBlob does not begin with a callingconvention byte,
				// so it differs from the various other signatures that are stored into Metadata.
				typeSig = Load(accessor, module);
			}

			module.AddSignature(ref typeSig);
			image.TypeSpecSignatures[rid - 1] = typeSig;

			return typeSig;
		}

		internal static TypeSignature Load(IBinaryAccessor accessor, Module module)
		{
			int elementType = accessor.ReadCompressedInteger();
			switch (elementType)
			{
				case Metadata.ElementType.Class:
					return TypeReference.LoadClass(accessor, module);

				case Metadata.ElementType.ValueType:
					return TypeReference.LoadValueType(accessor, module);

				case Metadata.ElementType.ByRef:
					return ByRefType.LoadByRef(accessor, module);

				case Metadata.ElementType.Ptr:
					return PointerType.LoadPtr(accessor, module);

				case Metadata.ElementType.FnPtr:
					return FunctionPointer.LoadFnPtr(accessor, module);

				case Metadata.ElementType.Array:
					return ArrayType.LoadArray(accessor, module);

				case Metadata.ElementType.SzArray:
					return ArrayType.LoadSzArray(accessor, module);

				case Metadata.ElementType.Var:
					return GenericParameterType.LoadVar(accessor, module);

				case Metadata.ElementType.MVar:
					return GenericParameterType.LoadMVar(accessor, module);

				case Metadata.ElementType.GenericInst:
					return GenericTypeReference.LoadGeneric(accessor, module);

				case Metadata.ElementType.CModOpt:
					return CustomModifier.LoadModOpt(accessor, module);

				case Metadata.ElementType.CModReqD:
					return CustomModifier.LoadModReq(accessor, module);

				case Metadata.ElementType.Pinned:
					return PinnedType.LoadPinned(accessor, module);

				default:
					return TypeReference.GetPrimitiveType(elementType, module.Assembly);
			}
		}

		internal static TypeSignature[] LoadGenericArguments(IBinaryAccessor accessor, Module module)
		{
			int count = accessor.ReadCompressedInteger();

			var array = new TypeSignature[count];

			for (int i = 0; i < count; i++)
			{
				array[i] = Load(accessor, module);
			}

			return array;
		}

		internal static TypeSignature[] LoadMethodArguments(IBinaryAccessor accessor, Module module, int count)
		{
			var array = new TypeSignature[count];

			for (int i = 0; i < count; i++)
			{
				array[i] = Load(accessor, module);
			}

			return array;
		}

		/// If and only if optional parameters are specified in a vararg method reference at the call site,
		/// they are preceded by a sentinel'an ellipsis in ILAsm notation
		/// Example: call vararg void Print(string, ... int32, float32, string)
		/// </remarks>
		internal static TypeSignature[] LoadVarArgMethodArguments(IBinaryAccessor accessor, Module module, int count, out int varArgIndex)
		{
			varArgIndex = -1;
			var array = new TypeSignature[count];

			for (int i = 0; i < count; i++)
			{
				long offset = accessor.Position;
				int flag = accessor.ReadCompressedInteger();
				if (flag == Metadata.ElementType.Sentinel)
				{
					varArgIndex = i;
				}
				else
				{
					accessor.Position = offset;
				}

				array[i] = Load(accessor, module);
			}

			return array;
		}

		#endregion
	}
}
