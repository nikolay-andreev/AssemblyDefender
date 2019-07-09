using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedGenericParameter : IGenericParameter
	{
		#region Fields

		private IGenericParameter _declaringGenericParameter;
		private ICodeNode _owner;
		private IReadOnlyList<IType> _constraints;

		#endregion

		#region Ctors

		internal ReferencedGenericParameter(IGenericParameter declaringGenericParameter, ICodeNode owner)
		{
			_declaringGenericParameter = declaringGenericParameter;
			_owner = owner;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _declaringGenericParameter.Name; }
		}

		public GenericParameterVariance Variance
		{
			get { return _declaringGenericParameter.Variance; }
		}

		public bool DefaultConstructorConstraint
		{
			get { return _declaringGenericParameter.DefaultConstructorConstraint; }
		}

		public bool ReferenceTypeConstraint
		{
			get { return _declaringGenericParameter.ReferenceTypeConstraint; }
		}

		public bool ValueTypeConstraint
		{
			get { return _declaringGenericParameter.ValueTypeConstraint; }
		}

		public ICodeNode Owner
		{
			get { return _owner; }
		}

		public IReadOnlyList<IType> Constraints
		{
			get
			{
				if (_constraints == null)
				{
					_constraints = _owner.AssemblyManager.Resolve(_declaringGenericParameter.Constraints, (ICodeNode)_owner, true);
				}

				return _constraints;
			}
		}

		IReadOnlyList<ITypeSignature> IGenericParameterBase.Constraints
		{
			get { return Constraints; }
		}

		#endregion
	}
}
