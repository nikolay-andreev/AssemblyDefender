using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public abstract class SignatureVisitor
	{
		public virtual void Visit(Signature signature)
		{
			switch (signature.SignatureType)
			{
				case SignatureType.Assembly:
					Visit((AssemblyReference)signature);
					break;

				case SignatureType.Module:
					Visit((ModuleReference)signature);
					break;

				case SignatureType.File:
					Visit((FileReference)signature);
					break;

				case SignatureType.Type:
					Visit((TypeSignature)signature);
					break;

				case SignatureType.Method:
					Visit((MethodSignature)signature);
					break;

				case SignatureType.Field:
					Visit((FieldReference)signature);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		public virtual void Visit(AssemblyReference assemblyRef)
		{
		}

		public virtual void Visit(ModuleReference moduleRef)
		{
		}

		public virtual void Visit(FileReference fileRef)
		{
		}

		public virtual void Visit(TypeSignature typeSig)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					Visit((ArrayType)typeSig);
					break;

				case TypeElementCode.ByRef:
					Visit((ByRefType)typeSig);
					break;

				case TypeElementCode.CustomModifier:
					Visit((CustomModifier)typeSig);
					break;

				case TypeElementCode.FunctionPointer:
					Visit((FunctionPointer)typeSig);
					break;

				case TypeElementCode.GenericParameter:
					Visit((GenericParameterType)typeSig);
					break;

				case TypeElementCode.GenericType:
					Visit((GenericTypeReference)typeSig);
					break;

				case TypeElementCode.Pinned:
					Visit((PinnedType)typeSig);
					break;

				case TypeElementCode.Pointer:
					Visit((PointerType)typeSig);
					break;

				case TypeElementCode.DeclaringType:
					Visit((TypeReference)typeSig);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		public virtual void Visit(ArrayType arrayType)
		{
			Visit(arrayType.ElementType);
		}

		public virtual void Visit(ByRefType byRefType)
		{
			Visit(byRefType.ElementType);
		}

		public virtual void Visit(CustomModifier customModifier)
		{
			if (customModifier.Modifier != null)
			{
				Visit(customModifier.Modifier);
			}

			Visit(customModifier.ElementType);
		}

		public virtual void Visit(FunctionPointer functionPointer)
		{
			Visit(functionPointer.CallSite);
		}

		public virtual void Visit(GenericParameterType genericType)
		{
		}

		public virtual void Visit(PinnedType pinnedType)
		{
			Visit(pinnedType.ElementType);
		}

		public virtual void Visit(PointerType pointerType)
		{
			Visit(pointerType.ElementType);
		}

		public virtual void Visit(TypeReference typeRef)
		{
			if (typeRef.Owner != null)
			{
				Visit(typeRef.Owner);
			}
		}

		public virtual void Visit(GenericTypeReference genericTypeRef)
		{
			Visit(genericTypeRef.DeclaringType);

			for (int i = 0; i < genericTypeRef.GenericArguments.Count; i++)
			{
				Visit(genericTypeRef.GenericArguments[i]);
			}
		}

		public virtual void Visit(MethodSignature methodSig)
		{
			switch (methodSig.Type)
			{
				case MethodSignatureType.CallSite:
					Visit((CallSite)methodSig);
					break;

				case MethodSignatureType.GenericMethod:
					Visit((GenericMethodReference)methodSig);
					break;

				case MethodSignatureType.DeclaringMethod:
					Visit((MethodReference)methodSig);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		public virtual void Visit(CallSite callSite)
		{
			for (int i = 0; i < callSite.Arguments.Count; i++)
			{
				Visit(callSite.Arguments[i]);
			}
		}

		public virtual void Visit(MethodReference methodRef)
		{
			Visit(methodRef.Owner);
			Visit(methodRef.CallSite);
		}

		public virtual void Visit(GenericMethodReference genericMethodRef)
		{
			Visit(genericMethodRef.DeclaringMethod);

			for (int i = 0; i < genericMethodRef.GenericArguments.Count; i++)
			{
				Visit(genericMethodRef.GenericArguments[i]);
			}
		}

		public virtual void Visit(FieldReference fieldRef)
		{
			Visit(fieldRef.FieldType);
			Visit(fieldRef.Owner);
		}
	}
}
