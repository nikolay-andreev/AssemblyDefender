using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedCustomModifier : ReferencedType
	{
		private IType _elementType;
		private IType _modifier;
		private CustomModifierType _modifierType;

		internal ReferencedCustomModifier(IType elementType, IType modifier, CustomModifierType modifierType)
			: base(elementType)
		{
			_elementType = elementType;
			_modifier = modifier;
			_modifierType = modifierType;
		}

		public override string Name
		{
			get { return _elementType.Name; }
		}

		public override string Namespace
		{
			get { return _elementType.Namespace; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.CustomModifier; }
		}

		public override IType ElementType
		{
			get { return _elementType; }
		}

		public override IType GetCustomModifier(out CustomModifierType modifierType)
		{
			modifierType = _modifierType;
			return _modifier;
		}
	}
}
