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
	public class ConfigurationAnalizer
	{
		#region Fields

		private bool _renameConfigurationMembers;
		private BuildAssembly _assembly;

		#endregion

		#region Ctors

		private ConfigurationAnalizer(BuildAssembly assembly, bool renameConfigurationMembers)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_assembly = assembly;
			_renameConfigurationMembers = renameConfigurationMembers;
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
			if (IsConfigurationSettings(type))
			{
				ProcessConfigurationSettings(type);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Analyze(nestedType);
			}
		}

		private void ProcessConfigurationSettings(BuildType type)
		{
			if (!_renameConfigurationMembers) 
			{
				type.Rename = false;
			}

			type.Strip = false;

			foreach (BuildProperty property in type.Properties)
			{
				if (!property.IsStatic)
				{
					if (!_renameConfigurationMembers)
					{
						property.Rename = false;
					}

					property.Strip = false;
				}
			}
		}

		private bool IsConfigurationSettings(BuildType type)
		{
			if (type.Name != "SettingsBase")
				return false;

			if (type.Namespace != "System.Configuration")
				return false;

			return true;
		}

		#endregion

		#region Static

		public static void Analyze(BuildAssembly assembly,bool renameConfigurationMembers)
		{
			var analyzer = new ConfigurationAnalizer(assembly, renameConfigurationMembers);
			analyzer.Analyze();
		}

		#endregion
	}
}
