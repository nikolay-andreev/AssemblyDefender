using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	/// <summary>
	/// Change hierarchy members with generated names.
	/// </summary>
	public class MemberReferenceMapper : SignatureBuilder
	{
		#region Fields

		private Module _module;
		private List<TupleStruct<TypeSignature, MethodReturnType>> _methodReturnTypes = new List<TupleStruct<TypeSignature, MethodReturnType>>();
		private List<TupleStruct<TypeSignature, MethodParameter>> _methodParameters = new List<TupleStruct<TypeSignature, MethodParameter>>();
		private List<TupleStruct<TypeSignature, FieldDeclaration>> _fieldTypes = new List<TupleStruct<TypeSignature, FieldDeclaration>>();
		private List<TupleStruct<TypeReference, int, Module>> _exportedTypes = new List<TupleStruct<TypeReference, int, Module>>();

		#endregion

		#region Ctors

		public MemberReferenceMapper()
		{
		}

		public MemberReferenceMapper(Module module)
		{
			_module = module;
		}

		#endregion

		#region Methods

		public void PostBuild()
		{
			// Method return types
			foreach (var tuple in _methodReturnTypes)
			{
				tuple.Item2.Type = tuple.Item1;
			}

			// Method parameters
			foreach (var tuple in _methodParameters)
			{
				tuple.Item2.Type = tuple.Item1;
			}

			// Field types
			foreach (var tuple in _fieldTypes)
			{
				tuple.Item2.FieldType = tuple.Item1;
			}

			// Modules
			foreach (var tuple in _exportedTypes)
			{
				tuple.Item3.ExportedTypes[tuple.Item2] = tuple.Item1;
			}

			// Clear
			_methodReturnTypes.Clear();
			_methodParameters.Clear();
			_fieldTypes.Clear();
			_exportedTypes.Clear();
		}

		#region Assembly

		public override void Build(Assembly assembly)
		{
			_module = assembly.Module;
			base.Build(assembly);
		}

		public override bool Build(ref AssemblyReference assemblyRef)
		{
			var assembly = assemblyRef.Resolve(_module) as BuildAssembly;
			if (assembly == null)
				return false;

			return Build(ref assemblyRef, assembly);
		}

		private bool Build(ref AssemblyReference assemblyRef, BuildAssembly assembly)
		{
			bool changed = false;

			// Name
			string name;
			if (assembly.NameChanged)
			{
				name = assembly.NewName;
				changed = true;
			}
			else
			{
				name = assemblyRef.Name;
			}

			// Culture
			string culture;
			if (assembly.CultureChanged)
			{
				culture = assembly.NewCulture;
				changed = true;
			}
			else
			{
				culture = assemblyRef.Culture;
			}

			// Version
			Version version;
			if (assembly.VersionChanged)
			{
				version = assembly.NewVersion;
				changed = true;
			}
			else
			{
				version = assemblyRef.Version;
			}

			// Version
			byte[] publicKeyToken;
			if (assembly.PublicKeyChanged)
			{
				publicKeyToken = assembly.NewPublicKeyToken;
				changed = true;
			}
			else
			{
				publicKeyToken = assemblyRef.PublicKeyToken;
			}

			if (!changed)
				return false;

			assemblyRef = new AssemblyReference(name, culture, version, publicKeyToken);
			return true;
		}

		#endregion

		#region Module

		public override void Build(Module module)
		{
			_module = module;
			base.Build(module);
		}

		public override bool Build(ref ModuleReference moduleRef)
		{
			var module = moduleRef.Resolve(_module) as BuildModule;
			if (module == null)
				return false;

			return Build(ref moduleRef, module);
		}

		public override bool Build(ref FileReference fileRef)
		{
			var moduleRef = new ModuleReference(fileRef.Name);
			if (!Build(ref moduleRef))
				return false;

			fileRef = new FileReference(moduleRef.Name, fileRef.ContainsMetadata, fileRef.HashValue);
			return true;
		}

		private bool Build(ref ModuleReference moduleRef, BuildModule module)
		{
			if (!module.NameChanged)
				return false;

			moduleRef = new ModuleReference(module.NewName);
			return true;
		}

		#endregion

		#region Type

		public override bool Build(ref TypeReference typeRef)
		{
			var type = typeRef.Resolve(_module);
			if (type != null)
			{
				var buildType = type.DeclaringType as BuildType;
				if (buildType != null)
				{
					return Build(ref typeRef, buildType);
				}
			}

			return base.Build(ref typeRef);
		}

		private bool Build(ref TypeReference typeRef, BuildType type)
		{
			bool changed = false;

			// Name
			string name;
			if (type.NameChanged)
			{
				name = type.NewName;
				changed = true;
			}
			else
			{
				name = type.Name;
			}

			// Namespace
			string ns;
			if (type.NamespaceChanged)
			{
				ns = type.NewNamespace;
				changed = true;
			}
			else
			{
				ns = type.Namespace;
			}

			// Owner
			var owner = typeRef.Owner;
			if (type.IsNested)
			{
				var enclosingTypeRef = (TypeReference)owner;
				var enclosingType = (BuildType)type.GetEnclosingType();
				if (Build(ref enclosingTypeRef, enclosingType))
				{
					owner = enclosingTypeRef;
					changed = true;
				}
			}
			else if (owner != null)
			{
				changed |= Build(ref owner);
			}

			if (!changed)
				return false;

			bool isValueType = typeRef.IsValueType.HasValue ? typeRef.IsValueType.Value : type.IsValueType();

			typeRef = new TypeReference(name, ns, owner, isValueType);
			return true;
		}

		public override void Build(ExportedTypeCollection exportedTypes)
		{
			for (int i = 0; i < exportedTypes.Count; i++)
			{
				var exportedType = exportedTypes[i];
				if (Build(ref exportedType))
				{
					_exportedTypes.Add(new TupleStruct<TypeReference, int, Module>(exportedType, i, _module));
				}
			}
		}

		#endregion

		#region Method

		public override void Build(MethodReturnType returnType)
		{
			var type = returnType.Type;
			if (Build(ref type))
				_methodReturnTypes.Add(new TupleStruct<TypeSignature, MethodReturnType>(type, returnType));

			if (returnType.MarshalType != null)
				Build(returnType.MarshalType);

			Build(returnType.CustomAttributes);
		}

		public override void Build(MethodParameter parameter)
		{
			var type = parameter.Type;
			if (Build(ref type))
				_methodParameters.Add(new TupleStruct<TypeSignature, MethodParameter>(type, parameter));

			if (parameter.MarshalType != null)
				Build(parameter.MarshalType);

			Build(parameter.CustomAttributes);
		}

		public override bool Build(ref MethodReference methodRef)
		{
			var method = methodRef.Resolve(_module, true);
			if (method != null)
			{
				var buildMethod = method.DeclaringMethod as BuildMethod;
				if (buildMethod != null)
				{
					return Build(ref methodRef, buildMethod);
				}
			}

			return base.Build(ref methodRef);
		}

		private bool Build(ref MethodReference methodRef, BuildMethod method)
		{
			bool changed = false;

			// Name
			string name;
			if (method.NameChanged)
			{
				name = method.NewName;
				changed = true;
			}
			else
			{
				name = method.Name;
			}

			// Owner
			var owner = methodRef.Owner;
			changed |= Build(ref owner);

			// Call site
			var callSite = methodRef.CallSite;
			changed |= Build(ref callSite);

			if (!changed)
				return false;

			methodRef = new MethodReference(name, owner, callSite);
			return true;
		}

		#endregion

		#region Field

		public override void Build(FieldDeclaration field)
		{
			var fieldType = field.FieldType;
			if (Build(ref fieldType))
				_fieldTypes.Add(new TupleStruct<TypeSignature, FieldDeclaration>(fieldType, field));

			if (field.MarshalFieldType != null)
				Build(field.MarshalFieldType);

			Build(field.CustomAttributes);

			// Force resolving of data size.
			if (field.HasData)
			{
				FieldDataType dataType;
				field.GetData(out dataType);
			}
		}

		public override bool Build(ref FieldReference fieldRef)
		{
			var field = fieldRef.Resolve(_module);
			if (field != null)
			{
				var buildField = field.DeclaringField as BuildField;
				if (buildField != null)
				{
					return Build(ref fieldRef, buildField);
				}
			}

			return base.Build(ref fieldRef);
		}

		private bool Build(ref FieldReference fieldRef, BuildField field)
		{
			bool changed = false;

			// Name
			string name;
			if (field.NameChanged)
			{
				name = field.NewName;
				changed = true;
			}
			else
			{
				name = field.Name;
			}

			// Field type
			var fieldType = fieldRef.FieldType;
			changed |= Build(ref fieldType);

			// Owner
			var owner = fieldRef.Owner;
			changed |= Build(ref owner);

			if (!changed)
				return false;

			fieldRef = new FieldReference(name, fieldType, owner);
			return true;
		}

		#endregion

		#region CustomAttribute

		public override void Build(CustomAttribute customAttribute)
		{
			MapCustomAttribute(customAttribute);
			base.Build(customAttribute);
		}

		private void MapCustomAttribute(CustomAttribute customAttribute)
		{
			var arguments = customAttribute.NamedArguments;
			if (arguments.Count == 0)
				return;

			var constructor = customAttribute.Constructor;
			if (constructor == null)
				return;

			var ctorMethod = constructor.Resolve(_module);
			if (ctorMethod == null)
				return;

			var type = ctorMethod.Owner.DeclaringType as BuildType;
			if (type == null)
				return;

			MapCustomAttributeArguments(arguments, type);
		}

		private void MapCustomAttributeArguments(
			CustomAttributeNamedArgumentCollection arguments,
			BuildType type)
		{
			// Collect fields
			var fields = new Dictionary<string, BuildField>();
			foreach (BuildField field in type.Fields)
			{
				if (!fields.ContainsKey(field.Name))
					fields.Add(field.Name, field);
			}

			// Collect properties
			var properties = new Dictionary<string, BuildProperty>();
			foreach (BuildProperty property in type.Properties)
			{
				if (!properties.ContainsKey(property.Name))
					properties.Add(property.Name, property);
			}

			// Map
			for (int i = 0; i < arguments.Count; i++)
			{
				var argument = arguments[i];
				switch (argument.Type)
				{
					case CustomAttributeNamedArgumentType.Field:
						{
							BuildField field;
							if (fields.TryGetValue(argument.Name, out field))
							{
								if (MapCustomAttributeFieldArgument(ref argument, field))
								{
									arguments[i] = argument;
								}
							}
						}
						break;

					case CustomAttributeNamedArgumentType.Property:
						{
							BuildProperty property;
							if (properties.TryGetValue(argument.Name, out property))
							{
								if (MapCustomAttributePropertyArgument(ref argument, property))
								{
									arguments[i] = argument;
								}
							}
						}
						break;

					default:
						throw new InvalidOperationException();
				}
			}
		}

		private bool MapCustomAttributeFieldArgument(
			ref CustomAttributeNamedArgument argument,
			BuildField field)
		{
			if (!field.NameChanged)
				return false;

			argument = new CustomAttributeNamedArgument(
				field.NewName,
				argument.Type,
				argument.TypedValue);

			return true;
		}

		private bool MapCustomAttributePropertyArgument(
			ref CustomAttributeNamedArgument argument,
			BuildProperty property)
		{
			if (!property.NameChanged)
				return false;

			argument = new CustomAttributeNamedArgument(
				property.NewName,
				argument.Type,
				argument.TypedValue);

			return true;
		}

		#endregion

		#region SecurityAttribute

		public override void Build(SecurityAttribute securityAttribute)
		{
			MapSecurityAttribute(securityAttribute);
			base.Build(securityAttribute);
		}

		private void MapSecurityAttribute(SecurityAttribute securityAttribute)
		{
			var arguments = securityAttribute.NamedArguments;
			if (arguments.Count == 0)
				return;

			var typeSig = securityAttribute.Type;
			if (typeSig == null)
				return;

			var type = typeSig.Resolve(_module);
			if (type == null)
				return;

			var buildType = type.DeclaringType as BuildType;
			if (buildType == null)
				return;

			MapCustomAttributeArguments(arguments, buildType);
		}

		#endregion

		#endregion

		#region Static

		public static void Map(BuildAssembly assembly)
		{
			var mapper = new MemberReferenceMapper();
			mapper.Build(assembly);
			mapper.PostBuild();
		}

		#endregion
	}
}
