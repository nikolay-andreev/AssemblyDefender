using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	/// <summary>
	/// internal static int ControlFlow1;
	/// internal static int ControlFlow2;
	/// internal static int ControlFlow3;
	/// internal static int ControlFlow4;
	/// internal static int ControlFlow5;
	/// internal static int ControlFlow6;
	/// internal static int ControlFlow7;
	/// internal static int ControlFlow8;
	/// internal static void InitializeControlFlow();
	/// </summary>
	internal class ControlFlowGenerator
	{
		#region Fields

		private MainType _mainType;
		private BuildModule _module;

		#endregion

		#region Ctors

		internal ControlFlowGenerator(BuildModule module)
		{
			_module = module;
			_mainType = module.MainType;
		}

		#endregion

		#region Methods

		internal void Generate()
		{
			GenerateFields();
			GenerateCctor();
			GenerateInitialize();
		}

		private void GenerateFields()
		{
			var fields = _mainType.Fields;

			// ControlFlow5 : [mscorlib]System.Int32
			var field = fields.Add();
			field.Name = "ControlFlow5";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

			// ControlFlow3 : [mscorlib]System.Int32
			field = fields.Add();
			field.Name = "ControlFlow3";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

			// ControlFlow1 : [mscorlib]System.Int32
			field = fields.Add();
			field.Name = "ControlFlow1";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

			// ControlFlow8 : [mscorlib]System.Int32
			field = fields.Add();
			field.Name = "ControlFlow8";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

			// ControlFlow6 : [mscorlib]System.Int32
			field = fields.Add();
			field.Name = "ControlFlow6";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

			// ControlFlow2 : [mscorlib]System.Int32
			field = fields.Add();
			field.Name = "ControlFlow2";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

			// ControlFlow4 : [mscorlib]System.Int32
			field = fields.Add();
			field.Name = "ControlFlow4";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

			// ControlFlow7 : [mscorlib]System.Int32
			field = fields.Add();
			field.Name = "ControlFlow7";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
		}

		private void GenerateCctor()
		{
			var instructions = new List<Instruction>(20);

			instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
			instructions.Add(new Instruction(OpCodes.Stsfld,
				new FieldReference(
					"ControlFlow5",
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
					new TypeReference(_mainType.Name, _mainType.Namespace, false))));

			instructions.Add(new Instruction(OpCodes.Ldc_I4_2));
			instructions.Add(new Instruction(OpCodes.Stsfld,
				new FieldReference(
					"ControlFlow3",
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
					new TypeReference(_mainType.Name, _mainType.Namespace, false))));

			instructions.Add(new Instruction(OpCodes.Ldc_I4_3));
			instructions.Add(new Instruction(OpCodes.Stsfld,
				new FieldReference(
					"ControlFlow1",
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
					new TypeReference(_mainType.Name, _mainType.Namespace, false))));

			instructions.Add(new Instruction(OpCodes.Ldc_I4_4));
			instructions.Add(new Instruction(OpCodes.Stsfld,
				new FieldReference(
					"ControlFlow8",
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
					new TypeReference(_mainType.Name, _mainType.Namespace, false))));

			instructions.Add(new Instruction(OpCodes.Ldc_I4_5));
			instructions.Add(new Instruction(OpCodes.Stsfld,
				new FieldReference(
					"ControlFlow6",
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
					new TypeReference(_mainType.Name, _mainType.Namespace, false))));

			instructions.Add(new Instruction(OpCodes.Ldc_I4_6));
			instructions.Add(new Instruction(OpCodes.Stsfld,
				new FieldReference(
					"ControlFlow2",
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
					new TypeReference(_mainType.Name, _mainType.Namespace, false))));

			instructions.Add(new Instruction(OpCodes.Ldc_I4_7));
			instructions.Add(new Instruction(OpCodes.Stsfld,
				new FieldReference(
					"ControlFlow4",
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
					new TypeReference(_mainType.Name, _mainType.Namespace, false))));

			instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
			instructions.Add(new Instruction(OpCodes.Stsfld,
				new FieldReference(
					"ControlFlow7",
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
					new TypeReference(_mainType.Name, _mainType.Namespace, false))));

			instructions.Add(new Instruction(OpCodes.Call,
				new MethodReference(
					"InitializeControlFlow",
					new TypeReference(_mainType.Name, _mainType.Namespace, false),
					new CallSite(
						false,
						false,
						MethodCallingConvention.Default,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
						new TypeSignature[0],
						-1,
						0))));

			var cctorMethod = _mainType.Methods.GetOrCreateStaticConstructor();
			var methodBody = MethodBody.Load(cctorMethod);
			methodBody.Instructions.InsertRange(methodBody.Instructions.Count - 1, instructions);
			methodBody.Build(cctorMethod);
		}

		/// <summary>
		/// static InitializeControlFlow() : [mscorlib]System.Void
		/// </summary>
		private void GenerateInitialize()
		{
			var method = _mainType.Methods.Add();
			method.Name = "InitializeControlFlow";
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
				methodBody.MaxStackSize = 2;
				methodBody.InitLocals = false;

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow5",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)42));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble, (int)152));
					instructions.Add(new Instruction(OpCodes.Br, (int)185));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble, (int)148));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)-115));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow5",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Bgt_S, (sbyte)39));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)120));
					instructions.Add(new Instruction(OpCodes.Br, (int)-154));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow5",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow5",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)-73));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow5",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble, (int)-145));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble, (int)-222));
					instructions.Add(new Instruction(OpCodes.Br, (int)-269));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)42));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow5",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble, (int)152));
					instructions.Add(new Instruction(OpCodes.Br, (int)185));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow5",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble, (int)148));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)-115));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Bgt_S, (sbyte)39));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)120));
					instructions.Add(new Instruction(OpCodes.Br, (int)-154));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow5",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow5",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow2",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)-73));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow7",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow3",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble, (int)-145));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow4",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow8",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow6",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"ControlFlow1",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ble, (int)-222));
					instructions.Add(new Instruction(OpCodes.Br, (int)-269));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		#endregion

		#region Static

		public static void Generate(BuildModule module)
		{
			var generator = new ControlFlowGenerator(module);
			generator.Generate();
		}

		#endregion

		#region C# Code

		/***************************************************************************
		internal static int ControlFlow5 = 1;
		internal static int ControlFlow3 = 2;
		internal static int ControlFlow1 = 3;
		internal static int ControlFlow8 = 4;
		internal static int ControlFlow6 = 5;
		internal static int ControlFlow2 = 6;
		internal static int ControlFlow4 = 7;
		internal static int ControlFlow7 = 8;

		internal static void InitializeControlFlow()
		{
		L1:
			ControlFlow4 = ControlFlow7 - ControlFlow5;
			if (ControlFlow4 - ControlFlow8 > ControlFlow2) goto L2; else goto L3;

		L2:
			ControlFlow6 = ControlFlow8 - ControlFlow7;
			if (ControlFlow6 - ControlFlow3 > ControlFlow8) goto L8; else goto L7;

		L3:
			ControlFlow7 = ControlFlow6 - ControlFlow1;
			if (ControlFlow7 - ControlFlow4 > ControlFlow6) goto L1; else goto L8;

		L4:
			ControlFlow3 = ControlFlow7 + ControlFlow2;
			if (ControlFlow3 - ControlFlow2 > ControlFlow5) goto L6; else goto L5;

		L5:
			ControlFlow2 = ControlFlow6 + ControlFlow2;
			if (ControlFlow2 - ControlFlow8 > ControlFlow3) goto L2; else goto L11;

		L6:
			ControlFlow5 = ControlFlow4 - ControlFlow7;
			if (ControlFlow5 - ControlFlow8 > ControlFlow2) goto END; else goto L5;

		L7:
			ControlFlow1 = ControlFlow5 + ControlFlow1;
			if (ControlFlow1 - ControlFlow6 > ControlFlow3) goto END; else goto L4;

		L8:
			ControlFlow8 = ControlFlow4 + ControlFlow2;
			if (ControlFlow8 - ControlFlow6 > ControlFlow1) goto L2; else goto L3;

		L11:
			ControlFlow4 = ControlFlow3 - ControlFlow1;
			if (ControlFlow4 - ControlFlow8 > ControlFlow2) goto L12; else goto L13;

		L12:
			ControlFlow6 = ControlFlow6 - ControlFlow5;
			if (ControlFlow6 - ControlFlow3 > ControlFlow8) goto L18; else goto L17;

		L13:
			ControlFlow7 = ControlFlow7 + ControlFlow5;
			if (ControlFlow7 - ControlFlow4 > ControlFlow6) goto L11; else goto L18;

		L14:
			ControlFlow3 = ControlFlow4 - ControlFlow1;
			if (ControlFlow3 + ControlFlow2 > ControlFlow1) goto L16; else goto L15;

		L15:
			ControlFlow2 = ControlFlow6 - ControlFlow4;
			if (ControlFlow2 - ControlFlow8 > ControlFlow3) goto L12; else goto END;

		L16:
			ControlFlow5 = ControlFlow4 + ControlFlow1;
			if (ControlFlow5 + ControlFlow1 > ControlFlow2) goto END; else goto L15;

		L17:
			ControlFlow1 = ControlFlow3 - ControlFlow7;
			if (ControlFlow1 - ControlFlow6 > ControlFlow3) goto END; else goto L14;

		L18:
			ControlFlow8 = ControlFlow1 + ControlFlow4;
			if (ControlFlow8 + ControlFlow6 > ControlFlow1) goto L12; else goto L13;

		END:
			return;
		}
		***************************************************************************/

		#endregion
	}
}
