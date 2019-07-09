using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	internal class ReferencedMethodParameter : IMethodParameter
	{
		#region Fields

		private IType _type;
		private IMethod _owner;
		private IMethodParameter _declaringParameter;

		#endregion

		#region Ctors

		internal ReferencedMethodParameter(IMethod owner, IMethodParameter declaringParameter)
		{
			_owner = owner;
			_declaringParameter = declaringParameter;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _declaringParameter.Name; }
		}

		public bool IsIn
		{
			get { return _declaringParameter.IsIn; }
		}

		public bool IsOut
		{
			get { return _declaringParameter.IsOut; }
		}

		public bool IsOptional
		{
			get { return _declaringParameter.IsOptional; }
		}

		public bool IsLcid
		{
			get { return _declaringParameter.IsLcid; }
		}

		public IType Type
		{
			get
			{
				if (_type == null)
				{
					_type = _owner.AssemblyManager.Resolve(_declaringParameter.Type, _owner, true);
				}

				return _type;
			}
		}

		public IMethod Owner
		{
			get { return _owner; }
		}

		ITypeSignature IMethodParameterBase.Type
		{
			get { return Type; }
		}

		#endregion
	}
}
