using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Baml;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public static class ResourceHelper
	{
		public static void RenameWpfResource(BuildAssembly assembly)
		{
			if (!assembly.NameChanged)
				return;

			var resource = assembly.GetWpfResource();

			foreach (var satelliteResource in resource.SatelliteResources)
			{
				satelliteResource.Name = BamlUtils.GetWpfResourceName(assembly.NewName, satelliteResource.Assembly.Culture);
			}

			resource.Name = BamlUtils.GetWpfResourceName(assembly.NewName);
		}

		public static void RenameSatelliteAssemblies(BuildAssembly assembly)
		{
			bool changed = false;

			// Name
			string name;
			if (assembly.NameChanged)
			{
				name = assembly.NewName;
				changed = true;
			}
			else
			{
				name = assembly.Name;
			}

			// Version
			Version version;
			if (assembly.VersionChanged)
			{
				version = assembly.NewVersion;
				changed = true;
			}
			else
			{
				version = assembly.Version;
			}

			// PublicKey
			byte[] publicKey;
			if (assembly.PublicKeyChanged)
			{
				publicKey = assembly.NewPublicKey;
				changed = true;
			}
			else
			{
				publicKey = assembly.PublicKey;
			}

			if (!changed)
				return;

			foreach (var satelliteAssembly in assembly.SatelliteAssemblies)
			{
				satelliteAssembly.Name = name + ".resources";
				satelliteAssembly.Module.Name = name + ".resources.dll";
				satelliteAssembly.Version = version;
				satelliteAssembly.PublicKey = publicKey;
			}
		}
	}
}
