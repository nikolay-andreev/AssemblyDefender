using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Baml
{
	internal class BamlImageBuilder
	{
		#region Fields

		private int _pos;
		private Blob _blob;
		private int _tempPos;
		private Blob _tempBlob;
		private BamlImage _image;
		private Scope _currentScope;
		private Dictionary<BamlAssemblyInfo, short> _assemblyToID = new Dictionary<BamlAssemblyInfo, short>();
		private Dictionary<BamlTypeInfo, short> _typeToID = new Dictionary<BamlTypeInfo, short>();
		private Dictionary<BamlPropertyInfo, short> _propertyToID = new Dictionary<BamlPropertyInfo, short>();
		private Dictionary<BamlStringInfo, short> _stringToID = new Dictionary<BamlStringInfo, short>();
		private Dictionary<BamlNode, int> _recordToPosition = new Dictionary<BamlNode, int>(0x40);
		private List<Scope> _scopes = new List<Scope>();
		private Stack<Scope> _scopeStack = new Stack<Scope>();

		#endregion

		#region Ctors

		internal BamlImageBuilder(BamlImage image)
		{
			_image = image;
		}

		#endregion

		#region Methods

		internal Blob Build()
		{
			_pos = 0;
			_blob = new Blob();
			_tempBlob = new Blob();

			WriteHeader();

			WriteRecords();

			MapKeys();

			_tempBlob = null;

			return _blob;
		}

		private void WriteHeader()
		{
			byte[] featureBytes = Encoding.Unicode.GetBytes(_image.FeatureID);
			_blob.Write(ref _pos, (int)featureBytes.Length);
			_blob.Write(ref _pos, (byte[])featureBytes);
			WriteVersionPair(_image.ReaderVersion);
			WriteVersionPair(_image.UpdaterVersion);
			WriteVersionPair(_image.WriterVersion);
		}

		private void WriteRecords()
		{
			_currentScope =
				new Scope()
				{
					Current = _image.FirstNode,
				};

			_scopes.Add(_currentScope);

			while (_currentScope != null)
			{
				while (_currentScope.Current != null)
				{
					var node = _currentScope.Current;
					_currentScope.Current = node.NextSibling;
					_recordToPosition.Add(node, _pos);
					Write(node);
				}

				EndScope();
			}
		}

		private void Write(BamlNode node)
		{
			switch (node.NodeType)
			{
				case BamlNodeType.Document:
					WriteDocument((BamlDocument)node);
					break;

				case BamlNodeType.Element:
					WriteElement((BamlElement)node);
					break;

				case BamlNodeType.Property:
					WriteProperty((BamlProperty)node);
					break;

				case BamlNodeType.PropertyCustom:
					WritePropertyCustom((BamlPropertyCustom)node);
					break;

				case BamlNodeType.PropertyComplex:
					WritePropertyComplex((BamlPropertyComplex)node);
					break;

				case BamlNodeType.PropertyArray:
					WritePropertyArray((BamlPropertyArray)node);
					break;

				case BamlNodeType.PropertyIList:
					WritePropertyIList((BamlPropertyIList)node);
					break;

				case BamlNodeType.PropertyIDictionary:
					WritePropertyIDictionary((BamlPropertyIDictionary)node);
					break;

				case BamlNodeType.LiteralContent:
					WriteLiteralContent((BamlLiteralContent)node);
					break;

				case BamlNodeType.Text:
					WriteText((BamlText)node);
					break;

				case BamlNodeType.TextWithConverter:
					WriteTextWithConverter((BamlTextWithConverter)node);
					break;

				case BamlNodeType.RoutedEvent:
					WriteRoutedEvent((BamlRoutedEvent)node);
					break;

				case BamlNodeType.ClrEvent:
					throw new BamlException(SR.BamlLoadError);

				case BamlNodeType.XmlnsProperty:
					WriteXmlnsProperty((BamlXmlnsProperty)node);
					break;

				case BamlNodeType.XmlAttribute:
					throw new BamlException(SR.BamlLoadError);

				case BamlNodeType.ProcessingInstruction:
					throw new BamlException(SR.BamlLoadError);

				case BamlNodeType.Comment:
					throw new BamlException(SR.BamlLoadError);

				case BamlNodeType.DefTag:
					throw new BamlException(SR.BamlLoadError);

				case BamlNodeType.DefAttribute:
					WriteDefAttribute((BamlDefAttribute)node);
					break;

				case BamlNodeType.EndAttributes:
					throw new BamlException(SR.BamlLoadError);

				case BamlNodeType.PIMapping:
					WritePIMapping((BamlPIMapping)node);
					break;

				case BamlNodeType.AssemblyInfo:
					WriteAssemblyInfo((BamlAssemblyInfo)node);
					break;

				case BamlNodeType.TypeInfo:
					WriteTypeInfo((BamlTypeInfo)node);
					break;

				case BamlNodeType.TypeSerializerInfo:
					WriteTypeSerializerInfo((BamlTypeSerializerInfo)node);
					break;

				case BamlNodeType.AttributeInfo:
					WritePropertyInfo((BamlPropertyInfo)node);
					break;

				case BamlNodeType.StringInfo:
					WriteStringInfo((BamlStringInfo)node);
					break;

				case BamlNodeType.PropertyStringReference:
					WritePropertyStringReference((BamlPropertyStringReference)node);
					break;

				case BamlNodeType.PropertyTypeReference:
					WritePropertyTypeReference((BamlPropertyTypeReference)node);
					break;

				case BamlNodeType.PropertyWithExtension:
					WritePropertyWithExtension((BamlPropertyWithExtension)node);
					break;

				case BamlNodeType.PropertyWithConverter:
					WritePropertyWithConverter((BamlPropertyWithConverter)node);
					break;

				case BamlNodeType.DeferableContent:
					WriteDeferableContent((BamlDeferableContent)node);
					break;

				case BamlNodeType.DefAttributeKeyString:
					WriteDefAttributeKeyString((BamlDefAttributeKeyString)node);
					break;

				case BamlNodeType.DefAttributeKeyType:
					WriteDefAttributeKeyType((BamlDefAttributeKeyType)node);
					break;

				case BamlNodeType.KeyElement:
					WriteKeyElement((BamlKeyElement)node);
					break;

				case BamlNodeType.ConstructorParameters:
					WriteConstructorParameters((BamlConstructorParameters)node);
					break;

				case BamlNodeType.ConstructorParameterType:
					WriteConstructorParameterType((BamlConstructorParameterType)node);
					break;

				case BamlNodeType.ConnectionId:
					WriteConnectionId((BamlConnectionId)node);
					break;

				case BamlNodeType.ContentProperty:
					WriteContentProperty((BamlContentProperty)node);
					break;

				case BamlNodeType.NamedElement:
					WriteNamedElement((BamlNamedElement)node);
					break;

				case BamlNodeType.StaticResource:
					WriteStaticResource((BamlStaticResource)node);
					break;

				case BamlNodeType.StaticResourceId:
					WriteStaticResourceId((BamlStaticResourceId)node);
					break;

				case BamlNodeType.TextWithId:
					WriteTextWithId((BamlTextWithId)node);
					break;

				case BamlNodeType.PresentationOptionsAttribute:
					WritePresentationOptionsAttribute((BamlPresentationOptionsAttribute)node);
					break;

				case BamlNodeType.LineNumberAndPosition:
					WriteLineNumberAndPosition((BamlLineNumberAndPosition)node);
					break;

				case BamlNodeType.LinePosition:
					WriteLinePosition((BamlLinePosition)node);
					break;

				case BamlNodeType.OptimizedStaticResource:
					WriteOptimizedStaticResource((BamlOptimizedStaticResource)node);
					break;

				case BamlNodeType.PropertyWithStaticResourceId:
					WritePropertyWithStaticResourceId((BamlPropertyWithStaticResourceId)node);
					break;

				default:
					throw new NotImplementedException();
			}
		}

		private void WriteDocument(BamlDocument node)
		{
			_blob.Write(ref _pos, (byte)RecordType.DocumentStart);
			_blob.Write(ref _pos, (bool)node.LoadAsync);
			_blob.Write(ref _pos, (int)node.MaxAsyncRecords);
			_blob.Write(ref _pos, (bool)node.DebugBaml);

			BeginScope(node, RecordType.DocumentEnd);
		}

		private void WriteElement(BamlElement node)
		{
			_blob.Write(ref _pos, (byte)RecordType.ElementStart);
			_blob.Write(ref _pos, (short)GetTypeId(node.Type));
			_blob.Write(ref _pos, (byte)node.Flags);

			BeginScope(node, RecordType.ElementEnd);
		}

		private void WriteProperty(BamlProperty node)
		{
			BeginVarSize();

			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Value);

			WriteVarSize(RecordType.Property);
		}

		private void WritePropertyCustom(BamlPropertyCustom node)
		{
			BeginVarSize();

			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));

			var value = node.Value;

			short converterId = (short)value.ValueType;
			if (node.IsValueType)
			{
				converterId |= 0x4000;
			}

			_blob.Write(ref _pos, (short)converterId);

			switch (value.ValueType)
			{
				case BamlPropertyValueType.Property:
					{
						var propertyValue = (BamlPropertyValueWithProperty)value;
						if (propertyValue.Property != null)
						{
							_blob.Write(ref _pos, (short)GetPropertyId(propertyValue.Property));
						}
						else
						{
							_blob.Write(ref _pos, (short)GetTypeId(propertyValue.Type));
							_blob.WriteLengthPrefixedString(ref _pos, (string)propertyValue.Name);
						}
					}
					break;

				case BamlPropertyValueType.Boolean:
					{
						var boolValue = (BamlPropertyBoolValue)value;
						_blob.Write(ref _pos, (bool)boolValue.Value);
					}
					break;

				default:
					{
						var customValue = (BamlPropertyCustomValue)value;
						_blob.Write(ref _pos, customValue.Data);
					}
					break;
			}

			WriteVarSize(RecordType.PropertyCustom);
		}

		private void WritePropertyComplex(BamlPropertyComplex node)
		{
			_blob.Write(ref _pos, (byte)RecordType.PropertyComplexStart);
			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));

			BeginScope(node, RecordType.PropertyComplexEnd);
		}

		private void WritePropertyArray(BamlPropertyArray node)
		{
			_blob.Write(ref _pos, (byte)RecordType.PropertyArrayStart);
			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));

			BeginScope(node, RecordType.PropertyArrayEnd);
		}

		private void WritePropertyIList(BamlPropertyIList node)
		{
			_blob.Write(ref _pos, (byte)RecordType.PropertyIListStart);
			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));

			BeginScope(node, RecordType.PropertyIListEnd);
		}

		private void WritePropertyIDictionary(BamlPropertyIDictionary node)
		{
			_blob.Write(ref _pos, (byte)RecordType.PropertyIDictionaryStart);
			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));

			BeginScope(node, RecordType.PropertyIDictionaryEnd);
		}

		private void WriteLiteralContent(BamlLiteralContent node)
		{
			BeginVarSize();

			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Value);
			_blob.Write(ref _pos, (int)node.Num1);
			_blob.Write(ref _pos, (int)node.Num2);

			WriteVarSize(RecordType.LiteralContent);
		}

		private void WriteText(BamlText node)
		{
			BeginVarSize();

			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Value);

			WriteVarSize(RecordType.Text);
		}

		private void WriteTextWithConverter(BamlTextWithConverter node)
		{
			BeginVarSize();

			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Value);
			_blob.Write(ref _pos, (short)GetTypeId(node.ConverterType));

			WriteVarSize(RecordType.TextWithConverter);
		}

		private void WriteRoutedEvent(BamlRoutedEvent node)
		{
			BeginVarSize();

			_blob.Write(ref _pos, (short)GetPropertyId(node.Property));
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Value);

			WriteVarSize(RecordType.RoutedEvent);
		}

		private void WriteXmlnsProperty(BamlXmlnsProperty node)
		{
			BeginVarSize();

			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Prefix);
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.XmlNamespace);

			var assemblies = node.Assemblies;

			_blob.Write(ref _pos, (short)assemblies.Count);

			foreach (var assembly in assemblies)
			{
				_blob.Write(ref _pos, (short)GetAssemblyId(assembly));
			}

			WriteVarSize(RecordType.XmlnsProperty);
		}

		private void WriteDefAttribute(BamlDefAttribute node)
		{
			BeginVarSize();

			_blob.WriteLengthPrefixedString(ref _pos, node.Value, Encoding.UTF8);
			_blob.Write(ref _pos, (short)GetStringId(node.Name));

			WriteVarSize(RecordType.DefAttribute);
		}

		private void WritePIMapping(BamlPIMapping node)
		{
			BeginVarSize();

			_blob.WriteLengthPrefixedString(ref _pos, (string)node.XmlNamespace);
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.ClrNamespace);
			_blob.Write(ref _pos, (short)GetAssemblyId(node.Assembly));

			WriteVarSize(RecordType.PIMapping);
		}

		private void WriteAssemblyInfo(BamlAssemblyInfo record)
		{
			BeginVarSize();

			short asssemblyId = AddAssembly(record);
			_blob.Write(ref _pos, (short)asssemblyId);
			_blob.WriteLengthPrefixedString(ref _pos, record.Name);

			WriteVarSize(RecordType.AssemblyInfo);
		}

		private void WriteTypeInfo(BamlTypeInfo node)
		{
			BeginVarSize();

			short typeId = AddType(node);
			_blob.Write(ref _pos, (short)typeId);
			_blob.Write(ref _pos, (short)((ushort)GetAssemblyId(node.Assembly) | ((byte)node.Flags) << 12));
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Name);

			WriteVarSize(RecordType.TypeInfo);
		}

		private void WriteTypeSerializerInfo(BamlTypeSerializerInfo node)
		{
			BeginVarSize();

			short typeId = AddType(node);
			_blob.Write(ref _pos, (short)typeId);
			_blob.Write(ref _pos, (short)((ushort)GetAssemblyId(node.Assembly) | ((byte)node.Flags) << 12));
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Name);
			_blob.Write(ref _pos, (short)GetTypeId(node.SerializerType));

			WriteVarSize(RecordType.TypeSerializerInfo);
		}

		private void WritePropertyInfo(BamlPropertyInfo node)
		{
			BeginVarSize();

			short propertyId = AddProperty(node);
			_blob.Write(ref _pos, (short)propertyId);
			_blob.Write(ref _pos, (short)GetTypeId(node.Type));
			_blob.Write(ref _pos, (byte)node.Usage);
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Name);

			WriteVarSize(RecordType.AttributeInfo);
		}

		private void WriteStringInfo(BamlStringInfo node)
		{
			BeginVarSize();

			short stringId = AddString(node);
			_blob.Write(ref _pos, (short)stringId);
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Value);

			WriteVarSize(RecordType.StringInfo);
		}

		private void WritePropertyStringReference(BamlPropertyStringReference node)
		{
			_blob.Write(ref _pos, (byte)RecordType.PropertyStringReference);
			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));
			_blob.Write(ref _pos, (short)GetStringId(node.Value));
		}

		private void WritePropertyTypeReference(BamlPropertyTypeReference node)
		{
			_blob.Write(ref _pos, (byte)RecordType.PropertyTypeReference);
			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));
			_blob.Write(ref _pos, (short)GetTypeId(node.Value));
		}

		private void WritePropertyWithExtension(BamlPropertyWithExtension node)
		{
			_blob.Write(ref _pos, (byte)RecordType.PropertyWithExtension);
			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));

			var extension = node.Extension;

			short flags = (short)extension.Type;

			if (node.IsValueType)
				flags |= 0x4000;

			if (node.IsStaticType)
				flags |= 0x2000;

			_blob.Write(ref _pos, (short)flags);
			_blob.Write(ref _pos, (short)GetExtensionValueId(extension.Value));
		}

		private void WritePropertyWithConverter(BamlPropertyWithConverter node)
		{
			BeginVarSize();

			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Value);
			_blob.Write(ref _pos, (short)GetTypeId(node.ConverterType));

			WriteVarSize(RecordType.PropertyWithConverter);
		}

		private void WriteDeferableContent(BamlDeferableContent node)
		{
			_blob.Write(ref _pos, (byte)RecordType.DeferableContentStart);
			_pos += 4;

			BeginScope(node);
		}

		private void WriteDeferableContentEnd(BamlDeferableContent node)
		{
			int position = _recordToPosition[node];
			position++; // Record type
			int size = _pos - (position + 4);
			_blob.Write(ref position, (int)size);
		}

		private void WriteDefAttributeKeyString(BamlDefAttributeKeyString node)
		{
			BeginVarSize();

			_blob.Write(ref _pos, (short)GetStringId(node.Value));
			_pos += 4; // ValuePosition
			_blob.Write(ref _pos, (bool)node.Shared);
			_blob.Write(ref _pos, (bool)node.SharedSet);

			WriteVarSize(RecordType.DefAttributeKeyString);

			AddKey(_pos - 6, node.ValueNode);
		}

		private void WriteDefAttributeKeyType(BamlDefAttributeKeyType node)
		{
			_blob.Write(ref _pos, (byte)RecordType.DefAttributeKeyType);
			_blob.Write(ref _pos, (short)GetTypeId(node.Value));
			_blob.Write(ref _pos, (byte)node.TypeFlags);
			_pos += 4; // ValuePosition
			_blob.Write(ref _pos, (bool)node.Shared);
			_blob.Write(ref _pos, (bool)node.SharedSet);

			AddKey(_pos - 6, node.ValueNode);
		}

		private void WriteKeyElement(BamlKeyElement node)
		{
			_blob.Write(ref _pos, (byte)RecordType.KeyElementStart);
			_blob.Write(ref _pos, (short)GetTypeId(node.Type));
			_blob.Write(ref _pos, (byte)node.TypeFlags);
			_pos += 4;
			_blob.Write(ref _pos, (bool)node.Shared);
			_blob.Write(ref _pos, (bool)node.SharedSet);

			AddKey(_pos - 6, node.ValueNode);
			BeginScope(node, RecordType.KeyElementEnd);
		}

		private void WriteConstructorParameters(BamlConstructorParameters node)
		{
			_blob.Write(ref _pos, (byte)RecordType.ConstructorParametersStart);

			BeginScope(node, RecordType.ConstructorParametersEnd);
		}

		private void WriteConstructorParameterType(BamlConstructorParameterType node)
		{
			_blob.Write(ref _pos, (byte)RecordType.ConstructorParameterType);
			_blob.Write(ref _pos, (short)GetTypeId(node.Type));
		}

		private void WriteConnectionId(BamlConnectionId node)
		{
			_blob.Write(ref _pos, (byte)RecordType.ConnectionId);
			_blob.Write(ref _pos, (int)node.Value);
		}

		private void WriteContentProperty(BamlContentProperty node)
		{
			_blob.Write(ref _pos, (byte)RecordType.ContentProperty);
			_blob.Write(ref _pos, (short)GetPropertyId(node.Property));
		}

		private void WriteNamedElement(BamlNamedElement node)
		{
			_blob.Write(ref _pos, (byte)RecordType.NamedElementStart);
			_blob.Write(ref _pos, (short)GetTypeId(node.Type));
			_blob.WriteLengthPrefixedString(ref _pos, (string)node.RuntimeName);

			BeginScope(node, RecordType.ElementEnd);
		}

		private void WriteStaticResource(BamlStaticResource node)
		{
			_blob.Write(ref _pos, (byte)RecordType.StaticResourceStart);
			_blob.Write(ref _pos, (short)GetTypeId(node.Type));
			_blob.Write(ref _pos, (byte)node.Flags);

			BeginScope(node, RecordType.StaticResourceEnd);
		}

		private void WriteStaticResourceId(BamlStaticResourceId node)
		{
			_blob.Write(ref _pos, (byte)RecordType.StaticResourceId);
			_blob.Write(ref _pos, (short)node.Value);
		}

		private void WriteTextWithId(BamlTextWithId node)
		{
			BeginVarSize();

			_blob.Write(ref _pos, (short)GetStringId(node.Value));

			WriteVarSize(RecordType.TextWithId);
		}

		private void WritePresentationOptionsAttribute(BamlPresentationOptionsAttribute node)
		{
			BeginVarSize();

			_blob.WriteLengthPrefixedString(ref _pos, (string)node.Value);
			_blob.Write(ref _pos, (short)GetStringId(node.Name));

			WriteVarSize(RecordType.PresentationOptionsAttribute);
		}

		private void WriteLineNumberAndPosition(BamlLineNumberAndPosition node)
		{
			_blob.Write(ref _pos, (byte)RecordType.LineNumberAndPosition);
			_blob.Write(ref _pos, (int)node.LineNumber);
			_blob.Write(ref _pos, (int)node.LineOffset);
		}

		private void WriteLinePosition(BamlLinePosition node)
		{
			_blob.Write(ref _pos, (byte)RecordType.LinePosition);
			_blob.Write(ref _pos, (int)node.LineOffset);
		}

		private void WriteOptimizedStaticResource(BamlOptimizedStaticResource node)
		{
			_blob.Write(ref _pos, (byte)RecordType.OptimizedStaticResource);
			_blob.Write(ref _pos, (byte)node.Flags);
			_blob.Write(ref _pos, (short)GetExtensionValueId(node.Value));
		}

		private void WritePropertyWithStaticResourceId(BamlPropertyWithStaticResourceId node)
		{
			_blob.Write(ref _pos, (byte)RecordType.PropertyWithStaticResourceId);
			_blob.Write(ref _pos, (short)GetPropertyId(node.DeclaringProperty));
			_blob.Write(ref _pos, (short)node.StaticResourceId);
		}

		private void BeginVarSize()
		{
			var blob = _tempBlob;

			_tempPos = _pos;
			_tempBlob = _blob;

			_blob = blob;
			_pos = 0;
		}

		private void WriteVarSize(RecordType recordType)
		{
			int recordSize = _pos;

			var blob = _blob;

			_pos = _tempPos;
			_blob = _tempBlob;

			_tempBlob = blob;

			_blob.Write(ref _pos, (byte)recordType);
			WriteRecordSize(recordSize);
			_blob.Write(ref _pos, _tempBlob.GetBuffer(), 0, recordSize);
		}

		private void WriteVersionPair(VersionPair versionPair)
		{
			_blob.Write(ref _pos, (short)versionPair.Major);
			_blob.Write(ref _pos, (short)versionPair.Minor);
		}

		private void WriteRecordSize(int size)
		{
			int recordSize = (SizeOf7bitEncodedSize(SizeOf7bitEncodedSize(size) + size) + size);
			_blob.Write7BitEncodedInt(ref _pos, recordSize);
		}

		private short GetAssemblyId(IBamlAssembly assembly)
		{
			switch (assembly.Kind)
			{
				case BamlAssemblyKind.Declaration:
					{
						short assemblyId;
						if (!_assemblyToID.TryGetValue((BamlAssemblyInfo)assembly, out assemblyId))
						{
							throw new BamlException(SR.BamlLoadError);
						}

						return assemblyId;
					}

				case BamlAssemblyKind.Known:
					{
						short assemblyId = (short)((BamlKnownAssembly)assembly).KnownCode;
						return (short)-assemblyId;
					}

				default:
					throw new NotImplementedException();
			}
		}

		private short GetTypeId(IBamlType type)
		{
			switch (type.Kind)
			{
				case BamlTypeKind.Declaration:
					{
						short typeId;
						if (!_typeToID.TryGetValue((BamlTypeInfo)type, out typeId))
						{
							throw new BamlException(SR.BamlLoadError);
						}

						return typeId;
					}

				case BamlTypeKind.Known:
					{
						short typeId = (short)((BamlKnownType)type).KnownCode;
						return (short)-typeId;
					}

				default:
					throw new NotImplementedException();
			}
		}

		private short GetPropertyId(IBamlProperty property)
		{
			switch (property.Kind)
			{
				case BamlPropertyKind.Declaration:
					{
						short propertyId;
						if (!_propertyToID.TryGetValue((BamlPropertyInfo)property, out propertyId))
						{
							throw new BamlException(SR.BamlLoadError);
						}

						return propertyId;
					}

				case BamlPropertyKind.Known:
					{
						short propertyId = (short)((BamlKnownProperty)property).KnownCode;
						return (short)-propertyId;
					}

				default:
					throw new NotImplementedException();
			}
		}

		private short GetStringId(IBamlString stringInfo)
		{
			switch (stringInfo.Kind)
			{
				case BamlStringKind.Declaration:
					{
						short stringId;
						if (!_stringToID.TryGetValue((BamlStringInfo)stringInfo, out stringId))
						{
							throw new BamlException(SR.BamlLoadError);
						}

						return stringId;
					}

				case BamlStringKind.Known:
					{
						short stringId = (short)((BamlKnownString)stringInfo).KnownCode;
						return (short)-stringId;
					}

				default:
					throw new NotImplementedException();
			}
		}

		private short GetExtensionValueId(BamlExtensionValue extensionValue)
		{
			switch (extensionValue.ValueType)
			{
				case BamlExtensionValueType.Property:
					return GetPropertyId(((BamlExtensionPropertyValue)extensionValue).Value);

				case BamlExtensionValueType.Resource:
					{
						var resourceExtensionValue = (BamlExtensionResourceValue)extensionValue;

						short valueId = (short)resourceExtensionValue.Value;

						if (!resourceExtensionValue.IsKey)
							valueId += 0xe8;

						return (short)-valueId;
					}

				case BamlExtensionValueType.String:
					return GetStringId(((BamlExtensionStringValue)extensionValue).Value);

				case BamlExtensionValueType.Type:
					return GetTypeId(((BamlExtensionTypeValue)extensionValue).Value);

				default:
					throw new NotImplementedException();
			}
		}

		private short AddAssembly(BamlAssemblyInfo asembly)
		{
			short assemblyId = (short)_assemblyToID.Count;
			_assemblyToID.Add(asembly, assemblyId);

			return assemblyId;
		}

		private short AddType(BamlTypeInfo type)
		{
			short typeId = (short)_typeToID.Count;
			_typeToID.Add(type, typeId);

			return typeId;
		}

		private short AddProperty(BamlPropertyInfo property)
		{
			short propertyId = (short)_propertyToID.Count;
			_propertyToID.Add(property, propertyId);

			return propertyId;
		}

		private short AddString(BamlStringInfo stringInfo)
		{
			short stringId = (short)_stringToID.Count;
			_stringToID.Add(stringInfo, stringId);

			return stringId;
		}

		private void BeginScope(BamlBlock block, RecordType endRecord)
		{
			var scope = new Scope()
			{
				Block = block,
				Current = block.FirstChild,
				EndRecord = endRecord,
				DefKeys = _currentScope.DefKeys,
			};

			_scopeStack.Push(_currentScope);

			_currentScope = scope;

			_scopes.Add(scope);
		}

		private void BeginScope(BamlDeferableContent block)
		{
			var scope = new Scope()
			{
				Block = block,
				Current = block.FirstChild,
				DefKeys = new List<DefKeyNode>(),
			};

			_scopeStack.Push(_currentScope);

			_currentScope = scope;

			_scopes.Add(scope);
		}

		private void EndScope()
		{
			if (_currentScope.Block != null)
			{
				if (_currentScope.Block.NodeType == BamlNodeType.DeferableContent)
				{
					WriteDeferableContentEnd((BamlDeferableContent)_currentScope.Block);
				}
				else
				{
					_blob.Write(ref _pos, (byte)_currentScope.EndRecord);
				}
			}

			if (_scopeStack.Count > 0)
			{
				_currentScope = _scopeStack.Pop();
			}
			else
			{
				_currentScope = null;
			}
		}

		private void AddKey(int position, BamlNode value)
		{
			if (_currentScope.DefKeys == null)
			{
				throw new BamlException(SR.BamlLoadError);
			}

			var key = new DefKeyNode()
			{
				Position = position,
				Value = value,
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
					int valuePosition = _recordToPosition[key.Value] - startPosition;
					if (valuePosition < 0)
					{
						throw new BamlException(SR.BamlLoadError);
					}

					int pos = key.Position;

					_blob.Write(ref pos, (int)valuePosition);
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

			return _recordToPosition[node];
		}

		private static int SizeOf7bitEncodedSize(int size)
		{
			if ((size & -128) == 0)
				return 1;

			if ((size & -16384) == 0)
				return 2;

			if ((size & -2097152) == 0)
				return 3;

			if ((size & -268435456) == 0)
				return 4;

			return 5;
		}

		#endregion

		#region Nested types

		private class Scope
		{
			internal BamlBlock Block;
			internal BamlNode Current;
			internal RecordType EndRecord;
			internal List<DefKeyNode> DefKeys;
		}

		private class DefKeyNode
		{
			internal int Position;
			internal BamlNode Value;
		}

		#endregion
	}
}
