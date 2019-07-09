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
	public class BindableAttributeAnalizer
	{
		#region Fields

		private bool _renameBindableMembers;
		private BuildAssembly _assembly;

		#endregion

		#region Ctors

		private BindableAttributeAnalizer(BuildAssembly assembly, bool renameBindableMembers)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_assembly = assembly;
			_renameBindableMembers = renameBindableMembers;
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
			var attr = CA.BindableAttribute.FindFirst(type.CustomAttributes);
			if (attr != null && attr.Bindable)
			{
				if (!_renameBindableMembers)
				{
					type.Rename = false;
				}

				type.Strip = false;
			}

			foreach (BuildMethod method in type.Methods)
			{
				Analyze(method);
			}

			foreach (BuildField field in type.Fields)
			{
				Analyze(field);
			}

			foreach (BuildProperty property in type.Properties)
			{
				Analyze(property);
			}

			foreach (BuildEvent e in type.Events)
			{
				Analyze(e);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Analyze(nestedType);
			}
		}

		private void Analyze(BuildMethod method)
		{
			var attr = CA.BindableAttribute.FindFirst(method.CustomAttributes);
			if (attr != null && attr.Bindable)
			{
				if (!_renameBindableMembers)
				{
					method.Rename = false;
				}

				method.Strip = false;
			}
		}

		private void Analyze(BuildField field)
		{
			var attr = CA.BindableAttribute.FindFirst(field.CustomAttributes);
			if (attr != null && attr.Bindable)
			{
				if (!_renameBindableMembers)
				{
					field.Rename = false;
				}

				field.Strip = false;
			}
		}

		private void Analyze(BuildProperty property)
		{
			var attr = CA.BindableAttribute.FindFirst(property.CustomAttributes);
			if (attr != null && attr.Bindable)
			{
				if (!_renameBindableMembers)
				{
					property.Rename = false;
				}

				property.Strip = false;
			}
		}

		private void Analyze(BuildEvent e)
		{
			var attr = CA.BindableAttribute.FindFirst(e.CustomAttributes);
			if (attr != null && attr.Bindable)
			{
				if (!_renameBindableMembers)
				{
					e.Rename = false;
				}

				e.Strip = false;
			}
		}

		#endregion

		#region Static

		public static void Analyze(BuildAssembly assembly, bool renameBindableMembers)
		{
			var analyzer = new BindableAttributeAnalizer(assembly, renameBindableMembers);
			analyzer.Analyze();
		}

		#endregion
	}
}
