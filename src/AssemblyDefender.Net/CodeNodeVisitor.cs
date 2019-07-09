using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public abstract class CodeNodeVisitor
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

		public virtual void Visit(MemberNode entity)
		{
			switch (entity.MemberType)
			{
				case MemberType.Type:
					Visit((TypeDeclaration)entity);
					break;

				case MemberType.Method:
					Visit((MethodDeclaration)entity);
					break;

				case MemberType.Field:
					Visit((FieldDeclaration)entity);
					break;

				case MemberType.Property:
					Visit((PropertyDeclaration)entity);
					break;

				case MemberType.Event:
					Visit((EventDeclaration)entity);
					break;

				case MemberType.Resource:
					Visit((Resource)entity);
					break;

				case MemberType.CustomAttribute:
					Visit((CustomAttribute)entity);
					break;

				case MemberType.SecurityAttribute:
					Visit((SecurityAttribute)entity);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		#region Assembly

		public virtual void Visit(Assembly assembly)
		{
			Visit(assembly.Modules);
			Visit(assembly.CustomAttributes);
			Visit(assembly.SecurityAttributes);
		}

		public virtual void Visit(AssemblyReferenceCollection assemblyReferences)
		{
			for (int i = 0; i < assemblyReferences.Count; i++)
			{
				Visit(assemblyReferences[i]);
			}
		}

		public virtual void Visit(AssemblyReference assemblyRef)
		{
		}

		#endregion

		#region Module

		public virtual void Visit(ModuleCollection modules)
		{
			foreach (var module in modules)
			{
				Visit(module);
			}
		}

		public virtual void Visit(Module module)
		{
			if (module.EntryPoint != null)
				Visit(module.EntryPoint);

			Visit(module.AssemblyReferences);
			Visit(module.ModuleReferences);
			Visit(module.Files);
			Visit(module.ExportedTypes);
			Visit(module.Resources);
			Visit(module.Types);
			Visit(module.CustomAttributes);
		}

		public virtual void Visit(ModuleReferenceCollection moduleReferences)
		{
			for (int i = 0; i < moduleReferences.Count; i++)
			{
				Visit(moduleReferences[i]);
			}
		}

		public virtual void Visit(ModuleReference moduleRef)
		{
		}

		#endregion

		#region File

		public virtual void Visit(FileReferenceCollection fileReferences)
		{
			for (int i = 0; i < fileReferences.Count; i++)
			{
				Visit(fileReferences[i]);
			}
		}

		public virtual void Visit(FileReference fileRef)
		{
		}

		#endregion

		#region Type

		public virtual void Visit(TypeDeclarationCollection types)
		{
			foreach (var type in types)
			{
				Visit(type);
			}
		}

		public virtual void Visit(TypeDeclaration type)
		{
			if (type.BaseType != null)
				Visit(type.BaseType);

			Visit(type.Interfaces);
			Visit(type.GenericParameters);
			Visit(type.CustomAttributes);
			Visit(type.SecurityAttributes);
			Visit(type.Methods);
			Visit(type.Fields);
			Visit(type.Properties);
			Visit(type.Events);
			Visit(type.NestedTypes);
		}

		public virtual void Visit(TypeInterfaceCollection typeInterfaces)
		{
			for (int i = 0; i < typeInterfaces.Count; i++)
			{
				Visit(typeInterfaces[i]);
			}
		}

		public virtual void Visit(ExportedTypeCollection exportedTypes)
		{
			for (int i = 0; i < exportedTypes.Count; i++)
			{
				Visit(exportedTypes[i]);
			}
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
			Visit(customModifier.Modifier);
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
				Visit(typeRef.Owner);
		}

		public virtual void Visit(GenericTypeReference genericTypeRef)
		{
			Visit(genericTypeRef.DeclaringType);

			var genericArguments = genericTypeRef.GenericArguments;
			for (int i = 0; i < genericArguments.Count; i++)
			{
				Visit(genericArguments[i]);
			}
		}

		#endregion

		#region Method

		public virtual void Visit(MethodDeclarationCollection methods)
		{
			foreach (var method in methods)
			{
				Visit(method);
			}
		}

		public virtual void Visit(MethodDeclaration method)
		{
			Visit(method.ReturnType);

			if (method.PInvoke != null)
				Visit(method.PInvoke);

			Visit(method.Parameters);
			Visit(method.GenericParameters);
			Visit(method.Overrides);
			Visit(method.CustomAttributes);
			Visit(method.SecurityAttributes);

			if (MethodBody.IsValid(method))
			{
				var body = MethodBody.Load(method);
				Visit(body, method);
			}
		}

		public virtual void Visit(MethodReturnType returnType)
		{
			Visit(returnType.Type);

			if (returnType.MarshalType != null)
				Visit(returnType.MarshalType);

			Visit(returnType.CustomAttributes);
		}

		public virtual void Visit(MethodParameterCollection parameters)
		{
			foreach (var parameter in parameters)
			{
				Visit(parameter);
			}
		}

		public virtual void Visit(MethodParameter parameter)
		{
			Visit(parameter.Type);

			if (parameter.MarshalType != null)
				Visit(parameter.MarshalType);

			Visit(parameter.CustomAttributes);
		}

		public virtual void Visit(MethodOverrideCollection overrides)
		{
			for (int i = 0; i < overrides.Count; i++)
			{
				Visit(overrides[i]);
			}
		}

		public virtual void Visit(MethodBody methodBody, MethodDeclaration method)
		{
			var localVariables = methodBody.LocalVariables;
			for (int i = 0; i < localVariables.Count; i++)
			{
				Visit(localVariables[i]);
			}

			foreach (var eh in methodBody.ExceptionHandlers)
			{
				if (eh.CatchType != null)
					Visit(eh.CatchType);
			}

			var instructions = methodBody.Instructions;
			for (int i = 0; i < instructions.Count; i++)
			{
				var instruction = instructions[i];
				var signature = instruction.Value as Signature;
				if (signature != null)
					Visit(signature);
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
			Visit(callSite.ReturnType);

			var arguments = callSite.Arguments;
			for (int i = 0; i < arguments.Count; i++)
			{
				Visit(arguments[i]);
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

			var genericArguments = genericMethodRef.GenericArguments;
			for (int i = 0; i < genericArguments.Count; i++)
			{
				Visit(genericArguments[i]);
			}
		}

		#endregion

		#region Field

		public virtual void Visit(FieldDeclarationCollection fields)
		{
			foreach (var field in fields)
			{
				Visit(field);
			}
		}

		public virtual void Visit(FieldDeclaration field)
		{
			Visit(field.FieldType);

			if (field.MarshalFieldType != null)
				Visit(field.MarshalFieldType);

			Visit(field.CustomAttributes);
		}

		public virtual void Visit(FieldReference fieldRef)
		{
			Visit(fieldRef.FieldType);
			Visit(fieldRef.Owner);
		}

		#endregion

		#region Property

		public virtual void Visit(PropertyDeclarationCollection properties)
		{
			foreach (var property in properties)
			{
				Visit(property);
			}
		}

		public virtual void Visit(PropertyDeclaration property)
		{
			Visit(property.ReturnType);
			Visit(property.Parameters);

			if (property.GetMethod != null)
				Visit(property.GetMethod);

			if (property.SetMethod != null)
				Visit(property.SetMethod);

			Visit(property.CustomAttributes);
		}

		public virtual void Visit(PropertyParameterCollection parameters)
		{
			for (int i = 0; i < parameters.Count; i++)
			{
				Visit(parameters[i]);
			}
		}

		#endregion

		#region Event

		public virtual void Visit(EventDeclarationCollection events)
		{
			foreach (var eventDecl in events)
			{
				Visit(eventDecl);
			}
		}

		public virtual void Visit(EventDeclaration eventDecl)
		{
			Visit(eventDecl.EventType);

			if (eventDecl.AddMethod != null)
				Visit(eventDecl.AddMethod);

			if (eventDecl.RemoveMethod != null)
				Visit(eventDecl.RemoveMethod);

			if (eventDecl.InvokeMethod != null)
				Visit(eventDecl.InvokeMethod);

			Visit(eventDecl.CustomAttributes);
		}

		#endregion

		#region Generic

		public virtual void Visit(GenericParameterCollection genericParameters)
		{
			foreach (var genericParameter in genericParameters)
			{
				Visit(genericParameter);
			}
		}

		public virtual void Visit(GenericParameter genericParameter)
		{
			Visit(genericParameter.Constraints);
			Visit(genericParameter.CustomAttributes);
		}

		public virtual void Visit(GenericParameterConstraintCollection constraints)
		{
			for (int i = 0; i < constraints.Count; i++)
			{
				Visit(constraints[i]);
			}
		}

		#endregion

		#region PInvoke

		public virtual void Visit(PInvoke pinvoke)
		{
			if (pinvoke.ImportScope != null)
			{
				Visit(pinvoke.ImportScope);
			}
		}

		#endregion

		#region Marshal

		public virtual void Visit(MarshalType marshalType)
		{
		}

		#endregion

		#region Resource

		public virtual void Visit(ResourceCollection resources)
		{
			foreach (var resource in resources)
			{
				Visit(resource);
			}
		}

		public virtual void Visit(Resource resource)
		{
			Visit(resource.CustomAttributes);
		}

		#endregion

		#region CustomAttribute

		public virtual void Visit(CustomAttributeCollection customAttributes)
		{
			foreach (var customAttribute in customAttributes)
			{
				Visit(customAttribute);
			}
		}

		public virtual void Visit(CustomAttribute customAttribute)
		{
			Visit(customAttribute.Constructor);
			Visit(customAttribute.CtorArguments);
			Visit(customAttribute.NamedArguments);
		}

		public virtual void Visit(CustomAttributeCtorArgumentCollection arguments)
		{
			for (int i = 0; i < arguments.Count; i++)
			{
				Visit(arguments[i]);
			}
		}

		public virtual void Visit(CustomAttributeTypedArgument argument)
		{
			Visit(argument.Type);

			object value = argument.Value;

			if (value != null)
			{
				if (value is Signature)
				{
					Visit((Signature)value);
				}
				else if (value is CustomAttributeTypedArgument)
				{
					Visit((CustomAttributeTypedArgument)value);
				}
				else if (value is Signature[])
				{
					var array = (Signature[])value;
					for (int i = 0; i < array.Length; i++)
					{
						var sigValue = array[i];
						if (sigValue != null)
							Visit(sigValue);
					}
				}
				else if (value is CustomAttributeTypedArgument[])
				{
					var array = (CustomAttributeTypedArgument[])value;
					for (int i = 0; i < array.Length; i++)
					{
						Visit(array[i]);
					}
				}
			}
		}

		public virtual void Visit(CustomAttributeNamedArgumentCollection arguments)
		{
			for (int i = 0; i < arguments.Count; i++)
			{
				Visit(arguments[i]);
			}
		}

		public virtual void Visit(CustomAttributeNamedArgument argument)
		{
			Visit(argument.TypedValue);
		}

		#endregion

		#region SecurityAttribute

		public virtual void Visit(SecurityAttributeCollection securityAttributes)
		{
			foreach (var securityAttribute in securityAttributes)
			{
				Visit(securityAttribute);
			}
		}

		public virtual void Visit(SecurityAttribute securityAttribute)
		{
			if (securityAttribute.Type != null)
				Visit(securityAttribute.Type);

			Visit(securityAttribute.NamedArguments);
		}

		#endregion
	}
}
