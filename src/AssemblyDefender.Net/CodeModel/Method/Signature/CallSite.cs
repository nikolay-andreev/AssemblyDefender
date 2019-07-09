using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class CallSite : MethodSignature
	{
		#region Fields

		private bool _hasThis;
		private bool _explicitThis;
		private int _varArgIndex = -1;
		private int _genericParameterCount;
		private MethodCallingConvention _callConv;
		private TypeSignature _returnType;
		private IReadOnlyList<TypeSignature> _arguments;

		#endregion

		#region Ctors

		private CallSite()
		{
		}

		public CallSite(
			bool hasThis,
			bool explicitThis,
			MethodCallingConvention callConv,
			TypeSignature returnType,
			TypeSignature[] arguments,
			int varArgIndex,
			int genericParameterCount)
		{
			if (returnType == null)
				throw new ArgumentNullException("returnType");

			_hasThis = hasThis;
			_explicitThis = hasThis && explicitThis;
			_genericParameterCount = genericParameterCount;
			_callConv = callConv;
			_returnType = returnType;
			_arguments = ReadOnlyList<TypeSignature>.Create(arguments);

			if (callConv == MethodCallingConvention.VarArgs)
				_varArgIndex = varArgIndex;
		}

		public CallSite(
			bool hasThis,
			bool explicitThis,
			MethodCallingConvention callConv,
			TypeSignature returnType,
			IReadOnlyList<TypeSignature> arguments,
			int varArgIndex,
			int genericParameterCount)
		{
			if (returnType == null)
				throw new ArgumentNullException("returnType");

			_hasThis = hasThis;
			_explicitThis = hasThis && explicitThis;
			_genericParameterCount = genericParameterCount;
			_callConv = callConv;
			_returnType = returnType;
			_arguments = arguments ?? ReadOnlyList<TypeSignature>.Empty;

			if (callConv == MethodCallingConvention.VarArgs)
				_varArgIndex = varArgIndex;
		}

		#endregion

		#region Properties

		public override bool HasThis
		{
			get { return _hasThis; }
		}

		public override bool ExplicitThis
		{
			get { return _explicitThis; }
		}

		public override int VarArgIndex
		{
			get { return _varArgIndex; }
		}

		public override int GenericParameterCount
		{
			get { return _genericParameterCount; }
		}

		public override MethodCallingConvention CallConv
		{
			get { return _callConv; }
		}

		public override TypeSignature ReturnType
		{
			get { return _returnType; }
		}

		public override IReadOnlyList<TypeSignature> Arguments
		{
			get { return _arguments; }
		}

		public override MethodSignatureType Type
		{
			get { return MethodSignatureType.CallSite; }
		}

		#endregion

		#region Methods

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _returnType);

			int count = _arguments.Count;
			if (count > 0)
			{
				var arguments = new TypeSignature[count];
				for (int i = 0; i < count; i++)
				{
					var argument = _arguments[i];
					module.AddSignature(ref argument);
					arguments[i] = argument;
				}

				_arguments = ReadOnlyList<TypeSignature>.Create(arguments);
			}
		}

		#endregion

		#region Static

		internal static CallSite LoadStandAloneSig(Module module, int rid)
		{
			var image = module.Image;

			var callSite = image.StandAloneSigSignatures[rid - 1] as CallSite;
			if (callSite != null)
				return callSite;

			int blobID = image.GetStandAloneSig(rid);

			using (var accessor = image.OpenBlob(blobID))
			{
				callSite = LoadCallSite(accessor, module);
			}

			module.AddSignature(ref callSite);
			image.StandAloneSigSignatures[rid - 1] = callSite;

			return callSite;
		}

		internal static CallSite LoadCallSite(IBinaryAccessor accessor, Module module)
		{
			byte sigType = accessor.ReadByte();
			return LoadCallSite(accessor, module, sigType);
		}

		internal static CallSite LoadCallSite(IBinaryAccessor accessor, Module module, byte sigType)
		{
			var callSite = new CallSite();

			callSite._hasThis = (sigType & Metadata.SignatureType.HasThis) == Metadata.SignatureType.HasThis;
			callSite._explicitThis = (sigType & Metadata.SignatureType.ExplicitThis) == Metadata.SignatureType.ExplicitThis;
			callSite._callConv = (MethodCallingConvention)(sigType & Metadata.SignatureType.MethodCallConvMask);

			if ((sigType & Metadata.SignatureType.Generic) == Metadata.SignatureType.Generic)
			{
				callSite._genericParameterCount = accessor.ReadCompressedInteger();
			}

			int paramCount = accessor.ReadCompressedInteger();

			callSite._returnType =
				TypeSignature.Load(accessor, module) ??
				TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, module.Assembly);

			bool isVarArgs = (callSite._callConv == MethodCallingConvention.VarArgs);

			if (callSite._callConv == MethodCallingConvention.VarArgs)
			{
				int varArgIndex;
				var arguments = TypeSignature.LoadVarArgMethodArguments(accessor, module, paramCount, out varArgIndex);
				callSite._arguments = ReadOnlyList<TypeSignature>.Create(arguments);
				callSite._varArgIndex = varArgIndex;
			}
			else
			{
				var arguments = TypeSignature.LoadMethodArguments(accessor, module, paramCount);
				callSite._arguments = ReadOnlyList<TypeSignature>.Create(arguments);
			}

			return callSite;
		}

		#endregion
	}
}
