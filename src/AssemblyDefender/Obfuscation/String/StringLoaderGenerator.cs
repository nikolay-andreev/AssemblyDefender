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
	public class StringLoaderGenerator
	{
		#region Fields

		private int _dataID;
		private MainType _mainType;
		private BuildAssembly _assembly;
		private BuildModule _module;
		private IReadOnlyList<string> _strings;

		#endregion

		#region Ctors

		public StringLoaderGenerator(BuildAssembly assembly, IReadOnlyList<string> strings)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (strings == null)
				throw new ArgumentNullException("strings");

			_assembly = assembly;
			_module = (BuildModule)assembly.Module;
			_strings = strings;
		}

		#endregion

		#region Methods

		public void Generate()
		{
			if (_strings.Count == 0)
				return;

			BuildData();

			_mainType = _module.MainType;

			// Methods
			var initMethod = (BuildMethod)_mainType.Methods.Add();
			LoadStrings(initMethod);
			_mainType.AddStartupMethod(initMethod);

			// Fields
			GenerateFields();

			// Dependencies
			_mainType.GenerateReadStringIntern();
		}

		private void GenerateFields()
		{
			// [GeneratedCode]AssemblyDefender::Strings : [mscorlib]System.String[]
			var field = _mainType.Fields.Add();
			field.Name = "Strings";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				new ArrayType(
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
		}

		/// <summary>
		/// static LoadStrings() : [mscorlib]System.Void
		/// </summary>
		private void LoadStrings(MethodDeclaration method)
		{
			method.Name = "LoadStrings";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Body
			{
				var methodBody = new MethodBody();
				methodBody.MaxStackSize = 5;
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new TypeReference("Encoding", "System.Text",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)_dataID));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetData",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new ArrayType(
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"get_UTF8",
							new TypeReference("Encoding", "System.Text",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Encoding", "System.Text",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)_strings.Count));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"Strings",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)20));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"Strings",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadStringIntern",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
									new TypeReference("Encoding", "System.Text",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stelem_Ref));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)_strings.Count));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-28));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		private void BuildData()
		{
			int pos = 0;
			var blob = new Blob();
			var encoding = Encoding.UTF8;

			foreach (string s in _strings)
			{
				blob.WriteLengthPrefixedString(ref pos, s, encoding);
			}

			var storage = ((BuildAssembly)_module.Assembly).ResourceStorage;
			_dataID = storage.Add(blob.GetBuffer(), 0, blob.Length, true, true);
		}

		#endregion

		#region Static

		public static void Generate(BuildAssembly assembly, HashList<string> strings)
		{
			var generator = new StringLoaderGenerator(assembly, strings);
			generator.Generate();
		}

		#endregion

		#region C# Code

		/***************************************************************************
		internal static string[] Strings;

		internal static void LoadStrings()
		{
			byte[] buffer = GetData(1234567);
			int pos = 0;
			var encoding = Encoding.UTF8;
			Strings = new string[1234568];
			for (int i = 0; i < 1234568; i++)
			{
				Strings[i] = ReadStringIntern(buffer, ref pos, encoding);
			}
		}
		***************************************************************************/

		#endregion
	}
}
