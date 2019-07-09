using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public enum BamlPropertyValueType
	{
		Property = 0x89,
		Enum = 0xc3,
		Boolean = 0x2e,
		SolidColorBrush = 0x2e8,
		Int32Collection = 0x2e9,
		Path = 0x2ea,
		Point3DCollection = 0x2eb,
		PointCollection = 0x2ec,
		Vector3DCollection = 0x2f0,
	}
}
