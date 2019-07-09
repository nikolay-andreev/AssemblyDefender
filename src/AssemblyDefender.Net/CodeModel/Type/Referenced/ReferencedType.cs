using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal abstract class ReferencedType : IType
	{
		#region Fields

		protected IModule _module;
		protected AssemblyManager _assemblyManager;

		#endregion

		#region Ctors

		protected ReferencedType()
		{
		}

		protected ReferencedType(IType elementType)
		{
			_module = elementType.Module;
			_assemblyManager = elementType.AssemblyManager;
		}

		protected ReferencedType(AssemblyManager assemblyManager)
		{
			_assemblyManager = assemblyManager;
		}

		#endregion

		#region Properties

		public virtual string Name
		{
			get { return null; }
		}

		public virtual string Namespace
		{
			get { return null; }
		}

		public string FullName
		{
			get { return CodeModelUtils.GetTypeName(Name, Namespace); }
		}

		public bool IsNested
		{
			get { return EnclosingType != null; }
		}

		public virtual bool HasGenericContext
		{
			get { return false; }
		}

		public virtual bool IsInterface
		{
			get { return false; }
		}

		public virtual bool IsAbstract
		{
			get { return false; }
		}

		public virtual bool IsSealed
		{
			get { return false; }
		}

		public virtual int? PackingSize
		{
			get { return null; }
		}

		public virtual int? ClassSize
		{
			get { return null; }
		}

		public virtual TypeVisibilityFlags Visibility
		{
			get { return TypeVisibilityFlags.Private; }
		}

		public virtual TypeLayoutFlags Layout
		{
			get { return TypeLayoutFlags.Auto; }
		}

		public virtual TypeCharSetFlags CharSet
		{
			get { return TypeCharSetFlags.Ansi; }
		}

		public virtual IType ElementType
		{
			get { return null; }
		}

		public virtual IType DeclaringType
		{
			get { return null; }
		}

		public virtual IType BaseType
		{
			get { return null; }
		}

		public virtual IType EnclosingType
		{
			get { return null; }
		}

		public virtual IReadOnlyList<IType> GenericArguments
		{
			get { return ReadOnlyList<IType>.Empty; }
		}

		public virtual IReadOnlyList<IGenericParameter> GenericParameters
		{
			get { return ReadOnlyList<IGenericParameter>.Empty; }
		}

		public virtual IReadOnlyList<IType> Interfaces
		{
			get { return ReadOnlyList<IType>.Empty; }
		}

		public virtual IReadOnlyList<IMethod> Methods
		{
			get { return ReadOnlyList<IMethod>.Empty; }
		}

		public virtual IReadOnlyList<IField> Fields
		{
			get { return ReadOnlyList<IField>.Empty; }
		}

		public virtual IReadOnlyList<IProperty> Properties
		{
			get { return ReadOnlyList<IProperty>.Empty; }
		}

		public virtual IReadOnlyList<IEvent> Events
		{
			get { return ReadOnlyList<IEvent>.Empty; }
		}

		public virtual IReadOnlyList<IType> NestedTypes
		{
			get { return ReadOnlyList<IType>.Empty; }
		}

		public virtual ISignature ResolutionScope
		{
			get { return null; }
		}

		public virtual ISignature Owner
		{
			get { return null; }
		}

		public IModule Module
		{
			get { return _module; }
		}

		public IAssembly Assembly
		{
			get { return _module != null ? _module.Assembly : null; }
		}

		public AssemblyManager AssemblyManager
		{
			get { return _assemblyManager; }
		}

		public abstract TypeElementCode ElementCode
		{
			get;
		}

		public virtual IReadOnlyList<ArrayDimension> ArrayDimensions
		{
			get { return null; }
		}

		EntityType ICodeNode.EntityType
		{
			get { return EntityType.Type; }
		}

		SignatureType ISignature.SignatureType
		{
			get { return SignatureType.Type; }
		}

		ITypeSignature ITypeBase.BaseType
		{
			get { return BaseType; }
		}

		IReadOnlyList<ITypeSignature> ITypeBase.Interfaces
		{
			get { return Interfaces; }
		}

		ITypeSignature ITypeSignature.ElementType
		{
			get { return ElementType; }
		}

		ITypeSignature ITypeSignature.DeclaringType
		{
			get { return DeclaringType; }
		}

		ITypeSignature ITypeSignature.EnclosingType
		{
			get { return EnclosingType; }
		}

		IReadOnlyList<ITypeSignature> ITypeSignature.GenericArguments
		{
			get { return GenericArguments; }
		}

		#endregion

		#region Methods

		public virtual IType GetCustomModifier(out CustomModifierType modifierType)
		{
			modifierType = CustomModifierType.ModOpt;
			return null;
		}

		public virtual ICallSite GetFunctionPointer()
		{
			return null;
		}

		public virtual IType GetGenericArgument(bool isMethod, int position)
		{
			return null;
		}

		public virtual void GetGenericParameter(out bool isMethod, out int position)
		{
			isMethod = false;
			position = -1;
		}

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		ITypeSignature ITypeSignature.GetCustomModifier(out CustomModifierType modifierType)
		{
			return GetCustomModifier(out modifierType);
		}

		IMethodSignature ITypeSignature.GetFunctionPointer()
		{
			return GetFunctionPointer();
		}

		internal IType Intern()
		{
			IType type = this;
			_assemblyManager.InternNode<IType>(ref type);

			return type;
		}

		#endregion
	}
}
