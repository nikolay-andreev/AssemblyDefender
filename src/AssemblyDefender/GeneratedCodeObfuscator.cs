using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Net.IL;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class GeneratedCodeObfuscator
	{
		#region Fields

		private string _mainTypeNamespace;
		private MainType _mainType;
		private BuildModule _module;
		private MemberNameGenerator _nameGenerator;
		private MainTypeFunctionPointerBuilder _functionPointerBuilder;
		private MainTypeCallProxyBuilder _callProxyBuilder;
		private List<BuildType> _cryptoInvokeTypes = new List<BuildType>();

		#endregion

		#region Ctors

		public GeneratedCodeObfuscator(BuildModule module, MemberNameGenerator nameGenerator)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			if (nameGenerator == null)
				throw new ArgumentNullException("nameGenerator");

			_module = module;
			_nameGenerator = nameGenerator;
			_mainType = _module.MainType;
			_mainTypeNamespace = _module.MainTypeNamespace;

			if (_mainType != null)
			{
				_callProxyBuilder = new MainTypeCallProxyBuilder(_module);

				if (_module.IsPrimeModule)
				{
					_functionPointerBuilder = new MainTypeFunctionPointerBuilder(_module);
				}
			}
		}

		#endregion

		#region Methods

		public void Obfuscate()
		{
			LoadCryptoInvokeTypes();
			ObfuscateControlFlow();
			Rename();

			if (_functionPointerBuilder != null)
			{
				_functionPointerBuilder.MapMemberReferences();
			}
		}

		private void LoadCryptoInvokeTypes()
		{
			foreach (var cryptoInvokeType in _module.ILCryptoInvokeTypes)
			{
				_cryptoInvokeTypes.Add((BuildType)_module.Types.FindBackward(cryptoInvokeType.TypeName, _mainTypeNamespace, true));
			}
		}

		private bool IsMainTypeInitMethod(BuildMethod method)
		{
			return
				method.IsStaticConstructor() ||
				method.Name == "InitializeControlFlow" ||
				method.Name == "InitializeMethodPointers";

		}

		private void BuildFunctionPointers()
		{
			if (!_functionPointerBuilder.HasFunctionPointers)
				return;

			_functionPointerBuilder.BuildFunctionPointers();

			var assembler = _module.Assembler;
			assembler.Tasks.Add(_functionPointerBuilder, 1550);
		}

		#endregion

		#region Rename

		private void Rename()
		{
			if (_mainType != null)
			{
				RenameType(_mainType);
			}

			foreach (var type in _cryptoInvokeTypes)
			{
				RenameType(type);
			}
		}

		public void RenameType(BuildType type)
		{
			_nameGenerator.Reset();

			// Nested types
			foreach (BuildType nestedType in type.NestedTypes)
			{
				if (!nestedType.NameChanged)
				{
					nestedType.NewName = _nameGenerator.GenerateString();
					nestedType.NameChanged = true;
					nestedType.Rename = true;
				}
			}

			_nameGenerator.Reset();

			// Methods
			RenameMethods(type.Methods);

			// Fields
			foreach (BuildField field in type.Fields)
			{
				if (!field.NameChanged)
				{
					field.NewName = _nameGenerator.GenerateString();
					field.NameChanged = true;
					field.Rename = true;
				}
			}

			// Children members
			foreach (BuildType nestedType in type.NestedTypes)
			{
				RenameType(nestedType);
			}
		}

		private void RenameMethods(MethodDeclarationCollection methods)
		{
			if (methods.Count == 0)
				return;

			var renameMethods = new List<MethodDeclaration>(methods.Count);

			// Collect
			foreach (BuildMethod method in methods)
			{
				if (!method.NameChanged && MemberRenameHelper.CanRename(method))
				{
					renameMethods.Add(method);
				}
			}

			var groups = MemberRenameHelper.GroupMethods(renameMethods);

			int stringIndex = _nameGenerator.StringIndex;

			foreach (var group in groups.Values)
			{
				_nameGenerator.Reset();

				foreach (BuildMethod method in group)
				{
					method.NewName = _nameGenerator.GenerateString();
					method.NameChanged = true;
					method.Rename = true;
				}

				if (stringIndex < _nameGenerator.StringIndex)
					stringIndex = _nameGenerator.StringIndex;
			}

			_nameGenerator.StringIndex = stringIndex;
		}

		#endregion

		#region Control flow

		private void ObfuscateControlFlow()
		{
			if (_mainType != null)
			{
				ObfuscateMainTypeControlFlow();
			}

			foreach (var type in _cryptoInvokeTypes)
			{
				ObfuscateControlFlow((BuildMethod)type.Methods[1]);
			}

			if (_callProxyBuilder != null)
			{
				_callProxyBuilder.Generate();
			}

			if (_functionPointerBuilder != null)
			{
				BuildFunctionPointers();
			}

			if (_mainType != null)
			{
				ObfuscateMainTypeControlFlow2();
			}
		}

		private void ObfuscateMainTypeControlFlow()
		{
			foreach (BuildMethod method in _mainType.Methods)
			{
				if (IsMainTypeInitMethod(method))
					continue;

				ObfuscateControlFlow(method);
			}

			foreach (BuildType nestedType in _mainType.NestedTypes)
			{
				ObfuscateControlFlow(nestedType);
			}
		}

		private void ObfuscateMainTypeControlFlow2()
		{
			foreach (BuildMethod method in _mainType.Methods)
			{
				if (!IsMainTypeInitMethod(method))
					continue;

				ObfuscateControlFlowInitMethod(method);
			}
		}

		private void ObfuscateControlFlow(BuildType type)
		{
			foreach (BuildMethod method in type.Methods)
			{
				ObfuscateControlFlow(method);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				ObfuscateControlFlow(nestedType);
			}
		}

		private void ObfuscateControlFlow(BuildMethod method)
		{
			if (!MethodBody.IsValid(method))
				return;

			var methodBody = MethodBody.Load(method);

			var ilBody = ILBody.Load(methodBody);

			// Call proxy
			if (_callProxyBuilder != null)
			{
				_callProxyBuilder.Build(ilBody);
			}

			// Function pointers
			if (_functionPointerBuilder != null)
			{
				_functionPointerBuilder.Build(ilBody);
			}

			// Obfuscate
			var obfuscator = new ControlFlowObfuscator(ilBody, method);
			obfuscator.Obfuscate();

			// Calculate max stack size
			ilBody.CalculateMaxStackSize(method);

			// Save
			methodBody = ilBody.Build();

			methodBody.Build(method);
		}

		private void ObfuscateControlFlowInitMethod(BuildMethod method)
		{
			if (!MethodBody.IsValid(method))
				return;

			var methodBody = MethodBody.Load(method);

			var ilBody = ILBody.Load(methodBody);

			// Obfuscate
			var obfuscator = new ControlFlowObfuscator(ilBody, method);
			obfuscator.DoNotUseFieldNumber = true;
			obfuscator.Obfuscate();

			// Calculate max stack size
			ilBody.CalculateMaxStackSize(method);

			// Save
			methodBody = ilBody.Build();

			methodBody.Build(method);
		}

		#endregion

		#region Static

		public static void Obfuscate(BuildAssembly assembly, MemberNameGenerator nameGenerator)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Obfuscate(module, nameGenerator);
			}
		}

		public static void Obfuscate(BuildModule module, MemberNameGenerator nameGenerator)
		{
			var obfuscator = new GeneratedCodeObfuscator(module, nameGenerator);
			obfuscator.Obfuscate();
		}

		#endregion
	}
}
