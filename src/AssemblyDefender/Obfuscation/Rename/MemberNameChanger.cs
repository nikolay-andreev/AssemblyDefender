using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using CA = AssemblyDefender.Net.CustomAttributes;

namespace AssemblyDefender
{
	/// <summary>
	/// Change members with generated names.
	/// </summary>
	public static class MemberNameChanger
	{
		public static void Change(BuildAssembly assembly)
		{
			if (assembly.NameChanged)
			{
				assembly.Name = assembly.NewName;
			}

			if (assembly.CultureChanged)
			{
				assembly.Culture = assembly.NewCulture;
			}

			if (assembly.VersionChanged)
			{
				assembly.Version = assembly.NewVersion;
			}

			if (assembly.PublicKeyChanged)
			{
				assembly.PublicKey = assembly.NewPublicKey;
			}

			var customAttributes = assembly.CustomAttributes;

			// Title
			if (assembly.TitleChanged)
			{
				CA.AssemblyTitleAttribute.Clear(customAttributes);

				string newTitle = assembly.NewTitle;
				if (!string.IsNullOrEmpty(newTitle))
				{
					var attribute = new CA.AssemblyTitleAttribute();
					attribute.Title = newTitle;
					attribute.Build(customAttributes.Add());
				}
			}

			// Description
			if (assembly.DescriptionChanged)
			{
				CA.AssemblyDescriptionAttribute.Clear(customAttributes);

				string newDescription = assembly.NewDescription;
				if (!string.IsNullOrEmpty(newDescription))
				{
					var attribute = new CA.AssemblyDescriptionAttribute();
					attribute.Description = newDescription;
					attribute.Build(customAttributes.Add());
				}
			}

			// Company
			if (assembly.CompanyChanged)
			{
				CA.AssemblyCompanyAttribute.Clear(customAttributes);

				string newCompany = assembly.NewCompany;
				if (!string.IsNullOrEmpty(newCompany))
				{
					var attribute = new CA.AssemblyCompanyAttribute();
					attribute.Company = newCompany;
					attribute.Build(customAttributes.Add());
				}
			}

			// Product
			if (assembly.ProductChanged)
			{
				CA.AssemblyProductAttribute.Clear(customAttributes);

				string newProduct = assembly.NewProduct;
				if (!string.IsNullOrEmpty(newProduct))
				{
					var attribute = new CA.AssemblyProductAttribute();
					attribute.Product = newProduct;
					attribute.Build(customAttributes.Add());
				}
			}

			// Copyright
			if (assembly.CopyrightChanged)
			{
				CA.AssemblyCopyrightAttribute.Clear(customAttributes);

				string newCopyright = assembly.NewCopyright;
				if (!string.IsNullOrEmpty(newCopyright))
				{
					var attribute = new CA.AssemblyCopyrightAttribute();
					attribute.Copyright = newCopyright;
					attribute.Build(customAttributes.Add());
				}
			}

			// Trademark
			if (assembly.TrademarkChanged)
			{
				CA.AssemblyTrademarkAttribute.Clear(customAttributes);

				string newTrademark = assembly.NewTrademark;
				if (!string.IsNullOrEmpty(newTrademark))
				{
					var attribute = new CA.AssemblyTrademarkAttribute();
					attribute.Trademark = newTrademark;
					attribute.Build(customAttributes.Add());
				}
			}

			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Change(module);
			}
		}

		public static void Change(BuildModule module)
		{
			if (module.NameChanged)
			{
				module.Name = module.NewName;
			}

			foreach (BuildType type in module.Types)
			{
				Change(type);
			}
		}

		public static void Change(BuildType type)
		{
			if (type.NameChanged)
			{
				type.Name = type.NewName;

				foreach (var genericParameter in type.GenericParameters)
				{
					genericParameter.Name = null;
				}
			}

			if (type.NamespaceChanged)
			{
				type.Namespace = type.NewNamespace;
			}

			foreach (BuildMethod method in type.Methods)
			{
				Change(method);
			}

			foreach (BuildField field in type.Fields)
			{
				Change(field);
			}

			foreach (BuildProperty property in type.Properties)
			{
				Change(property);
			}

			foreach (BuildEvent e in type.Events)
			{
				Change(e);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Change(nestedType);
			}
		}

		public static void Change(BuildMethod method)
		{
			if (method.NameChanged)
			{
				method.Name = method.NewName;
			}

			if (method.Rename || method.IsConstructor())
			{
				foreach (var parameter in method.Parameters)
				{
					parameter.Name = null;
				}

				foreach (var genericParameter in method.GenericParameters)
				{
					genericParameter.Name = null;
				}
			}
		}

		public static void Change(BuildField field)
		{
			if (field.NameChanged)
			{
				field.Name = field.NewName;
			}
		}

		public static void Change(BuildProperty property)
		{
			if (property.NameChanged)
			{
				property.Name = property.NewName;
			}
		}

		public static void Change(BuildEvent e)
		{
			if (e.NameChanged)
			{
				e.Name = e.NewName;
			}
		}
	}
}
