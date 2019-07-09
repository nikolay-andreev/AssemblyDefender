using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Baml;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	public class DependencyPropertyAnalizer
	{
		#region Fields

		private BuildAssembly _assembly;

		#endregion

		#region Ctors

		private DependencyPropertyAnalizer(BuildAssembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_assembly = assembly;
		}

		#endregion

		#region Methods

		private void Analyze()
		{
			foreach (BuildModule module in _assembly.Modules)
			{
				Analyze(module);
			}
		}

		private void Analyze(BuildModule module)
		{
			foreach (BuildType type in module.Types)
			{
				Analyze(type);
			}
		}

		private void Analyze(BuildType type)
		{
			foreach (BuildField field in type.Fields)
			{
				Analyze(field);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Analyze(nestedType);
			}
		}

		private void Analyze(BuildField field)
		{
			if (field.IsStatic &&
				field.Name.EndsWith("Property") &&
				IsDependencyPropertyType(field.FieldType))
			{
				field.Rename = false;
				field.Strip = false;

				var ownerType = (BuildType)field.GetOwnerType();
				string name = field.Name.Substring(0, field.Name.Length - 8); // Remove 'Property' suffix

				// Fix methods used by extension properties
				foreach (BuildMethod method in ownerType.Methods)
				{
					if ((method.Name == "Get" + name) || (method.Name == "Set" + name))
					{
						method.Rename = false;
						method.Strip = false;
					}
				}

				// Fix properties releated to this dependency property
				foreach (BuildProperty property in ownerType.Properties)
				{
					if (property.Name == name)
					{
						property.Rename = false;
						property.Strip = false;
					}
				}
			}
		}

		private bool IsDependencyPropertyType(TypeSignature typeSig)
		{
			if (typeSig.Name != "DependencyProperty")
				return false;

			if (typeSig.Namespace != "System.Windows")
				return false;

			return true;
		}

		#endregion

		#region Static

		public static void Analyze(BuildAssembly assembly)
		{
			var analyzer = new DependencyPropertyAnalizer(assembly);
			analyzer.Analyze();
		}

		#endregion
	}
}
