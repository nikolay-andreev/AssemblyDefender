using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	internal class StartupMethodGenerator
	{
		#region Fields

		private Random _random;
		private BuildModule _module;
		private IEnumerable<BuildMethod> _startupMethods;

		#endregion

		#region Ctors

		internal StartupMethodGenerator(BuildModule module, IEnumerable<BuildMethod> startupMethods)
		{
			_module = module;
			_startupMethods = startupMethods;
			_random = _module.RandomGenerator;
		}

		#endregion

		#region Methods

		internal void Generate()
		{
			var mainType = _module.MainType;

			var proxyMethod = (BuildMethod)mainType.Methods.Add();
			proxyMethod.Name = _random.NextString(12, true);
			proxyMethod.Visibility = MethodVisibilityFlags.Assembly;
			proxyMethod.IsStatic = true;
			proxyMethod.IsHideBySig = true;
			proxyMethod.ReturnType.Type = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);

			var methodBody = new MethodBody();
			var instructions = methodBody.Instructions;

			foreach (var startupMethod in _startupMethods)
			{
				instructions.Add(new Instruction(OpCodes.Call, startupMethod.ToReference(startupMethod.Module)));
			}

			instructions.Add(new Instruction(OpCodes.Ret));

			methodBody.Build(proxyMethod);

			// Call proxy method from <Module>.cctor
			GenerateGlobalCCtor(proxyMethod);
		}

		private void GenerateGlobalCCtor(BuildMethod proxyMethod)
		{
			var ownerType = _module.Types.GetOrCreateGlobal();

			var method = ownerType.Methods.GetOrCreateStaticConstructor();

			MethodBody methodBody;
			if (MethodBody.IsValid(method))
			{
				methodBody = MethodBody.Load(method);
			}
			else
			{
				methodBody = new MethodBody();
				methodBody.Instructions.Add(new Instruction(OpCodes.Ret));
			}

			if (methodBody.MaxStackSize < 2)
				methodBody.MaxStackSize = 2;

			methodBody.Instructions.Insert(0, new Instruction(OpCodes.Call, proxyMethod.ToReference(proxyMethod.Module)));
			methodBody.ShiftRightExceptionHandlerOffsets(1);
			methodBody.Build(method);
		}

		#endregion

		#region Static

		public static void Generate(BuildModule module, IEnumerable<BuildMethod> startupMethods)
		{
			var generator = new StartupMethodGenerator(module, startupMethods);
			generator.Generate();
		}

		#endregion
	}
}
