using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedField : IField
	{
		#region Fields

		private IField _declaringField;
		private IType _fieldType;
		private IType _ownerType;
		private IModule _module;

		#endregion

		#region Ctors

		internal ReferencedField(IField declaringField, IType ownerType)
		{
			_declaringField = declaringField;
			_ownerType = ownerType;
			_module = declaringField.Module;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _declaringField.Name; }
		}

		public bool HasGenericContext
		{
			get { return _ownerType.HasGenericContext; }
		}

		public bool IsStatic
		{
			get { return _declaringField.IsStatic; }
		}

		public bool IsInitOnly
		{
			get { return _declaringField.IsInitOnly; }
		}

		public bool IsLiteral
		{
			get { return _declaringField.IsLiteral; }
		}

		public bool IsNotSerialized
		{
			get { return _declaringField.IsNotSerialized; }
		}

		public bool IsSpecialName
		{
			get { return _declaringField.IsSpecialName; }
		}

		public bool IsRuntimeSpecialName
		{
			get { return _declaringField.IsRuntimeSpecialName; }
		}

		public FieldVisibilityFlags Visibility
		{
			get { return _declaringField.Visibility; }
		}

		public ConstantInfo? DefaultValue
		{
			get { return _declaringField.DefaultValue; }
		}

		public IType FieldType
		{
			get
			{
				if (_fieldType == null)
				{
					_fieldType = AssemblyManager.Resolve(((IFieldSignature)_declaringField).FieldType, this, true);
				}

				return _fieldType;
			}
		}

		public IType Owner
		{
			get { return _ownerType; }
		}

		public IField DeclaringField
		{
			get { return _declaringField; }
		}

		public EntityType EntityType
		{
			get { return EntityType.Field; }
		}

		public SignatureType SignatureType
		{
			get { return SignatureType.Field; }
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

		ITypeSignature IFieldSignature.FieldType
		{
			get { return FieldType; }
		}

		ITypeSignature IFieldSignature.Owner
		{
			get { return _ownerType; }
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

		internal IField Intern()
		{
			IField field = this;
			AssemblyManager.InternNode<IField>(ref field);

			return field;
		}

		#endregion
	}
}
