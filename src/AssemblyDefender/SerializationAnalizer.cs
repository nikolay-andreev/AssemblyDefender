using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using CA = AssemblyDefender.Net.CustomAttributes;

namespace AssemblyDefender
{
	public class SerializationAnalizer
	{
		#region Fields

		private bool _renameSerializableMembers;
		private BuildAssembly _assembly;

		#endregion

		#region Ctors

		private SerializationAnalizer(BuildAssembly assembly, bool renameSerializableMembers)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_assembly = assembly;
			_renameSerializableMembers = renameSerializableMembers;
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
				Analyze(type, false);
			}
		}

		private void Analyze(BuildType type, bool serializable)
		{
			LoadSerializableAttributes(ref serializable, type.CustomAttributes);

			if (serializable)
			{
				if (!_renameSerializableMembers)
				{
					type.Rename = false;
				}

				type.Strip = false;
			}

			foreach (BuildMethod method in type.Methods)
			{
				Analyze(method, serializable);
			}

			foreach (BuildField field in type.Fields)
			{
				Analyze(field, serializable);
			}

			foreach (BuildProperty property in type.Properties)
			{
				Analyze(property, serializable);
			}

			foreach (BuildEvent e in type.Events)
			{
				Analyze(e, serializable);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Analyze(nestedType, serializable);
			}
		}

		private void Analyze(BuildMethod method, bool serializable)
		{
			LoadSerializableAttributes(ref serializable, method.CustomAttributes);

			if (serializable)
			{
				if (!_renameSerializableMembers)
				{
					method.Rename = false;
				}

				method.Strip = false;
			}
		}

		private void Analyze(BuildField field, bool serializable)
		{
			LoadSerializableAttributes(ref serializable, field.CustomAttributes);

			if (serializable)
			{
				if (!_renameSerializableMembers)
				{
					field.Rename = false;
				}

				field.Strip = false;
			}
		}

		private void Analyze(BuildProperty property, bool serializable)
		{
			LoadSerializableAttributes(ref serializable, property.CustomAttributes);

			if (serializable)
			{
				if (!_renameSerializableMembers)
				{
					property.Rename = false;
				}

				property.Strip = false;
			}
		}

		private void Analyze(BuildEvent e, bool serializable)
		{
			LoadSerializableAttributes(ref serializable, e.CustomAttributes);

			if (serializable)
			{
				if (!_renameSerializableMembers)
				{
					e.Rename = false;
				}

				e.Strip = false;
			}
		}

		private void LoadSerializableAttributes(ref bool serializable, CustomAttributeCollection customAttributes)
		{
			if (CA.SerializableAttribute.IsDefined(customAttributes))
			{
				serializable = true;
			}

			if (CA.NonSerializedAttribute.IsDefined(customAttributes))
			{
				serializable = false;
			}
		}

		#endregion

		#region Static

		public static void Analyze(BuildAssembly assembly, bool renameSerializableMembers)
		{
			var analyzer = new SerializationAnalizer(assembly, renameSerializableMembers);
			analyzer.Analyze();
		}

		#endregion
	}
}
