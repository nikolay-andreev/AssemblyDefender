using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class AssemblyMergeGenerator
	{
		#region Fields

		private int _dataID;
		private string[] _assemblyNames;
		private BuildModule _module;
		private MainType _mainType;

		#endregion

		#region Ctors

		public AssemblyMergeGenerator(BuildModule module, string[] assemblyNames)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			_module = module;
			_assemblyNames = assemblyNames;
		}

		#endregion

		#region Methods

		public void Generate()
		{
			if (_assemblyNames.Length == 0)
				return;

			BuildData();

			_mainType = _module.MainType;

			// Methods
			var methods = _mainType.Methods;
			var initMethod = (BuildMethod)methods.Add();
			MergeInitialize(initMethod);
			OnMergedAssemblyResolve(methods.Add());
			GetMergedAssemblyNames(methods.Add());

			_mainType.AddStartupMethod(initMethod);

			// Fields
			GenerateFields();
			_mainType.AddLockObjectField("_mergeLockObject");

			// Dependencies
			_mainType.GenerateGetAssemblyName();
			_mainType.GenerateReadString();
			_mainType.GenerateRead7BitEncodedInt();
		}

		private void BuildData()
		{
			int pos = 0;
			var blob = new Blob();

			blob.Write7BitEncodedInt(ref pos, _assemblyNames.Length);

			for (int i = 0; i < _assemblyNames.Length; i++)
			{
				blob.WriteLengthPrefixedString(ref pos, _assemblyNames[i].ToLower(), Encoding.UTF8);
			}

			var resourceStorage = _module.ResourceStorage;
			_dataID = resourceStorage.Add(blob.GetBuffer(), 0, blob.Length, true);
		}

		private void GenerateFields()
		{
			var fields = _mainType.Fields;

			// _mergedAssemblyNames : System.String[]
			var field = fields.Add();
			field.Name = "_mergedAssemblyNames";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				new ArrayType(
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
		}

		/// <summary>
		/// static MergeInitialize() : [mscorlib]System.Void
		/// </summary>
		private void MergeInitialize(MethodDeclaration method)
		{
			method.Name = "MergeInitialize";
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
							"OnMergedAssemblyResolve",
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
		/// static OnMergedAssemblyResolve([mscorlib]System.Object, [mscorlib]System.ResolveEventArgs) : [mscorlib]System.Reflection.Assembly
		/// </summary>
		private void OnMergedAssemblyResolve(MethodDeclaration method)
		{
			method.Name = "OnMergedAssemblyResolve";
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
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetMergedAssemblyNames",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new ArrayType(
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
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
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)21));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
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
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)6));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetExecutingAssembly",
							new TypeReference("Assembly", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
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
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-27));
					instructions.Add(new Instruction(OpCodes.Ldnull));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static GetMergedAssemblyNames() : [mscorlib]System.String[]
		/// </summary>
		private void GetMergedAssemblyNames(MethodDeclaration method)
		{
			method.Name = "GetMergedAssemblyNames";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
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
						new TypeReference("Encoding", "System.Text",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
				}

				// Exception handlers
				{
					var exceptionHandlers = methodBody.ExceptionHandlers;
					exceptionHandlers.Add(
						new ExceptionHandler(
							ExceptionHandlerType.Finally,
							20, 82, 102, 8, 0));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_mergedAssemblyNames",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)103));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_mergeLockObject",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
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
							"_mergedAssemblyNames",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)73));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)20));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
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
					instructions.Add(new Instruction(OpCodes.Stelem_Ref));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-25));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"_mergedAssemblyNames",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
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
							"_mergedAssemblyNames",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		#endregion

		#region Static

		public static void Generate(BuildModule module, string[] assemblyNames)
		{
			var generator = new AssemblyMergeGenerator(module, assemblyNames);
			generator.Generate();
		}

		#endregion

		#region C# Code

		/***************************************************************************
		internal static string[] _mergedAssemblyNames;
		internal static object _mergeLockObject = new object();

		internal static void MergeInitialize()
		{
			AppDomain.CurrentDomain.AssemblyResolve += OnMergedAssemblyResolve;
		}

		internal static Assembly OnMergedAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assemblyNames = GetMergedAssemblyNames();
			var assemblyName = GetAssemblyName(args.Name);

			for (int i = 0; i < assemblyNames.Length; i++)
			{
				if (assemblyNames[i] == assemblyName)
				{
					return Assembly.GetExecutingAssembly();
				}
			}

			return null;
		}

		internal static string[] GetMergedAssemblyNames()
		{
			if (_mergedAssemblyNames == null)
			{
				lock (_mergeLockObject)
				{
					if (_mergedAssemblyNames == null)
					{
						byte[] buffer = GetData(1234567);
						var encoding = Encoding.UTF8;
						int pos = 0;
						int count = Read7BitEncodedInt(buffer, ref pos);
						var mergedAssemblyNames = new string[count];

						for (int i = 0; i < count; i++)
						{
							mergedAssemblyNames[i] = ReadString(buffer, ref pos, encoding);
						}

						_mergedAssemblyNames = mergedAssemblyNames;
					}
				}
			}

			return _mergedAssemblyNames;
		}
		***************************************************************************/

		#endregion
	}
}
