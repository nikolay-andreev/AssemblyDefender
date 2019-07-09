using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public enum BamlNodeType : byte
	{
		Document,
		Element,
		NamedElement,
		Property,
		PropertyCustom,
		PropertyComplex,
		PropertyArray,
		PropertyIList,
		PropertyIDictionary,
		LiteralContent,
		Text,
		TextWithConverter,
		RoutedEvent,
		ClrEvent,
		XmlnsProperty,
		XmlAttribute,
		ProcessingInstruction,
		Comment,
		DefTag,
		DefAttribute,
		EndAttributes,
		PIMapping,
		AssemblyInfo,
		TypeInfo,
		TypeSerializerInfo,
		AttributeInfo,
		StringInfo,
		PropertyStringReference,
		PropertyTypeReference,
		PropertyWithExtension,
		PropertyWithConverter,
		DeferableContent,
		DefAttributeKeyString,
		DefAttributeKeyType,
		KeyElement,
		ConstructorParameters,
		ConstructorParameterType,
		ConnectionId,
		ContentProperty,
		StaticResource,
		StaticResourceId,
		TextWithId,
		PresentationOptionsAttribute,
		LineNumberAndPosition,
		LinePosition,
		OptimizedStaticResource,
		PropertyWithStaticResourceId,
	}
}
