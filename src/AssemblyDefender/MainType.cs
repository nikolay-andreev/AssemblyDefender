using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class MainType : BuildType
	{
		#region Fields

		private int _genFlags;
		private int _genFlags2;
		private ResourceStorage _resourceStorage;
		private new BuildModule _module;
		private HashList<BuildMethod> _startupMethods;
		private Dictionary<BuildType, List<string>> _lockObjectFieldByOwner;

		#endregion

		#region Ctors

		protected internal MainType(BuildModule module)
			: base(module, 0, 0)
		{
			_module = module;
			_resourceStorage = ((BuildAssembly)module.Assembly).ResourceStorage;
			_startupMethods = new HashList<BuildMethod>();
			_lockObjectFieldByOwner = new Dictionary<BuildType, List<string>>();

			var randomGenerator = module.RandomGenerator;
			Name = module.MainTypeName ?? randomGenerator.NextString(12);
			Namespace = module.MainTypeNamespace;
			Visibility = TypeVisibilityFlags.Public;
			IsSealed = true;
			IsBeforeFieldInit = true;
			BaseType = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);
		}

		#endregion

		#region Properties

		public override bool IsMainType
		{
			get { return true; }
		}

		#endregion

		#region Methods

		public void AddStartupMethod(BuildMethod method)
		{
			_startupMethods.Add(method);
		}

		public void AddLockObjectField(string name)
		{
			AddLockObjectField(this, name);
		}

		public void AddLockObjectField(BuildType ownerType, string name)
		{
			List<string> names;
			if (!_lockObjectFieldByOwner.TryGetValue(ownerType, out names))
			{
				names = new List<string>();
				_lockObjectFieldByOwner.Add(ownerType, names);
			}

			if (!names.Contains(name))
			{
				names.Add(name);
			}
		}

		#endregion

		#region Generation

		/// <summary>
		/// Generate GetData, startup methods, lock objects.
		/// </summary>
		public void Generate()
		{
			if (_genFlags.IsBitAtIndexOn(0))
				return;

			_genFlags = _genFlags.SetBitAtIndex(0, true);

			// Data lock object is always generated. Required by MainTypeFunctionPointerBuilder.
			AddLockObjectField("_dataLockObject");

			// GetData
			if (_resourceStorage.Size > 0)
			{
				GenerateGetData();
			}

			// Startup methods
			if (_startupMethods.Count > 0)
			{
				StartupMethodGenerator.Generate(_module, _startupMethods);
			}

			// Lock objects
			if (_lockObjectFieldByOwner.Count > 0)
			{
				GenerateLockObjectFields();
			}

			if (Methods.Count > 0)
			{
				GenerateControlFlowValues();
			}
		}

		private void GenerateLockObjectFields()
		{
			foreach (var kvp in _lockObjectFieldByOwner)
			{
				GenerateLockObjectFields(kvp.Key, kvp.Value);
			}
		}

		private void GenerateLockObjectFields(BuildType ownerType, List<string> names)
		{
			var ownerTypeRef = ownerType.ToReference(ownerType.Module);
			var fields = ownerType.Fields;
			var instructions = new List<Instruction>(names.Count * 2);

			foreach (string name in names)
			{
				var field = fields.Add();
				field.Name = name;
				field.Visibility = FieldVisibilityFlags.Assembly;
				field.IsStatic = true;
				field.FieldType = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);

				instructions.Add(
					new Instruction(
						OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								ReadOnlyList<TypeSignature>.Empty,
								0,
								0))));

				instructions.Add(
					new Instruction(
						OpCodes.Stsfld,
						new FieldReference(
							name,
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
							ownerTypeRef)));
			}

			var cctorMethod = ownerType.Methods.GetOrCreateStaticConstructor();
			var methodBody = MethodBody.Load(cctorMethod);
			methodBody.Instructions.InsertRange(0, instructions);
			methodBody.Build(cctorMethod);
		}

		/// <summary>
		/// static GetData([mscorlib]System.Int32) : [mscorlib]System.Byte[]
		/// </summary>
		private void GenerateGetData()
		{
			// Methods
			GenerateGetData(Methods.Add());

			// Fields
			// _dataStream : System.IO.Stream
			var field = Fields.Add();
			field.Name = "_dataStream";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				new TypeReference("Stream", "System.IO",
					AssemblyReference.GetMscorlib(_module.Assembly), false);

			// Dependencies
			if (_resourceStorage.HasEncrypt)
			{
				GenerateDecrypt();
			}

			if (_resourceStorage.HasCompress)
			{
				GenerateDecompress();
			}

			GenerateReadInt32();
			GenerateReadStream7BitEncodedInt();
		}

		/// <summary>
		/// static GetData([mscorlib]System.Int32) : [mscorlib]System.Byte[]
		/// </summary>
		private void GenerateGetData(MethodDeclaration method)
		{
			method.Name = "GetData";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// dataId : [mscorlib]System.Int32
				var parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
				}

				// Exception handlers
				{
					var exceptionHandlers = methodBody.ExceptionHandlers;
					exceptionHandlers.Add(
						new ExceptionHandler(
							ExceptionHandlerType.Finally,
							12, 100, 112, 7, 0));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_dataLockObject",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
							new TypeReference(Name, Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
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
							"_dataStream",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference(Name, Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)30));
					instructions.Add(new Instruction(OpCodes.Ldtoken,
						new TypeReference(Name, Namespace, false)));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetTypeFromHandle",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("RuntimeTypeHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_Assembly",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Assembly", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldstr, _resourceStorage.ResourceName));
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
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"_dataStream",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference(Name, Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_dataStream",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference(Name, Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)_resourceStorage.EncryptKey));
					instructions.Add(new Instruction(OpCodes.Xor));
					instructions.Add(new Instruction(OpCodes.Conv_I8));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"set_Position",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_dataStream",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference(Name, Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ReadByte",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_dataStream",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference(Name, Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadStream7BitEncodedInt",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("Stream", "System.IO",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_dataStream",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference(Name, Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
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
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)7));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
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

					// Decrypt
					if (_resourceStorage.HasEncrypt)
					{
						instructions.Add(new Instruction(OpCodes.Ldloc_1));
						instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
						instructions.Add(new Instruction(OpCodes.And));
						instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
						instructions.Add(new Instruction(OpCodes.Bne_Un_S, (sbyte)13));
						instructions.Add(new Instruction(OpCodes.Ldloc_2));
						instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)_resourceStorage.EncryptKey));
						instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
						instructions.Add(new Instruction(OpCodes.Ldloc_0));
						instructions.Add(new Instruction(OpCodes.Call,
							new MethodReference(
								"Decrypt",
								new TypeReference(Name, Namespace, false),
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
					}

					// Decompress
					if (_resourceStorage.HasCompress)
					{
						instructions.Add(new Instruction(OpCodes.Ldloc_1));
						instructions.Add(new Instruction(OpCodes.Ldc_I4_2));
						instructions.Add(new Instruction(OpCodes.And));
						instructions.Add(new Instruction(OpCodes.Ldc_I4_2));
						instructions.Add(new Instruction(OpCodes.Bne_Un_S, (sbyte)7));
						instructions.Add(new Instruction(OpCodes.Ldloc_2));
						instructions.Add(new Instruction(OpCodes.Call,
							new MethodReference(
								"Decompress",
								new TypeReference(Name, Namespace, false),
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

						instructions.Add(new Instruction(OpCodes.Stloc_2));
					}

					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static CheckForExpiredEvaluation() : [mscorlib]System.Void
		/// </summary>
		public void GenerateCheckForExpiredEvaluation(int periodInDays)
		{
			if (_genFlags.IsBitAtIndexOn(1))
				return;

			_genFlags = _genFlags.SetBitAtIndex(1, true);

			int dataID = _resourceStorage.Add(Encoding.UTF8.GetBytes("This assembly was created using an evaluation version of AssemblyDefender, which has expired."), true);
			var buildDate = DateTime.Today.AddDays(-1);
			var expirationDate = DateTime.Today.AddDays(periodInDays);

			var method = (BuildMethod)Methods.Add();
			method.Name = "CheckForExpiredEvaluation";
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
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"get_Today",
							new TypeReference("DateTime", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), true),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("DateTime", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"To_time_t",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("DateTime", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)buildDate.To_time_t()));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)expirationDate.To_time_t()));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)30));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)dataID));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetData",
							new TypeReference(Name, Namespace, false),
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
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetString",
							new TypeReference("Encoding", "System.Text",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("Exception", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Throw));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}

			AddStartupMethod(method);
			GenerateToTimeT();
		}

		/// <summary>
		/// static GetAssemblyName([mscorlib]System.String) : [mscorlib]System.String
		/// </summary>
		public void GenerateGetAssemblyName()
		{
			if (_genFlags.IsBitAtIndexOn(2))
				return;

			_genFlags = _genFlags.SetBitAtIndex(2, true);

			var method = Methods.Add();
			method.Name = "GetAssemblyName";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// name : [mscorlib]System.String
				var parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly);
			}

			// Body
			{
				var methodBody = new MethodBody();
				methodBody.MaxStackSize = 3;
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)2));
					instructions.Add(new Instruction(OpCodes.Ldnull));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)44));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"IndexOf",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Char, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)10));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"Substring",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Starg_S, (byte)0));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"Trim",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ToLower",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static To_time_t([mscorlib]System.DateTime) : [mscorlib]System.Int32
		/// </summary>
		public void GenerateToTimeT()
		{
			if (_genFlags.IsBitAtIndexOn(3))
				return;

			_genFlags = _genFlags.SetBitAtIndex(3, true);

			var method = Methods.Add();
			method.Name = "To_time_t";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// dateTime : [mscorlib]System.DateTime
				var parameter = parameters.Add();
				parameter.Type =
					new TypeReference("DateTime", "System",
						AssemblyReference.GetMscorlib(_module.Assembly), true);
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
						new TypeReference("DaylightTime", "System.Globalization",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"get_CurrentTimeZone",
							new TypeReference("TimeZone", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("TimeZone", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"IsDaylightSavingTime",
							new TypeReference("TimeZone", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("DateTime", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)32));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"get_CurrentTimeZone",
							new TypeReference("TimeZone", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("TimeZone", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldarga_S, (byte)0));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"get_Year",
							new TypeReference("DateTime", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), true),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetDaylightChanges",
							new TypeReference("TimeZone", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("DaylightTime", "System.Globalization",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_Delta",
							new TypeReference("DaylightTime", "System.Globalization",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("TimeSpan", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"op_Subtraction",
							new TypeReference("DateTime", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), true),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("DateTime", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[]
								{
									new TypeReference("DateTime", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
									new TypeReference("TimeSpan", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Starg_S, (byte)0));
					instructions.Add(new Instruction(OpCodes.Ldarga_S, (byte)0));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"get_Ticks",
							new TypeReference("DateTime", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), true),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldc_I8, (long)621355968000000000));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)10000000));
					instructions.Add(new Instruction(OpCodes.Conv_I8));
					instructions.Add(new Instruction(OpCodes.Div));
					instructions.Add(new Instruction(OpCodes.Conv_I4));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ComputeHash([mscorlib]System.Byte[], [mscorlib]System.Int32, [mscorlib]System.Int32) : [mscorlib]System.Int32
		/// </summary>
		public void GenerateComputeHash()
		{
			if (_genFlags.IsBitAtIndexOn(4))
				return;

			_genFlags = _genFlags.SetBitAtIndex(4, true);

			var method = Methods.Add();
			method.Name = "ComputeHash";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// offset : [mscorlib]System.Int32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

				// count : [mscorlib]System.Int32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)-2128831035));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Mul));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)-20));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static Decrypt([mscorlib]System.Byte[], [mscorlib]System.Int32, [mscorlib]System.Int32, [mscorlib]System.Int32) : [mscorlib]System.Void
		/// </summary>
		public void GenerateDecrypt()
		{
			if (_genFlags.IsBitAtIndexOn(5))
				return;

			_genFlags = _genFlags.SetBitAtIndex(5, true);

			var method = Methods.Add();
			method.Name = "Decrypt";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// key : [mscorlib]System.Int32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

				// offset : [mscorlib]System.Int32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

				// count : [mscorlib]System.Int32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)-2128831035));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)90));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_4));
					instructions.Add(new Instruction(OpCodes.Rem));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)5));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Xor));
					instructions.Add(new Instruction(OpCodes.Starg_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Xor));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)16777619));
					instructions.Add(new Instruction(OpCodes.Mul));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_3));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)31));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)28));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_4));
					instructions.Add(new Instruction(OpCodes.Rem));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_3));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)31));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)-25));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Sub));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-94));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static Decompress([mscorlib]System.Byte[]) : [mscorlib]System.Byte[]
		/// </summary>
		public void GenerateDecompress()
		{
			if (_genFlags.IsBitAtIndexOn(6))
				return;

			_genFlags = _genFlags.SetBitAtIndex(6, true);

			var method = Methods.Add();
			method.Name = "Decompress";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// data : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));
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
						new TypeReference("MemoryStream", "System.IO",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						new TypeReference("GZipStream", "System.IO.Compression",
							AssemblyReference.GetSystem(_module.Assembly), false));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Exception handlers
				{
					var exceptionHandlers = methodBody.ExceptionHandlers;
					exceptionHandlers.Add(
						new ExceptionHandler(
							ExceptionHandlerType.Finally,
							19, 39, 58, 10, 0));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("MemoryStream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("MemoryStream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("GZipStream", "System.IO.Compression",
								AssemblyReference.GetSystem(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("Stream", "System.IO",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
									new TypeReference("CompressionMode", "System.IO.Compression",
										AssemblyReference.GetSystem(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)4096));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)9));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"Write",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldlen));
					instructions.Add(new Instruction(OpCodes.Conv_I4));
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
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)-24));
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)10));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
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
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ToArray",
							new TypeReference("MemoryStream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new ArrayType(
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

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
		public void GenerateControlFlowValues()
		{
			if (_genFlags.IsBitAtIndexOn(7))
				return;

			_genFlags = _genFlags.SetBitAtIndex(7, true);

			ControlFlowGenerator.Generate(_module);
		}

		#region Read buffer

		/// <summary>
		/// static ReadBoolean([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.Boolean
		/// </summary>
		public void GenerateReadBoolean()
		{
			if (_genFlags2.IsBitAtIndexOn(5))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(5, true);

			var method = Methods.Add();
			method.Name = "ReadBoolean";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ceq));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ceq));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadInt16([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.Int16
		/// </summary>
		public void GenerateReadInt16()
		{
			if (_genFlags2.IsBitAtIndexOn(6))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(6, true);

			var method = Methods.Add();
			method.Name = "ReadInt16";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int16, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Conv_I2));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadInt32([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.Int32
		/// </summary>
		public void GenerateReadInt32()
		{
			if (_genFlags2.IsBitAtIndexOn(7))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(7, true);

			var method = Methods.Add();
			method.Name = "ReadInt32";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadInt64([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.Int64
		/// </summary>
		public void GenerateReadInt64()
		{
			if (_genFlags2.IsBitAtIndexOn(8))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(8, true);

			var method = Methods.Add();
			method.Name = "ReadInt64";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Conv_U8));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Conv_I8));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)32));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadUInt16([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.UInt16
		/// </summary>
		public void GenerateReadUInt16()
		{
			if (_genFlags2.IsBitAtIndexOn(9))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(9, true);

			GenerateReadInt16();

			var method = Methods.Add();
			method.Name = "ReadUInt16";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt16, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt16",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int16, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Conv_U2));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadUInt32([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.UInt32
		/// </summary>
		public void GenerateReadUInt32()
		{
			if (_genFlags2.IsBitAtIndexOn(10))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(10, true);

			GenerateReadInt32();

			var method = Methods.Add();
			method.Name = "ReadUInt32";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt32, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
							new TypeReference(Name, Namespace, false),
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
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadUInt64([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.UInt64
		/// </summary>
		public void GenerateReadUInt64()
		{
			if (_genFlags2.IsBitAtIndexOn(11))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(11, true);

			GenerateReadInt64();

			var method = Methods.Add();
			method.Name = "ReadUInt64";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt64, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt64",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadSingle([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.Single
		/// </summary>
		public void GenerateReadSingle()
		{
			if (_genFlags2.IsBitAtIndexOn(12))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(12, true);

			GenerateReadInt32();

			var method = Methods.Add();
			method.Name = "ReadSingle";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Float32, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
							new TypeReference(Name, Namespace, false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)0));
					instructions.Add(new Instruction(OpCodes.Conv_U));
					instructions.Add(new Instruction(OpCodes.Ldind_R4));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadDouble([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.Double
		/// </summary>
		public void GenerateReadDouble()
		{
			if (_genFlags2.IsBitAtIndexOn(13))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(13, true);

			GenerateReadInt64();

			var method = Methods.Add();
			method.Name = "ReadDouble";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Float64, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt64",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)0));
					instructions.Add(new Instruction(OpCodes.Conv_U));
					instructions.Add(new Instruction(OpCodes.Ldind_R8));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadDecimal([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.Decimal
		/// </summary>
		public void GenerateReadDecimal()
		{
			if (_genFlags2.IsBitAtIndexOn(14))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(14, true);

			var method = Methods.Add();
			method.Name = "ReadDecimal";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					new TypeReference("Decimal", "System",
						AssemblyReference.GetMscorlib(_module.Assembly), true);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)10));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)10));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)13));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)13));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)17));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)17));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)18));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)18));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)19));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)19));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_4));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Stelem_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Stelem_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Stelem_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Stelem_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("Decimal", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), true),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadBytes([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Int32) : [mscorlib]System.Byte[]
		/// </summary>
		public void GenerateReadBytes()
		{
			if (_genFlags2.IsBitAtIndexOn(15))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(15, true);

			var method = Methods.Add();
			method.Name = "ReadBytes";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// count : [mscorlib]System.Int32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
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
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)7));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"BlockCopy",
							new TypeReference("Buffer", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("Array", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new TypeReference("Array", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadString([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Text.Encoding) : [mscorlib]System.String
		/// </summary>
		public void GenerateReadString()
		{
			if (_genFlags2.IsBitAtIndexOn(16))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(16, true);

			GenerateRead7BitEncodedInt();

			var method = Methods.Add();
			method.Name = "ReadString";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// encoding : [mscorlib]System.Text.Encoding
				parameter = parameters.Add();
				parameter.Type =
					new TypeReference("Encoding", "System.Text",
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(Name, Namespace, false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Bge_S, (sbyte)2));
					instructions.Add(new Instruction(OpCodes.Ldnull));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)6));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"Empty",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly))));
					instructions.Add(new Instruction(OpCodes.Ret));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetString",
							new TypeReference("Encoding", "System.Text",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadStringIntern([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Text.Encoding) : [mscorlib]System.String
		/// </summary>
		public void GenerateReadStringIntern()
		{
			if (_genFlags2.IsBitAtIndexOn(17))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(17, true);

			GenerateReadString();

			var method = Methods.Add();
			method.Name = "ReadStringIntern";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// encoding : [mscorlib]System.Text.Encoding
				parameter = parameters.Add();
				parameter.Type =
					new TypeReference("Encoding", "System.Text",
						AssemblyReference.GetMscorlib(_module.Assembly), false);
			}

			// Body
			{
				var methodBody = new MethodBody();
				methodBody.MaxStackSize = 3;
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadString",
							new TypeReference(Name, Namespace, false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Bge_S, (sbyte)7));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Intern",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
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
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static Read7BitEncodedInt([mscorlib]System.Byte[], [mscorlib]System.Int32&) : [mscorlib]System.Int32
		/// </summary>
		public void GenerateRead7BitEncodedInt()
		{
			if (_genFlags2.IsBitAtIndexOn(18))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(18, true);

			var method = Methods.Add();
			method.Name = "Read7BitEncodedInt";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)127));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)31));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_7));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)128));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)-37));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static ReadStream7BitEncodedInt([mscorlib]System.IO.Stream) : [mscorlib]System.Int32
		/// </summary>
		public void GenerateReadStream7BitEncodedInt()
		{
			if (_genFlags2.IsBitAtIndexOn(19))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(19, true);

			var method = Methods.Add();
			method.Name = "ReadStream7BitEncodedInt";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// stream : [mscorlib]System.IO.Stream
				var parameter = parameters.Add();
				parameter.Type =
					new TypeReference("Stream", "System.IO",
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ReadByte",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)127));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)31));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_7));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)128));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)-32));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		#endregion

		#region Write buffer

		/// <summary>
		/// static WriteBoolean([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Boolean) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteBoolean()
		{
			if (_genFlags2.IsBitAtIndexOn(20))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(20, true);

			var method = Methods.Add();
			method.Name = "WriteBoolean";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.Boolean
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly);
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static WriteInt16([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Int16) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteInt16()
		{
			if (_genFlags2.IsBitAtIndexOn(21))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(21, true);

			var method = Methods.Add();
			method.Name = "WriteInt16";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.Int16
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int16, _module.Assembly);
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static WriteInt32([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Int32) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteInt32()
		{
			if (_genFlags2.IsBitAtIndexOn(22))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(22, true);

			var method = Methods.Add();
			method.Name = "WriteInt32";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.Int32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static WriteInt64([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Int64) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteInt64()
		{
			if (_genFlags2.IsBitAtIndexOn(23))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(23, true);

			var method = Methods.Add();
			method.Name = "WriteInt64";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.Int64
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly);
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)32));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)40));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)48));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)56));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static WriteUInt16([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.UInt16) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteUInt16()
		{
			if (_genFlags2.IsBitAtIndexOn(24))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(24, true);

			GenerateWriteInt16();

			var method = Methods.Add();
			method.Name = "WriteUInt16";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.UInt16
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt16, _module.Assembly);
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
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Conv_I2));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"WriteInt16",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int16, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static WriteUInt32([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.UInt32) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteUInt32()
		{
			if (_genFlags2.IsBitAtIndexOn(25))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(25, true);

			GenerateWriteInt32();

			var method = Methods.Add();
			method.Name = "WriteUInt32";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.UInt32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt32, _module.Assembly);
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
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"WriteInt32",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static WriteUInt64([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.UInt64) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteUInt64()
		{
			if (_genFlags2.IsBitAtIndexOn(26))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(26, true);

			GenerateWriteInt64();

			var method = Methods.Add();
			method.Name = "WriteUInt64";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.UInt64
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt64, _module.Assembly);
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
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"WriteInt64",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static WriteSingle([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Single) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteSingle()
		{
			if (_genFlags2.IsBitAtIndexOn(27))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(27, true);

			GenerateWriteInt32();

			var method = Methods.Add();
			method.Name = "WriteSingle";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.Single
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Float32, _module.Assembly);
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
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldarga_S, (byte)2));
					instructions.Add(new Instruction(OpCodes.Conv_U));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"WriteInt32",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static WriteDouble([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Double) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteDouble()
		{
			if (_genFlags2.IsBitAtIndexOn(28))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(28, true);

			GenerateWriteInt64();

			var method = Methods.Add();
			method.Name = "WriteDouble";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.Double
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Float64, _module.Assembly);
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
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldarga_S, (byte)2));
					instructions.Add(new Instruction(OpCodes.Conv_U));
					instructions.Add(new Instruction(OpCodes.Ldind_I8));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"WriteInt64",
							new TypeReference(Name, Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int64, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static WriteDecimal([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Decimal) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWriteDecimal()
		{
			if (_genFlags2.IsBitAtIndexOn(29))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(29, true);

			var method = Methods.Add();
			method.Name = "WriteDecimal";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.Decimal
				parameter = parameters.Add();
				parameter.Type =
					new TypeReference("Decimal", "System",
						AssemblyReference.GetMscorlib(_module.Assembly), true);
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
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetBits",
							new TypeReference("Decimal", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), true),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new ArrayType(
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
								new TypeSignature[]
								{
									new TypeReference("Decimal", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldelem_I4));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Ldelem_I4));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_2));
					instructions.Add(new Instruction(OpCodes.Ldelem_I4));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_3));
					instructions.Add(new Instruction(OpCodes.Ldelem_I4));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)10));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)10));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)13));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)13));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)17));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)17));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)18));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)18));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)19));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)19));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shr));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static Write7BitEncodedInt([mscorlib]System.Byte[], [mscorlib]System.Int32&, [mscorlib]System.Int32) : [mscorlib]System.Void
		/// </summary>
		public void GenerateWrite7BitEncodedInt()
		{
			if (_genFlags2.IsBitAtIndexOn(30))
				return;

			_genFlags2 = _genFlags2.SetBitAtIndex(30, true);

			var method = Methods.Add();
			method.Name = "Write7BitEncodedInt";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// buffer : [mscorlib]System.Byte[]
				var parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));

				// pos : [mscorlib]System.Int32&
				parameter = parameters.Add();
				parameter.Type =
					new ByRefType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));

				// value : [mscorlib]System.Int32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)23));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldind_I4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stind_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)128));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Conv_U1));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_7));
					instructions.Add(new Instruction(OpCodes.Shr_Un));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)128));
					instructions.Add(new Instruction(OpCodes.Bge_Un_S, (sbyte)-31));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		#endregion

		#endregion

		#region Static

		public static void Create(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				module.CreateMainType();
			}
		}

		public static void Generate(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				module.MainType.Generate();

				// Strip not used
				var mainType = module.MainType;
				if (mainType.Methods.Count == 0 && mainType.Fields.Count == 0 && mainType.NestedTypes.Count == 0)
				{
					module.RemoveMainType();
				}
			}
		}

		#endregion

		#region C# Code

		/***************************************************************************
		internal static void CheckForExpiredEvaluation()
		{
			// BuildDate: 12345
			// ExpirationDate: 56789
			int today = To_time_t(DateTime.Today);
			if (today < 12345 || today > 56789)
			{
				byte[] buffer = GetData(1234567);
				string s = Encoding.UTF8.GetString(buffer);
				throw new Exception(s);
			}
		}

		internal static string GetAssemblyName(string name)
		{
			if (name == null)
				return null;

			int index = name.IndexOf(',');
			if (index > 0)
				name = name.Substring(0, index);

			return name.Trim().ToLower();
		}

		internal static int To_time_t(DateTime dateTime)
		{
			if (TimeZone.CurrentTimeZone.IsDaylightSavingTime(dateTime))
			{
				var daylightTime = TimeZone.CurrentTimeZone.GetDaylightChanges(dateTime.Year);
				dateTime = dateTime - daylightTime.Delta;
			}

			// Return time since 01.01.1970
			return (int)((dateTime.Ticks - 621355968000000000L) / TimeSpan.TicksPerSecond);
		}

		internal static void Decrypt(byte[] buffer, int key, int offset, int count)
		{
			// Init salt.
			int salt = -2128831035;

			for (int i = offset, num = 0; num < count; i++, num++)
			{
				byte b = buffer[i];

				int offset4 = (num % 4);

				if (offset4 == 0)
					key ^= salt; // Salt key.

				salt = (salt ^ b) * 16777619;

				byte b2 = (byte)(key >> (offset4 << 3));
				if (b2 == 0)
				{
					int j = 1;
					do
					{
						b2 = (byte)(key >> (((num + j++) % 4) << 3));
					}
					while (b2 == 0);
				}

				b -= b2;

				buffer[i] = b;
			}
		}

		internal static byte[] Decompress(byte[] data)
		{
			var destination = new MemoryStream();
			using (var source = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
			{
				byte[] bytes = new byte[4096];
				int n;
				while ((n = source.Read(bytes, 0, bytes.Length)) != 0)
				{
					destination.Write(bytes, 0, n);
				}
			}

			return destination.ToArray();
		}

		internal static int ReadStream7BitEncodedInt(Stream stream)
		{
			int num3;
			int num = 0;
			int num2 = 0;
			do
			{
				num3 = stream.ReadByte();
				num |= (num3 & 0x7f) << num2;
				num2 += 7;
			}
			while ((num3 & 0x80) != 0);

			return num;
		}

		#region Read buffer

		internal static bool ReadBoolean(byte[] buffer, ref int pos)
		{
			return (buffer[pos++] != 0);
		}

		internal static short ReadInt16(byte[] buffer, ref int pos)
		{
			return
				(short)
				(
					(buffer[pos++]) |
					(buffer[pos++] << 0x8)
				);
		}

		internal static int ReadInt32(byte[] buffer, ref int pos)
		{
			return
				(
					(buffer[pos++]) |
					(buffer[pos++] << 0x8) |
					(buffer[pos++] << 0x10) |
					(buffer[pos++] << 0x18)
				);
		}

		internal static long ReadInt64(byte[] buffer, ref int pos)
		{
			int i1 =
				(
					(buffer[pos++]) |
					(buffer[pos++] << 0x8) |
					(buffer[pos++] << 0x10) |
					(buffer[pos++] << 0x18)
				);

			int i2 =
				(
					(buffer[pos++] |
					(buffer[pos++] << 0x8)) |
					(buffer[pos++] << 0x10) |
					(buffer[pos++] << 0x18)
				);

			return (uint)i1 | ((long)i2 << 32);
		}

		internal static ushort ReadUInt16(byte[] buffer, ref int pos)
		{
			return (ushort)ReadInt16(buffer, ref pos);
		}

		internal static uint ReadUInt32(byte[] buffer, ref int pos)
		{
			return (uint)ReadInt32(buffer, ref pos);
		}

		internal static ulong ReadUInt64(byte[] buffer, ref int pos)
		{
			return (ulong)ReadInt64(buffer, ref pos);
		}

		internal static unsafe float ReadSingle(byte[] buffer, ref int pos)
		{
			int val = ReadInt32(buffer, ref pos);
			return *(float*)&val;
		}

		internal static unsafe double ReadDouble(byte[] buffer, ref int pos)
		{
			long val = ReadInt64(buffer, ref pos);
			return *(double*)&val;
		}

		internal static decimal ReadDecimal(byte[] buffer, ref int pos)
		{
			int lo =
				(
					(buffer[pos++]) |
					(buffer[pos++] << 0x8) |
					(buffer[pos++] << 0x10) |
					(buffer[pos++] << 0x18)
				);

			int mid =
				(
					(buffer[pos++]) |
					(buffer[pos++] << 0x8) |
					(buffer[pos++] << 0x10) |
					(buffer[pos++] << 0x18)
				);

			int hi =
				(
					(buffer[pos++]) |
					(buffer[pos++] << 0x8) |
					(buffer[pos++] << 0x10) |
					(buffer[pos++] << 0x18)
				);

			int flags =
				(
					(buffer[pos++]) |
					(buffer[pos++] << 0x8) |
					(buffer[pos++] << 0x10) |
					(buffer[pos++] << 0x18)
				);

			return new decimal(new int[] { lo, mid, hi, flags });
		}

		internal static byte[] ReadBytes(byte[] buffer, ref int pos, int count)
		{
			if (count == 0)
				return new byte[0];

			byte[] result = new byte[count];
			Buffer.BlockCopy(buffer, pos, result, 0, count);
			pos += count;

			return result;
		}

		internal static string ReadString(byte[] buffer, ref int pos, Encoding encoding)
		{
			int length = Read7BitEncodedInt(buffer, ref pos);
			if (length < 0)
				return null;

			if (length == 0)
				return string.Empty;

			string s = encoding.GetString(buffer, pos, length);

			pos += length;

			return s;
		}

		internal static string ReadStringIntern(byte[] buffer, ref int pos, Encoding encoding)
		{
			int p = pos;
			string s = ReadString(buffer, ref pos, encoding);
			if (p < pos)
			{
				s = string.Intern(s);
			}

			return s;
		}

		internal static int Read7BitEncodedInt(byte[] buffer, ref int pos)
		{
			int length = buffer.Length;

			byte num3;
			int num = 0;
			int num2 = 0;
			do
			{
				num3 = buffer[pos++];
				num |= (num3 & 0x7f) << num2;
				num2 += 7;
			}
			while ((num3 & 0x80) != 0);

			return num;
		}

		#endregion

		#region Write buffer

		internal static void WriteBoolean(byte[] buffer, ref int pos, bool value)
		{
			buffer[pos++] = (byte)(value ? 1 : 0);
		}

		internal static void WriteInt16(byte[] buffer, ref int pos, short value)
		{
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 0x8);
		}

		internal static void WriteInt32(byte[] buffer, ref int pos, int value)
		{
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 0x8);
			buffer[pos++] = (byte)(value >> 0x10);
			buffer[pos++] = (byte)(value >> 0x18);
		}

		internal static void WriteInt64(byte[] buffer, ref int pos, long value)
		{
			buffer[pos++] = (byte)value;
			buffer[pos++] = (byte)(value >> 0x8);
			buffer[pos++] = (byte)(value >> 0x10);
			buffer[pos++] = (byte)(value >> 0x18);
			buffer[pos++] = (byte)(value >> 0x20);
			buffer[pos++] = (byte)(value >> 0x28);
			buffer[pos++] = (byte)(value >> 0x30);
			buffer[pos++] = (byte)(value >> 0x38);
		}

		internal static void WriteUInt16(byte[] buffer, ref int pos, ushort value)
		{
			WriteInt16(buffer, ref pos, (short)value);
		}

		internal static void WriteUInt32(byte[] buffer, ref int pos, uint value)
		{
			WriteInt32(buffer, ref pos, (int)value);
		}

		internal static void WriteUInt64(byte[] buffer, ref int pos, ulong value)
		{
			WriteInt64(buffer, ref pos, (long)value);
		}

		internal static unsafe void WriteSingle(byte[] buffer, ref int pos, float value)
		{
			WriteInt32(buffer, ref pos, *((int*)&value));
		}

		internal static unsafe void WriteDouble(byte[] buffer, ref int pos, double value)
		{
			WriteInt64(buffer, ref pos, *((long*)&value));
		}

		internal static void WriteDecimal(byte[] buffer, ref int pos, decimal value)
		{
			int[] bits = decimal.GetBits(value);
			int lo = bits[0];
			int mid = bits[1];
			int hi = bits[2];
			int flags = bits[3];

			buffer[pos++] = (byte)lo;
			buffer[pos++] = (byte)(lo >> 0x8);
			buffer[pos++] = (byte)(lo >> 0x10);
			buffer[pos++] = (byte)(lo >> 0x18);

			buffer[pos++] = (byte)mid;
			buffer[pos++] = (byte)(mid >> 0x8);
			buffer[pos++] = (byte)(mid >> 0x10);
			buffer[pos++] = (byte)(mid >> 0x18);

			buffer[pos++] = (byte)hi;
			buffer[pos++] = (byte)(hi >> 0x8);
			buffer[pos++] = (byte)(hi >> 0x10);
			buffer[pos++] = (byte)(hi >> 0x18);

			buffer[pos++] = (byte)flags;
			buffer[pos++] = (byte)(flags >> 0x8);
			buffer[pos++] = (byte)(flags >> 0x10);
			buffer[pos++] = (byte)(flags >> 0x18);
		}

		internal static void Write7BitEncodedInt(byte[] buffer, ref int pos, int value)
		{
			uint num = (uint)value;
			while (num >= 0x80)
			{
				buffer[pos++] = (byte)(num | 0x80);
				num = num >> 7;
			}
		}

		#endregion

		#region Data

		internal static object _dataLockObject = new object();
		internal static int[] _dataPositions;

		internal static byte[] GetData(int dataId)
		{
			int bufferSize;
			int flags;
			byte[] buffer;
			lock (_dataLockObject)
			{
				using (var stream = typeof(AssemblyDefender).Assembly.GetManifestResourceStream("ResourceName"))
				{
					if (_dataPositions == null)
					{
						int count = ReadStream7BitEncodedInt(stream);
						int posBufferSize = count * 4;
						byte[] posBuffer = new byte[posBufferSize];
						stream.Read(posBuffer, 0, posBufferSize);

						int pos = 0;
						_dataPositions = new int[count];

						for (int i = 0; i < count; i++)
						{
							_dataPositions[i] = ReadInt32(posBuffer, ref pos);
						}
					}

					stream.Position = _dataPositions[(dataId ^ 1234567) - 1];
					bufferSize = ReadStream7BitEncodedInt(stream);
					flags = stream.ReadByte();
					buffer = new byte[bufferSize];
					stream.Read(buffer, 0, bufferSize);
				}
			}

			// Decrypt
			if ((flags & 1) == 1)
			{
				Decrypt(buffer, 1234567, 0, bufferSize);
			}

			// Decompress
			if ((flags & 2) == 2)
			{
				buffer = Decompress(buffer);
			}

			return buffer;
		}

		#endregion

		***************************************************************************/

		#endregion
	}
}
