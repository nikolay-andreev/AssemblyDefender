using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Net.IL;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.Resources;

namespace AssemblyDefender
{
	public class StripAnalyzer
	{
		#region Fields

		private BuildAssembly _assembly;
		private Queue<ICodeNode> _queue;

		#endregion

		#region Ctors

		public StripAnalyzer(BuildAssembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_assembly = assembly;
			_queue = new Queue<ICodeNode>();
		}

		#endregion

		#region Methods

		public void Analyze()
		{
			foreach (BuildModule module in _assembly.Modules)
			{
				Analyze(module);
			}

			// Process references
			var primeModule = _assembly.Module;

			var entryPointSig = _assembly.EntryPoint;
			if (entryPointSig != null && entryPointSig.SignatureType == SignatureType.Method)
			{
				Process((MethodSignature)_assembly.EntryPoint, primeModule, true);
			}

			Process(_assembly.ExportedTypes, primeModule);
			Process(_assembly.CustomAttributes, primeModule);
			Process(_assembly.SecurityAttributes, primeModule);

			// Process queue
			while (_queue.Count > 0)
			{
				Process(_queue.Dequeue());
			}
		}

		private void Analyze(BuildModule module)
		{
			Process(module.CustomAttributes, module);

			foreach (BuildType type in module.Types)
			{
				Analyze(type);
			}
		}

		private void Analyze(BuildType type)
		{
			if (type.Strip)
			{
				type.Strip = CanStrip(type);
			}

			if (!type.Strip)
			{
				Enqueue(type);
			}

			foreach (BuildMethod method in type.Methods)
			{
				if (!method.Strip)
				{
					Enqueue(method);
				}
			}

			foreach (BuildField field in type.Fields)
			{
				if (!field.Strip)
				{
					Enqueue(field);
				}
			}

			foreach (BuildProperty property in type.Properties)
			{
				if (!property.Strip)
				{
					Enqueue(property);
				}
			}

			foreach (BuildEvent e in type.Events)
			{
				if (!e.Strip)
				{
					Enqueue(e);
				}
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Analyze(nestedType);
			}
		}

		private void Enqueue(BuildType type)
		{
			if (!type.StripProcessed)
			{
				type.StripProcessed = true;
				_queue.Enqueue(type);
			}
		}

		private void Enqueue(BuildMethod method)
		{
			if (!method.StripProcessed)
			{
				method.StripProcessed = true;
				_queue.Enqueue(method);
			}
		}

		private void Enqueue(BuildField field)
		{
			if (!field.StripProcessed)
			{
				field.StripProcessed = true;
				_queue.Enqueue(field);
			}
		}

		private void Enqueue(BuildProperty property)
		{
			if (!property.StripProcessed)
			{
				property.StripProcessed = true;
				_queue.Enqueue(property);
			}
		}

		private void Enqueue(BuildEvent e)
		{
			if (!e.StripProcessed)
			{
				e.StripProcessed = true;
				_queue.Enqueue(e);
			}
		}

		private void UnstripAndEnqueue(BuildType type)
		{
			if (type.Strip)
			{
				type.Strip = false;
				Enqueue(type);
			}
		}

		private void UnstripAndEnqueue(BuildMethod method)
		{
			if (method.Strip)
			{
				method.Strip = false;
				Enqueue(method);
			}
		}

		private void UnstripAndEnqueueEncrypted(BuildMethod method)
		{
			if (!method.Strip || method.StripEncrypted)
				return;

			if (method.EncryptIL && !method.IsVirtual)
			{
				method.StripEncrypted = true;
			}
			else
			{
				method.Strip = false;
			}

			Enqueue(method);
		}

		private void UnstripAndEnqueue(BuildField field)
		{
			if (field.Strip)
			{
				field.Strip = false;
				Enqueue(field);
			}
		}

		private void UnstripAndEnqueue(BuildProperty property)
		{
			if (property.Strip)
			{
				property.Strip = false;
				Enqueue(property);
			}
		}

		private void UnstripAndEnqueue(BuildEvent e)
		{
			if (e.Strip)
			{
				e.Strip = false;
				Enqueue(e);
			}
		}

		private bool CanStrip(BuildType type)
		{
			if (type.IsGlobal())
				return false;

			return true;
		}

		private bool CanStrip(BuildMethod method)
		{
			if (method.IsVirtual)
				return false;

			if (method.IsConstructor())
				return false;

			if (method.CodeType != MethodCodeTypeFlags.CIL)
				return false;

			return true;
		}

		private bool CanStripCall(MethodSignature methodSig, OpCode opCode, List<Instruction> instructions, int index)
		{
			if (opCode.Type == OpCodeType.Ldftn)
				return false;

			int prevIndex = index - 1;
			if (prevIndex >= 0)
			{
				if (instructions[prevIndex].OpCode.Type == OpCodeType.Prefix)
					return false;
			}

			return true;
		}

		private void MapCustomAttributeArguments(CustomAttribute customAttribute, Module module)
		{
			var arguments = customAttribute.NamedArguments;
			if (arguments.Count == 0)
				return;

			var constructor = customAttribute.Constructor;
			if (constructor == null)
				return;

			var type = constructor.Owner.Resolve(module, false) as BuildType;
			if (type == null)
				return;

			MapCustomAttributeArguments(arguments, type);
		}

		private void MapCustomAttributeArguments(CustomAttributeNamedArgumentCollection arguments, BuildType type)
		{
			for (int i = 0; i < arguments.Count; i++)
			{
				var argument = arguments[i];
				switch (argument.Type)
				{
					case CustomAttributeNamedArgumentType.Field:
						{
							var field = type.Fields.Find(argument.Name) as BuildField;
							if (field != null)
							{
								UnstripAndEnqueue(field);
							}
						}
						break;

					case CustomAttributeNamedArgumentType.Property:
						{
							var property = type.Properties.Find(argument.Name) as BuildProperty;
							if (property != null)
							{
								UnstripAndEnqueue(property);
							}
						}
						break;

					default:
						throw new InvalidOperationException();
				}
			}
		}

		private void MapSecurityAttributeArguments(SecurityAttribute securityAttribute, Module module)
		{
			var arguments = securityAttribute.NamedArguments;
			if (arguments.Count == 0)
				return;

			var typeSig = securityAttribute.Type;
			if (typeSig == null)
				return;

			var type = typeSig.Resolve(module, false) as BuildType;
			if (type == null)
				return;

			MapCustomAttributeArguments(arguments, type);
		}

		private void Process(ICodeNode node)
		{
			switch (node.EntityType)
			{
				case EntityType.Type:
					Process((BuildType)node);
					break;

				case EntityType.Method:
					Process((BuildMethod)node);
					break;

				case EntityType.Field:
					Process((BuildField)node);
					break;

				case EntityType.Property:
					Process((BuildProperty)node);
					break;

				case EntityType.Event:
					Process((BuildEvent)node);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private void Process(BuildType type)
		{
			if (type.IsNested)
			{
				UnstripAndEnqueue((BuildType)type.GetEnclosingType());
			}

			// Enqueue members dependent on this type.
			// .ctor, .cctor, virtual methods, enum fields, ...
			foreach (BuildMethod method in type.Methods)
			{
				if (method.Strip && !CanStrip(method))
				{
					UnstripAndEnqueue(method);
				}
			}

			if (type.IsValueType())
			{
				foreach (BuildField field in type.Fields)
				{
					UnstripAndEnqueue(field);
				}
			}

			// Process references
			var module = type.Module;

			if (type.BaseType != null)
				Process(type.BaseType, module);

			Process(type.Interfaces, module);
			Process(type.GenericParameters, module);
			Process(type.CustomAttributes, module);
			Process(type.SecurityAttributes, module);
		}

		private void Process(BuildMethod method)
		{
			UnstripAndEnqueue((BuildType)method.GetOwnerType());

			// Process references
			var module = method.Module;
			Process(method.ReturnType, module);
			Process(method.Parameters, module);
			Process(method.GenericParameters, module);
			Process(method.Overrides, module);
			Process(method.CustomAttributes, module);
			Process(method.SecurityAttributes, module);

			if (MethodBody.IsValid(method))
			{
				var body = MethodBody.Load(method);
				Process(body, module);
			}
		}

		private void Process(BuildField field)
		{
			UnstripAndEnqueue((BuildType)field.GetOwnerType());

			// Process references
			var module = field.Module;
			Process(field.FieldType, module);
			Process(field.CustomAttributes, module);
		}

		private void Process(BuildProperty property)
		{
			UnstripAndEnqueue((BuildType)property.GetOwnerType());

			// Process references
			var module = property.Module;
			Process(property.ReturnType, module);
			Process(property.Parameters, module);

			if (property.GetMethod != null)
				Process(property.GetMethod, module, true);

			if (property.SetMethod != null)
				Process(property.SetMethod, module, true);

			Process(property.CustomAttributes, module);
		}

		private void Process(BuildEvent e)
		{
			UnstripAndEnqueue((BuildType)e.GetOwnerType());

			// Process references
			var module = e.Module;
			Process(e.EventType, module);

			if (e.AddMethod != null)
				Process(e.AddMethod, module, true);

			if (e.RemoveMethod != null)
				Process(e.RemoveMethod, module, true);

			if (e.InvokeMethod != null)
				Process(e.InvokeMethod, module, true);

			Process(e.CustomAttributes, module);
		}

		private void Process(Signature signature, Module module)
		{
			switch (signature.SignatureType)
			{
				case SignatureType.Type:
					Process((TypeSignature)signature, module);
					break;

				case SignatureType.Method:
					Process((MethodSignature)signature, module);
					break;

				case SignatureType.Field:
					Process((FieldReference)signature, module);
					break;
			}
		}

		private void Process(TypeSignature typeSig, Module module)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					{
						Process(typeSig.ElementType, module);
					}
					break;

				case TypeElementCode.ByRef:
					{
						Process(typeSig.ElementType, module);
					}
					break;

				case TypeElementCode.CustomModifier:
					{
						CustomModifierType modifierType;
						var modifier = typeSig.GetCustomModifier(out modifierType);
						Process(modifier, module);
						Process(typeSig.ElementType, module);
					}
					break;

				case TypeElementCode.FunctionPointer:
					{
						Process(typeSig.GetFunctionPointer(), module);
					}
					break;

				case TypeElementCode.GenericParameter:
					break;

				case TypeElementCode.GenericType:
					{
						Process(typeSig.DeclaringType, module);
						Process(typeSig.GenericArguments, module);
					}
					break;

				case TypeElementCode.Pinned:
					{
						Process(typeSig.ElementType, module);
					}
					break;

				case TypeElementCode.Pointer:
					{
						Process(typeSig.ElementType, module);
					}
					break;

				case TypeElementCode.DeclaringType:
					{
						var type = typeSig.Resolve(module, true).DeclaringType as BuildType;
						if (type != null)
						{
							UnstripAndEnqueue(type);
						}
					}
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private void Process(IEnumerable<TypeSignature> typeSigs, Module module)
		{
			foreach (var typeSig in typeSigs)
			{
				Process(typeSig, module);
			}
		}

		private void Process(MethodSignature methodSig, Module module, bool forceUnstrip = false)
		{
			switch (methodSig.Type)
			{
				case MethodSignatureType.CallSite:
					{
						Process(methodSig.ReturnType, module);
						Process(methodSig.Arguments, module);
					}
					break;

				case MethodSignatureType.DeclaringMethod:
					{
						Process(methodSig.CallSite, module);

						var method = methodSig.Resolve(module);
						if (method != null)
						{
							var buildMethod = method.DeclaringMethod as BuildMethod;
							if (buildMethod != null)
							{
								if (forceUnstrip)
									UnstripAndEnqueue(buildMethod);
								else
									UnstripAndEnqueueEncrypted(buildMethod);
							}
						}
						else
						{
							Process(methodSig.Owner, module);
						}
					}
					break;

				case MethodSignatureType.GenericMethod:
					{
						Process(methodSig.DeclaringMethod, module);
						Process(methodSig.GenericArguments, module);
					}
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private void Process(IEnumerable<MethodSignature> methodSigs, Module module)
		{
			foreach (var methodSig in methodSigs)
			{
				Process(methodSig, module);
			}
		}

		private void Process(FieldReference fieldRef, Module module)
		{
			var field = fieldRef.Resolve(module, true).DeclaringField as BuildField;
			if (field != null)
			{
				UnstripAndEnqueue(field);
			}
		}

		private void Process(MethodBody methodBody, Module module)
		{
			var localVariables = methodBody.LocalVariables;
			for (int i = 0; i < localVariables.Count; i++)
			{
				Process(localVariables[i], module);
			}

			foreach (var eh in methodBody.ExceptionHandlers)
			{
				if (eh.CatchType != null)
				{
					Process(eh.CatchType, module);
				}
			}

			var instructions = methodBody.Instructions;
			for (int i = 0; i < instructions.Count; i++)
			{
				var instruction = instructions[i];

				var value = instruction.Value;
				if (value == null)
					continue;

				var signature = instruction.Value as Signature;
				if (signature != null)
				{
					switch (signature.SignatureType)
					{
						case SignatureType.Type:
							Process((TypeSignature)signature, module);
							break;

						case SignatureType.Method:
							{
								var methodSig = (MethodSignature)signature;
								bool forceUnstrip = !CanStripCall(methodSig, instruction.OpCode, instructions, i);
								Process(methodSig, module, forceUnstrip);
							}
							break;

						case SignatureType.Field:
							Process((FieldReference)signature, module);
							break;
					}
				}
			}
		}

		private void Process(MethodReturnType returnType, Module module)
		{
			Process(returnType.Type, module);
			Process(returnType.CustomAttributes, module);
		}

		private void Process(MethodParameter parameter, Module module)
		{
			Process(parameter.Type, module);
			Process(parameter.CustomAttributes, module);
		}

		private void Process(IEnumerable<MethodParameter> parameters, Module module)
		{
			foreach (var parameter in parameters)
			{
				Process(parameter, module);
			}
		}

		private void Process(GenericParameter genericParameter, Module module)
		{
			foreach (var typeSig in genericParameter.Constraints)
			{
				Process(typeSig, module);
			}

			Process(genericParameter.CustomAttributes, module);
		}

		private void Process(IEnumerable<GenericParameter> genericParameters, Module module)
		{
			foreach (var genericParameter in genericParameters)
			{
				Process(genericParameter, module);
			}
		}

		private void Process(CustomAttribute customAttribute, Module module)
		{
			Process(customAttribute.Constructor, module);
			Process(customAttribute.CtorArguments, module);
			Process(customAttribute.NamedArguments, module);

			MapCustomAttributeArguments(customAttribute, module);
		}

		private void Process(IEnumerable<CustomAttribute> customAttributes, Module module)
		{
			foreach (var customAttribute in customAttributes)
			{
				Process(customAttribute, module);
			}
		}

		private void Process(CustomAttributeTypedArgument argument, Module module)
		{
			Process(argument.Type, module);

			object value = argument.Value;

			if (value != null)
			{
				if (value is Signature)
				{
					Process((Signature)value, module);
				}
				else if (value is CustomAttributeTypedArgument)
				{
					Process((CustomAttributeTypedArgument)value, module);
				}
				else if (value is Signature[])
				{
					var array = (Signature[])value;
					for (int i = 0; i < array.Length; i++)
					{
						var sigValue = array[i];
						if (sigValue != null)
						{
							Process(sigValue, module);
						}
					}
				}
				else if (value is CustomAttributeTypedArgument[])
				{
					var array = (CustomAttributeTypedArgument[])value;
					for (int i = 0; i < array.Length; i++)
					{
						Process(array[i], module);
					}
				}
			}
		}

		private void Process(CustomAttributeCtorArgumentCollection arguments, Module module)
		{
			foreach (var argument in arguments)
			{
				Process(argument, module);
			}
		}

		private void Process(CustomAttributeNamedArgument argument, Module module)
		{
			Process(argument.TypedValue, module);
		}

		private void Process(CustomAttributeNamedArgumentCollection arguments, Module module)
		{
			foreach (var argument in arguments)
			{
				Process(argument, module);
			}
		}

		private void Process(SecurityAttribute securityAttribute, Module module)
		{
			if (securityAttribute.Type != null)
				Process(securityAttribute.Type, module);

			Process(securityAttribute.NamedArguments, module);
			MapSecurityAttributeArguments(securityAttribute, module);
		}

		private void Process(IEnumerable<SecurityAttribute> securityAttributes, Module module)
		{
			foreach (var securityAttribute in securityAttributes)
			{
				Process(securityAttribute, module);
			}
		}

		#endregion

		#region Static

		public static void Analyze(BuildAssembly assembly)
		{
			var analyzer = new StripAnalyzer(assembly);
			analyzer.Analyze();
		}

		#endregion
	}
}
