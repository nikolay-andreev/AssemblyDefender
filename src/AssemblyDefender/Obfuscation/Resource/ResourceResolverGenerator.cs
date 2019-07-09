using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public class ResourceResolverGenerator
	{
		#region Fields

		private BuildModule _module;
		private MainType _mainType;

		#endregion

		#region Ctors

		public ResourceResolverGenerator(BuildAssembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_module = (BuildModule)assembly.Module;
		}

		#endregion

		#region Methods

		public void Generate()
		{
			_mainType = _module.MainType;

			// Methods
			var methods = _mainType.Methods;
			var initMethod = (BuildMethod)methods.Add();
			ResourceInitialize(initMethod);
			OnResourceResolve(methods.Add());
			_mainType.AddStartupMethod(initMethod);

			// Fields
			_mainType.AddLockObjectField("_resourceLockObject");

			// Dependencies
			_mainType.GenerateReadInt32();
			_mainType.GenerateComputeHash();
			_mainType.GenerateDecrypt();
			_mainType.GenerateDecompress();
		}

		/// <summary>
		/// static ResourceInitialize() : [mscorlib]System.Void
		/// </summary>
		private void ResourceInitialize(MethodDeclaration method)
		{
			method.Name = "ResourceInitialize";
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
				methodBody.MaxStackSize = 4;
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						new TypeReference("AppDomain", "System",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
				}

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
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldstr, "AD.x340587"));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetData",
							new TypeReference("AppDomain", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)34));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldstr, "AD.x340587"));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new GenericTypeReference(
								new TypeReference("Dictionary`2", "System.Collections.Generic",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									new TypeReference("Assembly", "System.Reflection",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
									new TypeReference("Assembly", "System.Reflection",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
								}
							),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"SetData",
							new TypeReference("AppDomain", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldnull));
					instructions.Add(new Instruction(OpCodes.Ldftn,
						new MethodReference(
							"OnResourceResolve",
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
							"add_ResourceResolve",
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
		/// static OnResourceResolve([mscorlib]System.Object, [mscorlib]System.ResolveEventArgs) : [mscorlib]System.Reflection.Assembly
		/// </summary>
		private void OnResourceResolve(MethodDeclaration method)
		{
			method.Name = "OnResourceResolve";
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
				methodBody.MaxStackSize = 4;
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						new GenericTypeReference(
							new TypeReference("Dictionary`2", "System.Collections.Generic",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeSignature[]
							{
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
							}
						));
					localVariables.Add(
						new TypeReference("Assembly", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						new TypeReference("Assembly", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
					localVariables.Add(
						new TypeReference("Stream", "System.IO",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Exception handlers
				{
					var exceptionHandlers = methodBody.ExceptionHandlers;
					exceptionHandlers.Add(
						new ExceptionHandler(
							ExceptionHandlerType.Finally,
							110, 197, 307, 12, 0));
					exceptionHandlers.Add(
						new ExceptionHandler(
							ExceptionHandlerType.Finally,
							50, 288, 338, 12, 0));
				}

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
					instructions.Add(new Instruction(OpCodes.Ldstr, "AD.x340587"));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetData",
							new TypeReference("AppDomain", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Isinst,
						new GenericTypeReference(
							new TypeReference("Dictionary`2", "System.Collections.Generic",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeSignature[]
							{
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
							}
						)));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)2));
					instructions.Add(new Instruction(OpCodes.Ldnull));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_RequestingAssembly",
							new TypeReference("ResolveEventArgs", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)2));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"TryGetValue",
							new GenericTypeReference(
								new TypeReference("Dictionary`2", "System.Collections.Generic",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									new TypeReference("Assembly", "System.Reflection",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
									new TypeReference("Assembly", "System.Reflection",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
								}
							),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly),
								new TypeSignature[]
								{
									new GenericParameterType(false, 0),
									new ByRefType(
										new GenericParameterType(false, 1)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Brtrue, (int)303));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_resourceLockObject",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)13));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)12));
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
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)2));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"TryGetValue",
							new GenericTypeReference(
								new TypeReference("Dictionary`2", "System.Collections.Generic",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									new TypeReference("Assembly", "System.Reflection",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
									new TypeReference("Assembly", "System.Reflection",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
								}
							),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly),
								new TypeSignature[]
								{
									new GenericParameterType(false, 0),
									new ByRefType(
										new GenericParameterType(false, 1)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Brtrue, (int)257));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetManifestResourceNames",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new ArrayType(
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Br, (int)230));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetManifestResourceStream",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Stream", "System.IO",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)12));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_Length",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Conv_I8));
					instructions.Add(new Instruction(OpCodes.Bge_S, (sbyte)5));
					instructions.Add(new Instruction(OpCodes.Leave, (int)188));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"Read",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Pop));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)6));
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)ResourceObfuscator.ResourceSignature));
					instructions.Add(new Instruction(OpCodes.Beq_S, (sbyte)5));
					instructions.Add(new Instruction(OpCodes.Leave, (int)138));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)6));
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)6));
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)10));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_Length",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Conv_I4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"Read",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Pop));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ComputeHash",
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
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Beq_S, (sbyte)2));
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)58));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Decrypt",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)10));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Bne_Un_S, (sbyte)9));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Decompress",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new ArrayType(
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
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
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"Add",
							new GenericTypeReference(
								new TypeReference("Dictionary`2", "System.Collections.Generic",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									new TypeReference("Assembly", "System.Reflection",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
									new TypeReference("Assembly", "System.Reflection",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
								}
							),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new GenericParameterType(false, 0),
									new GenericParameterType(false, 1),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)29));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)7));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"Dispose",
							new TypeReference("IDisposable", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Endfinally));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldlen));
					instructions.Add(new Instruction(OpCodes.Conv_I4));
					instructions.Add(new Instruction(OpCodes.Blt, (int)-241));
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)12));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)7));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)13));
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
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		#endregion

		#region Static

		public static void Generate(BuildAssembly assembly)
		{
			var generator = new ResourceResolverGenerator(assembly);
			generator.Generate();
		}

		#endregion

		#region C# Code

		/***************************************************************************
		internal const int ResourceSignature = 0x53524441;
		internal const string ResourceDataKey = "AD.x340587";
		internal static object _resourceLockObject = new object();

		internal static void ResourceInitialize()
		{
			var appDomain = AppDomain.CurrentDomain;
			if (null == appDomain.GetData(ResourceDataKey))
			{
				appDomain.SetData(ResourceDataKey, new Dictionary<Assembly, Assembly>());
				appDomain.ResourceResolve += OnResourceResolve;
			}
		}

		internal static Assembly OnResourceResolve(object sender, ResolveEventArgs args)
		{
			var assemblyMapping = AppDomain.CurrentDomain.GetData(ResourceDataKey) as Dictionary<Assembly, Assembly>;
			if (assemblyMapping == null)
				return null;

			var requestingAssembly = args.RequestingAssembly;

			Assembly resourceAssembly;
			if (!assemblyMapping.TryGetValue(requestingAssembly, out resourceAssembly))
			{
				lock (_resourceLockObject)
				{
					if (!assemblyMapping.TryGetValue(requestingAssembly, out resourceAssembly))
					{
						foreach (string resourceName in requestingAssembly.GetManifestResourceNames())
						{
							using (var stream = requestingAssembly.GetManifestResourceStream(resourceName))
							{
								if (stream == null || stream.Length < 20)
									continue;

								// Signature(4) + Hash(4) + EncryptKey(4) + Flags(1) + Unused(1)
								byte[] buffer = new byte[14];
								stream.Read(buffer, 0, 14);

								int pos = 0;
								int signature = ReadInt32(buffer, ref pos);
								if (signature != ResourceSignature)
									continue;

								int hashCode = ReadInt32(buffer, ref pos);
								int encryptKey = ReadInt32(buffer, ref pos);
								int flags = buffer[pos];

								int bufferSize = (int)stream.Length - 14;
								buffer = new byte[bufferSize];
								stream.Read(buffer, 0, bufferSize);

								if (hashCode != ComputeHash(buffer, 0, bufferSize))
									continue;

								// Decrypt
								Decrypt(buffer, encryptKey, 0, bufferSize);

								// Decompress
								if ((flags & 1) == 1)
								{
									buffer = Decompress(buffer);
								}

								resourceAssembly = Assembly.Load(buffer);
								assemblyMapping.Add(requestingAssembly, resourceAssembly);
								break;
							}
						}
					}
				}
			}

			return resourceAssembly;
		}
		***************************************************************************/

		#endregion
	}
}
