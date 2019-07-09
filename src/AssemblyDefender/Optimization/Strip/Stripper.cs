using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public static class Stripper
	{
		public static void Strip(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Strip(module);
			}
		}

		public static void Strip(BuildModule module)
		{
			Strip(module.Types);
		}

		private static void Strip(TypeDeclarationCollection types)
		{
			for (int i = types.Count - 1; i >= 0; i--)
			{
				var type = (BuildType)types[i];
				if (type.Strip)
				{
					types.RemoveAt(i);
				}
				else
				{
					Strip(type.Methods);
					Strip(type.Fields);
					Strip(type.Properties);
					Strip(type.Events);
					Strip(type.NestedTypes);
				}
			}
		}

		private static void Strip(MethodDeclarationCollection methods)
		{
			for (int i = methods.Count - 1; i >= 0; i--)
			{
				var method = (BuildMethod)methods[i];
				if (method.Strip)
				{
					methods.RemoveAt(i);
				}
			}
		}

		private static void Strip(FieldDeclarationCollection fields)
		{
			for (int i = fields.Count - 1; i >= 0; i--)
			{
				var field = (BuildField)fields[i];
				if (field.Strip)
				{
					fields.RemoveAt(i);
				}
			}
		}

		private static void Strip(PropertyDeclarationCollection properties)
		{
			for (int i = properties.Count - 1; i >= 0; i--)
			{
				var property = (BuildProperty)properties[i];
				if (property.Strip)
				{
					properties.RemoveAt(i);
				}
			}
		}

		private static void Strip(EventDeclarationCollection events)
		{
			for (int i = events.Count - 1; i >= 0; i--)
			{
				var e = (BuildEvent)events[i];
				if (e.Strip)
				{
					events.RemoveAt(i);
				}
			}
		}
	}
}
