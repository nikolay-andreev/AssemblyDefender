using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Resources;

namespace AssemblyDefender.Baml
{
	public static class BamlUtils
	{
		public static string GetWpfResourceName(string assemblyName)
		{
			return string.Format("{0}.g.resources", assemblyName);
		}

		public static string GetWpfResourceName(string assemblyName, string culture)
		{
			return string.Format("{0}.g.{1}.resources", assemblyName, culture);
		}

		public static bool ContainsWpfResources(Assembly assembly)
		{
			string resourceName = GetWpfResourceName(assembly.Name);
			foreach (var resource in assembly.Resources)
			{
				if (resource.Name == resourceName)
					return true;
			}

			return false;
		}

		public static bool ContainsBamlResources(Resource resource)
		{
			var data = resource.GetData();

			if (!ResourceUtils.IsResource(data))
				return false;

			using (var reader = new ResourceReaderEx(data))
			{
				while (reader.Read())
				{
					if (reader.TypeCode != ResourceTypeCode.Stream && reader.TypeCode != ResourceTypeCode.ByteArray)
						continue;

					if (0 == string.Compare(Path.GetExtension(reader.Name), BamlImage.FileExtension, true))
						return true;
				}
			}

			return false;
		}

		#region Assembly

		public static Assembly Resolve(this IBamlAssembly bamlAssembly, Assembly ownerAssembly, bool throwOnFailure = false)
		{
			var assembly = bamlAssembly.Resolve(ownerAssembly);
			if (assembly == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(Net.SR.AssemblyResolveError, bamlAssembly.ToString()));
				}

				return null;
			}

			return assembly;
		}

		public static AssemblyReference ToReference(this IBamlAssembly bamlAssembly, Assembly ownerAssembly, bool throwOnFailure = false)
		{
			var assemblyRef = bamlAssembly.ToReference(ownerAssembly);
			if (assemblyRef == null)
			{
				if (throwOnFailure)
				{
					throw new BamlException(SR.BamlLoadError);
				}

				return null;
			}

			return assemblyRef;
		}

		#endregion

		#region Type

		public static TypeDeclaration Resolve(this IBamlType bamlType, Assembly ownerAssembly, bool throwOnFailure = false)
		{
			var type = bamlType.Resolve(ownerAssembly);
			if (type == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(Net.SR.TypeResolveError, bamlType.ToString()));
				}

				return null;
			}

			return type;
		}

		#endregion

		#region Property

		public static MemberNode Resolve(IBamlProperty bamlProperty, Assembly ownerAssembly, bool throwOnFailure = false)
		{
			var property = bamlProperty.Resolve(ownerAssembly);
			if (property == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(Net.SR.PropertyResolveError, bamlProperty.ToString()));
				}

				return null;
			}

			return property;
		}

		#endregion
	}
}
