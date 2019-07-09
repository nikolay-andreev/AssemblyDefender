using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public static class TypeSealer
	{
		#region Analyze

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
			AnalyzeType(type);

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Analyze(nestedType);
			}
		}

		private static void AnalyzeType(BuildType type)
		{
			if (type.IsInterface)
				return;

			if (type.SealTypeProcessed && !type.SealType)
				return;

			var baseType = ((IType)type).BaseType;
			while (baseType != null)
			{
				baseType = baseType.DeclaringType;

				type = baseType as BuildType;
				if (type != null)
				{
					if (type.SealTypeProcessed && !type.SealType)
						break;

					type.SealType = false;
					type.SealTypeProcessed = true;
				}

				baseType = baseType.BaseType;
			}
		}

		#endregion

		#region Change

		public static void Seal(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Seal(module);
			}
		}

		public static void Seal(BuildModule module)
		{
			foreach (BuildType type in module.Types)
			{
				Seal(type);
			}
		}

		public static void Seal(BuildType type)
		{
			if (type.IsMainType)
				return;

			if (type.IsInterface)
				return;

			if (type.SealType && !type.IsSealed)
			{
				type.IsSealed = true;
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Seal(nestedType);
			}
		}

		#endregion
	}
}
