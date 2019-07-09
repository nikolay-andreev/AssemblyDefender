using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedCallSite : ICallSite
	{
		#region Fields

		private bool _hasThis;
		private bool _explicitThis;
		private int _varArgIndex;
		private MethodCallingConvention _callConv;
		private IType _returnType;
		private IReadOnlyList<IType> _arguments;
		private AssemblyManager _assemblyManager;

		#endregion

		#region Ctors

		public ReferencedCallSite(
			AssemblyManager assemblyManager, bool hasThis, bool explicitThis, int varArgIndex,
			MethodCallingConvention callConv, IType returnType, IReadOnlyList<IType> arguments)
		{
			_hasThis = hasThis;
			_explicitThis = explicitThis;
			_callConv = callConv;
			_returnType = returnType;
			_arguments = arguments;
			_varArgIndex = varArgIndex;
			_assemblyManager = assemblyManager;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return null; }
		}

		public bool IsStatic
		{
			get { return !_hasThis; }
		}

		public bool HasThis
		{
			get { return _hasThis; }
		}

		public bool ExplicitThis
		{
			get { return _explicitThis; }
		}

		public int VarArgIndex
		{
			get { return _varArgIndex; }
		}

		public int GenericParameterCount
		{
			get { return 0; }
		}

		public MethodCallingConvention CallConv
		{
			get { return _callConv; }
		}

		public IType ReturnType
		{
			get { return _returnType; }
		}

		public IReadOnlyList<IType> Arguments
		{
			get { return _arguments; }
		}

		public ITypeSignature Owner
		{
			get { return null; }
		}

		public SignatureType SignatureType
		{
			get { return SignatureType.Method; }
		}

		public AssemblyManager AssemblyManager
		{
			get { return _assemblyManager; }
		}

		IMethodSignature IMethodSignature.DeclaringMethod
		{
			get { return null; }
		}

		ITypeSignature IMethodSignature.ReturnType
		{
			get { return _returnType; }
		}

		IReadOnlyList<ITypeSignature> IMethodSignature.Arguments
		{
			get { return _arguments; }
		}

		IReadOnlyList<ITypeSignature> IMethodSignature.GenericArguments
		{
			get { return ReadOnlyList<ITypeSignature>.Empty; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return this.ToString(null, SignaturePrintingFlags.None);
		}

		internal ICallSite Intern()
		{
			ICallSite callSite = this;
			AssemblyManager.InternNode<ICallSite>(ref callSite);

			return callSite;
		}

		#endregion
	}
}
