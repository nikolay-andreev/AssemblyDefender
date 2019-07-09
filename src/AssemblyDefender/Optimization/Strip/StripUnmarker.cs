using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public static class StripUnmarker
	{
		public static void Unmark(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Unmark(module);
			}
		}

		public static void Unmark(BuildModule module)
		{
			foreach (BuildType type in module.Types)
			{
				Unmark(type);
			}
		}

		private static void Unmark(BuildType type)
		{
			if (type.Strip)
			{
				type.Rename = false;
				type.SealType = false;
				type.StripObfuscationAttribute = false;
				type.NameChanged = false;
				type.NamespaceChanged = false;
			}

			foreach (BuildMethod method in type.Methods)
			{
				Unmark(method);
			}

			foreach (BuildField field in type.Fields)
			{
				Unmark(field);
			}

			foreach (BuildProperty property in type.Properties)
			{
				Unmark(property);
			}

			foreach (BuildEvent e in type.Events)
			{
				Unmark(e);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Unmark(nestedType);
			}
		}

		private static void Unmark(BuildMethod method)
		{
			if (method.Strip)
			{
				method.DevirtualizeMethod = false;
				method.StripObfuscationAttribute = false;

				if (!method.StripEncrypted)
				{
					method.Rename = false;
					method.NameChanged = false;
					method.EncryptIL = false;
					method.ObfuscateControlFlow = false;
					method.ObfuscateStrings = false;
				}
			}
		}

		private static void Unmark(BuildField field)
		{
			if (field.Strip)
			{
				field.Rename = false;
				field.ObfuscateStrings = false;
				field.StripObfuscationAttribute = false;
				field.NameChanged = false;
			}
		}

		private static void Unmark(BuildProperty property)
		{
			if (property.Strip)
			{
				property.Rename = false;
				property.StripObfuscationAttribute = false;
				property.NameChanged = false;
			}
		}

		private static void Unmark(BuildEvent e)
		{
			if (e.Strip)
			{
				e.Rename = false;
				e.StripObfuscationAttribute = false;
				e.NameChanged = false;
			}
		}
	}
}
