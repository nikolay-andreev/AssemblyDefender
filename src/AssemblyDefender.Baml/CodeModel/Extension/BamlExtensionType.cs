using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	/// <summary>
	/// Represents extension type. Value is defined in known types with negative sign.
	/// </summary>
	/// <example>
	/// To get known type code: (BamlKnownTypeCode)-extensionType
	/// </example>
	public enum BamlExtensionType
	{
		StaticExtension = 0x25a,
		StaticResource = 0x25b,
		DynamicResource = 0xbd,
		TemplateBinding = 0x27a,
		Type = 0x2b3,
	}
}
