using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public abstract class SignatureBuilder
	{
		public virtual bool Build(ref Signature signature)
		{
			switch (signature.SignatureType)
			{
				case SignatureType.Assembly:
					{
						var assemblyRef = (AssemblyReference)signature;
						if (!Build(ref assemblyRef))
							return false;

						signature = assemblyRef;
						return true;
					}

				case SignatureType.Module:
					{
						var moduleRef = (ModuleReference)signature;
						if (!Build(ref moduleRef))
							return false;

						signature = moduleRef;
						return true;
					}

				case SignatureType.File:
					{
						var fileRef = (FileReference)signature;
						if (!Build(ref fileRef))
							return false;

						signature = fileRef;
						return true;
					}

				case SignatureType.Type:
					{
						var typeSig = (TypeSignature)signature;
						if (!Build(ref typeSig))
							return false;

						signature = typeSig;
						return true;
					}

				case SignatureType.Method:
					{
						var methodSig = (MethodSignature)signature;
						if (!Build(ref methodSig))
							return false;

						signature = methodSig;
						return true;
					}

				case SignatureType.Field:
					{
						var fieldRef = (FieldReference)signature;
						if (!Build(ref fieldRef))
							return false;

						signature = fieldRef;
						return true;
					}

				default:
					throw new InvalidOperationException();
			}
		}

		public virtual bool Build(ref AssemblyReference assemblyRef)
		{
			return false;
		}

		public virtual bool Build(ref ModuleReference moduleRef)
		{
			return false;
		}

		public virtual bool Build(ref FileReference fileRef)
		{
			return false;
		}

		public virtual bool Build(ref TypeSignature typeSig)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					{
						var arrayType = (ArrayType)typeSig;
						if (!Build(ref arrayType))
							return false;

						typeSig = arrayType;
						return true;
					}

				case TypeElementCode.ByRef:
					{
						var byRefType = (ByRefType)typeSig;
						if (!Build(ref byRefType))
							return false;

						typeSig = byRefType;
						return true;
					}

				case TypeElementCode.CustomModifier:
					{
						var customModifier = (CustomModifier)typeSig;
						if (!Build(ref customModifier))
							return false;

						typeSig = customModifier;
						return true;
					}

				case TypeElementCode.FunctionPointer:
					{
						var functionPointer = (FunctionPointer)typeSig;
						if (!Build(ref functionPointer))
							return false;

						typeSig = functionPointer;
						return true;
					}

				case TypeElementCode.GenericParameter:
					{
						var genericType = (GenericParameterType)typeSig;
						if (!Build(ref genericType))
							return false;

						typeSig = genericType;
						return true;
					}

				case TypeElementCode.GenericType:
					{
						var genericTypeRef = (GenericTypeReference)typeSig;
						if (!Build(ref genericTypeRef))
							return false;

						typeSig = genericTypeRef;
						return true;
					}

				case TypeElementCode.Pinned:
					{
						var pinnedType = (PinnedType)typeSig;
						if (!Build(ref pinnedType))
							return false;

						typeSig = pinnedType;
						return true;
					}

				case TypeElementCode.Pointer:
					{
						var pointerType = (PointerType)typeSig;
						if (!Build(ref pointerType))
							return false;

						typeSig = pointerType;
						return true;
					}

				case TypeElementCode.DeclaringType:
					{
						var typeRef = (TypeReference)typeSig;
						if (!Build(ref typeRef))
							return false;

						typeSig = typeRef;
						return true;
					}

				default:
					throw new InvalidOperationException();
			}
		}

		public virtual bool Build(ref ArrayType arrayType)
		{
			var elementType = arrayType.ElementType;
			if (!Build(ref elementType))
				return false;

			arrayType = new ArrayType(elementType, arrayType.ArrayDimensions);
			return true;
		}

		public virtual bool Build(ref ByRefType byRefType)
		{
			var elementType = byRefType.ElementType;
			if (!Build(ref elementType))
				return false;

			byRefType = new ByRefType(elementType);
			return true;
		}

		public virtual bool Build(ref CustomModifier customModifier)
		{
			bool changed = false;

			var modifier = customModifier.Modifier;
			if (modifier != null)
				changed |= Build(ref modifier);

			var elementType = customModifier.ElementType;
			changed |= Build(ref elementType);

			if (!changed)
				return false;

			customModifier = new CustomModifier(elementType, modifier, customModifier.ModifierType);
			return true;
		}

		public virtual bool Build(ref FunctionPointer functionPointer)
		{
			var callSite = functionPointer.CallSite;
			if (!Build(ref callSite))
				return false;

			functionPointer = new FunctionPointer(callSite);
			return true;
		}

		public virtual bool Build(ref GenericParameterType genericType)
		{
			return false;
		}

		public virtual bool Build(ref PinnedType pinnedType)
		{
			var elementType = pinnedType.ElementType;
			if (!Build(ref elementType))
				return false;

			pinnedType = new PinnedType(elementType);
			return true;
		}

		public virtual bool Build(ref PointerType pointerType)
		{
			var elementType = pointerType.ElementType;
			if (!Build(ref elementType))
				return false;

			pointerType = new PointerType(elementType);
			return true;
		}

		public virtual bool Build(ref TypeReference typeRef)
		{
			bool changed = false;

			var owner = typeRef.Owner;
			if (owner != null)
			{
				changed |= Build(ref owner);
			}

			if (!changed)
				return false;

			typeRef = new TypeReference(typeRef.Name, typeRef.Namespace, owner, typeRef.IsValueType);
			return true;
		}

		public virtual bool Build(ref GenericTypeReference genericTypeRef)
		{
			bool changed = false;

			var typeRef = genericTypeRef.DeclaringType;
			changed |= Build(ref typeRef);

			var genericArguments = new TypeSignature[genericTypeRef.GenericArguments.Count];
			for (int i = 0; i < genericArguments.Length; i++)
			{
				var genericArgument = genericTypeRef.GenericArguments[i];
				changed |= Build(ref genericArgument);
				genericArguments[i] = genericArgument;
			}

			if (!changed)
				return false;

			genericTypeRef = new GenericTypeReference(typeRef, genericArguments);
			return true;
		}

		public virtual bool Build(ref MethodSignature methodSig)
		{
			switch (methodSig.Type)
			{
				case MethodSignatureType.CallSite:
					{
						var callSite = (CallSite)methodSig;
						if (!Build(ref callSite))
							return false;

						methodSig = callSite;
						return true;
					}

				case MethodSignatureType.GenericMethod:
					{
						var genericMethodRef = (GenericMethodReference)methodSig;
						if (!Build(ref genericMethodRef))
							return false;

						methodSig = genericMethodRef;
						return true;
					}

				case MethodSignatureType.DeclaringMethod:
					{
						var methodRef = (MethodReference)methodSig;
						if (!Build(ref methodRef))
							return false;

						methodSig = methodRef;
						return true;
					}

				default:
					throw new InvalidOperationException();
			}
		}

		public virtual bool Build(ref CallSite callSite)
		{
			bool changed = false;

			var returnType = callSite.ReturnType;
			changed |= Build(ref returnType);

			var arguments = new TypeSignature[callSite.Arguments.Count];
			for (int i = 0; i < arguments.Length; i++)
			{
				var argument = callSite.Arguments[i];
				changed |= Build(ref argument);
				arguments[i] = argument;
			}

			if (!changed)
				return false;

			callSite = new CallSite(
				callSite.HasThis,
				callSite.ExplicitThis,
				callSite.CallConv,
				returnType,
				arguments,
				callSite.VarArgIndex,
				callSite.GenericParameterCount);

			return true;
		}

		public virtual bool Build(ref MethodReference methodRef)
		{
			bool changed = false;

			var owner = methodRef.Owner;
			changed |= Build(ref owner);

			var callSite = methodRef.CallSite;
			changed |= Build(ref callSite);

			if (!changed)
				return false;

			methodRef = new MethodReference(methodRef.Name, owner, callSite);
			return true;
		}

		public virtual bool Build(ref GenericMethodReference genericMethodRef)
		{
			bool changed = false;

			var methodRef = genericMethodRef.DeclaringMethod;
			changed |= Build(ref methodRef);

			var genericArguments = new TypeSignature[genericMethodRef.GenericArguments.Count];
			for (int i = 0; i < genericArguments.Length; i++)
			{
				var genericArgument = genericMethodRef.GenericArguments[i];
				changed |= Build(ref genericArgument);
				genericArguments[i] = genericArgument;
			}

			if (!changed)
				return false;

			genericMethodRef = new GenericMethodReference(methodRef, genericArguments);
			return true;
		}

		public virtual bool Build(ref FieldReference fieldRef)
		{
			bool changed = false;

			var fieldType = fieldRef.FieldType;
			changed |= Build(ref fieldType);

			var owner = fieldRef.Owner;
			changed |= Build(ref owner);

			if (!changed)
				return false;

			fieldRef = new FieldReference(fieldRef.Name, fieldType, owner);
			return true;
		}

		#region Assembly

		public virtual void Build(Assembly assembly)
		{
			Build(assembly.Modules);
			Build(assembly.CustomAttributes);
			Build(assembly.SecurityAttributes);
		}

		public virtual void Build(AssemblyReferenceCollection assemblyReferences)
		{
			for (int i = 0; i < assemblyReferences.Count; i++)
			{
				var assemblyRef = assemblyReferences[i];
				if (Build(ref assemblyRef))
				{
					assemblyReferences[i] = assemblyRef;
				}
			}
		}

		#endregion

		#region Module

		public virtual void Build(ModuleCollection modules)
		{
			foreach (var module in modules)
			{
				Build(module);
			}
		}

		public virtual void Build(Module module)
		{
			if (module.EntryPoint != null)
			{
				var entryPointSig = module.EntryPoint;
				if (Build(ref entryPointSig))
					module.EntryPoint = entryPointSig;
			}

			Build(module.AssemblyReferences);
			Build(module.ModuleReferences);
			Build(module.Files);
			Build(module.ExportedTypes);
			Build(module.Resources);
			Build(module.Types);
			Build(module.CustomAttributes);
		}

		public virtual void Build(ModuleReferenceCollection moduleReferences)
		{
			for (int i = 0; i < moduleReferences.Count; i++)
			{
				var moduleRef = moduleReferences[i];
				if (Build(ref moduleRef))
				{
					moduleReferences[i] = moduleRef;
				}
			}
		}

		#endregion

		#region File

		public virtual void Build(FileReferenceCollection fileReferences)
		{
			for (int i = 0; i < fileReferences.Count; i++)
			{
				var fileRef = fileReferences[i];
				if (Build(ref fileRef))
				{
					fileReferences[i] = fileRef;
				}
			}
		}

		#endregion

		#region Type

		public virtual void Build(TypeDeclarationCollection types)
		{
			foreach (var type in types)
			{
				Build(type);
			}
		}

		public virtual void Build(TypeDeclaration type)
		{
			if (type.BaseType != null)
			{
				var baseType = type.BaseType;
				if (Build(ref baseType))
				{
					type.BaseType = baseType;
				}
			}

			Build(type.Interfaces);
			Build(type.GenericParameters);
			Build(type.NestedTypes);
			Build(type.Methods);
			Build(type.Fields);
			Build(type.Properties);
			Build(type.Events);
			Build(type.CustomAttributes);
			Build(type.SecurityAttributes);
		}

		public virtual void Build(TypeInterfaceCollection typeInterfaces)
		{
			for (int i = 0; i < typeInterfaces.Count; i++)
			{
				var typeInterface = typeInterfaces[i];
				if (Build(ref typeInterface))
				{
					typeInterfaces[i] = typeInterface;
				}
			}
		}

		public virtual void Build(ExportedTypeCollection exportedTypes)
		{
			for (int i = 0; i < exportedTypes.Count; i++)
			{
				var exportedType = exportedTypes[i];
				if (Build(ref exportedType))
				{
					exportedTypes[i] = exportedType;
				}
			}
		}

		#endregion

		#region Method

		public virtual void Build(MethodDeclarationCollection methods)
		{
			foreach (var method in methods)
			{
				Build(method);
			}
		}

		public virtual void Build(MethodDeclaration method)
		{
			Build(method.ReturnType);

			if (method.PInvoke != null)
				Build(method.PInvoke);

			Build(method.Parameters);
			Build(method.GenericParameters);
			Build(method.Overrides);
			Build(method.CustomAttributes);
			Build(method.SecurityAttributes);

			if (MethodBody.IsValid(method))
			{
				var body = MethodBody.Load(method);
				if (Build(body))
				{
					body.Build(method);
				}
			}
		}

		public virtual void Build(MethodReturnType returnType)
		{
			var type = returnType.Type;
			if (Build(ref type))
				returnType.Type = type;

			if (returnType.MarshalType != null)
				Build(returnType.MarshalType);

			Build(returnType.CustomAttributes);
		}

		public virtual void Build(MethodParameterCollection parameters)
		{
			foreach (var parameter in parameters)
			{
				Build(parameter);
			}
		}

		public virtual void Build(MethodParameter parameter)
		{
			var type = parameter.Type;
			if (Build(ref type))
				parameter.Type = type;

			if (parameter.MarshalType != null)
				Build(parameter.MarshalType);

			Build(parameter.CustomAttributes);
		}

		public virtual void Build(MethodOverrideCollection overrides)
		{
			for (int i = 0; i < overrides.Count; i++)
			{
				var methodOverride = overrides[i];
				if (Build(ref methodOverride))
				{
					overrides[i] = methodOverride;
				}
			}
		}

		public virtual bool Build(MethodBody methodBody)
		{
			bool changed = false;

			var localVariables = methodBody.LocalVariables;
			for (int i = 0; i < localVariables.Count; i++)
			{
				var localVariable = localVariables[i];
				if (Build(ref localVariable))
				{
					localVariables[i] = localVariable;
					changed = true;
				}
			}

			foreach (var eh in methodBody.ExceptionHandlers)
			{
				if (eh.CatchType != null)
				{
					var catchType = eh.CatchType;
					if (Build(ref catchType))
					{
						eh.CatchType = catchType;
						changed = true;
					}
				}
			}

			var instructions = methodBody.Instructions;
			for (int i = 0; i < instructions.Count; i++)
			{
				var instruction = instructions[i];
				var signature = instruction.Value as Signature;
				if (signature != null)
				{
					if (Build(ref signature))
					{
						instructions[i] = new Instruction(instruction.OpCode, signature);
						changed = true;
					}
				}
			}

			return changed;
		}

		#endregion

		#region Field

		public virtual void Build(FieldDeclarationCollection fields)
		{
			foreach (var field in fields)
			{
				Build(field);
			}
		}

		public virtual void Build(FieldDeclaration field)
		{
			var fieldType = field.FieldType;
			if (Build(ref fieldType))
				field.FieldType = fieldType;

			if (field.MarshalFieldType != null)
				Build(field.MarshalFieldType);

			Build(field.CustomAttributes);
		}

		#endregion

		#region Property

		public virtual void Build(PropertyDeclarationCollection properties)
		{
			foreach (var property in properties)
			{
				Build(property);
			}
		}

		public virtual void Build(PropertyDeclaration property)
		{
			var returnType = property.ReturnType;
			if (Build(ref returnType))
				property.ReturnType = returnType;

			Build(property.Parameters);

			if (property.GetMethod != null)
			{
				var methodRef = property.GetMethod;
				if (Build(ref methodRef))
					property.GetMethod = methodRef;
			}

			if (property.SetMethod != null)
			{
				var methodRef = property.SetMethod;
				if (Build(ref methodRef))
					property.SetMethod = methodRef;
			}

			Build(property.CustomAttributes);
		}

		public virtual void Build(PropertyParameterCollection parameters)
		{
			for (int i = 0; i < parameters.Count; i++)
			{
				var parameter = parameters[i];
				if (Build(ref parameter))
				{
					parameters[i] = parameter;
				}
			}
		}

		#endregion

		#region Event

		public virtual void Build(EventDeclarationCollection events)
		{
			foreach (var eventDecl in events)
			{
				Build(eventDecl);
			}
		}

		public virtual void Build(EventDeclaration eventDecl)
		{
			var eventType = eventDecl.EventType;
			if (Build(ref eventType))
				eventDecl.EventType = eventType;

			if (eventDecl.AddMethod != null)
			{
				var methodRef = eventDecl.AddMethod;
				if (Build(ref methodRef))
					eventDecl.AddMethod = methodRef;
			}

			if (eventDecl.RemoveMethod != null)
			{
				var methodRef = eventDecl.RemoveMethod;
				if (Build(ref methodRef))
					eventDecl.RemoveMethod = methodRef;
			}

			if (eventDecl.InvokeMethod != null)
			{
				var methodRef = eventDecl.InvokeMethod;
				if (Build(ref methodRef))
					eventDecl.InvokeMethod = methodRef;
			}

			Build(eventDecl.CustomAttributes);
		}

		#endregion

		#region Generic

		public virtual void Build(GenericParameterCollection genericParameters)
		{
			foreach (var genericParameter in genericParameters)
			{
				Build(genericParameter);
			}
		}

		public virtual void Build(GenericParameter genericParameter)
		{
			Build(genericParameter.Constraints);
			Build(genericParameter.CustomAttributes);
		}

		public virtual void Build(GenericParameterConstraintCollection constraints)
		{
			for (int i = 0; i < constraints.Count; i++)
			{
				var constraint = constraints[i];
				if (Build(ref constraint))
				{
					constraints[i] = constraint;
				}
			}
		}

		#endregion

		#region PInvoke

		public virtual void Build(PInvoke pinvoke)
		{
			if (pinvoke.ImportScope != null)
			{
				var importScope = pinvoke.ImportScope;
				if (Build(ref importScope))
					pinvoke.ImportScope = importScope;
			}
		}

		#endregion

		#region Marshal

		public virtual void Build(MarshalType marshalType)
		{
		}

		#endregion

		#region Resource

		public virtual void Build(ResourceCollection resources)
		{
			foreach (var resource in resources)
			{
				Build(resource);
			}
		}

		public virtual void Build(Resource resource)
		{
			Build(resource.CustomAttributes);
		}

		#endregion

		#region CustomAttribute

		public virtual void Build(CustomAttributeCollection customAttributes)
		{
			foreach (var customAttribute in customAttributes)
			{
				Build(customAttribute);
			}
		}

		public virtual void Build(CustomAttribute customAttribute)
		{
			var constructor = customAttribute.Constructor;
			if (Build(ref constructor))
				customAttribute.Constructor = constructor;

			Build(customAttribute.CtorArguments);
			Build(customAttribute.NamedArguments);
		}

		public virtual void Build(CustomAttributeCtorArgumentCollection arguments)
		{
			for (int i = 0; i < arguments.Count; i++)
			{
				var argument = arguments[i];
				if (Build(ref argument))
				{
					arguments[i] = argument;
				}
			}
		}

		public virtual bool Build(ref CustomAttributeTypedArgument argument)
		{
			bool changed = true;

			var type = argument.Type;
			changed |= Build(ref type);

			object value = argument.Value;

			if (value != null)
			{
				if (value is Signature)
				{
					var sigValue = (Signature)value;
					if (Build(ref sigValue))
					{
						value = sigValue;
						changed = true;
					}
				}
				else if (value is CustomAttributeTypedArgument)
				{
					var typedValue = (CustomAttributeTypedArgument)value;
					if (Build(ref typedValue))
					{
						value = typedValue;
						changed = true;
					}
				}
				else if (value is Signature[])
				{
					var array = (Signature[])value;
					for (int i = 0; i < array.Length; i++)
					{
						var sigValue = array[i];
						if (sigValue != null)
						{
							if (Build(ref sigValue))
							{
								array[i] = sigValue;
								changed = true;
							}
						}
					}

					if (changed)
						value = array;
				}
				else if (value is CustomAttributeTypedArgument[])
				{
					var array = (CustomAttributeTypedArgument[])value;
					for (int i = 0; i < array.Length; i++)
					{
						var typedValue = array[i];
						if (Build(ref typedValue))
						{
							array[i] = typedValue;
							changed = true;
						}
					}

					if (changed)
						value = array;
				}
			}

			if (!changed)
				return false;

			argument = new CustomAttributeTypedArgument(value, type);
			return true;
		}

		public virtual void Build(CustomAttributeNamedArgumentCollection arguments)
		{
			for (int i = 0; i < arguments.Count; i++)
			{
				var argument = arguments[i];
				if (Build(ref argument))
				{
					arguments[i] = argument;
				}
			}
		}

		public virtual bool Build(ref CustomAttributeNamedArgument argument)
		{
			var typedValue = argument.TypedValue;
			if (!Build(ref typedValue))
				return false;

			argument = new CustomAttributeNamedArgument(argument.Name, argument.Type, typedValue);
			return true;
		}

		#endregion

		#region SecurityAttribute

		public virtual void Build(SecurityAttributeCollection securityAttributes)
		{
			foreach (var securityAttribute in securityAttributes)
			{
				Build(securityAttribute);
			}
		}

		public virtual void Build(SecurityAttribute securityAttribute)
		{
			if (securityAttribute.Type != null)
			{
				var type = securityAttribute.Type;
				if (Build(ref type))
					securityAttribute.Type = type;
			}

			Build(securityAttribute.NamedArguments);
		}

		#endregion

		#region Misc

		public void Build(TypeSignature[] array)
		{
			Build(array, 0, array.Length);
		}

		public void Build(TypeSignature[] array, int index, int count)
		{
			for (int i = index; i < count; i++)
			{
				var item = array[i];
				if (Build(ref item))
				{
					array[i] = item;
				}
			}
		}

		#endregion
	}
}
