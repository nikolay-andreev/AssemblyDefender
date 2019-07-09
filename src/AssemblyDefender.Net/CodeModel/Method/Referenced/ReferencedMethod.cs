using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedMethod : IMethod
	{
		#region Fields

		private IMethod _declaringMethod;
		private IType _ownerType;
		private IType _returnType;
		private IReadOnlyList<IMethod> _overrides;
		private ReferencedMethodParameterCollection _parameters;
		private IReadOnlyList<IType> _genericArguments;
		private IReadOnlyList<IGenericParameter> _genericParameters;
		private IModule _module;

		#endregion

		#region Ctors

		internal ReferencedMethod(IMethod declaringMethod, IType ownerType, IReadOnlyList<IType> genericArguments)
		{
			_declaringMethod = declaringMethod;
			_ownerType = ownerType;
			_genericArguments = genericArguments;
			_module = declaringMethod.Module;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _declaringMethod.Name; }
		}

		public bool HasGenericContext
		{
			get { return _genericArguments.Count > 0 || _ownerType.HasGenericContext; }
		}

		public bool IsStatic
		{
			get { return _declaringMethod.IsStatic; }
		}

		public bool HasThis
		{
			get { return _declaringMethod.HasThis; }
		}

		public bool ExplicitThis
		{
			get { return _declaringMethod.ExplicitThis; }
		}

		public MethodCallingConvention CallConv
		{
			get { return _declaringMethod.CallConv; }
		}

		public MethodVisibilityFlags Visibility
		{
			get { return _declaringMethod.Visibility; }
		}

		public bool IsFinal
		{
			get { return _declaringMethod.IsFinal; }
		}

		public bool IsVirtual
		{
			get { return _declaringMethod.IsVirtual; }
		}

		public bool IsHideBySig
		{
			get { return _declaringMethod.IsHideBySig; }
		}

		public bool IsNewSlot
		{
			get { return _declaringMethod.IsNewSlot; }
		}

		public bool IsStrict
		{
			get { return _declaringMethod.IsStrict; }
		}

		public bool IsAbstract
		{
			get { return _declaringMethod.IsAbstract; }
		}

		public bool IsSpecialName
		{
			get { return _declaringMethod.IsSpecialName; }
		}

		public bool IsRuntimeSpecialName
		{
			get { return _declaringMethod.IsRuntimeSpecialName; }
		}

		public bool RequireSecObject
		{
			get { return _declaringMethod.RequireSecObject; }
		}

		public bool IsUnmanaged
		{
			get { return _declaringMethod.IsUnmanaged; }
		}

		public bool IsForwardRef
		{
			get { return _declaringMethod.IsForwardRef; }
		}

		public bool IsPreserveSig
		{
			get { return _declaringMethod.IsPreserveSig; }
		}

		public bool IsInternalCall
		{
			get { return _declaringMethod.IsInternalCall; }
		}

		public bool IsSynchronized
		{
			get { return _declaringMethod.IsSynchronized; }
		}

		public bool IsNoInlining
		{
			get { return _declaringMethod.IsNoInlining; }
		}

		public MethodCodeTypeFlags CodeType
		{
			get { return _declaringMethod.CodeType; }
		}

		public SignatureType SignatureType
		{
			get { return SignatureType.Method; }
		}

		public EntityType EntityType
		{
			get { return EntityType.Method; }
		}

		public IMethod DeclaringMethod
		{
			get { return _declaringMethod; }
		}

		public IType Owner
		{
			get { return _ownerType; }
		}

		public IType ReturnType
		{
			get
			{
				if (_returnType == null)
				{
					_returnType = AssemblyManager.Resolve(((IMethodSignature)_declaringMethod).ReturnType, this, true);
				}

				return _returnType;
			}
		}

		public IReadOnlyList<IMethod> Overrides
		{
			get
			{
				if (_overrides == null)
				{
					_overrides = AssemblyManager.Resolve(((IMethodBase)_declaringMethod).Overrides, this, true, true);
				}

				return _overrides;
			}
		}

		public IReadOnlyList<IMethodParameter> Parameters
		{
			get
			{
				if (_parameters == null)
				{
					LoadParameters();
				}

				return _parameters;
			}
		}

		public IReadOnlyList<IType> GenericArguments
		{
			get { return _genericArguments; }
		}

		public IReadOnlyList<IGenericParameter> GenericParameters
		{
			get
			{
				if (_genericParameters == null)
				{
					LoadGenericParameters();
				}

				return _genericParameters;
			}
		}

		public IAssembly Assembly
		{
			get { return _module.Assembly; }
		}

		public IModule Module
		{
			get { return _module; }
		}

		public AssemblyManager AssemblyManager
		{
			get { return _module.AssemblyManager; }
		}

		IReadOnlyList<IMethodSignature> IMethodBase.Overrides
		{
			get { return Overrides; }
		}

		int IMethodSignature.VarArgIndex
		{
			get { return -1; }
		}

		int IMethodSignature.GenericParameterCount
		{
			get { return _declaringMethod.GenericParameterCount; }
		}

		ITypeSignature IMethodSignature.ReturnType
		{
			get { return ReturnType; }
		}

		ITypeSignature IMethodSignature.Owner
		{
			get { return _ownerType; }
		}

		IMethodSignature IMethodSignature.DeclaringMethod
		{
			get { return _declaringMethod; }
		}

		IReadOnlyList<ITypeSignature> IMethodSignature.Arguments
		{
			get
			{
				if (_parameters == null)
				{
					LoadParameters();
				}

				return _parameters;
			}
		}

		IReadOnlyList<ITypeSignature> IMethodSignature.GenericArguments
		{
			get { return _genericArguments; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		public IType GetGenericArgument(bool isMethod, int position)
		{
			var genericArguments = isMethod ? _genericArguments : _ownerType.GenericArguments;
			if (genericArguments.Count > position)
			{
				return genericArguments[position];
			}

			return null;
		}

		internal IMethod Intern()
		{
			IMethod method = this;
			AssemblyManager.InternNode<IMethod>(ref method);

			return method;
		}

		private void LoadParameters()
		{
			var declaringParameters = _declaringMethod.Parameters;
			var parameters = new IMethodParameter[declaringParameters.Count];
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new ReferencedMethodParameter(this, declaringParameters[i]);
			}

			_parameters = new ReferencedMethodParameterCollection(parameters);
		}

		private void LoadGenericParameters()
		{
			var declaringGenericParameters = _declaringMethod.GenericParameters;
			var genericParameters = new IGenericParameter[declaringGenericParameters.Count];
			for (int i = 0; i < genericParameters.Length; i++)
			{
				genericParameters[i] = new ReferencedGenericParameter(declaringGenericParameters[i], this);
			}

			_genericParameters = ReadOnlyList<IGenericParameter>.Create(genericParameters);
		}

		#endregion
	}
}
