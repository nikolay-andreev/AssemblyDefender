using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class GenericParameter : CodeNode, IGenericParameter, ICustomAttributeProvider
	{
		#region Fields

		private int _rid;
		private string _name;
		private int _flags;
		private MemberNode _owner;
		private GenericParameterConstraintCollection _constraints;
		private CustomAttributeCollection _customAttributes;
		private IReadOnlyList<IType> _resolvedConstraints;

		#endregion

		#region Ctors

		internal GenericParameter(CodeNode parent, MemberNode owner)
			: base(parent)
		{
			_owner = owner;
			IsNew = true;
		}

		internal GenericParameter(CodeNode parent, MemberNode owner, int rid)
			: base(parent)
		{
			_rid = rid;
			_owner = owner;
			Load();
		}

		#endregion

		#region Properties

		public int RID
		{
			get { return _rid; }
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value.NullIfEmpty();
				OnChanged();
			}
		}

		public int Flags
		{
			get { return _flags; }
			set
			{
				_flags = value;
				OnChanged();
			}
		}

		public GenericParameterVariance Variance
		{
			get { return (GenericParameterVariance)_flags.GetBits(GenericParamFlags.VarianceMask); }
			set
			{
				_flags = _flags.SetBits(GenericParamFlags.VarianceMask, (int)value);
				OnChanged();
			}
		}

		public bool DefaultConstructorConstraint
		{
			get { return _flags.IsBitsOn(GenericParamFlags.DefaultConstructorConstraint); }
			set
			{
				_flags = _flags.SetBits(GenericParamFlags.DefaultConstructorConstraint, value);
				OnChanged();
			}
		}

		public bool ReferenceTypeConstraint
		{
			get { return _flags.IsBitsOn(GenericParamFlags.ReferenceTypeConstraint); }
			set
			{
				_flags = _flags.SetBits(GenericParamFlags.ReferenceTypeConstraint, value);
				if (value)
					_flags = _flags.SetBits(GenericParamFlags.NotNullableValueTypeConstraint, false);

				OnChanged();
			}
		}

		public bool ValueTypeConstraint
		{
			get { return _flags.IsBitsOn(GenericParamFlags.NotNullableValueTypeConstraint); }
			set
			{
				_flags = _flags.SetBits(GenericParamFlags.NotNullableValueTypeConstraint, value);
				if (value)
					_flags = _flags.SetBits(GenericParamFlags.ReferenceTypeConstraint, false);

				OnChanged();
			}
		}

		public GenericParameterConstraintCollection Constraints
		{
			get
			{
				if (_constraints == null)
				{
					_constraints = new GenericParameterConstraintCollection(this);
				}

				return _constraints;
			}
		}

		public CustomAttributeCollection CustomAttributes
		{
			get
			{
				if (_customAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.GenericParam, _rid);
					_customAttributes = new CustomAttributeCollection(this, token);
				}

				return _customAttributes;
			}
		}

		public MemberNode Owner
		{
			get { return _owner; }
		}

		#endregion

		#region Methods

		public void CopyTo(GenericParameter copy)
		{
			copy._name = _name;
			copy._flags = _flags;
			Constraints.CopyTo(copy.Constraints);
			CustomAttributes.CopyTo(copy.CustomAttributes);
		}

		internal void InvalidatedSignatures()
		{
			_resolvedConstraints = null;
		}

		private void Load()
		{
			var image = _module.Image;

			GenericParamRow row;
			image.GetGenericParam(_rid, out row);

			_name = image.GetString(row.Name);
			_flags = row.Flags;
		}

		#endregion

		#region IGenericParameter Members

		ICodeNode IGenericParameter.Owner
		{
			get { return (ICodeNode)_owner; }
		}

		IReadOnlyList<IType> IGenericParameter.Constraints
		{
			get
			{
				if (_resolvedConstraints == null)
				{
					_resolvedConstraints = AssemblyManager.Resolve(Constraints, (ICodeNode)_owner, true);
				}

				return _resolvedConstraints;
			}
		}

		IReadOnlyList<ITypeSignature> IGenericParameterBase.Constraints
		{
			get { return Constraints; }
		}

		#endregion
	}
}
