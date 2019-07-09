using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using CA = AssemblyDefender.Net.CustomAttributes;

namespace AssemblyDefender
{
	public static class ObfuscationAttributeStripper
	{
		public static void Strip(BuildAssembly assembly)
		{
			if (assembly.StripObfuscationAttribute)
			{
				CA.ObfuscationAttribute.RemoveMarkedAsStrip(assembly.CustomAttributes);
			}

			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Strip(module);
			}
		}

		public static void Strip(BuildModule module)
		{
			if (module.StripObfuscationAttribute)
			{
				CA.ObfuscationAttribute.RemoveMarkedAsStrip(module.CustomAttributes);
			}

			foreach (BuildType type in module.Types)
			{
				Strip(type);
			}
		}

		public static void Strip(BuildType type)
		{
			if (type.IsMainType)
				return;

			if (type.StripObfuscationAttribute)
			{
				CA.ObfuscationAttribute.RemoveMarkedAsStrip(type.CustomAttributes);
			}

			foreach (BuildMethod method in type.Methods)
			{
				if (method.StripObfuscationAttribute)
				{
					CA.ObfuscationAttribute.RemoveMarkedAsStrip(method.CustomAttributes);
				}
			}

			foreach (BuildField field in type.Fields)
			{
				if (field.StripObfuscationAttribute)
				{
					CA.ObfuscationAttribute.RemoveMarkedAsStrip(field.CustomAttributes);
				}
			}

			foreach (BuildProperty property in type.Properties)
			{
				if (property.StripObfuscationAttribute)
				{
					CA.ObfuscationAttribute.RemoveMarkedAsStrip(property.CustomAttributes);
				}
			}

			foreach (BuildEvent e in type.Events)
			{
				if (e.StripObfuscationAttribute)
				{
					CA.ObfuscationAttribute.RemoveMarkedAsStrip(e.CustomAttributes);
				}
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Strip(nestedType);
			}
		}
	}
}
