using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public abstract class SignaturePredicate
	{
		protected bool _defaultValue;

		public virtual bool Predicate(Signature signature)
		{
			switch (signature.SignatureType)
			{
				case SignatureType.Assembly:
					{
						if (_defaultValue != Predicate((AssemblyReference)signature))
							return !_defaultValue;
					}
					break;

				case SignatureType.Module:
					{
						if (_defaultValue != Predicate((ModuleReference)signature))
							return !_defaultValue;
					}
					break;

				case SignatureType.File:
					{
						if (_defaultValue != Predicate((FileReference)signature))
							return !_defaultValue;
					}
					break;

				case SignatureType.Type:
					{
						if (_defaultValue != Predicate((TypeSignature)signature))
							return !_defaultValue;
					}
					break;

				case SignatureType.Method:
					{
						if (_defaultValue != Predicate((MethodSignature)signature))
							return !_defaultValue;
					}
					break;

				case SignatureType.Field:
					{
						if (_defaultValue != Predicate((FieldReference)signature))
							return !_defaultValue;
					}
					break;

				default:
					throw new InvalidOperationException();
			}

			return _defaultValue;
		}

		public virtual bool Predicate(AssemblyReference assemblyRef)
		{
			return _defaultValue;
		}

		public virtual bool Predicate(ModuleReference moduleRef)
		{
			return _defaultValue;
		}

		public virtual bool Predicate(FileReference fileRef)
		{
			return _defaultValue;
		}

		public virtual bool Predicate(TypeSignature typeSig)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					{
						if (_defaultValue != Predicate((ArrayType)typeSig))
							return !_defaultValue;
					}
					break;

				case TypeElementCode.ByRef:
					{
						if (_defaultValue != Predicate((ByRefType)typeSig))
							return !_defaultValue;
					}
					break;

				case TypeElementCode.CustomModifier:
					{
						if (_defaultValue != Predicate((CustomModifier)typeSig))
							return !_defaultValue;
					}
					break;

				case TypeElementCode.FunctionPointer:
					{
						if (_defaultValue != Predicate((FunctionPointer)typeSig))
							return !_defaultValue;
					}
					break;

				case TypeElementCode.GenericParameter:
					{
						if (_defaultValue != Predicate((GenericParameterType)typeSig))
							return !_defaultValue;
					}
					break;

				case TypeElementCode.GenericType:
					{
						if (_defaultValue != Predicate((GenericTypeReference)typeSig))
							return !_defaultValue;
					}
					break;

				case TypeElementCode.Pinned:
					{
						if (_defaultValue != Predicate((PinnedType)typeSig))
							return !_defaultValue;
					}
					break;

				case TypeElementCode.Pointer:
					{
						if (_defaultValue != Predicate((PointerType)typeSig))
							return !_defaultValue;
					}
					break;

				case TypeElementCode.DeclaringType:
					{
						if (_defaultValue != Predicate((TypeReference)typeSig))
							return !_defaultValue;
					}
					break;

				default:
					throw new InvalidOperationException();
			}

			return _defaultValue;
		}

		public virtual bool Predicate(ArrayType arrayType)
		{
			if (_defaultValue != Predicate(arrayType.ElementType))
				return !_defaultValue;

			return _defaultValue;
		}

		public virtual bool Predicate(ByRefType byRefType)
		{
			if (_defaultValue != Predicate(byRefType.ElementType))
				return !_defaultValue;

			return _defaultValue;
		}

		public virtual bool Predicate(CustomModifier customModifier)
		{
			if (customModifier.Modifier != null)
			{
				if (_defaultValue != Predicate(customModifier.Modifier))
					return !_defaultValue;
			}

			if (_defaultValue != Predicate(customModifier.ElementType))
				return !_defaultValue;

			return _defaultValue;
		}

		public virtual bool Predicate(FunctionPointer functionPointer)
		{
			if (_defaultValue != Predicate(functionPointer.CallSite))
				return !_defaultValue;

			return _defaultValue;
		}

		public virtual bool Predicate(GenericParameterType genericType)
		{
			return _defaultValue;
		}

		public virtual bool Predicate(PinnedType pinnedType)
		{
			if (_defaultValue != Predicate(pinnedType.ElementType))
				return !_defaultValue;

			return _defaultValue;
		}

		public virtual bool Predicate(PointerType pointerType)
		{
			if (_defaultValue != Predicate(pointerType.ElementType))
				return !_defaultValue;

			return _defaultValue;
		}

		public virtual bool Predicate(TypeReference typeRef)
		{
			if (typeRef.Owner != null)
			{
				if (_defaultValue != Predicate(typeRef.Owner))
					return !_defaultValue;
			}

			return _defaultValue;
		}

		public virtual bool Predicate(GenericTypeReference genericTypeRef)
		{
			if (_defaultValue != Predicate(genericTypeRef.DeclaringType))
				return !_defaultValue;

			for (int i = 0; i < genericTypeRef.GenericArguments.Count; i++)
			{
				if (_defaultValue != Predicate(genericTypeRef.GenericArguments[i]))
					return !_defaultValue;
			}

			return _defaultValue;
		}

		public virtual bool Predicate(MethodSignature methodSig)
		{
			switch (methodSig.Type)
			{
				case MethodSignatureType.CallSite:
					{
						if (_defaultValue != Predicate((CallSite)methodSig))
							return !_defaultValue;
					}
					break;

				case MethodSignatureType.GenericMethod:
					{
						if (_defaultValue != Predicate((GenericMethodReference)methodSig))
							return !_defaultValue;
					}
					break;

				case MethodSignatureType.DeclaringMethod:
					{
						if (_defaultValue != Predicate((MethodReference)methodSig))
							return !_defaultValue;
					}
					break;

				default:
					throw new InvalidOperationException();
			}

			return _defaultValue;
		}

		public virtual bool Predicate(CallSite callSite)
		{
			for (int i = 0; i < callSite.Arguments.Count; i++)
			{
				if (_defaultValue != Predicate(callSite.Arguments[i]))
					return !_defaultValue;
			}

			return _defaultValue;
		}

		public virtual bool Predicate(MethodReference methodRef)
		{
			if (_defaultValue != Predicate(methodRef.Owner))
				return !_defaultValue;

			if (_defaultValue != Predicate(methodRef.CallSite))
				return !_defaultValue;

			return _defaultValue;
		}

		public virtual bool Predicate(GenericMethodReference genericMethodRef)
		{
			if (_defaultValue != Predicate(genericMethodRef.DeclaringMethod))
				return !_defaultValue;

			for (int i = 0; i < genericMethodRef.GenericArguments.Count; i++)
			{
				if (_defaultValue != Predicate(genericMethodRef.GenericArguments[i]))
					return !_defaultValue;
			}

			return _defaultValue;
		}

		public virtual bool Predicate(FieldReference fieldRef)
		{
			if (_defaultValue != Predicate(fieldRef.FieldType))
				return !_defaultValue;

			if (_defaultValue != Predicate(fieldRef.Owner))
				return !_defaultValue;

			return _defaultValue;
		}
	}
}
