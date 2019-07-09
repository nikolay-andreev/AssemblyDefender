using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Baml
{
	internal class BamlImageLoader
	{
		#region Fields

		private int _currentRecordPosition;
		private IBinaryAccessor _accessor;
		private BamlNode _firstRecord;
		private Scope _currentScope;
		private List<IBamlAssembly> _assemblies = new List<IBamlAssembly>();
		private List<IBamlType> _types = new List<IBamlType>();
		private List<IBamlProperty> _properties = new List<IBamlProperty>();
		private List<IBamlString> _strings = new List<IBamlString>();
		private List<Scope> _scopes = new List<Scope>();
		private Stack<Scope> _scopeStack = new Stack<Scope>();
		private Dictionary<int, BamlNode> _positionToNode = new Dictionary<int, BamlNode>(0x40);
		private Dictionary<BamlNode, int> _nodeToPosition = new Dictionary<BamlNode, int>(0x40);

		#endregion

		#region Ctors

		internal BamlImageLoader(IBinaryAccessor accessor)
		{
			_accessor = accessor;
		}

		#endregion

		#region Methods

		internal BamlImage Load()
		{
			var image = new BamlImage();

			ReadHeader(image);

			ReadRecords();

			MapKeys();

			image.FirstNode = _firstRecord;

			return image;
		}

		private void ReadHeader(BamlImage image)
		{
			int featureByteCount = _accessor.ReadInt32();
			image.FeatureID = Encoding.Unicode.GetString(_accessor.ReadBytes(featureByteCount));
			image.ReaderVersion = new VersionPair(_accessor.ReadInt16(), _accessor.ReadInt16());
			image.UpdaterVersion = new VersionPair(_accessor.ReadInt16(), _accessor.ReadInt16());
			image.WriterVersion = new VersionPair(_accessor.ReadInt16(), _accessor.ReadInt16());
		}

		private void ReadRecords()
		{
			_currentScope =
				new Scope()
				{
					Length = _accessor.Length,
				};

			_scopes.Add(_currentScope);

			while (ReadRecord()) { };
		}

		private bool ReadRecord()
		{
			long position = _accessor.Position;

			_currentRecordPosition = (int)position;

			while (_currentScope != null && _currentScope.Length >= 0 && _currentScope.Length <= position)
			{
				EndScope();
			}

			if (_currentScope == null)
				return false;

			var recordType = (RecordType)_accessor.ReadByte();
			ReadRecord(recordType);

			return true;
		}

		private void ReadRecord(RecordType recordType)
		{
			switch (recordType)
			{
				case RecordType.DocumentStart:
					ReadDocument();
					break;

				case RecordType.DocumentEnd:
					EndScope(recordType);
					break;

				case RecordType.ElementStart:
					ReadElement();
					break;

				case RecordType.ElementEnd:
					EndScope(recordType);
					break;

				case RecordType.Property:
					ReadProperty();
					break;

				case RecordType.PropertyCustom:
					ReadPropertyCustom();
					break;

				case RecordType.PropertyComplexStart:
					ReadPropertyComplex();
					break;

				case RecordType.PropertyComplexEnd:
					EndScope(recordType);
					break;

				case RecordType.PropertyArrayStart:
					ReadPropertyArray();
					break;

				case RecordType.PropertyArrayEnd:
					EndScope(recordType);
					break;

				case RecordType.PropertyIListStart:
					ReadPropertyIList();
					break;

				case RecordType.PropertyIListEnd:
					EndScope(recordType);
					break;

				case RecordType.PropertyIDictionaryStart:
					ReadPropertyIDictionary();
					break;

				case RecordType.PropertyIDictionaryEnd:
					EndScope(recordType);
					break;

				case RecordType.LiteralContent:
					ReadLiteralContent();
					break;

				case RecordType.Text:
					ReadText();
					break;

				case RecordType.TextWithConverter:
					ReadTextWithConverter();
					break;

				case RecordType.RoutedEvent:
					ReadRoutedEvent();
					break;

				case RecordType.ClrEvent:
					ReadClrEvent();
					break;

				case RecordType.XmlnsProperty:
					ReadXmlnsProperty();
					break;

				case RecordType.XmlAttribute:
					ReadXmlAttribute();
					break;

				case RecordType.ProcessingInstruction:
					ReadProcessingInstruction();
					break;

				case RecordType.Comment:
					ReadComment();
					break;

				case RecordType.DefTag:
					ReadDefTag();
					break;

				case RecordType.DefAttribute:
					ReadDefAttribute();
					break;

				case RecordType.EndAttributes:
					ReadEndAttributes();
					break;

				case RecordType.PIMapping:
					ReadPIMapping();
					break;

				case RecordType.AssemblyInfo:
					ReadAssemblyInfo();
					break;

				case RecordType.TypeInfo:
					ReadTypeInfo();
					break;

				case RecordType.TypeSerializerInfo:
					ReadTypeSerializerInfo();
					break;

				case RecordType.AttributeInfo:
					ReadAttributeInfo();
					break;

				case RecordType.StringInfo:
					ReadStringInfo();
					break;

				case RecordType.PropertyStringReference:
					ReadPropertyStringReference();
					break;

				case RecordType.PropertyTypeReference:
					ReadPropertyTypeReference();
					break;

				case RecordType.PropertyWithExtension:
					ReadPropertyWithExtension();
					break;

				case RecordType.PropertyWithConverter:
					ReadPropertyWithConverter();
					break;

				case RecordType.DeferableContentStart:
					ReadDeferableContent();
					break;

				case RecordType.DefAttributeKeyString:
					ReadDefAttributeKeyString();
					break;

				case RecordType.DefAttributeKeyType:
					ReadDefAttributeKeyType();
					break;

				case RecordType.KeyElementStart:
					ReadKeyElement();
					break;

				case RecordType.KeyElementEnd:
					EndScope(recordType);
					break;

				case RecordType.ConstructorParametersStart:
					ReadConstructorParameters();
					break;

				case RecordType.ConstructorParametersEnd:
					EndScope(recordType);
					break;

				case RecordType.ConstructorParameterType:
					ReadConstructorParameterType();
					break;

				case RecordType.ConnectionId:
					ReadConnectionId();
					break;

				case RecordType.ContentProperty:
					ReadContentProperty();
					break;

				case RecordType.NamedElementStart:
					ReadNamedElement();
					break;

				case RecordType.StaticResourceStart:
					ReadStaticResource();
					break;

				case RecordType.StaticResourceEnd:
					EndScope(recordType);
					break;

				case RecordType.StaticResourceId:
					ReadStaticResourceId();
					break;

				case RecordType.TextWithId:
					ReadTextWithId();
					break;

				case RecordType.PresentationOptionsAttribute:
					ReadPresentationOptionsAttribute();
					break;

				case RecordType.LineNumberAndPosition:
					ReadLineNumberAndPosition();
					break;

				case RecordType.LinePosition:
					ReadLinePosition();
					break;

				case RecordType.OptimizedStaticResource:
					ReadOptimizedStaticResource();
					break;

				case RecordType.PropertyWithStaticResourceId:
					ReadPropertyWithStaticResourceId();
					break;

				case RecordType.LastRecordType:
					break;

				default:
					throw new BamlException(SR.BamlLoadError);
			}
		}

		private void ReadDocument()
		{
			var node = new BamlDocument();
			node.LoadAsync = _accessor.ReadBoolean();
			node.MaxAsyncRecords = _accessor.ReadInt32();
			node.DebugBaml = _accessor.ReadBoolean();

			AddNode(node);
			BeginScope(node, RecordType.DocumentEnd);
		}

		private void ReadElement()
		{
			var node = new BamlElement();
			node.Type = GetType(_accessor.ReadInt16());
			node.Flags = (BamlElementFlags)_accessor.ReadByte();

			AddNode(node);
			BeginScope(node, RecordType.ElementEnd);
		}

		private void ReadProperty()
		{
			ReadRecordSize();

			var node = new BamlProperty();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());
			node.Value = _accessor.ReadLengthPrefixedString(Encoding.UTF8);

			AddNode(node);
		}

		private void ReadPropertyCustom()
		{
			int size = ReadRecordSize();

			var node = new BamlPropertyCustom();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());

			short converterId = _accessor.ReadInt16();
			if ((converterId & 0x4000) == 0x4000)
			{
				node.IsValueType = true;
				converterId = (short)(converterId & ~0x4000);
			}

			node.Value = ReadPropertyValue(converterId, size - 4, node.IsValueType);

			AddNode(node);
		}

		private void ReadPropertyComplex()
		{
			var node = new BamlPropertyComplex();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());

			AddNode(node);
			BeginScope(node, RecordType.PropertyComplexEnd);
		}

		private void ReadPropertyArray()
		{
			var node = new BamlPropertyArray();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());

			AddNode(node);
			BeginScope(node, RecordType.PropertyArrayEnd);
		}

		private void ReadPropertyIList()
		{
			var node = new BamlPropertyIList();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());

			AddNode(node);
			BeginScope(node, RecordType.PropertyIListEnd);
		}

		private void ReadPropertyIDictionary()
		{
			var node = new BamlPropertyIDictionary();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());

			AddNode(node);
			BeginScope(node, RecordType.PropertyIDictionaryEnd);
		}

		private void ReadLiteralContent()
		{
			ReadRecordSize();

			var node = new BamlLiteralContent();
			node.Value = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
			node.Num1 = _accessor.ReadInt32();
			node.Num2 = _accessor.ReadInt32();

			AddNode(node);
		}

		private void ReadText()
		{
			ReadRecordSize();

			var node = new BamlText();
			node.Value = _accessor.ReadLengthPrefixedString(Encoding.UTF8);

			AddNode(node);
		}

		private void ReadTextWithConverter()
		{
			ReadRecordSize();

			var node = new BamlTextWithConverter();
			node.Value = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
			node.ConverterType = GetType(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadRoutedEvent()
		{
			ReadRecordSize();

			var node = new BamlRoutedEvent();
			node.Property = GetProperty(_accessor.ReadInt16());
			node.Value = _accessor.ReadLengthPrefixedString(Encoding.UTF8);

			AddNode(node);
		}

		private void ReadClrEvent()
		{
			throw new BamlException(SR.BamlLoadError);
		}

		private void ReadXmlnsProperty()
		{
			ReadRecordSize();

			var node = new BamlXmlnsProperty();
			node.Prefix = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
			node.XmlNamespace = _accessor.ReadLengthPrefixedString(Encoding.UTF8);

			int asssemblyCount = _accessor.ReadInt16();
			var assemblies = node.Assemblies;
			for (int i = 0; i < asssemblyCount; i++)
			{
				assemblies.Add(GetAssembly(_accessor.ReadInt16()));
			}

			AddNode(node);
		}

		private void ReadXmlAttribute()
		{
			throw new BamlException(SR.BamlLoadError);
		}

		private void ReadProcessingInstruction()
		{
			throw new BamlException(SR.BamlLoadError);
		}

		private void ReadComment()
		{
			throw new BamlException(SR.BamlLoadError);
		}

		private void ReadDefTag()
		{
			throw new BamlException(SR.BamlLoadError);
		}

		private void ReadDefAttribute()
		{
			ReadRecordSize();

			var node = new BamlDefAttribute();
			node.Value = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
			node.Name = GetString(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadEndAttributes()
		{
			throw new BamlException(SR.BamlLoadError);
		}

		private void ReadPIMapping()
		{
			ReadRecordSize();

			var node = new BamlPIMapping();
			node.XmlNamespace = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
			node.ClrNamespace = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
			node.Assembly = GetAssembly(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadAssemblyInfo()
		{
			ReadRecordSize();

			var node = new BamlAssemblyInfo();

			short assemblyId = _accessor.ReadInt16();
			node.Name = _accessor.ReadLengthPrefixedString(Encoding.UTF8);

			AddAssembly(assemblyId, node);
			AddNode(node);
		}

		private void ReadTypeInfo()
		{
			ReadRecordSize();

			var node = new BamlTypeInfo();

			short typeId = _accessor.ReadInt16();
			short flagsAndAssemblyId = _accessor.ReadInt16();
			node.Flags = (BamlTypeFlags)(flagsAndAssemblyId >> 12);
			node.Assembly = GetAssembly((short)(flagsAndAssemblyId & 0xfff));
			node.Name = _accessor.ReadLengthPrefixedString(Encoding.UTF8);

			AddType(typeId, node);
			AddNode(node);
		}

		private void ReadTypeSerializerInfo()
		{
			ReadRecordSize();

			var node = new BamlTypeSerializerInfo();
			short typeId = _accessor.ReadInt16();
			short flagsAndAssemblyId = _accessor.ReadInt16();
			node.Flags = (BamlTypeFlags)(flagsAndAssemblyId >> 12);
			node.Assembly = GetAssembly((short)(flagsAndAssemblyId & 0xfff));
			node.Name = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
			node.SerializerType = GetType(_accessor.ReadInt16());

			AddType(typeId, node);
			AddNode(node);
		}

		private void ReadAttributeInfo()
		{
			ReadRecordSize();

			var node = new BamlPropertyInfo();

			short attributeId = _accessor.ReadInt16();
			node.Type = GetType(_accessor.ReadInt16());
			node.Usage = (BamlPropertyUsage)_accessor.ReadByte();
			node.Name = _accessor.ReadLengthPrefixedString(Encoding.UTF8);

			AddProperty(attributeId, node);
			AddNode(node);
		}

		private void ReadStringInfo()
		{
			ReadRecordSize();

			var node = new BamlStringInfo();

			short stringId = _accessor.ReadInt16();
			node.Value = _accessor.ReadLengthPrefixedString(Encoding.UTF8);

			AddString(stringId, node);
			AddNode(node);
		}

		private void ReadPropertyStringReference()
		{
			var node = new BamlPropertyStringReference();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());
			node.Value = GetString(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadPropertyTypeReference()
		{
			var node = new BamlPropertyTypeReference();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());
			node.Value = GetType(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadPropertyWithExtension()
		{
			var node = new BamlPropertyWithExtension();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());

			short flags = _accessor.ReadInt16();
			short valueId = _accessor.ReadInt16();

			node.IsValueType = (flags & 0x4000) == 0x4000;
			node.IsStaticType = (flags & 0x2000) == 0x2000;

			var extensionType = (BamlExtensionType)(flags & 0xfff);

			BamlExtensionValue value;
			switch (extensionType)
			{
				case BamlExtensionType.StaticExtension:
					{
						value = GetStaticExtensionValue(valueId);
					}
					break;

				case BamlExtensionType.StaticResource:
				case BamlExtensionType.DynamicResource:
					{
						if (node.IsValueType)
						{
							value = new BamlExtensionTypeValue(GetType(valueId));
						}
						else if (node.IsStaticType)
						{
							value = GetStaticExtensionValue(valueId);
						}
						else
						{
							value = new BamlExtensionStringValue(GetString(valueId));
						}
					}
					break;

				case BamlExtensionType.TemplateBinding:
					{
						value = new BamlExtensionPropertyValue(GetProperty(valueId));
					}
					break;

				case BamlExtensionType.Type:
					{
						value = new BamlExtensionTypeValue(GetType(valueId));
					}
					break;

				default:
					{
						value = new BamlExtensionStringValue(GetString(valueId));
					}
					break;
			}

			node.Extension = new BamlExtension(extensionType, value);

			AddNode(node);
		}

		private void ReadPropertyWithConverter()
		{
			ReadRecordSize();

			var node = new BamlPropertyWithConverter();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());
			node.Value = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
			node.ConverterType = GetType(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadDeferableContent()
		{
			var node = new BamlDeferableContent();

			int contentByteSize = _accessor.ReadInt32();

			AddNode(node);

			long streamLength = _accessor.Position + contentByteSize;

			BeginScope(node, streamLength);
		}

		private void ReadDefAttributeKeyString()
		{
			ReadRecordSize();

			var node = new BamlDefAttributeKeyString();
			node.Value = GetString(_accessor.ReadInt16());
			int valuePosition = _accessor.ReadInt32();
			node.Shared = _accessor.ReadBoolean();
			node.SharedSet = _accessor.ReadBoolean();

			AddNode(node);
			AddKey(node, valuePosition);
		}

		private void ReadDefAttributeKeyType()
		{
			var node = new BamlDefAttributeKeyType();
			node.Value = GetType(_accessor.ReadInt16());
			node.TypeFlags = (BamlElementFlags)_accessor.ReadByte();
			int valuePosition = _accessor.ReadInt32();
			node.Shared = _accessor.ReadBoolean();
			node.SharedSet = _accessor.ReadBoolean();

			AddNode(node);
			AddKey(node, valuePosition);
		}

		private void ReadKeyElement()
		{
			var node = new BamlKeyElement();
			node.Type = GetType(_accessor.ReadInt16());
			node.TypeFlags = (BamlElementFlags)_accessor.ReadByte();
			int valuePosition = _accessor.ReadInt32();
			node.Shared = _accessor.ReadBoolean();
			node.SharedSet = _accessor.ReadBoolean();

			AddNode(node);
			AddKey(node, valuePosition);
			BeginScope(node, RecordType.KeyElementEnd);
		}

		private void ReadConstructorParameters()
		{
			var node = new BamlConstructorParameters();
			AddNode(node);
			BeginScope(node, RecordType.ConstructorParametersEnd);
		}

		private void ReadConstructorParameterType()
		{
			var node = new BamlConstructorParameterType();
			node.Type = GetType(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadConnectionId()
		{
			var node = new BamlConnectionId();
			node.Value = _accessor.ReadInt32();

			AddNode(node);
		}

		private void ReadContentProperty()
		{
			var node = new BamlContentProperty();
			node.Property = GetProperty(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadNamedElement()
		{
			var node = new BamlNamedElement();
			node.Type = GetType(_accessor.ReadInt16());
			node.RuntimeName = _accessor.ReadLengthPrefixedString(Encoding.UTF8);

			AddNode(node);
			BeginScope(node, RecordType.ElementEnd);
		}

		private void ReadStaticResource()
		{
			var node = new BamlStaticResource();
			node.Type = GetType(_accessor.ReadInt16());
			node.Flags = (BamlElementFlags)_accessor.ReadByte();

			AddNode(node);
			BeginScope(node, RecordType.StaticResourceEnd);
		}

		private void ReadStaticResourceId()
		{
			var node = new BamlStaticResourceId();
			node.Value = _accessor.ReadInt16();

			AddNode(node);
		}

		private void ReadTextWithId()
		{
			ReadRecordSize();

			var node = new BamlTextWithId();
			node.Value = GetString(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadPresentationOptionsAttribute()
		{
			ReadRecordSize();

			var node = new BamlPresentationOptionsAttribute();
			node.Value = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
			node.Name = GetString(_accessor.ReadInt16());

			AddNode(node);
		}

		private void ReadLineNumberAndPosition()
		{
			var node = new BamlLineNumberAndPosition();
			node.LineNumber = _accessor.ReadInt32();
			node.LineOffset = _accessor.ReadInt32();

			AddNode(node);
		}

		private void ReadLinePosition()
		{
			var node = new BamlLinePosition();
			node.LineOffset = _accessor.ReadInt32();

			AddNode(node);
		}

		private void ReadOptimizedStaticResource()
		{
			var node = new BamlOptimizedStaticResource();

			node.Flags = (BamlOptimizedStaticResourceFlags)_accessor.ReadByte();

			short valueId = _accessor.ReadInt16();

			BamlExtensionValue value;
			switch (node.Flags)
			{
				case BamlOptimizedStaticResourceFlags.ValueType:
					{
						value = new BamlExtensionTypeValue(GetType(valueId));
					}
					break;

				case BamlOptimizedStaticResourceFlags.StaticType:
					{
						value = GetStaticExtensionValue(valueId);
					}
					break;

				default:
					{
						value = new BamlExtensionStringValue(GetString(valueId));
					}
					break;
			}

			node.Value = value;

			AddNode(node);
		}

		private void ReadPropertyWithStaticResourceId()
		{
			var node = new BamlPropertyWithStaticResourceId();
			node.DeclaringProperty = GetProperty(_accessor.ReadInt16());
			node.StaticResourceId = _accessor.ReadInt16();

			AddNode(node);
		}

		private BamlPropertyValue ReadPropertyValue(short converterId, int dataSize, bool isValueType)
		{
			switch (converterId)
			{
				case 0:
				case 0x89:
					{
						if (dataSize == 2)
						{
							var property = GetProperty(_accessor.ReadInt16());
							return new BamlPropertyValueWithProperty(property);
						}
						else // isValueType=true
						{
							var type = GetType(_accessor.ReadInt16());
							string name = _accessor.ReadLengthPrefixedString(Encoding.UTF8);
							return new BamlPropertyValueWithProperty(name, type);
						}
					}

				case 0x2e:
					{
						return new BamlPropertyBoolValue(_accessor.ReadBoolean());
					}

				default:
					{
						byte[] data = _accessor.ReadBytes(dataSize);
						return new BamlPropertyCustomValue(data, (BamlPropertyValueType)converterId);
					}
			}
		}

		private int ReadRecordSize()
		{
			long position = _accessor.Position;
			int size = _accessor.Read7BitEncodedInt();
			int sizeSize = (int)(_accessor.Position - position);
			return size - sizeSize;
		}

		private void AddAssembly(short assemblyId, BamlAssemblyInfo assembly)
		{
			int assemblyCount = _assemblies.Count;
			if (assemblyId > assemblyCount)
			{
				throw new BamlException(SR.BamlLoadError);
			}

			if (assemblyId == assemblyCount)
			{
				_assemblies.Add(assembly);
			}
		}

		private IBamlAssembly GetAssembly(short assemblyId)
		{
			if (assemblyId >= 0 && assemblyId < _assemblies.Count)
			{
				return _assemblies[assemblyId];
			}
			else
			{
				var assemblyCode = (BamlKnownAssemblyCode)(-assemblyId);
				return new BamlKnownAssembly(assemblyCode);
			}
		}

		private void AddType(short typeId, BamlTypeInfo type)
		{
			int typeCount = _types.Count;
			if (typeId > typeCount)
			{
				throw new BamlException(SR.BamlLoadError);
			}

			if (typeId == typeCount)
			{
				_types.Add(type);
			}
		}

		private IBamlType GetType(short typeId)
		{
			if (typeId >= 0 && typeId < _types.Count)
			{
				return _types[typeId];
			}
			else
			{
				var typeCode = (BamlKnownTypeCode)(-typeId);
				return new BamlKnownType(typeCode);
			}
		}

		private void AddProperty(short propertyId, BamlPropertyInfo property)
		{
			int propertyCount = _properties.Count;
			if (propertyId > propertyCount)
			{
				throw new BamlException(SR.BamlLoadError);
			}

			if (propertyId == propertyCount)
			{
				_properties.Add(property);
			}
		}

		private IBamlProperty GetProperty(short propertyId)
		{
			if (propertyId >= 0 && propertyId < _properties.Count)
			{
				return _properties[propertyId];
			}
			else
			{
				var propertyCode = (BamlKnownPropertyCode)(-propertyId);
				return new BamlKnownProperty(propertyCode);
			}
		}

		private void AddString(short stringId, BamlStringInfo stringInfo)
		{
			int stringCount = _strings.Count;
			if (stringId > stringCount)
			{
				throw new BamlException(SR.BamlLoadError);
			}

			if (stringId == stringCount)
			{
				_strings.Add(stringInfo);
			}
		}

		private IBamlString GetString(short stringId)
		{
			if (stringId >= 0 && stringId < _strings.Count)
			{
				return _strings[stringId];
			}
			else
			{
				var stringCode = (BamlKnownStringCode)(-stringId);
				return new BamlKnownString(stringCode);
			}
		}

		private BamlExtensionValue GetStaticExtensionValue(short valueId)
		{
			if (valueId >= 0)
			{
				var property = GetProperty(valueId);
				return new BamlExtensionPropertyValue(property);
			}
			else
			{
				valueId = (short)-valueId;
				bool isKey;
				if (valueId > 0xe8)
				{
					valueId = (short)(valueId - 0xe8);
					isKey = false;
				}
				else
				{
					isKey = true;
				}

				var resourceID = (SystemResourceKeyID)valueId;

				return new BamlExtensionResourceValue(resourceID, isKey);
			}
		}

		private void AddNode(BamlNode node)
		{
			if (_currentScope.Current != null)
			{
				_currentScope.Current.AddNext(node);
				_currentScope.Current = node;
			}
			else
			{
				_currentScope.Current = node;

				if (_currentScope.Block != null)
				{
					_currentScope.Block.FirstChild = node;
				}
				else
				{
					_firstRecord = node;
				}
			}

			_positionToNode.Add(_currentRecordPosition, node);
			_nodeToPosition.Add(node, _currentRecordPosition);
		}

		private void AddKey(BamlNode node, int valuePosition)
		{
			if (_currentScope.DefKeys == null)
			{
				throw new BamlException(SR.BamlLoadError);
			}

			var key = new DefKeyNode()
			{
				Node = node,
				ValuePosition = valuePosition,
			};

			_currentScope.DefKeys.Add(key);
		}

		private void MapKeys()
		{
			foreach (var scope in _scopes)
			{
				if (scope.DefKeys == null || scope.DefKeys.Count == 0 || scope.Block.NodeType != BamlNodeType.DeferableContent)
					continue;

				int startPosition = GetKeyStartPosition(scope.Block.FirstChild);

				foreach (var key in scope.DefKeys)
				{
					int valuePosition = startPosition + key.ValuePosition;

					BamlNode valueNode;
					if (!_positionToNode.TryGetValue(valuePosition, out valueNode))
						continue;

					var node = key.Node;
					switch (node.NodeType)
					{
						case BamlNodeType.DefAttributeKeyString:
							{
								((BamlDefAttributeKeyString)node).ValueNode = valueNode;
							}
							break;

						case BamlNodeType.DefAttributeKeyType:
							{
								((BamlDefAttributeKeyType)node).ValueNode = valueNode;
							}
							break;

						case BamlNodeType.KeyElement:
							{
								((BamlKeyElement)node).ValueNode = valueNode;
							}
							break;

						default:
							throw new NotImplementedException();
					}
				}
			}
		}

		private int GetKeyStartPosition(BamlNode node)
		{
			bool found = false;

			while (node != null && !found)
			{
				switch (node.NodeType)
				{
					case BamlNodeType.DefAttributeKeyString:
					case BamlNodeType.DefAttributeKeyType:
					case BamlNodeType.KeyElement:
					case BamlNodeType.StaticResource:
					case BamlNodeType.OptimizedStaticResource:
						{
							node = node.NextSibling;
						}
						break;

					default:
						{
							found = true;
						}
						break;
				}
			}

			if (node == null)
				return 0;

			return _nodeToPosition[node];
		}

		private void BeginScope(BamlBlock block, RecordType endRecord)
		{
			var scope = new Scope()
			{
				Block = block,
				EndRecord = endRecord,
				DefKeys = _currentScope.DefKeys,
			};

			_scopeStack.Push(_currentScope);

			_currentScope = scope;

			_scopes.Add(scope);
		}

		private void BeginScope(BamlDeferableContent block, long streamLength)
		{
			var scope = new Scope()
			{
				Block = block,
				Length = streamLength,
				DefKeys = new List<DefKeyNode>(),
			};

			_scopeStack.Push(_currentScope);

			_currentScope = scope;

			_scopes.Add(scope);
		}

		private void EndScope()
		{
			if (_scopeStack.Count > 0)
			{
				_currentScope = _scopeStack.Pop();
			}
			else
			{
				_currentScope = null;
			}
		}

		private void EndScope(RecordType recordType)
		{
			if (_currentScope.EndRecord != recordType)
			{
				throw new BamlException(SR.BamlLoadError);
			}

			_currentScope = _scopeStack.Pop();
		}

		#endregion

		#region Nested types

		private class Scope
		{
			internal long Length = -1;
			internal RecordType EndRecord;
			internal BamlBlock Block;
			internal BamlNode Current;
			internal List<DefKeyNode> DefKeys;
		}

		private class DefKeyNode
		{
			internal BamlNode Node;
			internal int ValuePosition;
		}

		#endregion
	}
}
