using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedProperty : IProperty
	{
		#region Fields

		private IProperty _declaringProperty;
		private IType _returnType;
		private IType _ownerType;
		private IMethod _getMethod;
		private IMethod _setMethod;
		private IReadOnlyList<IType> _parameters;
		private IModule _module;

		#endregion

		#region Ctors

		internal ReferencedProperty(IProperty declaringProperty, IType ownerType)
		{
			_declaringProperty = declaringProperty;
			_ownerType = ownerType;
			_module = declaringProperty.Module;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _declaringProperty.Name; }
		}

		public bool HasGenericContext
		{
			get { return _ownerType.HasGenericContext; }
		}

		public IType ReturnType
		{
			get
			{
				if (_returnType == null)
				{
					_returnType = AssemblyManager.Resolve(((IPropertySignature)_declaringProperty).ReturnType, this, true);
				}

				return _returnType;
			}
		}

		public IType Owner
		{
			get { return _ownerType; }
		}

		public IMethod GetMethod
		{
			get
			{
				if (_getMethod == null)
				{
					_getMethod = AssemblyManager.Resolve(((IPropertyBase)_declaringProperty).GetMethod, this, false, true);
				}

				return _getMethod;
			}
		}

		public IMethod SetMethod
		{
			get
			{
				if (_setMethod == null)
				{
					_setMethod = AssemblyManager.Resolve(((IPropertyBase)_declaringProperty).SetMethod, this, false, true);
				}

				return _setMethod;
			}
		}

		public IProperty DeclaringProperty
		{
			get { return _declaringProperty; }
		}

		public IReadOnlyList<IType> Parameters
		{
			get
			{
				if (_parameters == null)
				{
					_parameters = AssemblyManager.Resolve(((IPropertySignature)_declaringProperty).Arguments, this, true);
				}

				return _parameters;
			}
		}

		public EntityType EntityType
		{
			get { return EntityType.Property; }
		}

		public SignatureType SignatureType
		{
			get { return SignatureType.Property; }
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

		ITypeSignature IPropertySignature.ReturnType
		{
			get { return ReturnType; }
		}

		ITypeSignature IPropertySignature.Owner
		{
			get { return _ownerType; }
		}

		IMethodSignature IPropertyBase.GetMethod
		{
			get { return GetMethod; }
		}

		IMethodSignature IPropertyBase.SetMethod
		{
			get { return SetMethod; }
		}

		IReadOnlyList<ITypeSignature> IPropertySignature.Arguments
		{
			get { return Parameters; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		public IType GetGenericArgument(bool isMethod, int position)
		{
			var genericArguments = _ownerType.GenericArguments;
			if (genericArguments.Count > position)
			{
				return genericArguments[position];
			}

			return null;
		}

		internal IProperty Intern()
		{
			IProperty property = this;
			AssemblyManager.InternNode<IProperty>(ref property);

			return property;
		}

		#endregion
	}
}
