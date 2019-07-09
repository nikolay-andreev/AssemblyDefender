using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	/// <summary>
	/// Analization for:
	/// new ResourceManager(typeof(X)) -> unmark X
	/// new ComponentResourceManager(typeof(X)) -> unmark X
	/// new Bitmap(typeof(X)) -> unmark X
	/// </summary>
	public class CodeAnalizer
	{
		#region Fields

		private BuildAssembly _assembly;

		#endregion

		#region Ctors

		private CodeAnalizer(BuildAssembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_assembly = assembly;
		}

		#endregion

		#region Methods

		private void Analyze()
		{
			foreach (BuildModule module in _assembly.Modules)
			{
				Analyze(module);
			}
		}

		private void Analyze(BuildModule module)
		{
			foreach (BuildType type in module.Types)
			{
				Analyze(type);
			}
		}

		private void Analyze(BuildType type)
		{
			foreach (BuildMethod method in type.Methods)
			{
				Analyze(method);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Analyze(nestedType);
			}
		}

		private void Analyze(BuildMethod method)
		{
			if (!MethodBody.IsValid(method))
				return;

			var body = MethodBody.Load(method);

			var instructions = body.Instructions;
			for (int i = 0; i < instructions.Count; i++)
			{
				var instruction = instructions[i];
				var opCode = instruction.OpCode;
				if (opCode == OpCodes.Newobj)
				{
					var calledMethodSig = instruction.Value as MethodReference;
					if (calledMethodSig != null)
					{
						if (IsResourceManagerCtorWithType(calledMethodSig))
						{
							var typeSig = FindPrevLdtoken(instructions, i - 1) as TypeSignature;
							if (typeSig != null)
							{
								Unmark(typeSig);
							}
						}
						else if (IsComponentResourceManagerCtorWithType(calledMethodSig))
						{
							var typeSig = FindPrevLdtoken(instructions, i - 1) as TypeSignature;
							if (typeSig != null)
							{
								Unmark(typeSig);
							}
						}
						else if (IsBitmapCtorWithType(calledMethodSig))
						{
							var typeSig = FindPrevLdtoken(instructions, i - 1) as TypeSignature;
							if (typeSig != null)
							{
								Unmark(typeSig);
							}
						}
					}
				}
			}
		}

		private bool IsResourceManagerCtorWithType(MethodReference methodSig)
		{
			if (methodSig.IsStatic)
				return false;

			if (methodSig.Name != CodeModelUtils.MethodConstructorName)
				return false;

			var ownerTypeSig = methodSig.Owner;
			if (ownerTypeSig.Name != "ResourceManager")
				return false;

			if (ownerTypeSig.Namespace != "System.Resources")
				return false;

			var arguments = methodSig.Arguments;
			if (arguments.Count == 0)
				return false;

			if (!IsSystemType(arguments[0]))
				return false;

			return true;
		}

		private bool IsComponentResourceManagerCtorWithType(MethodReference methodSig)
		{
			if (methodSig.IsStatic)
				return false;

			if (methodSig.Name != CodeModelUtils.MethodConstructorName)
				return false;

			var ownerTypeSig = methodSig.Owner;
			if (ownerTypeSig.Name != "ComponentResourceManager")
				return false;

			if (ownerTypeSig.Namespace != "System.ComponentModel")
				return false;

			var arguments = methodSig.Arguments;
			if (arguments.Count == 0)
				return false;

			if (!IsSystemType(arguments[0]))
				return false;

			return true;
		}

		private bool IsBitmapCtorWithType(MethodReference methodSig)
		{
			if (methodSig.IsStatic)
				return false;

			if (methodSig.Name != CodeModelUtils.MethodConstructorName)
				return false;

			var ownerTypeSig = methodSig.Owner;
			if (ownerTypeSig.Name != "Bitmap")
				return false;

			if (ownerTypeSig.Namespace != "System.Drawing")
				return false;

			var arguments = methodSig.Arguments;
			if (arguments.Count == 0)
				return false;

			if (!IsSystemType(arguments[0]))
				return false;

			return true;
		}

		private bool IsSystemType(TypeSignature typeSig)
		{
			if (typeSig.Name != "Type")
				return false;

			if (typeSig.Namespace != "System")
				return false;

			return true;
		}

		private Signature FindPrevLdtoken(List<Instruction> instructions, int startIndex)
		{
			for (int i = startIndex; i >= 0; i--)
			{
				var instruction = instructions[i];
				var opCode = instruction.OpCode;

				if (opCode == OpCodes.Ldtoken)
				{
					return instruction.Value as Signature;
				}
			}

			return null;
		}

		private void Unmark(TypeSignature typeSig)
		{
			var type = typeSig.Resolve(_assembly) as BuildType;
			if (type == null)
				return;

			type.Rename = false;
		}

		#endregion

		#region Static

		public static void Analyze(BuildAssembly assembly)
		{
			var analyzer = new CodeAnalizer(assembly);
			analyzer.Analyze();
		}

		#endregion
	}
}
