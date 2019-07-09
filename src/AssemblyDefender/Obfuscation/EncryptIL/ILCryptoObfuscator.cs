using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ILCryptoObfuscator
	{
		#region Fields

		private int _count;
		private int _dataMapBlobID;
		private BuildModule _module;
		private MainType _mainType;
		private ILCryptoBlobBuilder _blobBuilder;
		private List<int> _dataIDList;
		private Dictionary<int, List<BuildMethod>> _methodsByInvokeType;

		#endregion

		#region Ctors

		public ILCryptoObfuscator(BuildAssembly assembly, HashList<string> strings)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_module = (BuildModule)assembly.Module;
			_mainType = _module.MainType;
			_blobBuilder = new ILCryptoBlobBuilder(assembly, strings);
			_dataIDList = new List<int>();
			_methodsByInvokeType = new Dictionary<int, List<BuildMethod>>();
		}

		#endregion

		#region Properties

		public ILCryptoBlobBuilder BlobBuilder
		{
			get { return _blobBuilder; }
		}

		#endregion

		#region Methods

		public void Obfuscate()
		{
			foreach (BuildType type in _module.Types)
			{
				Collect(type);
			}

			foreach (var methods in _methodsByInvokeType.Values)
			{
				Obfuscate(methods);
			}

			if (_count > 0)
			{
				BuildMethodDataMap();
				GenerateDecrypt();

				var assembler = _module.Assembler;
				assembler.Tasks.Add(_blobBuilder, 1500);
			}
		}

		private void Obfuscate(List<BuildMethod> methods)
		{
			if (methods.Count == 0)
				return;

			var invokeType = methods[0].ILCrypto.InvokeMethod.OwnerType;

			GenerateInvokeType(invokeType);

			foreach (var method in methods)
			{
				int dataID = _blobBuilder.Build(method);
				_dataIDList.Add(dataID);
				GenerateProxyMethod(method);
			}

			_count += methods.Count;
		}

		private void Collect(BuildType type)
		{
			if (type.IsMainType)
				return;

			int genericParameterCount = type.GenericParameters.Count;

			List<BuildMethod> methods;
			if (!_methodsByInvokeType.TryGetValue(genericParameterCount, out methods))
			{
				methods = new List<BuildMethod>();
				_methodsByInvokeType.Add(genericParameterCount, methods);
			}

			foreach (BuildMethod method in type.Methods)
			{
				if (method.EncryptIL)
				{
					methods.Add(method);
				}
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Collect(nestedType);
			}
		}

		private void BuildMethodDataMap()
		{
			var storage = ((BuildAssembly)_module.Assembly).ResourceStorage;

			int pos = 0;
			var blob = new Blob(_dataIDList.Count * 4);
			for (int i = 0; i < _dataIDList.Count; i++)
			{
				blob.Write(ref pos, (int)_dataIDList[i]);
			}

			_dataMapBlobID = storage.Add(blob.GetBuffer(), 0, blob.Length, false, false);
		}

		private void GenerateProxyMethod(BuildMethod method)
		{
			var cryptoMethod = method.ILCrypto;

			var invokeMethodSig = GetInvokeMethodSig(cryptoMethod);

			// Create body.
			var methodBody = new MethodBody();

			var instructions = methodBody.Instructions;

			// Load local variables.
			int argumentCount = invokeMethodSig.Arguments.Count - 1;
			for (int i = 0; i < argumentCount; i++)
			{
				instructions.Add(Instruction.GetLdarg(i));
			}

			instructions.Add(Instruction.GetLdc(cryptoMethod.MethodID));

			// Call delegate's Invoke method.
			instructions.Add(new Instruction(OpCodes.Call, invokeMethodSig));

			// Ret
			instructions.Add(new Instruction(OpCodes.Ret));

			methodBody.MaxStackSize = invokeMethodSig.Arguments.Count + 1;

			methodBody.Build(method);
		}

		private void GenerateInvokeType(ILCryptoInvokeType invokeType)
		{
			var type = _module.Types.Add();
			type.Name = invokeType.TypeName;
			type.Namespace = _mainType.Namespace;
			type.Visibility = TypeVisibilityFlags.Public;
			type.IsAbstract = true;
			type.IsSealed = true;
			type.BaseType = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);

			// Generic parameters
			for (int i = 0; i < invokeType.GenericParameterCount; i++)
			{
				type.GenericParameters.Add();
			}

			// Fields
			{
				var fields = type.Fields;

				// _delegates : [mscorlib]System.Object[]
				var field = fields.Add();
				field.Name = "_delegates";
				field.Visibility = FieldVisibilityFlags.Private;
				field.IsStatic = true;
				field.FieldType =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));

				if (invokeType.GenericParameterCount > 0)
				{
					// _genericTypeArguments : [mscorlib]System.Type[]
					field = fields.Add();
					field.Name = "_genericTypeArguments";
					field.Visibility = FieldVisibilityFlags.Private;
					field.IsStatic = true;
					field.FieldType =
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly));
				}
			}

			// Methods
			{
				var methods = type.Methods;

				var invokeTypeSig = GetInvokeTypeSig(invokeType);

				GenerateInvokeType_CCtor(methods.Add(), invokeType, invokeTypeSig);
				GenerateInvokeType_Decrypt(methods.Add(), invokeType, invokeTypeSig);

				foreach (var invokeMethod in invokeType.InvokeMethods)
				{
					GenerateInvokeType_Invoke(methods.Add(), invokeType, invokeTypeSig, invokeMethod);
				}
			}
		}

		/// <summary>
		/// static .cctor() : [mscorlib]System.Void
		/// </summary>
		private void GenerateInvokeType_CCtor(MethodDeclaration method, ILCryptoInvokeType invokeType, TypeSignature invokeTypeSig)
		{
			method.Name = CodeModelUtils.MethodStaticConstructorName;
			method.Visibility = MethodVisibilityFlags.Private;
			method.IsStatic = true;
			method.IsHideBySig = true;
			method.IsSpecialName = true;
			method.IsRuntimeSpecialName = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly);
			}

			// Body
			{
				var methodBody = new MethodBody();

				methodBody.MaxStackSize =
					invokeType.GenericParameterCount == 0 ?
						MethodBody.DefaultMaxStackSize :
						invokeType.GenericParameterCount + 2;

				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;

					if (invokeType.GenericParameterCount > 0)
					{
						localVariables.Add(
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)));
					}
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;

					// _delegates = new object[count];
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)invokeType.MethodCount));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"_delegates",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly)),
								invokeTypeSig)));

					// _genericTypeArguments = new Type[] { typeof(T1), typeof(T2), .... };
					if (invokeType.GenericParameterCount > 0)
					{
						instructions.Add(Instruction.GetLdc(invokeType.GenericParameterCount));
						instructions.Add(new Instruction(OpCodes.Newarr,
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)));
						instructions.Add(new Instruction(OpCodes.Stloc_0));

						for (int i = 0; i < invokeType.GenericParameterCount; i++)
						{
							instructions.Add(new Instruction(OpCodes.Ldloc_0));
							instructions.Add(Instruction.GetLdc(i));
							instructions.Add(new Instruction(OpCodes.Ldtoken,
								new GenericParameterType(false, i)));
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
							instructions.Add(new Instruction(OpCodes.Stelem_Ref));
						}

						instructions.Add(new Instruction(OpCodes.Ldloc_0));
						instructions.Add(new Instruction(OpCodes.Stsfld,
							new FieldReference(
								"_genericTypeArguments",
								new ArrayType(
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									invokeTypeSig)));
					}

					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static Invoke[...](..., [mscorlib]System.Int32) : xxx
		/// </summary>
		private void GenerateInvokeType_Invoke(MethodDeclaration method, ILCryptoInvokeType invokeType, TypeSignature invokeTypeSig, ILCryptoInvokeMethod invokeMethod)
		{
			method.Name = invokeType.InvokeMethodName;
			method.Visibility = MethodVisibilityFlags.Public;
			method.IsStatic = true;
			method.IsHideBySig = true;

			var callSite = invokeMethod.CallSite;
			int argumentCount = callSite.Arguments.Count;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type = callSite.ReturnType;
			}

			// Parameters
			{
				var parameters = method.Parameters;
				var arguments = callSite.Arguments;
				for (int i = 0; i < argumentCount; i++)
				{
					var parameter = parameters.Add();
					parameter.Type = arguments[i];
				}
			}

			// Generic parameters
			if (invokeMethod.GenericParameterCount > 0)
			{
				var genericParameters = method.GenericParameters;

				for (int i = 0; i < invokeMethod.GenericParameterCount; i++)
				{
					genericParameters.Add();
				}
			}

			// Body
			{
				var methodBody = new MethodBody();
				methodBody.MaxStackSize = Math.Max(argumentCount + 1, 3);
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_delegates",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly)),
								invokeTypeSig)));
					instructions.Add(Instruction.GetLdarg(argumentCount - 1));
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));

					var ldargIndex = Instruction.GetLdarg(argumentCount - 1);

					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)(ldargIndex.Size + 6)));
					instructions.Add(ldargIndex);
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Decrypt",
							invokeTypeSig,
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));

					instructions.Add(new Instruction(OpCodes.Ldloc_0));

					for (int i = 0; i < argumentCount - 1; i++)
					{
						instructions.Add(Instruction.GetLdarg(i));
					}

					// Call invoke method
					var delegateType = invokeMethod.DelegateType;

					TypeSignature delegateTypeSig;
					if (invokeMethod.GenericParameterCount > 0)
					{
						var delegateGenericArguments = new TypeSignature[invokeMethod.GenericParameterCount];
						for (int i = 0; i < invokeMethod.GenericParameterCount; i++)
						{
							delegateGenericArguments[i] = new GenericParameterType(true, i);
						}

						delegateTypeSig = new GenericTypeReference(delegateType.DeclaringType, delegateGenericArguments);
					}
					else
					{
						delegateTypeSig = delegateType.DeclaringType;
					}

					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"Invoke",
							delegateTypeSig,
							delegateType.InvokeCallSite)));

					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static Decrypt([mscorlib]System.Int32) : [mscorlib]System.Object
		/// </summary>
		private void GenerateInvokeType_Decrypt(MethodDeclaration method, ILCryptoInvokeType invokeType, TypeSignature invokeTypeSig)
		{
			method.Name = "Decrypt";
			method.Visibility = MethodVisibilityFlags.Private;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);
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
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(Instruction.GetLdc(_count));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_delegates",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly)),
							invokeTypeSig)));

					if (invokeType.GenericParameterCount > 0)
					{
						instructions.Add(new Instruction(OpCodes.Ldsfld,
							new FieldReference(
								"_genericTypeArguments",
								new ArrayType(
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									invokeTypeSig)));
					}
					else
					{
						instructions.Add(new Instruction(OpCodes.Ldsfld,
							new FieldReference(
								"EmptyTypes",
								new ArrayType(
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly))));
					}

					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"DecryptMethod",
							new TypeReference(_mainType.Name, _mainType.Namespace, false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		private void GenerateDecrypt()
		{
			// Methods
			var initMethod = (BuildMethod)_mainType.Methods.Add();
			GenerateDecrypt_Init(initMethod);
			GenerateDecrypt_Decrypt(_mainType.Methods.Add());
			_mainType.AddStartupMethod(initMethod);

			// Fields
			{
				var field = _mainType.Fields.Add();
				field.Name = "_methodOffsets";
				field.Visibility = FieldVisibilityFlags.Assembly;
				field.IsStatic = true;
				field.FieldType =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));
			}

			// Dependencies
			_mainType.GenerateReadInt32();
			_mainType.GenerateReadBytes();
			_mainType.GenerateReadString();
			_mainType.GenerateRead7BitEncodedInt();
			_mainType.GenerateReadStream7BitEncodedInt();
			_mainType.GenerateWriteInt32();
		}

		/// <summary>
		/// static ILCryptoInitialize() : [mscorlib]System.Void
		/// </summary>
		private void GenerateDecrypt_Init(MethodDeclaration method)
		{
			method.Name = "ILCryptoInitialize";
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
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)_dataMapBlobID));
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
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"_methodOffsets",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		/// <summary>
		/// static DecryptMethod([mscorlib]System.Int32, [mscorlib]System.Int32, [mscorlib]System.Object[], [mscorlib]System.Type[]) : [mscorlib]System.Object
		/// </summary>
		private void GenerateDecrypt_Decrypt(MethodDeclaration method)
		{
			method.Name = "DecryptMethod";
			method.Visibility = MethodVisibilityFlags.Assembly;
			method.IsStatic = true;
			method.IsHideBySig = true;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly);
			}

			// Parameters
			{
				var parameters = method.Parameters;

				// index : [mscorlib]System.Int32
				var parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

				// offset : [mscorlib]System.Int32
				parameter = parameters.Add();
				parameter.Type =
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly);

				// delegates : [mscorlib]System.Object[]
				parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));

				// genericTypeArguments : [mscorlib]System.Type[]
				parameter = parameters.Add();
				parameter.Type =
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly));
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
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly));
					localVariables.Add(
						new TypeReference("Module", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new TypeReference("DynamicMethod", "System.Reflection.Emit",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						new TypeReference("DynamicILInfo", "System.Reflection.Emit",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					localVariables.Add(
						new TypeReference("SignatureHelper", "System.Reflection.Emit",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly));
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
						new TypeReference("MethodBase", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly));
					localVariables.Add(
						new TypeReference("MemberInfo", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						new TypeReference("MethodBase", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly)));
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
							9, 1184, 1193, 8, 0));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)39));
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
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)38));
					instructions.Add(new Instruction(OpCodes.Leave, (int)1177));
					instructions.Add(new Instruction(OpCodes.Ldarg_1));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_4));
					instructions.Add(new Instruction(OpCodes.Mul));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_methodOffsets",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace,false))));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetData",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"Strings",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace,false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"Strings",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace,false))));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"Strings",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace,false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Ldelem_Ref));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Concat",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldtoken,
						new TypeReference(_mainType.Name, _mainType.Namespace,false)));
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
							"get_Module",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Module", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"EmptyTypes",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveType",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)3));
					instructions.Add(new Instruction(OpCodes.Ldnull));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveType",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)10));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Beq_S, (sbyte)12));
					instructions.Add(new Instruction(OpCodes.Ldtoken,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly)));
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
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)18));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveType",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_2));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_2));
					instructions.Add(new Instruction(OpCodes.Bne_Un_S, (sbyte)61));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)13));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)13));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)29));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveType",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stelem_Ref));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)14));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)13));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-35));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)18));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("DynamicMethod", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)11));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)12));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("DynamicMethod", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new TypeReference("Module", "System.Reflection",
										AssemblyReference.GetMscorlib(_module.Assembly), false),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetDynamicILInfo",
							new TypeReference("DynamicMethod", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("DynamicILInfo", "System.Reflection.Emit",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_4));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_4));
					instructions.Add(new Instruction(OpCodes.Bne_Un_S, (sbyte)103));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetLocalVarSigHelper",
							new TypeReference("SignatureHelper", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								new TypeReference("SignatureHelper", "System.Reflection.Emit",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)18));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)19));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Ceq));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)21));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)55));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)20));
					instructions.Add(new Instruction(OpCodes.Brtrue_S, (sbyte)3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)13));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ceq));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ceq));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)22));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)18));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveType",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)22));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"AddArgument",
							new TypeReference("SignatureHelper", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)21));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)21));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)21));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)19));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-61));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)18));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetSignature",
							new TypeReference("SignatureHelper", "System.Reflection.Emit",
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)17));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)17));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_2));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)40));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)40));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_7));
					instructions.Add(new Instruction(OpCodes.Stelem_I1));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)40));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)17));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)17));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"SetLocalSignature",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
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
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)23));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)25));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Bne_Un_S, (sbyte)10));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)25));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)32));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)32));
					instructions.Add(new Instruction(OpCodes.Bne_Un, (int)589));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)26));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)26));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)27));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)28));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)19));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)27));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)28));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"Read7BitEncodedInt",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Stelem_I4));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)28));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)28));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)28));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)26));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-25));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)29));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)30));
					instructions.Add(new Instruction(OpCodes.Br, (int)520));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)29));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)27));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)30));
					instructions.Add(new Instruction(OpCodes.Ldelem_I4));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)29));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)29));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)31));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)31));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadInt32",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Conv_I8));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)-16777216));
					instructions.Add(new Instruction(OpCodes.Conv_U8));
					instructions.Add(new Instruction(OpCodes.And));
					instructions.Add(new Instruction(OpCodes.Conv_I4));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)32));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)32));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)100663296));
					instructions.Add(new Instruction(OpCodes.Bgt_S, (sbyte)58));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)33554432));
					instructions.Add(new Instruction(OpCodes.Bgt_S, (sbyte)23));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)16777216));
					instructions.Add(new Instruction(OpCodes.Beq_S, (sbyte)113));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)33554432));
					instructions.Add(new Instruction(OpCodes.Beq_S, (sbyte)104));
					instructions.Add(new Instruction(OpCodes.Br, (int)416));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)67108864));
					instructions.Add(new Instruction(OpCodes.Beq, (int)199));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)100663296));
					instructions.Add(new Instruction(OpCodes.Beq_S, (sbyte)109));
					instructions.Add(new Instruction(OpCodes.Br, (int)390));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)285212672));
					instructions.Add(new Instruction(OpCodes.Bgt_S, (sbyte)29));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)167772160));
					instructions.Add(new Instruction(OpCodes.Beq, (int)195));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)285212672));
					instructions.Add(new Instruction(OpCodes.Beq, (int)317));
					instructions.Add(new Instruction(OpCodes.Br, (int)352));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)452984832));
					instructions.Add(new Instruction(OpCodes.Beq_S, (sbyte)26));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)721420288));
					instructions.Add(new Instruction(OpCodes.Beq_S, (sbyte)48));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)41));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)1879048192));
					instructions.Add(new Instruction(OpCodes.Beq, (int)302));
					instructions.Add(new Instruction(OpCodes.Br, (int)317));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveType",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_TypeHandle",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeTypeHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetTokenFor",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("RuntimeTypeHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Br, (int)292));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveMethod",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("MethodBase", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)33));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)33));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_DeclaringType",
							new TypeReference("MemberInfo", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)34));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)34));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)33));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)33));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_MethodHandle",
							new TypeReference("MethodBase", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeMethodHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)33));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_DeclaringType",
							new TypeReference("MemberInfo", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_TypeHandle",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeTypeHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetTokenFor",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("RuntimeMethodHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
									new TypeReference("RuntimeTypeHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Br, (int)232));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)33));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_MethodHandle",
							new TypeReference("MethodBase", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeMethodHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetTokenFor",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("RuntimeMethodHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Br, (int)211));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveField",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("FieldInfo", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_FieldHandle",
							new TypeReference("FieldInfo", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeFieldHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetTokenFor",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("RuntimeFieldHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Br, (int)180));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Ldarg_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveMember",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("MemberInfo", "System.Reflection",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
									new ArrayType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)35));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)35));
					instructions.Add(new Instruction(OpCodes.Isinst,
						new TypeReference("FieldInfo", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false)));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)26));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)35));
					instructions.Add(new Instruction(OpCodes.Castclass,
						new TypeReference("FieldInfo", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false)));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_FieldHandle",
							new TypeReference("FieldInfo", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeFieldHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetTokenFor",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("RuntimeFieldHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Br, (int)131));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)35));
					instructions.Add(new Instruction(OpCodes.Isinst,
						new TypeReference("MethodBase", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false)));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)70));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)35));
					instructions.Add(new Instruction(OpCodes.Castclass,
						new TypeReference("MethodBase", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false)));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)36));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)36));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_DeclaringType",
							new TypeReference("MemberInfo", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)37));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)37));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)30));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)36));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_MethodHandle",
							new TypeReference("MethodBase", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeMethodHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)36));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_DeclaringType",
							new TypeReference("MemberInfo", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_TypeHandle",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeTypeHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetTokenFor",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("RuntimeMethodHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
									new TypeReference("RuntimeTypeHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)70));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)36));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_MethodHandle",
							new TypeReference("MethodBase", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeMethodHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetTokenFor",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("RuntimeMethodHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)52));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("InvalidOperationException", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Throw));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveSignature",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
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
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetTokenFor",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
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
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)26));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)6));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"ResolveString",
							new TypeReference("Module", "System.Reflection",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"GetTokenFor",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)6));
					instructions.Add(new Instruction(OpCodes.Newobj,
						new MethodReference(
							CodeModelUtils.MethodConstructorName,
							new TypeReference("InvalidOperationException", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Throw));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)29));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)9));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"WriteInt32",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)30));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)30));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)30));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)26));
					instructions.Add(new Instruction(OpCodes.Blt, (int)-529));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)23));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadBytes",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"SetCode",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
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
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)25));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Ble_S, (sbyte)17));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)1));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)25));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"ReadBytes",
							new TypeReference(_mainType.Name, _mainType.Namespace,false),
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
									new ByRefType(
										TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly)),
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"SetExceptions",
							new TypeReference("DynamicILInfo", "System.Reflection.Emit",
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
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)15));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)10));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"CreateDelegate",
							new TypeReference("DynamicMethod", "System.Reflection.Emit",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("Delegate", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), false),
								new TypeSignature[]
								{
									TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldarg_2));
					instructions.Add(new Instruction(OpCodes.Ldarg_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Stelem_Ref));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Brfalse_S, (sbyte)12));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)8));
					instructions.Add(new Instruction(OpCodes.Callvirt,
						new MethodReference(
							"get_TypeHandle",
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								new TypeReference("RuntimeTypeHandle", "System",
									AssemblyReference.GetMscorlib(_module.Assembly), true),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"RunClassConstructor",
							new TypeReference("RuntimeHelpers", "System.Runtime.CompilerServices",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new CallSite(
								false,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
								new TypeSignature[]
								{
									new TypeReference("RuntimeTypeHandle", "System",
										AssemblyReference.GetMscorlib(_module.Assembly), true),
								},
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)38));
					instructions.Add(new Instruction(OpCodes.Leave_S, (sbyte)8));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)39));
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
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)38));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		private TypeSignature GetInvokeTypeSig(ILCryptoInvokeType invokeType)
		{
			var typeRef = new TypeReference(invokeType.TypeName, _mainType.Namespace, false);
			if (invokeType.GenericParameterCount > 0)
			{
				var genericArgumens = new TypeSignature[invokeType.GenericParameterCount];
				for (int i = 0; i < invokeType.GenericParameterCount; i++)
				{
					genericArgumens[i] = new GenericParameterType(false, i);
				}

				return new GenericTypeReference(typeRef, genericArgumens);
			}
			else
			{
				return typeRef;
			}
		}

		private MethodSignature GetInvokeMethodSig(ILCryptoMethod cryptoMethod)
		{
			var invokeMethod = cryptoMethod.InvokeMethod;

			var invokeType = invokeMethod.OwnerType;

			var ownerTypeSig = GetInvokeTypeSig(invokeType);

			var methodRef = new MethodReference(
				invokeType.InvokeMethodName,
				ownerTypeSig,
				invokeMethod.CallSite);

			if (invokeMethod.GenericParameterCount > 0)
			{
				return new GenericMethodReference(methodRef, cryptoMethod.InvokeGenericArguments);
			}
			else
			{
				return methodRef;
			}
		}

		#endregion

		#region Static

		public static void Obfuscate(BuildAssembly assembly, HashList<string> strings)
		{
			var obfuscator = new ILCryptoObfuscator(assembly, strings);
			obfuscator.Obfuscate();
		}

		#endregion

		#region C# Code

		/***************************************************************************
		internal const int DecryptMethod_HasReturnTypeFlag = 1;
		internal const int DecryptMethod_HasParametersFlag = 1 << 1;
		internal const int DecryptMethod_HasLocalVariablesFlag = 1 << 2;
		internal const int DecryptMethod_HasPinnedLocalVariablesFlag = 1 << 3;
		internal const int DecryptMethod_HasExceptionsFlag = 1 << 4;
		internal const int DecryptMethod_HasFixupsFlag = 1 << 5;

		internal static byte[] _methodOffsets;

		internal static void ILCryptoInitialize()
		{
			_methodOffsets = GetData(1234567);
		}

		internal static object DecryptMethod(int index, int offset, object[] delegates, Type[] genericTypeArguments)
		{
			lock (delegates)
			{
				var del = delegates[index];
				if (del != null)
					return del;

				int pos = (offset + index) * 4;
				int methodOffset = ReadInt32(_methodOffsets, ref pos);

				byte[] buffer = GetData(methodOffset);
				pos = 0;

				byte flags = buffer[pos++];

				string methodName =
					Strings[Read7BitEncodedInt(buffer, ref pos)] + // Type name
					Strings[0] +  // dot
					Strings[Read7BitEncodedInt(buffer, ref pos)]; // Method name

				var module = typeof(AssemblyDefender).Module;

				var emptyTypes = Type.EmptyTypes;

				Type ownerType;
				int token = ReadInt32(buffer, ref pos);
				if (token != 0)
					ownerType = module.ResolveType(token, genericTypeArguments, emptyTypes);
				else
					ownerType = null;

				var delegateType = module.ResolveType(ReadInt32(buffer, ref pos), genericTypeArguments, emptyTypes);

				// Return type.
				var returnType =
					((flags & DecryptMethod_HasReturnTypeFlag) == DecryptMethod_HasReturnTypeFlag) ?
						module.ResolveType(ReadInt32(buffer, ref pos), genericTypeArguments, emptyTypes) :
						typeof(void);

				// Parameters
				Type[] parameters;
				if ((flags & DecryptMethod_HasParametersFlag) == DecryptMethod_HasParametersFlag)
				{
					int count = Read7BitEncodedInt(buffer, ref pos);
					parameters = new Type[count];
					for (int i = 0; i < count; i++)
					{
						parameters[i] = module.ResolveType(ReadInt32(buffer, ref pos), genericTypeArguments, emptyTypes);
					}
				}
				else
				{
					parameters = emptyTypes;
				}

				// Create method proxy.
				DynamicMethod dm;
				if (ownerType != null)
					dm = new DynamicMethod(methodName, returnType, parameters, ownerType, true);
				else
					dm = new DynamicMethod(methodName, returnType, parameters, module, true);

				var il = dm.GetDynamicILInfo();

				// Local variables.
				byte[] sigBytes;
				if ((flags & DecryptMethod_HasLocalVariablesFlag) == DecryptMethod_HasLocalVariablesFlag)
				{
					var sigHelper = SignatureHelper.GetLocalVarSigHelper();
					int count = Read7BitEncodedInt(buffer, ref pos);
					bool hasPinned = ((flags & DecryptMethod_HasPinnedLocalVariablesFlag) == DecryptMethod_HasPinnedLocalVariablesFlag);

					for (int i = 0; i < count; i++)
					{
						// Check for pinned.
						bool pinned = hasPinned ? buffer[pos++] != 0 : false;

						sigHelper.AddArgument(
							module.ResolveType(ReadInt32(buffer, ref pos), genericTypeArguments, emptyTypes),
							pinned);
					}

					sigBytes = sigHelper.GetSignature();
				}
				else
				{
					sigBytes = new byte[] { 7, 0, };
				}

				il.SetLocalSignature(sigBytes);

				// Read sizes
				int codeSize = Read7BitEncodedInt(buffer, ref pos);

				int maxStackSize = Read7BitEncodedInt(buffer, ref pos);

				int ehSize = 0;
				if ((flags & DecryptMethod_HasExceptionsFlag) == DecryptMethod_HasExceptionsFlag)
				{
					ehSize = Read7BitEncodedInt(buffer, ref pos);
				}

				// Apply fixups
				if ((flags & DecryptMethod_HasFixupsFlag) == DecryptMethod_HasFixupsFlag)
				{
					int count = Read7BitEncodedInt(buffer, ref pos);

					// Read fixups.
					int[] fixups = new int[count];
					for (int i = 0; i < count; i++)
					{
						fixups[i] = Read7BitEncodedInt(buffer, ref pos);
					}

					int currFixPos = pos;
					for (int i = 0; i < count; i++)
					{
						currFixPos += fixups[i];

						int fixPos = currFixPos;

						token = ReadInt32(buffer, ref fixPos);

						int tokenType = (int)(token & 0xff000000);
						switch (tokenType)
						{
							case 0x01000000: // TypeRef
							case 0x02000000: // TypeDef
							case 0x1B000000: // TypeSpec
								{
									token = il.GetTokenFor(module.ResolveType(token, genericTypeArguments, emptyTypes).TypeHandle);
								}
								break;

							case 0x06000000: // Method
							case 0x2B000000: // MethodSpec
								{
									var mi = module.ResolveMethod(token, genericTypeArguments, emptyTypes);
									var declaringType = mi.DeclaringType;
									if (declaringType != null)
									{
										token = il.GetTokenFor(mi.MethodHandle, mi.DeclaringType.TypeHandle);
									}
									else
									{
										token = il.GetTokenFor(mi.MethodHandle);
									}
								}
								break;

							case 0x04000000: // Field
								{
									token = il.GetTokenFor(module.ResolveField(token, genericTypeArguments, emptyTypes).FieldHandle);
								}
								break;

							case 0x0A000000: // MemberRef
								{
									var memberInfo = module.ResolveMember(token, genericTypeArguments, emptyTypes);
									if (memberInfo is FieldInfo)
									{
										token = il.GetTokenFor(((FieldInfo)memberInfo).FieldHandle);
									}
									else if (memberInfo is MethodBase)
									{
										var mi = (MethodBase)memberInfo;
										var declaringType = mi.DeclaringType;
										if (declaringType != null)
										{
											token = il.GetTokenFor(mi.MethodHandle, mi.DeclaringType.TypeHandle);
										}
										else
										{
											token = il.GetTokenFor(mi.MethodHandle);
										}
									}
									else
									{
										throw new InvalidOperationException();
									}
								}
								break;

							case 0x11000000: // StandAloneSig
								{
									token = il.GetTokenFor(module.ResolveSignature(token));
								}
								break;

							case 0x70000000: // String
								{
									token = il.GetTokenFor(module.ResolveString(token));
								}
								break;

							default:
								throw new InvalidOperationException();
						}

						WriteInt32(buffer, ref currFixPos, (int)token);
					}
				}

				// IL code.
				il.SetCode(
					ReadBytes(buffer, ref pos, codeSize),
					maxStackSize);

				// Exception handlers.
				if (ehSize > 0)
				{
					il.SetExceptions(ReadBytes(buffer, ref pos, ehSize));
				}

				// Create delegate to proxy method.
				del = dm.CreateDelegate(delegateType);

				delegates[index] = del;

				if (ownerType != null)
				{
					RuntimeHelpers.RunClassConstructor(ownerType.TypeHandle);
				}

				return del;
			}
		}
		***************************************************************************/

		#endregion
	}
}
