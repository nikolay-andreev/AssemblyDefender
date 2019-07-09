using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class DelegateTypeGenerator
	{
		#region Fields

		private BuildModule _module;

		#endregion

		#region Ctors

		public DelegateTypeGenerator(BuildModule module)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			_module = module;
		}

		#endregion

		#region Methods

		public void Generate()
		{
			foreach (var delegateType in _module.DelegateTypes)
			{
				Generate(delegateType);
			}
		}

		private void Generate(DelegateType delegateType)
		{
			if (delegateType.DeclaringType.Owner != null)
				return;

			var type = _module.Types.Add();

			var declaringTypeRef = delegateType.DeclaringType;
			type.Name = declaringTypeRef.Name;
			type.Namespace = declaringTypeRef.Namespace;
			type.Visibility = TypeVisibilityFlags.Public;
			type.IsSealed = true;

			type.BaseType =
				new TypeReference(
					"MulticastDelegate", "System",
					AssemblyReference.GetMscorlib(_module.Assembly), false);

			// Generic parameters
			if (delegateType.GenericParameterCount > 0)
			{
				var genericParameters = type.GenericParameters;
				for (int i = 0; i < delegateType.GenericParameterCount; i++)
				{
					genericParameters.Add();
				}
			}

			// Methods
			{
				var methods = type.Methods;
				GenerateCtor(methods.Add());
				GenerateInvoke(methods.Add(), delegateType);
			}
		}

		private void GenerateCtor(MethodDeclaration method)
		{
			method.Name = CodeModelUtils.MethodConstructorName;
			method.Visibility = MethodVisibilityFlags.Public;
			method.IsHideBySig = true;
			method.IsSpecialName = true;
			method.IsRuntimeSpecialName = true;
			method.CodeType = MethodCodeTypeFlags.Runtime;
			method.HasThis = true;
			method.ReturnType.Type =
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);

			// Parameters
			{
				var parameters = method.Parameters;

				// object
				var parameter = parameters.Add();
				parameter.Type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);

				// method
				parameter = parameters.Add();
				parameter.Type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly);
			}
		}

		private void GenerateInvoke(MethodDeclaration method, DelegateType delegateType)
		{
			var invokeCallSite = delegateType.InvokeCallSite;

			method.Name = "Invoke";
			method.Visibility = MethodVisibilityFlags.Public;
			method.IsVirtual = true;
			method.IsHideBySig = true;
			method.IsNewSlot = true;
			method.CodeType = MethodCodeTypeFlags.Runtime;
			method.HasThis = true;
			method.ReturnType.Type = invokeCallSite.ReturnType;

			// Parameters
			if (invokeCallSite.Arguments.Count > 0)
			{
				var parameters = method.Parameters;
				var arguments = invokeCallSite.Arguments;

				for (int i = 0; i < arguments.Count; i++)
				{
					var parameter = parameters.Add();
					parameter.Type = arguments[i];

					int parameterFlags = delegateType.InvokeParameterFlags[i];
					if ((parameterFlags & 1) == 1)
						parameter.IsIn = true;

					if ((parameterFlags & 2) == 2)
						parameter.IsOut = true;

					if ((parameterFlags & 4) == 4)
						parameter.IsOptional = true;
				}
			}
		}

		#endregion

		#region Static

		public static void Generate(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Generate(module);
			}
		}

		public static void Generate(BuildModule module)
		{
			var generator = new DelegateTypeGenerator(module);
			generator.Generate();
		}

		#endregion
	}
}
