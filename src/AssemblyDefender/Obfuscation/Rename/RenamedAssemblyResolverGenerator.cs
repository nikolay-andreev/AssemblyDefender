using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class RenamedAssemblyResolverGenerator
	{
		#region Fields

		private int _dataID;
		private BuildModule _module;
		private MainType _mainType;
		private List<TupleStruct<string, string>> _names = new List<TupleStruct<string, string>>();

		#endregion

		#region Ctors

		public RenamedAssemblyResolverGenerator(BuildModule module)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			_module = module;
		}

		#endregion

		#region Properties

		public int Count
		{
			get { return _names.Count; }
		}

		#endregion

		#region Methods

		public void AddAssembly(string oldName, string newName)
		{
			_names.Add(new TupleStruct<string, string>(oldName, newName));
		}

		public void Generate()
		{
			if (_names.Count == 0)
				return;

			_mainType = _module.MainType;

			BuildData();

			// Methods
			var methods = _mainType.Methods;
			var initMethod = (BuildMethod)methods.Add();
			RenamedAssemblyInitialize(initMethod);
			OnRenamedAssemblyResolve(methods.Add());
			GetRenamedAssemblies(methods.Add());

			_mainType.AddStartupMethod(initMethod);

			// Fields
			GenerateFields();
			_mainType.AddLockObjectField("_renameLockObject");

			// NestedTypes
			RenamedAssemblyInfo(_mainType.NestedTypes.Add());

			// Dependencies
			_mainType.GenerateGetAssemblyName();
			_mainType.GenerateReadInt32();
			_mainType.GenerateReadString();
			_mainType.GenerateRead7BitEncodedInt();
		}

		private void BuildData()
		{
			int pos = 0;
			var blob = new Blob();

			blob.Write7BitEncodedInt(ref pos, _names.Count);

			var encoding = Encoding.UTF8;

			for (int i = 0; i < _names.Count; i++)
			{
				var name = _names[i];
				blob.WriteLengthPrefixedString(ref pos, name.Item1.ToLower(), encoding);
				blob.WriteLengthPrefixedString(ref pos, name.Item2, encoding);
			}

			var resourceStorage = _module.ResourceStorage;
			_dataID = resourceStorage.Add(blob.GetBuffer(), 0, blob.Length, true);
		}

		private void GenerateFields()
		{
			var fields = _mainType.Fields;

			// _renamedAssemblies : RenamedAssemblyInfo[]
			var field = fields.Add();
			field.Name = "_renamedAssemblies";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				new ArrayType(
					new TypeReference(
						"RenamedAssemblyInfo", null,
						new TypeReference(_mainType.Name, _mainType.Namespace, false), false));
		}

		/// <summary>
		/// static RenamedAssemblyInitialize() : [mscorlib]System.Void
		/// </summary>
		private void RenamedAssemblyInitialize(MethodDeclaration method)
		{
			method.Name = "RenamedAssemblyInitialize";
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
				methodBody.MaxStackSize = 8;
				methodBody.InitLocals = true;

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"get_CurrentDomain",
							new TypeReference("AppDomain", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("AppDomain", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldnull));
					instructions.Add(new Instruction(OpCodes.Ldftn,
						new MethodReference(
							"OnRenamedAssemblyResolve",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
									new TypeReference("ResolveEventArgs", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("ResolveEventHandler", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"add_AssemblyResolve",
							new TypeReference("AppDomain", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("ResolveEventHandler", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static OnRenamedAssemblyResolve([mscorlib]System.Object, [mscorlib]System.ResolveEventArgs) : [mscorlib]System.Reflection.Assembly
		/// </summary>
		private void OnRenamedAssemblyResolve(MethodDeclaration method)
		{
			method.Name = "OnRenamedAssemblyResolve";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					new TypeReference("Assembly", "System.Reflection",
						AssemblyReference.GetMscorlib(_module.Assembly), false);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// sender : [mscorlib]System.Object
				var parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);

				// args : [mscorlib]System.ResolveEventArgs
				parameter = parameters.Add();
				parameter.Type =
					new TypeReference("ResolveEventArgs", "System",
						AssemblyReference.GetMscorlib(_module.Assembly), false);
			}

			// Body
			{
				var methodBody = new MethodBody();
				methodBody.MaxStackSize = 2;
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new TypeReference("RenamedAssemblyInfo",
							new TypeReference(_mainType.Name, _mainType.Namespace, false), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
				}

				// Exception handlers
				{
					var exceptionHandlers = methodBody.ExceptionHandlers;
					exceptionHandlers.Add(
						new ExceptionHandler(
							ExceptionHandlerType.Finally,
							61, 27, 88, 8, 0));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_Name",
							new TypeReference("ResolveEventArgs", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetAssemblyName",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetRenamedAssemblies",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new ArrayType(
									new TypeReference("RenamedAssemblyInfo",
										new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)85));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"OldAssemblyName",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"op_Equality",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)63));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"LoadedAssembly",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)48));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_renameLockObject",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Enter",
							new TypeReference("Monitor", "System.Threading",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"LoadedAssembly",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)17));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"NewAssemblyName",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Load",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stfld,
						new FieldReference(
							"LoadedAssembly",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Exit",
							new TypeReference("Monitor", "System.Threading",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Endfinally));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"LoadedAssembly",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldlen));
					instructions.Add(new Instruction(OpCodes.Conv_I4));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-91));
					instructions.Add(new Instruction(OpCodes.Ldnull));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static GetRenamedAssemblies() : [GeneratedCode]AssemblyDefender/RenamedAssemblyInfo[]
		/// </summary>
		private void GetRenamedAssemblies(MethodDeclaration method)
		{
			method.Name = "GetRenamedAssemblies";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					new ArrayType(
						new TypeReference("RenamedAssemblyInfo",
							new TypeReference(_mainType.Name, _mainType.Namespace, false), false));
			}

			// Body
			{
				var methodBody = new MethodBody();
				methodBody.MaxStackSize = 6;
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					localVariables.Add(
						new TypeReference("Encoding", "System.Text",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new TypeReference("RenamedAssemblyInfo",
							new TypeReference(_mainType.Name, _mainType.Namespace, false), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
				}

				// Exception handlers
				{
					var exceptionHandlers = methodBody.ExceptionHandlers;
					exceptionHandlers.Add(
						new ExceptionHandler(
							ExceptionHandlerType.Finally,
							23, 114, 137, 8, 0));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_renamedAssemblies",
							new ArrayType(
								new TypeReference("RenamedAssemblyInfo",
									new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Brtrue, (int)135));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_renameLockObject",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Enter",
							new TypeReference("Monitor", "System.Threading",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_renamedAssemblies",
							new ArrayType(
								new TypeReference("RenamedAssemblyInfo",
									new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)105));
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
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)2));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Newarr,
						new TypeReference("RenamedAssemblyInfo",
							new TypeReference(_mainType.Name, _mainType.Namespace, false), false)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)52));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)2));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadString",
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
					instructions.Add(new Instruction(OpCodes.Stfld,
						new FieldReference(
							"OldAssemblyName",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)2));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadString",
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
					instructions.Add(new Instruction(OpCodes.Stfld,
						new FieldReference(
							"NewAssemblyName",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new TypeReference("RenamedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Stelem_Ref));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-57));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"_renamedAssemblies",
							new ArrayType(
								new TypeReference("RenamedAssemblyInfo",
									new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Exit",
							new TypeReference("Monitor", "System.Threading",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Endfinally));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_renamedAssemblies",
							new ArrayType(
								new TypeReference("RenamedAssemblyInfo",
									new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// [GeneratedCode]AssemblyDefender/RenamedAssemblyInfo
		/// </summary>
		private void RenamedAssemblyInfo(TypeDeclaration type)
		{
			type.Name = "RenamedAssemblyInfo";
			type.Visibility = TypeVisibilityFlags.NestedAssembly;
			type.IsSealed = true;
			type.IsBeforeFieldInit = true;
			type.BaseType = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);

			// Methods
			{
				var methods = type.Methods;
				RenamedAssemblyInfo_Ctor(methods.Add());
			}

			// Fields
			{
				var fields = type.Fields;

				// [GeneratedCode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]AssemblyDefender/RenamedAssemblyInfo::OldAssemblyName : [mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.String
				var field = fields.Add();
				field.Name = "OldAssemblyName";
				field.Visibility = FieldVisibilityFlags.Assembly;
				field.FieldType =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly);

				// [GeneratedCode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]AssemblyDefender/RenamedAssemblyInfo::NewAssemblyName : [mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.String
				field = fields.Add();
				field.Name = "NewAssemblyName";
				field.Visibility = FieldVisibilityFlags.Assembly;
				field.FieldType =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly);

				// [GeneratedCode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]AssemblyDefender/RenamedAssemblyInfo::LoadedAssembly : [mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.Reflection.Assembly
				field = fields.Add();
				field.Name = "LoadedAssembly";
				field.Visibility = FieldVisibilityFlags.Assembly;
				field.FieldType =
					new TypeReference("Assembly", "System.Reflection",
						AssemblyReference.GetMscorlib(_module.Assembly), false);
			}
		}

		/// <summary>
		/// .ctor() : [mscorlib]System.Void
		/// </summary>
		private void RenamedAssemblyInfo_Ctor(MethodDeclaration method)
		{
			method.Name = CodeModelUtils.MethodConstructorName;
			method.Visibility = MethodVisibilityFlags.Public;
			method.IsHideBySig = true;
			method.IsSpecialName = true;
			method.IsRuntimeSpecialName = true;
			method.HasThis = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Body
			{
				var methodBody = new MethodBody();
				methodBody.MaxStackSize = 8;
				methodBody.InitLocals = true;

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		#endregion

		#region C# Code

		/***************************************************************************
		internal static RenamedAssemblyInfo[] _renamedAssemblies;
		internal static object _renameLockObject = new object();

		internal static void RenamedAssemblyInitialize()
		{
			AppDomain.CurrentDomain.AssemblyResolve += OnRenamedAssemblyResolve;
		}

		internal static Assembly OnRenamedAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assemblyName = GetAssemblyName(args.Name);

			var assemblies = GetRenamedAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				var assemblyInfo = assemblies[i];
				if (assemblyInfo.OldAssemblyName == assemblyName)
				{
					if (assemblyInfo.LoadedAssembly == null)
					{
						lock (_renameLockObject)
						{
							if (assemblyInfo.LoadedAssembly == null)
							{
								assemblyInfo.LoadedAssembly = Assembly.Load(assemblyInfo.NewAssemblyName);
							}
						}
					}

					return assemblyInfo.LoadedAssembly;
				}
			}

			return null;
		}

		internal static RenamedAssemblyInfo[] GetRenamedAssemblies()
		{
			if (_renamedAssemblies == null)
			{
				lock (_renameLockObject)
				{
					if (_renamedAssemblies == null)
					{
						byte[] buffer = GetData(1234567);
						var encoding = Encoding.UTF8;
						int pos = 0;
						int count = Read7BitEncodedInt(buffer, ref pos);
						var assemblies = new RenamedAssemblyInfo[count];

						for (int i = 0; i < count; i++)
						{
							assemblies[i] =
								new RenamedAssemblyInfo()
								{
									OldAssemblyName = ReadString(buffer, ref pos, encoding),
									NewAssemblyName = ReadString(buffer, ref pos, encoding),
								};
						}

						_renamedAssemblies = assemblies;
					}
				}
			}

			return _renamedAssemblies;
		}

		internal sealed class RenamedAssemblyInfo
		{
			internal string OldAssemblyName;
			internal string NewAssemblyName;
			internal Assembly LoadedAssembly;
		}
		***************************************************************************/

		#endregion
	}
}
