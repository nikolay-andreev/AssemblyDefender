using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Net.IL;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ILCryptoBlobBuilder : BuildTask
	{
		#region Fields

		private BuildModule _module;
		private ResourceStorage _storage;
		private HashList<string> _strings;
		private List<SignatureInfo> _signatures = new List<SignatureInfo>();
		private List<UserStringInfo> _userStrings = new List<UserStringInfo>();

		#endregion

		#region Ctors

		public ILCryptoBlobBuilder(BuildAssembly assembly, HashList<string> strings)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (strings == null)
				throw new ArgumentNullException("strings");

			_module = (BuildModule)assembly.Module;
			_strings = strings;
			_storage = assembly.ResourceStorage;
		}

		#endregion

		#region Methods

		public override void Build()
		{
			var moduleBuilder = PE.Tasks.Get<ModuleBuilder>();
			if (moduleBuilder == null)
				return;

			// Build tokens.
			for (int i = 0; i < _signatures.Count; i++)
			{
				var info = _signatures[i];
				int token = moduleBuilder.BuildSig(info.Signature);
				int pos = info.Position;

				var blob = _storage[info.BlobID];
				blob.Write(ref pos, (int)token);
			}

			// Build strings.
			for (int i = 0; i < _userStrings.Count; i++)
			{
				var info = _userStrings[i];
				int rid = moduleBuilder.AddUserString(info.String);
				int token = MetadataToken.Get(MetadataTokenType.String, rid);
				int pos = info.Position;

				var blob = _storage[info.BlobID];
				blob.Write(ref pos, (int)token);
			}
		}

		internal int Build(MethodDeclaration method)
		{
			var state = new BuildState();
			state.Method = method;
			state.MethodBody = MethodBody.Load(method);

			BuildIL(state);

			BuildEH(state);

			BuildFixups(state);

			BuildHeader(state);

			return Write(state);
		}

		private void BuildHeader(BuildState state)
		{
			var blob = state.HeaderBlob;
			int pos = 1;

			var method = (BuildMethod)state.Method;
			var methodBody = state.MethodBody;
			var ownerType = (BuildType)method.GetOwnerType();

			// Name
			{
				int index;

				string typeName = (ownerType.NameChanged) ? ownerType.NewName : ownerType.Name;
				_strings.TryAdd(typeName, out index);
				blob.Write7BitEncodedInt(ref pos, index);

				string methodName = (method.NameChanged) ? method.NewName : method.Name;
				_strings.TryAdd(methodName, out index);
				blob.Write7BitEncodedInt(ref pos, index);
			}

			var cryptoMethod = method.ILCrypto;

			// Owner type
			{
				if (!ownerType.IsGlobal())
				{
					var ownerTypeSig = ownerType.ToSignature(_module);
					var sigInfo = new SignatureInfo(ownerTypeSig, pos);
					state.HeaderSignatures.Add(sigInfo);
				}

				blob.Write(ref pos, (int)0);
			}

			// Delegate type
			{
				var delegateTypeSig = GetDelegateTypeSig(cryptoMethod);
				var sigInfo = new SignatureInfo(delegateTypeSig, pos);
				state.HeaderSignatures.Add(sigInfo);
				blob.Write(ref pos, (int)0);
			}

			// Return type
			{
				if (method.ReturnType.TypeCode != PrimitiveTypeCode.Void)
				{
					state.Flags |= BlobFlags.HasReturnType;

					var sigInfo = new SignatureInfo(method.ReturnType.Type, pos);
					state.HeaderSignatures.Add(sigInfo);
					blob.Write(ref pos, (int)0);
				}
			}

			// Parameters
			{
				var parameters = method.Parameters;

				int parameterCount = parameters.Count;

				if (method.HasThis)
					parameterCount++;

				if (parameterCount > 0)
				{
					state.Flags |= BlobFlags.HasParameters;

					blob.Write7BitEncodedInt(ref pos, parameterCount);

					if (method.HasThis)
					{
						var typeSig = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, method.Assembly);
						var sigInfo = new SignatureInfo(typeSig, pos);
						state.HeaderSignatures.Add(sigInfo);
						blob.Write(ref pos, (int)0);
					}

					for (int i = 0; i < parameters.Count; i++)
					{
						var typeSig = parameters[i].Type;
						var sigInfo = new SignatureInfo(typeSig, pos);
						state.HeaderSignatures.Add(sigInfo);
						blob.Write(ref pos, (int)0);
					}
				}
			}

			// Local variables
			if (methodBody.LocalVariables.Count > 0)
			{
				state.Flags |= BlobFlags.HasLocalVariables;

				var localVariables = methodBody.LocalVariables;

				blob.Write7BitEncodedInt(ref pos, localVariables.Count);

				// Check if pinned variable is present.
				bool hasPinned = false;
				for (int i = 0; i < localVariables.Count; i++)
				{
					var typeSig = localVariables[i];
					if (typeSig.ElementCode == TypeElementCode.Pinned)
					{
						hasPinned = true;
						state.Flags |= BlobFlags.HasPinnedLocalVariables;
						break;
					}
				}

				for (int i = 0; i < localVariables.Count; i++)
				{
					var typeSig = localVariables[i];

					if (hasPinned)
					{
						if (typeSig.ElementCode == TypeElementCode.Pinned)
						{
							blob.Write(ref pos, (byte)1);
							typeSig = typeSig.ElementType;
						}
						else
						{
							blob.Write(ref pos, (byte)0);
						}
					}

					var sigInfo = new SignatureInfo(typeSig, pos);
					state.HeaderSignatures.Add(sigInfo);
					blob.Write(ref pos, (int)0);
				}
			}

			// Sizes
			blob.Write7BitEncodedInt(ref pos, state.CodeSize);
			blob.Write7BitEncodedInt(ref pos, methodBody.MaxStackSize);

			if (state.EHSize > 0)
				blob.Write7BitEncodedInt(ref pos, state.EHSize);

			// Flags
			pos = 0;
			blob.Write(ref pos, (byte)state.Flags);
		}

		private void BuildIL(BuildState state)
		{
			var blob = state.ILBlob;
			int pos = 0;

			foreach (var instruction in state.MethodBody.Instructions)
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
							AddILSignature(state, (FieldReference)value, pos);
							blob.Write(ref pos, (int)0);
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
							AddILSignature(state, (MethodSignature)value, pos);
							blob.Write(ref pos, (int)0);
						}
						break;

					case OperandType.InlineR:
						{
							blob.Write(ref pos, (double)value);
						}
						break;

					case OperandType.InlineSig:
						{
							AddILSignature(state, (CallSite)value, pos);
							blob.Write(ref pos, (int)0);
						}
						break;

					case OperandType.InlineString:
						{
							AddILString(state, (string)value, pos);
							blob.Write(ref pos, (int)0);
						}
						break;

					case OperandType.InlineSwitch:
						{
							int[] branches = (int[])value;

							blob.Write(ref pos, (int)branches.Length);

							for (int i = 0; i < branches.Length; i++)
								blob.Write(ref pos, (int)branches[i]);
						}
						break;

					case OperandType.InlineTok:
						{
							AddILSignature(state, (Signature)value, pos);
							blob.Write(ref pos, (int)0);
						}
						break;

					case OperandType.InlineType:
						{
							AddILSignature(state, (TypeSignature)value, pos);
							blob.Write(ref pos, (int)0);
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

			state.CodeSize = blob.Length;
		}

		private void BuildEH(BuildState state)
		{
			if (state.MethodBody.ExceptionHandlers.Count == 0)
				return;

			var blob = state.ILBlob;
			int offset = blob.Length;
			int pos = offset;

			var method = state.Method;
			var methodBody = state.MethodBody;

			state.Flags |= BlobFlags.HasExceptions;

			var exceptionHandlers = methodBody.ExceptionHandlers;

			int flags = ILMethodSect.EHTable;

			bool isFat = false;

			// Determine if exception handlers have to be stored in fat format.
			if ((exceptionHandlers.Count * 12) + 4 > Byte.MaxValue)
			{
				isFat = true;
			}
			else
			{
				foreach (var exceptionHandler in exceptionHandlers)
				{
					if (exceptionHandler.TryOffset > UInt16.MaxValue ||
						exceptionHandler.TryLength > Byte.MaxValue ||
						exceptionHandler.HandlerOffset > UInt16.MaxValue ||
						exceptionHandler.HandlerLength > Byte.MaxValue)
					{
						isFat = true;
						break;
					}
				}
			}

			if (isFat)
			{
				flags |= ILMethodSect.FatFormat;

				// Fat format
				int length = (exceptionHandlers.Count * 24) + 4;
				flags |= (length << 8);
				blob.Write(ref pos, (int)flags);

				foreach (var handler in exceptionHandlers)
				{
					BuildILExceptionHandlerFat(blob, ref pos, state, handler);
				}
			}
			else
			{
				// Tiny format
				blob.Write(ref pos, (byte)flags);

				int length = (exceptionHandlers.Count * 12) + 4;
				blob.Write(ref pos, (byte)length);

				blob.Write(ref pos, (byte)0, 2); // padded

				foreach (var handler in exceptionHandlers)
				{
					BuildILExceptionHandlerTiny(blob, ref pos, state, handler);
				}
			}

			state.EHSize = blob.Length - offset;
		}

		private void BuildILExceptionHandlerFat(Blob blob, ref int pos, BuildState state, ExceptionHandler handler)
		{
			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					blob.Write(ref pos, (int)ILExceptionFlag.CATCH);
					break;

				case ExceptionHandlerType.Filter:
					blob.Write(ref pos, (int)ILExceptionFlag.FILTER);
					break;

				case ExceptionHandlerType.Finally:
					blob.Write(ref pos, (int)ILExceptionFlag.FINALLY);
					break;

				case ExceptionHandlerType.Fault:
					blob.Write(ref pos, (int)ILExceptionFlag.FAULT);
					break;
			}

			blob.Write(ref pos, (int)handler.TryOffset);
			blob.Write(ref pos, (int)handler.TryLength);
			blob.Write(ref pos, (int)handler.HandlerOffset);
			blob.Write(ref pos, (int)handler.HandlerLength);

			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					{
						AddILSignature(state, handler.CatchType, pos);
						blob.Write(ref pos, (int)0);
					}
					break;

				case ExceptionHandlerType.Filter:
					{
						blob.Write(ref pos, (int)handler.FilterOffset);
					}
					break;

				default:
					{
						blob.Write(ref pos, (int)0); // padded
					}
					break;
			}
		}

		private void BuildILExceptionHandlerTiny(Blob blob, ref int pos, BuildState state, ExceptionHandler handler)
		{
			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					blob.Write(ref pos, (short)ILExceptionFlag.CATCH);
					break;

				case ExceptionHandlerType.Filter:
					blob.Write(ref pos, (short)ILExceptionFlag.FILTER);
					break;

				case ExceptionHandlerType.Finally:
					blob.Write(ref pos, (short)ILExceptionFlag.FINALLY);
					break;

				case ExceptionHandlerType.Fault:
					blob.Write(ref pos, (short)ILExceptionFlag.FAULT);
					break;
			}

			blob.Write(ref pos, (ushort)handler.TryOffset);
			blob.Write(ref pos, (byte)handler.TryLength);
			blob.Write(ref pos, (ushort)handler.HandlerOffset);
			blob.Write(ref pos, (byte)handler.HandlerLength);

			switch (handler.Type)
			{
				case ExceptionHandlerType.Catch:
					{
						AddILSignature(state, handler.CatchType, pos);
						blob.Write(ref pos, (int)0);
					}
					break;

				case ExceptionHandlerType.Filter:
					{
						blob.Write(ref pos, (int)handler.FilterOffset);
					}
					break;

				default:
					{
						blob.Write(ref pos, (int)0); // padded
					}
					break;
			}
		}

		private void BuildFixups(BuildState state)
		{
			var fixups = state.Fixups;
			if (fixups.Count == 0)
				return;

			var blob = state.FixupBlob;
			int pos = 0;

			state.Flags |= BlobFlags.HasFixups;

			blob.Write7BitEncodedInt(ref pos, fixups.Count);

			for (int i = 0; i < fixups.Count; i++)
			{
				blob.Write7BitEncodedInt(ref pos, fixups[i]);
			}
		}

		private int Write(BuildState state)
		{
			int pos = 0;
			var blob = new Blob();

			int blobID = _storage.NextBlobID;

			// Header
			for (int i = 0; i < state.HeaderSignatures.Count; i++)
			{
				var sigInfo = state.HeaderSignatures[i];
				sigInfo.Position += pos;
				sigInfo.BlobID = blobID;
				_signatures.Add(sigInfo);
			}

			blob.Write(ref pos, state.HeaderBlob.GetBuffer(), 0, state.HeaderBlob.Length);

			// Fixups
			blob.Write(ref pos, state.FixupBlob.GetBuffer(), 0, state.FixupBlob.Length);

			// IL
			for (int i = 0; i < state.ILSignatures.Count; i++)
			{
				var sigInfo = state.ILSignatures[i];
				sigInfo.Position += pos;
				sigInfo.BlobID = blobID;
				_signatures.Add(sigInfo);
			}

			for (int i = 0; i < state.ILStrings.Count; i++)
			{
				var stringInfo = state.ILStrings[i];
				stringInfo.Position += pos;
				stringInfo.BlobID = blobID;
				_userStrings.Add(stringInfo);
			}

			blob.Write(ref pos, state.ILBlob.GetBuffer(), 0, state.ILBlob.Length);

			_storage.Add(blob.GetBuffer(), 0, blob.Length);

			return blobID;
		}

		private TypeSignature GetDelegateTypeSig(ILCryptoMethod cryptoMethod)
		{
			var delegateType = cryptoMethod.InvokeMethod.DelegateType;
			var delegateGenericArguments = cryptoMethod.DelegateGenericArguments;
			if (delegateGenericArguments != null && delegateGenericArguments.Length > 0)
				return new GenericTypeReference(delegateType.DeclaringType, delegateGenericArguments);
			else
				return delegateType.DeclaringType;
		}

		private void AddILSignature(BuildState state, Signature sig, int pos)
		{
			state.Fixups.Add(pos - state.FixOffset);
			state.FixOffset = pos + 4;

			state.ILSignatures.Add(new SignatureInfo(sig, pos));
		}

		private void AddILString(BuildState state, string s, int pos)
		{
			state.Fixups.Add(pos - state.FixOffset);
			state.FixOffset = pos + 4;

			state.ILStrings.Add(new UserStringInfo(s, pos));
		}

		private void MapMemberReferences()
		{
			var mapper = new MemberReferenceMapper(_module);

			for (int i = 0; i < _signatures.Count; i++)
			{
				var sigInfo = _signatures[i];
				var signature = sigInfo.Signature;
				if (mapper.Build(ref signature))
				{
					sigInfo.Signature = signature;
					_signatures[i] = sigInfo;
				}
			}
		}

		#endregion

		#region Static

		public static void MapMemberReferences(BuildAssembly assembly)
		{
			var module = (BuildModule)assembly.Module;
			var assembler = module.Assembler;
			var blobBuilder = assembler.Tasks.Get<ILCryptoBlobBuilder>();
			if (blobBuilder != null)
			{
				blobBuilder.MapMemberReferences();
			}
		}

		#endregion

		#region Nested types

		[Flags]
		private enum BlobFlags
		{
			None = 0,
			HasReturnType = 1,
			HasParameters = 1 << 1,
			HasLocalVariables = 1 << 2,
			HasPinnedLocalVariables = 1 << 3,
			HasExceptions = 1 << 4,
			HasFixups = 1 << 5,
		}

		private class BuildState
		{
			internal int CodeSize;
			internal int EHSize;
			internal BlobFlags Flags;
			internal MethodDeclaration Method;
			internal MethodBody MethodBody;
			internal Blob HeaderBlob = new Blob();
			internal Blob ILBlob = new Blob();
			internal Blob FixupBlob = new Blob();
			internal List<SignatureInfo> HeaderSignatures = new List<SignatureInfo>();
			internal List<SignatureInfo> ILSignatures = new List<SignatureInfo>();
			internal List<UserStringInfo> ILStrings = new List<UserStringInfo>();
			internal List<int> Fixups = new List<int>();
			internal int FixOffset = 0;
		}

		private class SignatureInfo
		{
			internal int BlobID;
			internal int Position;
			internal Signature Signature;

			internal SignatureInfo(Signature sig, int pos)
			{
				if (sig is FieldReference && ((FieldReference)sig).Name.Length == 1)
				{
				}

				this.Signature = sig;
				this.Position = pos;
			}
		}

		private class UserStringInfo
		{
			internal int BlobID;
			internal int Position;
			internal string String;

			internal UserStringInfo(string s, int pos)
			{
				this.String = s;
				this.Position = pos;
			}
		}

		#endregion
	}
}
