using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using AssemblyDefender.Baml;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;
using AssemblyDefender.Common.Resources;

namespace AssemblyDefender
{
	public class BamlMemberReferenceMapper
	{
		#region Fields

		private string _mainTypeNamespace;
		private Random _random;
		private BuildAssembly _assembly;
		private BuildLog _log;
		private State _state;

		#endregion

		#region Ctors

		public BamlMemberReferenceMapper(BuildAssembly assembly, BuildLog log)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (log == null)
				throw new ArgumentNullException("log");

			_assembly = assembly;
			_log = log;
			_mainTypeNamespace = assembly.MainTypeNamespace;
			_random = assembly.RandomGenerator;
		}

		#endregion

		#region Methods

		public void Map()
		{
			if (!_assembly.HasWpfResource)
				return;

			var resource = _assembly.GetWpfResource();
			if (!resource.HasWpfBaml)
				return;

			var data = resource.GetData();

			bool changed = false;

			using (var outputStream = new MemoryStream())
			{
				using (var reader = new ResourceReaderEx(data))
				{
					using (var writer = new ResourceWriter(outputStream))
					{
						while (reader.Read())
						{
							byte[] resourceData = reader.Data;
							Map(reader, ref resourceData, ref changed);
							writer.AddResourceData(reader.Name, reader.TypeName, resourceData);
						}
					}
				}

				if (changed)
				{
					resource.SetData(outputStream.ToArray());
				}
			}
		}

		private void Map(ResourceReaderEx reader, ref byte[] resourceData, ref bool changed)
		{
			if (reader.TypeCode != ResourceTypeCode.Stream && reader.TypeCode != ResourceTypeCode.ByteArray)
				return;

			string name = reader.Name;

			if (0 != string.Compare(Path.GetExtension(name), BamlImage.FileExtension, true))
				return;

			var resourceDataStream = new MemoryStream(resourceData);
			resourceDataStream.Position += 4;

			var bamlImage = BamlImage.Load(resourceDataStream);
			if (bamlImage == null)
				return;

			if (Map(bamlImage))
			{
				resourceData = bamlImage.Save();

				int pos = 0;
				var newResourceData = new byte[resourceData.Length + 4];
				BufferUtils.Write(newResourceData, ref pos, (int)resourceData.Length);
				BufferUtils.Write(newResourceData, ref pos, (byte[])resourceData);
				resourceData = newResourceData;

				changed = true;
			}
		}

		private bool Map(BamlImage bamlImage)
		{
			_state = new State();
			_state.Node = bamlImage.FirstNode;

			ProcessNodes();

			bool changed = false;

			foreach (var mapping in _state.Mappings)
			{
				changed |= mapping.ApplyMapping();
			}

			_state = null;

			return changed;
		}

		private void ProcessNodes()
		{
			while (_state.Node != null)
			{
				switch (_state.Node.NodeType)
				{
					case BamlNodeType.Element:
						ProcessElement();
						break;

					case BamlNodeType.XmlnsProperty:
						ProcessXmlNamespace();
						break;

					case BamlNodeType.PIMapping:
						ProcessXmlNamespaceMapping();
						break;

					case BamlNodeType.AssemblyInfo:
						ProcessAssembly();
						break;

					case BamlNodeType.TypeInfo:
						ProcessType();
						break;

					case BamlNodeType.AttributeInfo:
						ProcessMember();
						break;

					case BamlNodeType.PropertyTypeReference:
						ProcessPropertyTypeReference();
						break;

					case BamlNodeType.PropertyWithConverter:
						ProcessPropertyWithConverter();
						break;

					case BamlNodeType.ConstructorParameters:
						ProcessConstructorParameters();
						break;
				}

				MoveNext();
			}
		}

		private void ProcessElement()
		{
			var bamlElement = (BamlElement)_state.Node;

			_state.CurrentScope.ElementType = bamlElement.Type;

			if (_state.RootElementType == null)
			{
				_state.RootElementType = Resolve(bamlElement.Type);
			}
		}

		private void ProcessConstructorParameters()
		{
			var bamlElement = _state.Node.Parent as BamlElement;
			if (bamlElement == null)
				return;

			if (IsType(bamlElement.Type, BamlKnownTypeCode.TemplateBindingExtension))
			{
				ProcessTemplateBindingConstructorParameters();
			}
		}

		private void ProcessTemplateBindingConstructorParameters()
		{
			if (!MoveNext(BamlNodeType.Text))
				return;

			var bamlText = (BamlText)_state.Node;

			string name = bamlText.Value;
			if (string.IsNullOrEmpty(name))
				return;

			var controlTemplateScope = _state.CurrentScope.FindScopeByElementType(BamlKnownTypeCode.ControlTemplate);
			if (controlTemplateScope == null)
				return;

			var bamlTargetType = controlTemplateScope.ControlTemplateTargetType as BamlTypeInfo;
			if (bamlTargetType == null)
				return;

			var targetType = Resolve(bamlTargetType) as BuildType;
			string newName;
			if (!GetMemberChangedName(targetType, name, out newName))
				return;

			AddMapping(new TextMap(bamlText, newName));
		}

		private void ProcessPropertyTypeReference()
		{
			var propertyTypeReference = (BamlPropertyTypeReference)_state.Node;

			if (IsProperty(propertyTypeReference.DeclaringProperty, "TargetType", BamlKnownTypeCode.ControlTemplate))
			{
				_state.CurrentScope.ControlTemplateTargetType = propertyTypeReference.Value;
			}
		}

		private void ProcessPropertyWithConverter()
		{
			var bamlPropertyWithConverter = (BamlPropertyWithConverter)_state.Node;
			var declaringProperty = bamlPropertyWithConverter.DeclaringProperty;
			var converterType = bamlPropertyWithConverter.ConverterType;

			if (IsType(converterType, BamlKnownTypeCode.StringConverter))
			{
				if (IsProperty(declaringProperty, "TypeName", BamlKnownTypeCode.TypeExtension))
				{
					ProcessPropertyWithConverterTypeName(bamlPropertyWithConverter);
				}
				else if (_state.RootElementType != null)
				{
					ProcessPropertyWithConverterEvent(bamlPropertyWithConverter);
				}
			}
			else if (IsType(converterType, BamlKnownTypeCode.EnumConverter))
			{
				ProcessPropertyWithConverterEnum(bamlPropertyWithConverter);
			}
			else if (IsType(converterType, BamlKnownTypeCode.CommandConverter))
			{
				ProcessPropertyWithConverterCommand(bamlPropertyWithConverter);
			}
			else if (IsType(converterType, BamlKnownTypeCode.UriTypeConverter))
			{
				ProcessPropertyWithConverterPackUri(bamlPropertyWithConverter);
			}
		}

		private void ProcessPropertyWithConverterPackUri(BamlPropertyWithConverter bamlPropertyWithConverter)
		{
			string value = bamlPropertyWithConverter.Value;
			if (string.IsNullOrEmpty(value))
				return;

			var uri = PackUri.Parse(value);
			if (uri == null)
				return;

			if (uri.Assembly == null)
				return;

			var assemblyRef = uri.Assembly;

			var targetAssembly = _assembly.AssemblyManager.Resolve(assemblyRef, _assembly.Module, false) as BuildAssembly;
			if (targetAssembly == null)
				return;

			bool changed = false;
			string name = assemblyRef.Name;
			Version version = assemblyRef.Version;
			byte[] publicKeyToken = assemblyRef.PublicKeyToken;

			if (targetAssembly.NameChanged)
			{
				name = targetAssembly.NewName;
				changed = true;
			}

			if (version != null && targetAssembly.VersionChanged)
			{
				version = targetAssembly.NewVersion;
				changed = true;
			}

			if (publicKeyToken != null && targetAssembly.PublicKeyChanged)
			{
				publicKeyToken = targetAssembly.NewPublicKeyToken;
				changed = true;
			}

			if (changed)
			{
				uri.Assembly = new AssemblyReference(name, null, version, publicKeyToken);
				AddMapping(new PropertyWithConverterMap(bamlPropertyWithConverter, uri.ToString()));
			}
		}

		private void ProcessPropertyWithConverterTypeName(BamlPropertyWithConverter bamlPropertyWithConverter)
		{
			string value = bamlPropertyWithConverter.Value;
			if (string.IsNullOrEmpty(value))
				return;

			BuildType type;
			if (MapTypeNameToken(ref value, out type))
			{
				AddMapping(new PropertyWithConverterMap(bamlPropertyWithConverter, value));
			}
		}

		private bool ProcessPropertyWithConverterEvent(BamlPropertyWithConverter bamlPropertyWithConverter)
		{
			string value = bamlPropertyWithConverter.Value;
			if (string.IsNullOrEmpty(value))
				return false;

			var bamlProperty = bamlPropertyWithConverter.DeclaringProperty as BamlPropertyInfo;
			if (bamlProperty == null)
				return false;

			var bamlOwnerType = bamlProperty.Type as BamlTypeInfo;
			if (bamlOwnerType == null)
				return false;

			var type = Resolve(bamlOwnerType);
			if (type == null)
				return false;

			var e = FindEvent(type, bamlProperty.Name);
			if (e == null)
				return false;

			var delegateType = e.EventType.Resolve(e.Assembly) as TypeDeclaration;
			if (delegateType == null)
				return false;

			var invokeMethod = delegateType.Methods.Find("Invoke");
			if (invokeMethod == null)
				return false;

			var method = FindRootElementEventMethod(value, invokeMethod) as BuildMethod;
			if (method == null)
				return false;

			if (!method.NameChanged)
				return false;

			AddMapping(new PropertyWithConverterMap(bamlPropertyWithConverter, method.NewName));

			return true;
		}

		private void ProcessPropertyWithConverterEnum(BamlPropertyWithConverter bamlPropertyWithConverter)
		{
			string value = bamlPropertyWithConverter.Value;
			if (string.IsNullOrEmpty(value))
				return;

			var bamlProperty = bamlPropertyWithConverter.DeclaringProperty as BamlPropertyInfo;
			if (bamlProperty == null)
				return;

			var property = bamlProperty.Resolve(_assembly) as BuildProperty;
			if (property == null)
				return;

			var enumType = property.ReturnType.Resolve(property.Assembly) as BuildType;
			if (enumType == null)
				return;

			var enumField = enumType.Fields.Find(value) as BuildField;
			if (enumField == null)
				return;

			if (!enumField.NameChanged)
				return;

			AddMapping(new PropertyWithConverterMap(bamlPropertyWithConverter, enumField.NewName));
		}

		private void ProcessPropertyWithConverterCommand(BamlPropertyWithConverter bamlPropertyWithConverter)
		{
			string value = bamlPropertyWithConverter.Value;
			if (string.IsNullOrEmpty(value))
				return;

			value = value.Trim();

			int index = value.LastIndexOf(".", StringComparison.Ordinal);
			if (index < 0)
				return;

			string typeName = value.Substring(0, index);
			string localName = value.Substring(index + 1);

			BuildType type;
			bool changed = MapTypeNameToken(ref typeName, out type);

			if (type != null)
			{
				changed |= (GetChangedPropertyName(type, ref localName) || GetChangedFieldName(type, ref localName));
			}

			if (changed)
			{
				value = typeName + "." + localName;
				AddMapping(new PropertyWithConverterMap(bamlPropertyWithConverter, value));
			}
		}

		private void ProcessAssembly()
		{
			var bamlAssembly = (BamlAssemblyInfo)_state.Node;

			var assembly = Resolve(bamlAssembly) as BuildAssembly;
			if (assembly == null)
				return;

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
				name = assembly.Name;
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
				culture = assembly.Culture;
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
				version = assembly.Version;
			}

			// PublicKey
			byte[] publicKeyToken;
			if (assembly.PublicKeyChanged)
			{
				publicKeyToken = assembly.NewPublicKeyToken;
				changed = true;
			}
			else
			{
				publicKeyToken = assembly.PublicKeyToken;
			}

			if (changed)
			{
				AddMapping(
					new AssemblyNameMap(
						bamlAssembly,
						SignaturePrinter.PrintAssembly(name, culture, version, publicKeyToken)));
			}
		}

		private void ProcessType()
		{
			var bamlType = (BamlTypeInfo)_state.Node;

			var type = Resolve(bamlType) as BuildType;
			if (type == null)
				return;

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

			if (changed)
			{
				AddMapping(new TypeNameMap(bamlType, CodeModelUtils.GetTypeName(name, ns)));
			}
		}

		private void ProcessMember()
		{
			var bamlProperty = (BamlPropertyInfo)_state.Node;

			string name = bamlProperty.Name;
			if (string.IsNullOrEmpty(name))
				return;

			var memberNode = Resolve(bamlProperty) as MemberNode;
			if (memberNode == null)
				return;

			bool changed = false;
			string newName = null;

			switch (memberNode.MemberType)
			{
				case MemberType.Property:
					{
						var property = (BuildProperty)memberNode;
						if (property.NameChanged)
						{
							newName = property.NewName;
							changed = true;
						}
					}
					break;

				case MemberType.Field:
					{
						var field = (BuildField)memberNode;
						if (field.NameChanged)
						{
							newName = field.NewName;
							changed = true;
						}
					}
					break;

				case MemberType.Event:
					{
						var e = (BuildEvent)memberNode;
						if (e.NameChanged)
						{
							newName = e.NewName;
							changed = true;
						}
					}
					break;
			}

			if (changed)
			{
				AddMapping(new PropertyNameMap(bamlProperty, newName));
			}
		}

		private void ProcessXmlNamespace()
		{
			var bamlNs = (BamlXmlnsProperty)_state.Node;

			if (string.IsNullOrEmpty(bamlNs.Prefix))
				return;

			if (_state.XmlNamespaceByPrefix.ContainsKey(bamlNs.Prefix))
				return;

			_state.XmlNamespaceByPrefix.Add(
				bamlNs.Prefix,
				new ChangeNodePair<BamlXmlnsProperty>(bamlNs));

			var scopePrefixes = _state.CurrentScope.NamespacePrefixes;
			if (scopePrefixes == null)
			{
				scopePrefixes = new List<string>();
				_state.CurrentScope.NamespacePrefixes = scopePrefixes;
			}

			scopePrefixes.Add(bamlNs.Prefix);
		}

		private void ProcessXmlNamespaceMapping()
		{
			var bamlNsMapping = (BamlPIMapping)_state.Node;

			if (string.IsNullOrEmpty(bamlNsMapping.XmlNamespace))
				return;

			if (_state.MappingByXmlNamespace.ContainsKey(bamlNsMapping.XmlNamespace))
				return;

			_state.MappingByXmlNamespace.Add(
				bamlNsMapping.XmlNamespace,
				new ChangeNodePair<BamlPIMapping>(bamlNsMapping));
		}

		private bool MapTypeNameToken(ref string typeToken, out BuildType type)
		{
			type = null;

			var typeName = XamlTypeName.Parse(typeToken);
			if (typeName == null)
				return false;

			if (string.IsNullOrEmpty(typeName.Prefix))
				return false;

			bool changed = MapTypeName(typeName, out type);

			if (typeName.HasTypeArgs)
			{
				var typeArguments = typeName.TypeArguments;
				for (int i = 0; i < typeArguments.Count; i++)
				{
					var typeArg = typeArguments[i];
					BuildType notUsedType;
					if (MapTypeName(typeArg, out notUsedType))
					{
						typeArguments[i] = typeArg;
						changed = true;
					}
				}
			}

			if (changed)
			{
				typeToken = typeName.ToString();
			}

			return changed;
		}

		private bool MapTypeName(XamlTypeName typeName, out BuildType type)
		{
			type = null;

			ChangeNodePair<BamlXmlnsProperty> nsRecordPair;
			if (!_state.XmlNamespaceByPrefix.TryGetValue(typeName.Prefix, out nsRecordPair))
				return false;

			var nsRecord = nsRecordPair.OriginalNode;

			ChangeNodePair<BamlPIMapping> mappingRecordPair;
			if (!_state.MappingByXmlNamespace.TryGetValue(nsRecord.XmlNamespace, out mappingRecordPair))
				return false;

			var mappingRecord = mappingRecordPair.OriginalNode;

			var assembly = mappingRecord.Assembly.Resolve(_assembly);
			if (assembly == null)
				return false;

			var module = (BuildModule)assembly.Module;

			type = module.Types.Find(typeName.Name, mappingRecord.ClrNamespace) as BuildType;
			if (type == null)
				return false;

			if (!type.NameChanged)
				return false;

			typeName.Name = type.NewName;

			string xmlNamespace = string.Format("clr-namespace:{0}", _mainTypeNamespace);
			if (!object.ReferenceEquals(module.Assembly, assembly))
			{
				xmlNamespace += string.Format(";assembly={0}", module.Assembly.Name);
			}

			string newPrefix;
			if (!_state.PerfixByXmlNamespace.TryGetValue(xmlNamespace, out newPrefix))
			{
				// Add new namespace
				newPrefix = _random.NextString(6);
				_state.PerfixByXmlNamespace.Add(xmlNamespace, newPrefix);

				var newNsRecord = new BamlXmlnsProperty();
				newNsRecord.Prefix = newPrefix;
				newNsRecord.XmlNamespace = xmlNamespace;

				foreach (var bamlAssembly in nsRecord.Assemblies)
				{
					newNsRecord.Assemblies.Add(bamlAssembly);
				}

				nsRecordPair.NewNode = newNsRecord;
				nsRecord.AddNext(newNsRecord);

				// Add new namespace mapping
				var newMappingRecord = new BamlPIMapping();
				newMappingRecord.XmlNamespace = newNsRecord.XmlNamespace;
				newMappingRecord.ClrNamespace = _mainTypeNamespace;
				newMappingRecord.Assembly = mappingRecord.Assembly;
				mappingRecordPair.NewNode = newMappingRecord;
				mappingRecord.AddNext(newMappingRecord);
			}

			typeName.Prefix = newPrefix;

			return true;
		}

		private void ParseUri(string source, out string typeName, out string localName)
		{
			typeName = null;
			localName = source.Trim();
			int length = localName.LastIndexOf(".", StringComparison.Ordinal);
			if (length >= 0)
			{
				typeName = localName.Substring(0, length);
				localName = localName.Substring(length + 1);
			}
		}

		private bool GetChangedPropertyName(BuildType type, ref string name)
		{
			foreach (BuildProperty property in type.Properties)
			{
				if (property.Name == name)
				{
					if (!property.NameChanged)
						return false;

					name = property.NewName;
					return true;
				}
			}

			return false;
		}

		private bool GetChangedFieldName(BuildType type, ref string name)
		{
			foreach (BuildField field in type.Fields)
			{
				if (field.Name == name)
				{
					if (!field.NameChanged)
						return false;

					name = field.NewName;
					return true;
				}
			}

			return false;
		}

		private bool GetMemberChangedName(BuildType type, string name, out string newName)
		{
			newName = null;

			// Property
			var property = type.Properties.Find(name) as BuildProperty;
			if (property != null)
			{
				if (property.NameChanged)
				{
					newName = property.Name;
					return true;
				}

				return false;
			}

			// Event
			var e = type.Events.Find(name) as BuildEvent;
			if (e != null)
			{
				if (e.NameChanged)
				{
					newName = e.Name;
					return true;
				}

				return false;
			}

			// Field
			var field = type.Fields.Find(name) as BuildField;
			if (field != null)
			{
				if (field.NameChanged)
				{
					newName = field.Name;
					return true;
				}

				return false;
			}

			return false;
		}

		private MethodDeclaration FindRootElementEventMethod(string name, MethodDeclaration invokeMethod)
		{
			var comparer = SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName;
			foreach (var method in _state.RootElementType.Methods)
			{
				if (method.Name != name)
					continue;

				if (method.Parameters.Count != invokeMethod.Parameters.Count)
					continue;

				if (!comparer.Equals(method.ReturnType.Type, invokeMethod.ReturnType.Type))
					continue;

				bool failed = false;
				for (int i = 0; i < method.Parameters.Count; i++)
				{
					if (!comparer.Equals(method.Parameters[i].Type, invokeMethod.Parameters[i].Type))
					{
						failed = true;
						break;
					}
				}

				if (!failed)
					return method;
			}

			return null;
		}

		private void AddMapping(MappingBase mapping)
		{
			mapping.Mapper = this;
			mapping.Scope = _state.CurrentScope;
			_state.Mappings.Add(mapping);
		}

		private Assembly Resolve(IBamlAssembly bamlAssembly)
		{
			return bamlAssembly.Resolve(_assembly);
		}

		private TypeDeclaration Resolve(IBamlType bamlType)
		{
			return bamlType.Resolve(_assembly);
		}

		private MemberNode Resolve(IBamlProperty bamlProperty)
		{
			return bamlProperty.Resolve(_assembly);
		}

		private EventDeclaration FindEvent(TypeDeclaration type, string name)
		{
			while (true)
			{
				var e = type.Events.Find(name);
				if (e != null)
					return e;

				var baseType = ((IType)type).BaseType;
				if (baseType == null)
					break;

				type = (TypeDeclaration)baseType.DeclaringType;
			}

			return null;
		}

		private void EnterScope(BamlBlock block)
		{
			var currentScope = _state.CurrentScope;

			var scope = new Scope();
			scope.Block = block;
			scope.Parent = currentScope;
			scope.Level = currentScope.Level + 1;
			_state.CurrentScope = scope;
		}

		private void ExitScope()
		{
			var currentScope = _state.CurrentScope;

			if (currentScope.NamespacePrefixes != null)
			{
				foreach (string nsPrefix in currentScope.NamespacePrefixes)
				{
					_state.XmlNamespaceByPrefix.Remove(nsPrefix);
				}
			}

			if (currentScope.Parent != null)
			{
				_state.CurrentScope = currentScope.Parent;
			}
		}

		private bool MoveNext()
		{
			var node = _state.Node;
			while (true)
			{
				BamlNode nextNode = null;
				if (node.IsBlock)
				{
					nextNode = ((BamlBlock)node).FirstChild;
				}

				if (nextNode == null)
				{
					while (node.NextSibling == null && node.Parent != null)
					{
						ExitScope();
						node = node.Parent;
					}

					nextNode = node.NextSibling;
				}

				if (nextNode != null)
				{
					if (nextNode.IsBlock)
					{
						EnterScope((BamlBlock)nextNode);
					}
					else if (IsSkipNode(nextNode))
					{
						node = nextNode;
						continue;
					}

					_state.Node = nextNode;
					return true;
				}
				else
				{
					_state.Node = null;
					return false;
				}
			}
		}

		private bool MoveNext(BamlNodeType type)
		{
			var next = GetNext(_state.Node);
			if (next == null)
				return false;

			if (next.NodeType != type)
				return false;

			MoveNext();
			return true;
		}

		private bool MoveFirstChild(BamlNodeType type)
		{
			var node = _state.Node;
			if (!node.IsBlock)
				return false;

			var nextNode = ((BamlBlock)node).FirstChild;
			while (nextNode != null && IsSkipNode(nextNode))
			{
				nextNode = nextNode.NextSibling;
			}

			if (nextNode == null)
				return false;

			if (nextNode.NodeType != type)
				return false;

			MoveNext();
			return true;
		}

		private BamlNode GetNext(BamlNode node)
		{
			while (true)
			{
				BamlNode nextNode = null;
				if (node.IsBlock)
				{
					nextNode = ((BamlBlock)node).FirstChild;
				}

				if (nextNode == null)
				{
					while (node.NextSibling == null && node.Parent != null)
					{
						node = node.Parent;
					}

					nextNode = node.NextSibling;
				}

				if (nextNode != null)
				{
					if (IsSkipNode(nextNode))
					{
						node = nextNode;
						continue;
					}

					return nextNode;
				}
				else
				{
					return null;
				}
			}
		}

		private bool IsSkipNode(BamlNode node)
		{
			return
				node.NodeType == BamlNodeType.LineNumberAndPosition ||
				node.NodeType == BamlNodeType.LinePosition;
		}

		#endregion

		#region Static

		public static void Map(BuildAssembly assembly, BuildLog log)
		{
			var mapper = new BamlMemberReferenceMapper(assembly, log);
			mapper.Map();
		}

		private static bool IsType(IBamlType bamlType, BamlKnownTypeCode typeCode)
		{
			var bamlKnownType = bamlType as BamlKnownType;
			if (bamlKnownType == null)
				return false;

			if (bamlKnownType.KnownCode != typeCode)
				return false;

			return true;
		}

		private static bool IsType(IBamlType bamlType, string typeName, bool ignoreNamespace = false)
		{
			var bamlTypeInfo = bamlType as BamlTypeInfo;
			if (bamlTypeInfo == null)
				return false;

			string name = bamlTypeInfo.Name;
			if (string.IsNullOrEmpty(name))
				return false;

			if (ignoreNamespace)
			{
				if (!name.EndsWith(typeName))
					return false;

				// Check for dot
				if (name.Length > typeName.Length && name[name.Length - typeName.Length - 1] != '.')
					return false;
			}
			else
			{
				if (name != typeName)
					return false;
			}

			return true;
		}

		private static bool IsProperty(IBamlProperty bamlProperty, string name, BamlKnownTypeCode typeCode)
		{
			var bamlPropertyInfo = bamlProperty as BamlPropertyInfo;
			if (bamlPropertyInfo == null)
				return false;

			if (bamlPropertyInfo.Name != name)
				return false;

			if (!IsType(bamlPropertyInfo.Type, typeCode))
				return false;

			return true;
		}

		private static bool IsProperty(IBamlProperty bamlProperty, string name, string typeName, bool ignoreNamespace)
		{
			var bamlPropertyInfo = bamlProperty as BamlPropertyInfo;
			if (bamlPropertyInfo == null)
				return false;

			if (bamlPropertyInfo.Name != name)
				return false;

			if (!IsType(bamlPropertyInfo.Type, typeName, ignoreNamespace))
				return false;

			return true;
		}

		#endregion

		#region Nested types

		private class State
		{
			internal BamlNode Node;
			internal TypeDeclaration RootElementType;
			internal Scope CurrentScope = new Scope();
			internal List<MappingBase> Mappings = new List<MappingBase>();
			internal Dictionary<string, ChangeNodePair<BamlPIMapping>> MappingByXmlNamespace = new Dictionary<string, ChangeNodePair<BamlPIMapping>>();
			internal Dictionary<string, ChangeNodePair<BamlXmlnsProperty>> XmlNamespaceByPrefix = new Dictionary<string, ChangeNodePair<BamlXmlnsProperty>>();
			internal Dictionary<string, string> PerfixByXmlNamespace = new Dictionary<string, string>();
		}

		private class Scope
		{
			internal int Level;
			internal Scope Parent;
			internal BamlBlock Block;
			internal IBamlType ElementType;
			internal IBamlType ControlTemplateTargetType;
			internal List<string> NamespacePrefixes;

			internal Scope GetAtLevel(int level)
			{
				var scope = this;
				while (scope != null && scope.Level != level)
				{
					scope = scope.Parent;
				}

				return scope;
			}

			internal Scope FindScopeByElementType(BamlKnownTypeCode typeCode)
			{
				var scope = this;
				while (scope != null)
				{
					if (scope.ElementType != null && IsType(scope.ElementType, typeCode))
					{
						return scope;
					}

					scope = scope.Parent;
				}

				return null;
			}
		}

		private class ChangeNodePair<T>
			where T : BamlNode
		{
			internal T OriginalNode;
			internal T NewNode;

			public ChangeNodePair(T record)
			{
				OriginalNode = record;
			}
		}

		private abstract class MappingBase
		{
			internal Scope Scope;
			internal BamlMemberReferenceMapper Mapper;

			public abstract bool ApplyMapping();
		}

		private class AssemblyNameMap : MappingBase
		{
			private string _name;
			private BamlAssemblyInfo _bamlNode;

			internal AssemblyNameMap(BamlAssemblyInfo bamlNode, string name)
			{
				_bamlNode = bamlNode;
				_name = name;
			}

			public override bool ApplyMapping()
			{
				_bamlNode.Name = _name;
				return true;
			}
		}

		private class TypeNameMap : MappingBase
		{
			private string _name;
			private BamlTypeInfo _bamlNode;

			internal TypeNameMap(BamlTypeInfo bamlNode, string name)
			{
				_bamlNode = bamlNode;
				_name = name;
			}

			public override bool ApplyMapping()
			{
				_bamlNode.Name = _name;
				return true;
			}
		}

		private class PropertyNameMap : MappingBase
		{
			private string _name;
			private BamlPropertyInfo _bamlNode;

			internal PropertyNameMap(BamlPropertyInfo bamlNode, string name)
			{
				_bamlNode = bamlNode;
				_name = name;
			}

			public override bool ApplyMapping()
			{
				_bamlNode.Name = _name;
				return true;
			}
		}

		private class PropertyWithConverterMap : MappingBase
		{
			private string _value;
			private BamlPropertyWithConverter _bamlNode;

			internal PropertyWithConverterMap(BamlPropertyWithConverter bamlNode, string value)
			{
				_bamlNode = bamlNode;
				_value = value;
			}

			public override bool ApplyMapping()
			{
				_bamlNode.Value = _value;
				return true;
			}
		}

		private class TextMap : MappingBase
		{
			private string _value;
			private BamlText _bamlNode;

			internal TextMap(BamlText bamlNode, string value)
			{
				_bamlNode = bamlNode;
				_value = value;
			}

			public override bool ApplyMapping()
			{
				_bamlNode.Value = _value;
				return true;
			}
		}

		#endregion
	}
}
