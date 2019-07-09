using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public class AssemblyEmbedGenerator
	{
		#region Fields

		private int _dataID;
		private BuildModule _module;
		private MainType _mainType;

		#endregion

		#region Ctors

		public AssemblyEmbedGenerator(BuildModule module, int dataID)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			_module = module;
			_dataID = dataID;
		}

		#endregion

		#region Methods

		public void Generate()
		{
			_mainType = _module.MainType;

			// Methods
			var methods = _mainType.Methods;
			var initMethod = (BuildMethod)methods.Add();
			EmbedInitialize(initMethod);
			OnEmbeddedAssemblyResolve(methods.Add());
			GetEmbeddedAssemblyByIndex(methods.Add());
			GetEmbeddedAssembly(methods.Add());
			GetEmbeddedAssemblies(methods.Add());

			_mainType.AddStartupMethod(initMethod);

			// Fields
			GenerateFields();
			_mainType.AddLockObjectField("_embedLockObject");

			// Nested types
			EmbeddedAssemblyInfo(_mainType.NestedTypes.Add());

			// Dependencies
			_mainType.GenerateGetAssemblyName();
			_mainType.GenerateReadInt32();
			_mainType.GenerateReadString();
			_mainType.GenerateRead7BitEncodedInt();
		}

		private void GenerateFields()
		{
			var fields = _mainType.Fields;

			// _embeddedAssemblies : EmbeddedAssemblyInfo[]
			var field = fields.Add();
			field.Name = "_embeddedAssemblies";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				new ArrayType(
					new TypeReference("EmbeddedAssemblyInfo", null,
						new TypeReference(_mainType.Name, _mainType.Namespace, false), false));
		}

		/// <summary>
		/// static EmbedInitialize() : [mscorlib]System.Void
		/// </summary>
		private void EmbedInitialize(MethodDeclaration method)
		{
			method.Name = "EmbedInitialize";
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
							"OnEmbeddedAssemblyResolve",
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
		/// static OnEmbeddedAssemblyResolve([mscorlib]System.Object, [mscorlib]System.ResolveEventArgs) : [mscorlib]System.Reflection.Assembly
		/// </summary>
		private void OnEmbeddedAssemblyResolve(MethodDeclaration method)
		{
			method.Name = "OnEmbeddedAssemblyResolve";
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
						new ArrayType(
							new TypeReference("EmbeddedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new TypeReference("EmbeddedAssemblyInfo",
							new TypeReference(_mainType.Name, _mainType.Namespace, false), false));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetEmbeddedAssemblies",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new ArrayType(
									new TypeReference("EmbeddedAssemblyInfo",
										new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
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
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)29));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"AssemblyName",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new TypeReference("EmbeddedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
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
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)7));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetEmbeddedAssembly",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									new TypeReference("EmbeddedAssemblyInfo",
										new TypeReference(_mainType.Name, _mainType.Namespace, false), false),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldlen));
					instructions.Add(new Instruction(OpCodes.Conv_I4));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-35));
					instructions.Add(new Instruction(OpCodes.Ldnull));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static GetEmbeddedAssemblyByIndex([mscorlib]System.Int32) : [mscorlib]System.Reflection.Assembly
		/// </summary>
		private void GetEmbeddedAssemblyByIndex(MethodDeclaration method)
		{
			method.Name = "GetEmbeddedAssemblyByIndex";
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

				// index : [mscorlib]System.Int32
				var parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
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
							"GetEmbeddedAssemblies",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new ArrayType(
									new TypeReference("EmbeddedAssemblyInfo",
										new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetEmbeddedAssembly",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									new TypeReference("EmbeddedAssemblyInfo",
										new TypeReference(_mainType.Name, _mainType.Namespace, false), false),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static GetEmbeddedAssembly([GeneratedCode]AssemblyDefender/EmbeddedAssemblyInfo) : [mscorlib]System.Reflection.Assembly
		/// </summary>
		private void GetEmbeddedAssembly(MethodDeclaration method)
		{
			method.Name = "GetEmbeddedAssembly";
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

				// assemblyInfo : [GeneratedCode]AssemblyDefender/EmbeddedAssemblyInfo
				var parameter = parameters.Add();
				parameter.Type =
					new TypeReference("EmbeddedAssemblyInfo",
						new TypeReference(_mainType.Name, _mainType.Namespace, false), false);
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
				}

				// Exception handlers
				{
					var exceptionHandlers = methodBody.ExceptionHandlers;
					exceptionHandlers.Add(
						new ExceptionHandler(
							ExceptionHandlerType.Finally,
							20, 32, 52, 7, 0));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"LoadedAssembly",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference("EmbeddedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)51));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_embedLockObject",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
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
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"LoadedAssembly",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference("EmbeddedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)22));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"DataID",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference("EmbeddedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
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
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stfld,
						new FieldReference(
							"LoadedAssembly",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference("EmbeddedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)7));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
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
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldfld,
						new FieldReference(
							"LoadedAssembly",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference("EmbeddedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static GetEmbeddedAssemblies() : [GeneratedCode]AssemblyDefender/EmbeddedAssemblyInfo[]
		/// </summary>
		private void GetEmbeddedAssemblies(MethodDeclaration method)
		{
			method.Name = "GetEmbeddedAssemblies";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					new ArrayType(
						new TypeReference("EmbeddedAssemblyInfo",
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
							new TypeReference("EmbeddedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new TypeReference("EmbeddedAssemblyInfo",
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
							23, 113, 136, 8, 0));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_embeddedAssemblies",
							new ArrayType(
								new TypeReference("EmbeddedAssemblyInfo",
									new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Brtrue, (int)134));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_embedLockObject",
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
							"_embeddedAssemblies",
							new ArrayType(
								new TypeReference("EmbeddedAssemblyInfo",
									new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)104));
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
						new TypeReference("EmbeddedAssemblyInfo",
							new TypeReference(_mainType.Name, _mainType.Namespace, false), false)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)51));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("EmbeddedAssemblyInfo",
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
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
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
					instructions.Add(new Instruction(OpCodes.Stfld,
						new FieldReference(
							"DataID",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
							new TypeReference("EmbeddedAssemblyInfo",
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
							"AssemblyName",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new TypeReference("EmbeddedAssemblyInfo",
								new TypeReference(_mainType.Name, _mainType.Namespace, false), false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Stelem_Ref));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-56));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"_embeddedAssemblies",
							new ArrayType(
								new TypeReference("EmbeddedAssemblyInfo",
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
							"_embeddedAssemblies",
							new ArrayType(
								new TypeReference("EmbeddedAssemblyInfo",
									new TypeReference(_mainType.Name, _mainType.Namespace, false), false)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// [GeneratedCode]AssemblyDefender/EmbeddedAssemblyInfo
		/// </summary>
		private void EmbeddedAssemblyInfo(TypeDeclaration type)
		{
			type.Name = "EmbeddedAssemblyInfo";
			type.Visibility = TypeVisibilityFlags.NestedAssembly;
			type.IsSealed = true;
			type.IsBeforeFieldInit = true;
			type.BaseType = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);

			// Methods
			{
				var methods = type.Methods;
				EmbeddedAssemblyInfo_Ctor(methods.Add());
			}

			// Fields
			{
				var fields = type.Fields;

				// [GeneratedCode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]AssemblyDefender/EmbeddedAssemblyInfo::DataID : [mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.Int32
				var field = fields.Add();
				field.Name = "DataID";
				field.Visibility = FieldVisibilityFlags.Assembly;
				field.FieldType =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

				// [GeneratedCode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]AssemblyDefender/EmbeddedAssemblyInfo::AssemblyName : [mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.String
				field = fields.Add();
				field.Name = "AssemblyName";
				field.Visibility = FieldVisibilityFlags.Assembly;
				field.FieldType =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly);

				// [GeneratedCode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]AssemblyDefender/EmbeddedAssemblyInfo::LoadedAssembly : [mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.Reflection.Assembly
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
		private void EmbeddedAssemblyInfo_Ctor(MethodDeclaration method)
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

		#region Static

		public static void Generate(BuildModule module, int dataID)
		{
			var generator = new AssemblyEmbedGenerator(module, dataID);
			generator.Generate();
		}

		#endregion

		#region C# Code

		/***************************************************************************
		internal static EmbeddedAssemblyInfo[] _embeddedAssemblies;
		internal static object _embedLockObject = new object();

		internal static void EmbedInitialize()
		{
			AppDomain.CurrentDomain.AssemblyResolve += OnEmbeddedAssemblyResolve;
		}

		internal static Assembly OnEmbeddedAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var embeddedAssemblies = GetEmbeddedAssemblies();

			string assemblyName = GetAssemblyName(args.Name);

			for (int i = 0; i < embeddedAssemblies.Length; i++)
			{
				var embeddedAssembly = embeddedAssemblies[i];
				if (embeddedAssembly.AssemblyName == assemblyName)
				{
					return GetEmbeddedAssembly(embeddedAssembly);
				}
			}

			return null;
		}

		internal static Assembly GetEmbeddedAssemblyByIndex(int index)
		{
			return GetEmbeddedAssembly(GetEmbeddedAssemblies()[index]);
		}

		internal static Assembly GetEmbeddedAssembly(EmbeddedAssemblyInfo assemblyInfo)
		{
			if (assemblyInfo.LoadedAssembly == null)
			{
				lock (_embedLockObject)
				{
					if (assemblyInfo.LoadedAssembly == null)
					{
						assemblyInfo.LoadedAssembly = Assembly.Load(GetData(assemblyInfo.DataID));
					}
				}
			}

			return assemblyInfo.LoadedAssembly;
		}

		internal static EmbeddedAssemblyInfo[] GetEmbeddedAssemblies()
		{
			if (_embeddedAssemblies == null)
			{
				lock (_embedLockObject)
				{
					if (_embeddedAssemblies == null)
					{
						byte[] buffer = GetData(1234567);
						var encoding = Encoding.UTF8;
						int pos = 0;
						int count = Read7BitEncodedInt(buffer, ref pos);

						var embeddedAssemblies = new EmbeddedAssemblyInfo[count];

						for (int i = 0; i < count; i++)
						{
							embeddedAssemblies[i] =
								new EmbeddedAssemblyInfo()
								{
									DataID = ReadInt32(buffer, ref pos),
									AssemblyName = ReadString(buffer, ref pos, encoding),
								};
						}

						_embeddedAssemblies = embeddedAssemblies;
					}
				}
			}

			return _embeddedAssemblies;
		}

		internal sealed class EmbeddedAssemblyInfo
		{
			internal int DataID;
			internal string AssemblyName;
			internal Assembly LoadedAssembly;
		}
		***************************************************************************/

		#endregion
	}
}
