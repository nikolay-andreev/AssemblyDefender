using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public abstract class MethodSignature : Signature, IMethodSignature
	{
		#region Properties

		public virtual string Name
		{
			get { return null; }
		}

		public bool IsStatic
		{
			get { return !HasThis; }
		}

		public abstract bool HasThis
		{
			get;
		}

		public abstract bool ExplicitThis
		{
			get;
		}

		public abstract int VarArgIndex
		{
			get;
		}

		public abstract int GenericParameterCount
		{
			get;
		}

		public abstract MethodCallingConvention CallConv
		{
			get;
		}

		public abstract TypeSignature ReturnType
		{
			get;
		}

		public virtual TypeSignature Owner
		{
			get { return null; }
		}

		public virtual IReadOnlyList<TypeSignature> Arguments
		{
			get { return ReadOnlyList<TypeSignature>.Empty; }
		}

		public virtual IReadOnlyList<TypeSignature> GenericArguments
		{
			get { return ReadOnlyList<TypeSignature>.Empty; }
		}

		public abstract MethodSignatureType Type
		{
			get;
		}

		public virtual MethodReference DeclaringMethod
		{
			get { return null; }
		}

		public virtual CallSite CallSite
		{
			get { return (CallSite)this; }
		}

		public override SignatureType SignatureType
		{
			get { return SignatureType.Method; }
		}

		#endregion

		#region Methods

		public int GetArgumentCountNoVarArgs()
		{
			if (CallConv != MethodCallingConvention.VarArgs)
				return Arguments.Count;

			if (VarArgIndex < 0)
				return Arguments.Count;

			return VarArgIndex;
		}

		public override string ToString()
		{
			return this.ToString(null, SignaturePrintingFlags.None);
		}

		#endregion

		#region IMethodSignature

		ITypeSignature IMethodSignature.ReturnType
		{
			get { return ReturnType; }
		}

		ITypeSignature IMethodSignature.Owner
		{
			get { return Owner; }
		}

		IMethodSignature IMethodSignature.DeclaringMethod
		{
			get { return DeclaringMethod; }
		}

		IReadOnlyList<ITypeSignature> IMethodSignature.Arguments
		{
			get { return Arguments; }
		}

		IReadOnlyList<ITypeSignature> IMethodSignature.GenericArguments
		{
			get { return GenericArguments; }
		}

		#endregion

		#region Static

		internal static MethodSignature Load(Module module, int token)
		{
			switch (MetadataToken.GetType(token))
			{
				case MetadataTokenType.Method:
					return MethodReference.LoadMethodDef(module, MetadataToken.GetRID(token));

				case MetadataTokenType.MemberRef:
					return (MethodReference)MethodReference.LoadMemberRef(module, MetadataToken.GetRID(token));

				case MetadataTokenType.MethodSpec:
					return GenericMethodReference.LoadMethodSpec(module, MetadataToken.GetRID(token));

				default:
					throw new Exception(string.Format("Invalid method reference token {0}", token.ToString()));
			}
		}

		#endregion
	}
}
