using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class MethodBody
	{
		#region Fields

		public const int DefaultMaxStackSize = 8;

		private int _maxStackSize = DefaultMaxStackSize;
		private bool _initLocals = true;
		private int _localVarToken;
		private List<TypeSignature> _localVariables;
		private List<ExceptionHandler> _exceptionHandlers;
		private List<Instruction> _instructions;

		#endregion

		#region Ctors

		public MethodBody()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the maximum number of items on the operand stack when the method is executing.
		/// </summary>
		public int MaxStackSize
		{
			get { return _maxStackSize; }
			set { _maxStackSize = value; }
		}

		/// <summary>
		/// Gets a value indicating whether local variables in the method body are initialized to the
		/// default values for their types.
		/// </summary>
		public bool InitLocals
		{
			get { return _initLocals; }
			set { _initLocals = value; }
		}

		/// <summary>
		/// Gets a metadata token for the signature that describes the local variables for the method in metadata.
		/// </summary>
		public int LocalVarToken
		{
			get { return _localVarToken; }
		}

		/// <summary>
		/// Gets the list of local variables declared in the method body.
		/// </summary>
		public List<TypeSignature> LocalVariables
		{
			get
			{
				if (_localVariables == null)
				{
					_localVariables = new List<TypeSignature>();
				}

				return _localVariables;
			}
			set { _localVariables = value; }
		}

		/// <summary>
		/// Gets a list that includes all the exception-handling clauses in the method body.
		/// </summary>
		public List<ExceptionHandler> ExceptionHandlers
		{
			get
			{
				if (_exceptionHandlers == null)
				{
					_exceptionHandlers = new List<ExceptionHandler>();
				}

				return _exceptionHandlers;
			}
			set { _exceptionHandlers = value; }
		}

		public List<Instruction> Instructions
		{
			get
			{
				if (_instructions == null)
				{
					_instructions = new List<Instruction>();
				}

				return _instructions;
			}
			set { _instructions = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Shifts all exception handler offsets to the right by the size of instructions inserted.
		/// </summary>
		public void ShiftRightExceptionHandlerOffsets(int instructionCount)
		{
			if (instructionCount == 0)
				return;

			var exceptionHandlers = ExceptionHandlers;
			if (exceptionHandlers.Count == 0)
				return;

			int addSize = Instruction.GetSize(Instructions, 0, instructionCount);
			if (addSize == 0)
				return;

			for (int i = 0; i < exceptionHandlers.Count; i++)
			{
				var exceptionHandler = exceptionHandlers[i];
				exceptionHandler.TryOffset += addSize;
				exceptionHandler.HandlerOffset += addSize;

				if (exceptionHandler.Type == ExceptionHandlerType.Filter)
					exceptionHandler.FilterOffset += addSize;
			}
		}

		public void Build(MethodDeclaration method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			byte[] blob = Build(method.Module);
			method.SetBody(blob);
		}

		public byte[] Build(Module module)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			int pos = 0;
			var blob = new Blob();

			blob.Write7BitEncodedInt(ref pos, (int)_maxStackSize);

			blob.Write(ref pos, (bool)_initLocals);

			blob.Write(ref pos, (int)_localVarToken);

			BuildInstructions(blob, ref pos, module);

			BuildLocalVariables(blob, ref pos, module);

			BuildExceptionHandlers(blob, ref pos, module);

			return blob.ToArray();
		}

		private void BuildInstructions(Blob blob, ref int pos, Module module)
		{
			blob.Write7BitEncodedInt(ref pos, (int)Instructions.Count);

			foreach (var instruction in Instructions)
			{
				var opCode = instruction.OpCode;
				if (opCode.OpByte1 == 0xff)
				{
					// one byte opcode
					blob.Write(ref pos, (byte)opCode.OpByte2);
				}
				else
				{
					// two bytes opcode
					blob.Write(ref pos, (byte)opCode.OpByte1);
					blob.Write(ref pos, (byte)opCode.OpByte2);
				}

				object value = instruction.Value;
				switch (opCode.OperandType)
				{
					case OperandType.InlineBrTarget:
						{
							blob.Write(ref pos, (int)value);
						}
						break;

					case OperandType.InlineField:
						{
							var signature = (Signature)value;
							int rid = module.AddSignature(ref signature);
							blob.Write(ref pos, (int)rid);
						}
						break;

					case OperandType.InlineI:
						{
							blob.Write(ref pos, (int)value);
						}
						break;

					case OperandType.InlineI8:
						{
							blob.Write(ref pos, (long)value);
						}
						break;

					case OperandType.InlineMethod:
						{
							var signature = (Signature)value;
							int rid = module.AddSignature(ref signature);
							blob.Write(ref pos, (int)rid);
						}
						break;

					case OperandType.InlineR:
						{
							blob.Write(ref pos, (double)value);
						}
						break;

					case OperandType.InlineSig:
						{
							var signature = (Signature)value;
							int rid = module.AddSignature(ref signature);
							blob.Write(ref pos, (int)rid);
						}
						break;

					case OperandType.InlineString:
						{
							// Token of a userdefined string, whose RID portion is actually an offset in the #US blob stream.
							blob.WriteLengthPrefixedString(ref pos, (string)value, Encoding.Unicode);
						}
						break;

					case OperandType.InlineSwitch:
						{
							int[] branches = (int[])value;

							blob.Write(ref pos, (int)branches.Length);

							for (int i = 0; i < branches.Length; i++)
							{
								blob.Write(ref pos, (int)branches[i]);
							}
						}
						break;

					case OperandType.InlineTok:
						{
							var signature = (Signature)value;
							int rid = module.AddSignature(ref signature);
							blob.Write(ref pos, (int)rid);
						}
						break;

					case OperandType.InlineType:
						{
							var signature = (Signature)value;
							int rid = module.AddSignature(ref signature);
							blob.Write(ref pos, (int)rid);
						}
						break;

					case OperandType.InlineVar:
						{
							blob.Write(ref pos, (short)value);
						}
						break;

					case OperandType.ShortInlineBrTarget:
						{
							blob.Write(ref pos, (sbyte)value);
						}
						break;

					case OperandType.ShortInlineI:
						{
							blob.Write(ref pos, (byte)value);
						}
						break;

					case OperandType.ShortInlineR:
						{
							blob.Write(ref pos, (float)value);
						}
						break;

					case OperandType.ShortInlineVar:
						{
							blob.Write(ref pos, (byte)value);
						}
						break;
				}
			}
		}

		private void BuildLocalVariables(Blob blob, ref int pos, Module module)
		{
			var list = LocalVariables;

			blob.Write7BitEncodedInt(ref pos, (int)list.Count);

			for (int i = 0; i < list.Count; i++)
			{
				var localVar = list[i];
				int rid = module.AddSignature(ref localVar);
				blob.Write(ref pos, (int)rid);
			}
		}

		private void BuildExceptionHandlers(Blob blob, ref int pos, Module module)
		{
			blob.Write7BitEncodedInt(ref pos, (int)ExceptionHandlers.Count);

			foreach (var handler in ExceptionHandlers)
			{
				BuildExceptionHandler(blob, ref pos, handler, module);
			}
		}

		private void BuildExceptionHandler(Blob blob, ref int pos, ExceptionHandler handler, Module module)
		{
			blob.Write(ref pos, (int)handler.Type);
			blob.Write(ref pos, (int)handler.TryOffset);
			blob.Write(ref pos, (int)handler.TryLength);
			blob.Write(ref pos, (int)handler.HandlerOffset);
			blob.Write(ref pos, (int)handler.HandlerLength);
			blob.Write(ref pos, (int)handler.FilterOffset);

			var catchType = handler.CatchType;
			if (catchType != null)
			{
				int rid = module.AddSignature(ref catchType);
				blob.Write(ref pos, (int)rid);
			}
			else
			{
				blob.Write(ref pos, (int)0);
			}
		}

		#region Load from image

		private void Load(IBinaryAccessor accessor, Module module)
		{
			int flags = accessor.ReadByte();

			if ((flags & ILMethodFlags.FormatMask) == ILMethodFlags.FatFormat)
			{
				// Fat format
				accessor.ReadByte();
				_maxStackSize = accessor.ReadUInt16();
				int codeSize = accessor.ReadInt32();

				_initLocals = ((flags & ILMethodFlags.InitLocals) == ILMethodFlags.InitLocals);
				_localVarToken = accessor.ReadInt32();

				if (MetadataToken.GetType(_localVarToken) == MetadataTokenType.Signature)
				{
					LoadLocalVariables(module, MetadataToken.GetRID(_localVarToken));
				}

				LoadInstructions(accessor, module, codeSize);

				if ((flags & ILMethodFlags.MoreSects) == ILMethodFlags.MoreSects)
				{
					// More sections
					LoadSection(accessor, module);
				}
			}
			else
			{
				// Tiny format
				// Used when the method is tiny (< 64 bytes), and there are no local vars
				_maxStackSize = 8;
				int codeSize = flags >> 2;
				LoadInstructions(accessor, module, codeSize);
			}
		}

		private void LoadLocalVariables(Module module, int rid)
		{
			var image = module.Image;

			int blobID = image.GetStandAloneSig(rid);

			using (var accessor = image.OpenBlob(blobID))
			{
				byte sigType = accessor.ReadByte();
				if (sigType != Metadata.SignatureType.LocalSig)
					return;

				int count = accessor.ReadCompressedInteger();

				_localVariables = new List<TypeSignature>(count);

				for (int i = 0; i < count; i++)
				{
					_localVariables.Add(TypeSignature.Load(accessor, module));
				}
			}
		}

		private void LoadInstructions(IBinaryAccessor accessor, Module module, int codeSize)
		{
			long startOffset = accessor.Position;

			var image = module.Image;

			_instructions = new List<Instruction>();

			while (accessor.Position < startOffset + codeSize)
			{
				OpCode opCode;
				byte opByte = accessor.ReadByte();
				if (opByte == 0xFE)
				{
					opByte = accessor.ReadByte();
					opCode = OpCodes.OpCodeArray[256 + opByte];
				}
				else
				{
					opCode = OpCodes.OpCodeArray[opByte];
				}

				if (opCode == null)
				{
					throw new CodeModelException(string.Format(SR.AssemblyLoadError, module.Location));
				}

				object value;
				switch (opCode.OperandType)
				{
					case OperandType.InlineBrTarget:
						{
							value = accessor.ReadInt32();
						}
						break;

					case OperandType.InlineField:
						{
							int token = accessor.ReadInt32();
							value = FieldReference.Load(module, token);
						}
						break;

					case OperandType.InlineI:
						{
							value = accessor.ReadInt32();
						}
						break;

					case OperandType.InlineI8:
						{
							value = accessor.ReadInt64();
						}
						break;

					case OperandType.InlineMethod:
						{
							int token = accessor.ReadInt32();
							value = MethodReference.Load(module, token);
						}
						break;

					case OperandType.InlineR:
						{
							value = accessor.ReadDouble();
						}
						break;

					case OperandType.InlineSig:
						{
							int token = accessor.ReadInt32();
							if (MetadataToken.GetType(token) == MetadataTokenType.Signature)
							{
								int rid = MetadataToken.GetRID(token);
								value = CallSite.LoadStandAloneSig(module, rid);
							}
							else
							{
								throw new CodeModelException(SR.MethodBodyBlobNotValid);
							}
						}
						break;

					case OperandType.InlineString:
						{
							// Token of a userdefined string, whose RID portion is actually an offset in the #US blob stream.
							uint token = accessor.ReadUInt32();
							int rid = (int)(token & 0x00ffffff);
							value = image.GetUserString(rid);
						}
						break;

					case OperandType.InlineSwitch:
						{
							int count = accessor.ReadInt32();
							int[] targets = new int[count];
							for (int i = 0; i < count; i++)
							{
								targets[i] = accessor.ReadInt32();
							}

							value = targets;
						}
						break;

					case OperandType.InlineTok:
						{
							int token = accessor.ReadInt32();
							int rid = MetadataToken.GetRID(token);
							switch (MetadataToken.GetType(token))
							{
								case MetadataTokenType.Method:
									value = MethodReference.LoadMethodDef(module, rid);
									break;

								case MetadataTokenType.MethodSpec:
									value = GenericMethodReference.LoadMethodSpec(module, rid);
									break;

								case MetadataTokenType.MemberRef:
									value = MethodReference.LoadMemberRef(module, rid);
									break;

								case MetadataTokenType.Field:
									value = FieldReference.LoadFieldDef(module, rid);
									break;

								case MetadataTokenType.TypeDef:
									value = TypeReference.LoadTypeDef(module, rid);
									break;

								case MetadataTokenType.TypeRef:
									value = TypeReference.LoadTypeRef(module, rid);
									break;

								case MetadataTokenType.TypeSpec:
									value = TypeSignature.LoadTypeSpec(module, rid);
									break;

								default:
									throw new CodeModelException(SR.MethodBodyBlobNotValid);
							}
						}
						break;

					case OperandType.InlineType:
						{
							int token = accessor.ReadInt32();
							value = TypeSignature.Load(module, token);
						}
						break;

					case OperandType.InlineVar:
						{
							value = accessor.ReadInt16();
						}
						break;

					case OperandType.ShortInlineBrTarget:
						{
							value = accessor.ReadSByte();
						}
						break;

					case OperandType.ShortInlineI:
						{
							value = accessor.ReadByte();
						}
						break;

					case OperandType.ShortInlineR:
						{
							value = accessor.ReadSingle();
						}
						break;

					case OperandType.ShortInlineVar:
						{
							value = accessor.ReadByte();
						}
						break;

					default:
						{
							value = null;
						}
						break;
				}

				_instructions.Add(new Instruction(opCode, value));
			}
		}

		private void LoadSection(IBinaryAccessor accessor, Module module)
		{
			accessor.Align(4);

			int flags = accessor.ReadByte();

			if ((flags & ILMethodSect.EHTable) == ILMethodSect.EHTable)
			{
				// EHTable
				LoadExceptionHandlerSection(accessor, module, flags);
			}

			if ((flags & ILMethodSect.MoreSects) == ILMethodSect.MoreSects)
			{
				// More sections
				LoadSection(accessor, module);
			}
		}

		private void LoadExceptionHandlerSection(IBinaryAccessor accessor, Module module, int flags)
		{
			if (_exceptionHandlers == null)
			{
				_exceptionHandlers = new List<ExceptionHandler>();
			}

			if ((flags & 0x40) == 0x40)
			{
				// Fat format
				accessor.Position--;
				int length = (accessor.ReadInt32() >> 8) / 24;
				for (int i = 0; i < length; i++)
				{
					var handler = LoadFatExceptionHandler(accessor, module);
					_exceptionHandlers.Add(handler);
				}
			}
			else
			{
				// Tiny format
				int length = accessor.ReadByte() / 12;
				accessor.Position += 2; // padded
				for (int i = 0; i < length; i++)
				{
					var handler = LoadTinyExceptionHandler(accessor, module);
					_exceptionHandlers.Add(handler);
				}
			}
		}

		private ExceptionHandler LoadTinyExceptionHandler(IBinaryAccessor accessor, Module module)
		{
			var handler = new ExceptionHandler();

			int flags = accessor.ReadInt16() & ILExceptionFlag.MASK;
			switch (flags)
			{
				case 0:
					handler.Type = ExceptionHandlerType.Catch;
					break;
				case 1:
					handler.Type = ExceptionHandlerType.Filter;
					break;
				case 2:
					handler.Type = ExceptionHandlerType.Finally;
					break;
				case 4:
					handler.Type = ExceptionHandlerType.Fault;
					break;
			}

			handler.TryOffset = accessor.ReadUInt16();
			handler.TryLength = accessor.ReadByte();
			handler.HandlerOffset = accessor.ReadUInt16();
			handler.HandlerLength = accessor.ReadByte();

			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					{
						int token = accessor.ReadInt32();
						handler.CatchType = TypeSignature.Load(module, token);
					}
					break;

				case ExceptionHandlerType.Filter:
					{
						handler.FilterOffset = accessor.ReadInt32();
					}
					break;

				default:
					{
						accessor.Position += 4; // padded
					}
					break;
			}

			return handler;
		}

		private ExceptionHandler LoadFatExceptionHandler(IBinaryAccessor accessor, Module module)
		{
			var handler = new ExceptionHandler();

			int flags = accessor.ReadInt32() & ILExceptionFlag.MASK;
			switch (flags)
			{
				case 0:
					handler.Type = ExceptionHandlerType.Catch;
					break;
				case 1:
					handler.Type = ExceptionHandlerType.Filter;
					break;
				case 2:
					handler.Type = ExceptionHandlerType.Finally;
					break;
				case 4:
					handler.Type = ExceptionHandlerType.Fault;
					break;
			}

			handler.TryOffset = accessor.ReadInt32();
			handler.TryLength = accessor.ReadInt32();
			handler.HandlerOffset = accessor.ReadInt32();
			handler.HandlerLength = accessor.ReadInt32();

			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					{
						int token = accessor.ReadInt32();
						handler.CatchType = TypeSignature.Load(module, token);
					}
					break;

				case ExceptionHandlerType.Filter:
					{
						handler.FilterOffset = accessor.ReadInt32();
					}
					break;

				default:
					{
						accessor.Position += 4; // padded
					}
					break;
			}

			return handler;
		}

		#endregion

		#region Load from state

		private void StateLoad(IBinaryAccessor accessor, Module module)
		{
			_maxStackSize = accessor.Read7BitEncodedInt();

			_initLocals = accessor.ReadBoolean();

			_localVarToken = accessor.ReadInt32();

			StateLoadInstructions(accessor, module);

			StateLoadLocalVariables(accessor, module);

			StateLoadExceptionHandlers(accessor, module);
		}

		private void StateLoadInstructions(IBinaryAccessor accessor, Module module)
		{
			int instructionCount = accessor.Read7BitEncodedInt();

			_instructions = new List<Instruction>(instructionCount);

			for (int i = 0; i < instructionCount; i++)
			{
				OpCode opCode;
				byte opByte = accessor.ReadByte();
				if (opByte == 0xFE)
				{
					opByte = accessor.ReadByte();
					opCode = OpCodes.OpCodeArray[256 + opByte];
				}
				else
				{
					opCode = OpCodes.OpCodeArray[opByte];
				}

				object value;
				switch (opCode.OperandType)
				{
					case OperandType.InlineBrTarget:
						{
							value = accessor.ReadInt32();
						}
						break;

					case OperandType.InlineField:
						{
							int token = accessor.ReadInt32();
							value = module.GetSignature<Signature>(token);
						}
						break;

					case OperandType.InlineI:
						{
							value = accessor.ReadInt32();
						}
						break;

					case OperandType.InlineI8:
						{
							value = accessor.ReadInt64();
						}
						break;

					case OperandType.InlineMethod:
						{
							int token = accessor.ReadInt32();
							value = module.GetSignature<Signature>(token);
						}
						break;

					case OperandType.InlineR:
						{
							value = accessor.ReadDouble();
						}
						break;

					case OperandType.InlineSig:
						{
							int token = accessor.ReadInt32();
							value = module.GetSignature<Signature>(token);
						}
						break;

					case OperandType.InlineString:
						{
							// Token of a userdefined string, whose RID portion is actually an offset in the #US blob stream.
							value = accessor.ReadLengthPrefixedString(Encoding.Unicode);
						}
						break;

					case OperandType.InlineSwitch:
						{
							int count = accessor.ReadInt32();
							int[] targets = new int[count];
							for (int j = 0; j < count; j++)
							{
								targets[j] = accessor.ReadInt32();
							}

							value = targets;
						}
						break;

					case OperandType.InlineTok:
						{
							int token = accessor.ReadInt32();
							value = module.GetSignature<Signature>(token);
						}
						break;

					case OperandType.InlineType:
						{
							int token = accessor.ReadInt32();
							value = module.GetSignature<Signature>(token);
						}
						break;

					case OperandType.InlineVar:
						{
							value = accessor.ReadInt16();
						}
						break;

					case OperandType.ShortInlineBrTarget:
						{
							value = accessor.ReadSByte();
						}
						break;

					case OperandType.ShortInlineI:
						{
							value = accessor.ReadByte();
						}
						break;

					case OperandType.ShortInlineR:
						{
							value = accessor.ReadSingle();
						}
						break;

					case OperandType.ShortInlineVar:
						{
							value = accessor.ReadByte();
						}
						break;

					default:
						{
							value = null;
						}
						break;
				}

				_instructions.Add(new Instruction(opCode, value));
			}
		}

		private void StateLoadLocalVariables(IBinaryAccessor accessor, Module module)
		{
			int count = accessor.Read7BitEncodedInt();

			_localVariables = new List<TypeSignature>(count);

			for (int i = 0; i < count; i++)
			{
				int token = accessor.ReadInt32();
				_localVariables.Add(module.GetSignature<TypeSignature>(token));
			}
		}

		private void StateLoadExceptionHandlers(IBinaryAccessor accessor, Module module)
		{
			int count = accessor.Read7BitEncodedInt();

			_exceptionHandlers = new List<ExceptionHandler>(count);

			for (int i = 0; i < count; i++)
			{
				StateLoadExceptionHandler(accessor, module);
			}
		}

		private void StateLoadExceptionHandler(IBinaryAccessor accessor, Module module)
		{
			var handler = new ExceptionHandler();
			handler.Type = (ExceptionHandlerType)accessor.ReadInt32();
			handler.TryOffset = accessor.ReadInt32();
			handler.TryLength = accessor.ReadInt32();
			handler.HandlerOffset = accessor.ReadInt32();
			handler.HandlerLength = accessor.ReadInt32();
			handler.FilterOffset = accessor.ReadInt32();

			int catchToken = accessor.ReadInt32();
			handler.CatchType = module.GetSignature<TypeSignature>(catchToken);

			_exceptionHandlers.Add(handler);
		}

		#endregion

		#endregion

		#region Static

		public static bool IsValid(MethodDeclaration method)
		{
			if (!method.HasBody)
				return false;

			if (method.CodeType != MethodCodeTypeFlags.CIL)
				return false;

			return true;
		}

		public static MethodBody Load(MethodDeclaration method)
		{
			using (var accessor = method.OpenBodyStream())
			{
				return Load(accessor, method.Module, method.IsBodyChanged);
			}
		}

		public static MethodBody Load(IBinaryAccessor accessor, Module module, bool isLoadedFromState = false)
		{
			if (accessor == null)
				throw new CodeModelException(string.Format(SR.AssemblyLoadError, module.Location));

			var body = new MethodBody();

			if (isLoadedFromState)
			{
				body.StateLoad(accessor, module);
			}
			else
			{
				body.Load(accessor, module);
			}

			return body;
		}

		#endregion
	}
}
