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
	public class StringObfuscator
	{
		#region Fields

		private BuildModule _module;
		private FieldReference _stringFieldRef;
		private HashList<string> _strings;

		#endregion

		#region Ctors

		public StringObfuscator(BuildAssembly assembly, HashList<string> strings)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (strings == null)
				throw new ArgumentNullException("strings");

			_module = (BuildModule)assembly.Module;
			_strings = strings;

			var mainType = _module.MainType;

			_stringFieldRef =
				new FieldReference(
					"Strings",
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
					new TypeReference(mainType.Name, mainType.Namespace, false));
		}

		#endregion

		#region Methods

		public void Obfuscate()
		{
			foreach (BuildType type in _module.Types)
			{
				Obfuscate(type);
			}
		}

		private void Obfuscate(BuildType type)
		{
			if (type.IsMainType)
				return;

			foreach (BuildMethod method in type.Methods)
			{
				Obfuscate(method);
			}

			foreach (BuildField field in type.Fields)
			{
				Obfuscate(field);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Obfuscate(nestedType);
			}
		}

		private void Obfuscate(BuildMethod method)
		{
			if (!method.ObfuscateStrings)
				return;

			if (!MethodBody.IsValid(method))
				return;

			var methodBody = MethodBody.Load(method);
			if (!HasLoadStringInstruction(methodBody))
				return;

			var ilBody = ILBody.Load(methodBody);
			Obfuscate(ilBody);
			ilBody.CalculateMaxStackSize(method);

			methodBody = ilBody.Build();
			methodBody.Build(method);
		}

		private void Obfuscate(BuildField field)
		{
			if (!field.ObfuscateStrings)
				return;

			if (!field.DefaultValue.HasValue)
				return;

			var constantInfo = field.DefaultValue.Value;
			if (constantInfo.Type != ConstantType.String)
				return;

			var ownerType = field.GetOwnerType();

			var method = ownerType.Methods.GetOrCreateStaticConstructor();
			if (!MethodBody.IsValid(method))
				return;

			var methodBody = MethodBody.Load(method);

			var instructions = methodBody.Instructions;

			int index = AddString((string)constantInfo.Value);

			instructions.InsertRange(
				0,
				new Instruction[]
				{
					new Instruction(OpCodes.Ldsfld, _stringFieldRef),
					Instruction.GetLdc(index),
					new Instruction(OpCodes.Ldelem_Ref),
					new Instruction(OpCodes.Stsfld, field.ToReference(field.Module)),
				});

			field.IsLiteral = false;
			field.IsInitOnly = true;
			field.DefaultValue = null;
		}

		private void Obfuscate(ILBlock block)
		{
			var node = block.FirstChild;
			while (node != null)
			{
				switch (node.NodeType)
				{
					case ILNodeType.Block:
						{
							Obfuscate((ILBlock)node);
						}
						break;

					case ILNodeType.Instruction:
						{
							Obfuscate((ILInstruction)node);
						}
						break;
				}

				node = node.NextSibling;
			}
		}

		private void Obfuscate(ILInstruction instruction)
		{
			if (instruction.OpCode != OpCodes.Ldstr)
				return;

			int index = AddString((string)instruction.Value);
			instruction.OpCode = OpCodes.Ldsfld;
			instruction.Value = _stringFieldRef;

			var next = new ILInstruction(Instruction.GetLdc(index));
			instruction.AddNext(next);
			instruction = next;

			next = new ILInstruction(OpCodes.Ldelem_Ref);
			instruction.AddNext(next);
		}

		private bool HasLoadStringInstruction(MethodBody methodBody)
		{
			foreach (var instruction in methodBody.Instructions)
			{
				if (instruction.OpCode.OpValue == OpCodeValues.Ldstr)
				{
					return true;
				}
			}

			return false;
		}

		private int AddString(string value)
		{
			if (value == null)
				value = string.Empty;

			int index;
			_strings.TryAdd(value, out index);

			return index;
		}

		#endregion

		#region Static

		public static void Obfuscate(BuildAssembly assembly, HashList<string> strings)
		{
			var obfuscator = new StringObfuscator(assembly, strings);
			obfuscator.Obfuscate();
		}

		#endregion
	}
}
