using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Baml;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public static class MemberRenameHelper
	{
		public static bool CanRename(TypeDeclaration type)
		{
			// Do not rename global type.
			if (type.IsGlobal())
				return false;

			return true;
		}

		public static bool CanRename(MethodDeclaration method)
		{
			// Do not rename constructors.
			if (method.IsConstructor())
				return false;

			// Do not rename dynamic methods.
			if (method.CodeType == MethodCodeTypeFlags.Runtime)
				return false;

			return true;
		}

		public static void FixExportedTypeNames(BuildAssembly assembly, MemberNameGenerator nameGenerator)
		{
			foreach (var module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				var exportedTypes = module.ExportedTypes;
				for (int i = 0; i < exportedTypes.Count; i++)
				{
					var exportedType = exportedTypes[i].GetOutermostType();

					var type = exportedType.Resolve(module) as BuildType;
					if (type == null)
						continue;

					if (object.ReferenceEquals(type.Assembly, module.Assembly))
						continue;

					if (!type.Rename)
						continue;

					type.NewName = nameGenerator.GenerateUniqueString();
					type.NameChanged = true;
				}
			}
		}

		public static void BuildRenamedAssemblyResolver(BuildAssembly assembly, List<TupleStruct<string, string>> renamedAssemblyNames)
		{
			if (renamedAssemblyNames.Count == 0)
				return;

			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				var generator = new RenamedAssemblyResolverGenerator(module);

				foreach (var renamedName in renamedAssemblyNames)
				{
					generator.AddAssembly(renamedName.Item1, renamedName.Item2);
				}

				generator.Generate();
			}
		}

		public static Dictionary<CallSite, List<MethodDeclaration>> GroupMethods(IEnumerable<IMethod> methods)
		{
			var groups = new Dictionary<CallSite, List<MethodDeclaration>>(SignatureComparer.Default);

			foreach (BuildMethod method in methods)
			{
				var callSite = method.ToCallSite(method.Module);

				List<MethodDeclaration> group;
				if (!groups.TryGetValue(callSite, out group))
				{
					group = new List<MethodDeclaration>();
					groups.Add(callSite, group);
				}

				group.Add(method);
			}

			return groups;
		}
	}
}
