using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Net.IL;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	internal class MainTypeFunctionPointerBuilder : BuildTask
	{
		#region Fields

		private int _dataOffset;
		private Blob _blob;
		private BuildModule _module;
		private MainType _mainType;
		private ResourceStorage _resourceStorage;
		private MethodReference[] _signatures;
		private HashList<MethodReference> _methodRefs = new HashList<MethodReference>();

		#endregion

		#region Ctors

		internal MainTypeFunctionPointerBuilder(BuildModule module)
		{
			_module = module;
			_mainType = module.MainType;
			_resourceStorage = _module.ResourceStorage;
		}

		#endregion

		#region Properties

		internal bool HasFunctionPointers
		{
			get { return _methodRefs.Count > 0; }
		}

		#endregion

		#region Methods

		public override void Build()
		{
			var moduleBuilder = PE.Tasks.Get<ModuleBuilder>();
			if (moduleBuilder == null)
				return;

			// Build tokens
			int pos = 0;
			for (int i = 0; i < _signatures.Length; i++)
			{
				var signature = _signatures[i];
				int token = moduleBuilder.BuildMethodSigInMethodRefOrDef(signature);
				_blob.Write(ref pos, (int)token);
			}
		}

		internal void BuildFunctionPointers()
		{
			_signatures = _methodRefs.ToArray();
			_methodRefs = null;

			_dataOffset = _resourceStorage.Size;

			var resourceBlob = _resourceStorage.Blob;
			int offset = resourceBlob.Length;
			int size = _signatures.Length * 4;
			resourceBlob.Allocate(offset, size);
			_blob = new Blob(resourceBlob.GetBuffer(), offset, size, true);

			Generate();
		}

		internal void Build(ILBlock block)
		{
			var node = block.FirstChild;
			while (node != null)
			{
				switch (node.NodeType)
				{
					case ILNodeType.Block:
						Build((ILBlock)node);
						break;

					case ILNodeType.Instruction:
						Build((ILInstruction)node);
						break;
				}

				node = node.NextSibling;
			}
		}

		private bool Build(ILInstruction instruction)
		{
			var methodRef = instruction.Value as MethodReference;
			if (methodRef == null)
				return false;

			var opCode = instruction.OpCode;
			if (opCode != OpCodes.Call && opCode != OpCodes.Callvirt)
				return false;

			if (methodRef.HasThis ||
				methodRef.CallConv != MethodCallingConvention.Default)
				return false;

			int methodIndex;
			_methodRefs.TryAdd(methodRef, out methodIndex);

			instruction.OpCode = OpCodes.Ldsfld;
			instruction.Value =
				new FieldReference(
					"MethodPointers",
					new ArrayType(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly)),
					new TypeReference(_mainType.Name, _mainType.Namespace, false));
			instruction = instruction.AddNext(Instruction.GetLdc(methodIndex));
			instruction = instruction.AddNext(OpCodes.Ldelem_I);
			instruction = instruction.AddNext(OpCodes.Calli, methodRef.CallSite);

			return true;
		}

		private void Generate()
		{
			GeneraterFields();
			GenerateCctor();
			GenerateInitializeMethod();
		}

		private void GeneraterFields()
		{
			// MethodPointers : [mscorlib]System.IntPtr[]
			var field = _mainType.Fields.Add();
			field.Name = "MethodPointers";
			field.Visibility = FieldVisibilityFlags.Assembly;
			field.IsStatic = true;
			field.FieldType =
				new ArrayType(
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly));

			// _dataStream : System.IO.Stream
			if (null == _mainType.Fields.Find("_dataStream"))
			{
				field = _mainType.Fields.Add();
				field.Name = "_dataStream";
				field.Visibility = FieldVisibilityFlags.Assembly;
				field.IsStatic = true;
				field.FieldType =
					new TypeReference("Stream", "System.IO",
						AssemblyReference.GetMscorlib(_module.Assembly), false);
			}
		}

		private void GenerateCctor()
		{
			var callInit = new Instruction(OpCodes.Call,
				new MethodReference(
					"InitializeMethodPointers",
					new TypeReference(_mainType.Name, _mainType.Namespace, false),
					new CallSite(
						false,
						false,
						MethodCallingConvention.Default,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, _module.Assembly),
						new TypeSignature[0],
						-1,
						0)));

			var cctorMethod = _mainType.Methods.GetOrCreateStaticConstructor();
			var methodBody = MethodBody.Load(cctorMethod);
			methodBody.Instructions.Insert(methodBody.Instructions.Count - 1, callInit);
			methodBody.Build(cctorMethod);
		}

		/// <summary>
		/// static InitializeMethodPointers() : [mscorlib]System.Void
		/// </summary>
		private void GenerateInitializeMethod()
		{
			var method = _mainType.Methods.Add();
			method.Name = "InitializeMethodPointers";
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
				methodBody.MaxStackSize = 7;
				methodBody.InitLocals = true;

				// Local variables
				{
					var localVariables = methodBody.LocalVariables;
					localVariables.Add(
						new TypeReference("Module", "System.Reflection",
							AssemblyReference.GetMscorlib(_module.Assembly), false));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					localVariables.Add(
						new ArrayType(
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, _module.Assembly));
					localVariables.Add(
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _module.Assembly));
					localVariables.Add(
						new TypeReference("RuntimeMethodHandle", "System",
							AssemblyReference.GetMscorlib(_module.Assembly), true));
				}

				// Exception handlers
				{
					var exceptionHandlers = methodBody.ExceptionHandlers;
					exceptionHandlers.Add(
						new ExceptionHandler(
							ExceptionHandlerType.Finally,
							44, 64, 108, 8, 0));
				}

				// Instructions
				{
					var instructions = methodBody.Instructions;
					instructions.Add(new Instruction(OpCodes.Ldtoken,
						new TypeReference(_mainType.Name, _mainType.Namespace, false)));
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
					instructions.Add(new Instruction(OpCodes.Stloc_0));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)_signatures.Length));
					instructions.Add(new Instruction(OpCodes.Stloc_1));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_4));
					instructions.Add(new Instruction(OpCodes.Mul));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.UInt8, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stloc_2));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_dataLockObject",
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
					instructions.Add(new Instruction(OpCodes.Ldtoken,
						new TypeReference(_mainType.Name, _mainType.Namespace, false)));
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
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"_dataStream",
							new TypeReference("Stream", "System.IO",
								AssemblyReference.GetMscorlib(_module.Assembly), false),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)_dataOffset));
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
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
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
					instructions.Add(new Instruction(OpCodes.Pop));
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
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Newarr,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Stsfld,
						new FieldReference(
							"MethodPointers",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"EmptyTypes",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly)),
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Type, _module.Assembly))));
					instructions.Add(new Instruction(OpCodes.Stloc_3));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_0));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Br_S, (sbyte)92));
					instructions.Add(new Instruction(OpCodes.Ldsfld,
						new FieldReference(
							"MethodPointers",
							new ArrayType(
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly)),
							new TypeReference(_mainType.Name, _mainType.Namespace, false))));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldelema,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Ldloc_0));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_8));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)16));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldloc_2));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Dup));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)4));
					instructions.Add(new Instruction(OpCodes.Ldelem_U1));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_S, (byte)24));
					instructions.Add(new Instruction(OpCodes.Shl));
					instructions.Add(new Instruction(OpCodes.Or));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
					instructions.Add(new Instruction(OpCodes.Ldloc_3));
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
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Ldloca_S, (byte)7));
					instructions.Add(new Instruction(OpCodes.Call,
						new MethodReference(
							"GetFunctionPointer",
							new TypeReference("RuntimeMethodHandle", "System",
								AssemblyReference.GetMscorlib(_module.Assembly), true),
							new CallSite(
								true,
								false,
								MethodCallingConvention.Default,
								TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly),
								new TypeSignature[0],
								-1,
								0))));
					instructions.Add(new Instruction(OpCodes.Stobj,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.IntPtr, _module.Assembly)));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
					instructions.Add(new Instruction(OpCodes.Add));
					instructions.Add(new Instruction(OpCodes.Stloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_S, (byte)5));
					instructions.Add(new Instruction(OpCodes.Ldloc_1));
					instructions.Add(new Instruction(OpCodes.Blt_S, (sbyte)-97));
					instructions.Add(new Instruction(OpCodes.Ret));
				}

				methodBody.Build(method);
			}
		}

		internal void MapMemberReferences()
		{
			if (_signatures == null)
				return;

			var mapper = new MemberReferenceMapper(_module);

			for (int i = 0; i < _signatures.Length; i++)
			{
				var signature = _signatures[i];
				if (mapper.Build(ref signature))
				{
					_signatures[i] = signature;
				}
			}
		}

		#endregion

		#region C# Code

		/***************************************************************************
		internal static IntPtr[] MethodPointers;

		internal static void InitializeMethodPointers()
		{
			var module = typeof(AssemblyDefender).Module;
			int count = 7654321;
			byte[] buffer = new byte[count * 4];

			lock (_dataLockObject)
			{
				_dataStream = typeof(AssemblyDefender).Assembly.GetManifestResourceStream("ResourceName");
				_dataStream.Position = 1234567;
				_dataStream.Read(buffer, 0, buffer.Length);
			}

			MethodPointers = new IntPtr[count];
			var emptyTypes = Type.EmptyTypes;
			int pos = 0;
			for (int i = 0; i < count; i++)
			{
				MethodPointers[i] = module.ResolveMethod(
					(
						(buffer[pos++]) |
						(buffer[pos++] << 0x8) |
						(buffer[pos++] << 0x10) |
						(buffer[pos++] << 0x18)
					),
					emptyTypes,
					emptyTypes)
					.MethodHandle.GetFunctionPointer();
			}
		}
		***************************************************************************/

		#endregion
	}
}
