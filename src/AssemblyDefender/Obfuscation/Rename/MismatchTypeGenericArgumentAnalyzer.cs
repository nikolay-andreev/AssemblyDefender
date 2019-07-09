using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	/// <summary>
	/// Workaround for a bug in the CLR runtime. If type extends base type with generic arguments
	/// which are not in right order the TypeLoadException is thrown: Method override XXX connot
	/// find a method to replace.
	/// </summary>
	/// <example>
	/// VALID
	/// .class public auto ansi beforefieldinit Class2<T1,T2>
	///        extends class Class1<!T1,string>
	/// {
	/// }
	///
	/// INVALID
	/// .class public auto ansi beforefieldinit Class2<T1,T2>
	///        extends class Class1<!T2,string>
	/// {
	/// }
	/// </example>
	public static class MismatchTypeGenericArgumentAnalyzer
	{
		public static void Analyze(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Analyze(module);
			}
		}

		public static void Analyze(BuildModule module)
		{
			foreach (BuildType type in module.Types)
			{
				Analyze(type);
			}
		}

		public static void Analyze(BuildType type)
		{
			if (!IsBaseTypeValid(type))
			{
				UnmarkMethods(type);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Analyze(nestedType);
			}
		}

		private static bool IsBaseTypeValid(IType type)
		{
			var baseType = type.BaseType;
			if (baseType == null)
				return true;

			for (int i = 0; i < baseType.GenericArguments.Count; i++)
			{
				var genericArgument = baseType.GenericArguments[i];
				if (genericArgument.IsGenericParameter())
				{
					bool isMethod;
					int position;
					genericArgument.GetGenericParameter(out isMethod, out position);

					if (position != i)
						return false;
				}
			}

			return IsBaseTypeValid(baseType);
		}

		private static void UnmarkMethods(IType type)
		{
			foreach (var method in type.Methods)
			{
				var baseMethod = method.GetBaseMethod();
				if (baseMethod == null)
					continue;

				UnmarkMethod(method);
			}
		}

		private static void UnmarkMethod(IMethod method)
		{
			var buildMethod = method.DeclaringMethod as BuildMethod;
			if (buildMethod != null && buildMethod.Rename)
			{
				buildMethod.Rename = false;
				buildMethod.NameChanged = false;
			}

			var baseMethod = method.GetBaseMethod();
			if (baseMethod != null)
			{
				UnmarkMethod(baseMethod);
			}
		}
	}
}
