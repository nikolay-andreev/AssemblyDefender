using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class CustomModifier : TypeSignature
	{
		#region Fields

		private TypeSignature _modifier;
		private CustomModifierType _modifierType;
		private TypeSignature _elementType;

		#endregion

		#region Ctors

		private CustomModifier()
		{
		}

		public CustomModifier(TypeSignature elementType, TypeSignature modifier, CustomModifierType modifierType)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");

			_elementType = elementType;
			_modifier = modifier;
			_modifierType = modifierType;
		}

		#endregion

		#region Properties

		public override string Name
		{
			get { return _elementType.Name; }
		}

		public override string Namespace
		{
			get { return _elementType.Namespace; }
		}

		public TypeSignature Modifier
		{
			get { return _modifier; }
		}

		public CustomModifierType ModifierType
		{
			get { return _modifierType; }
		}

		public override TypeReference EnclosingType
		{
			get { return _elementType.EnclosingType; }
		}

		public override TypeReference DeclaringType
		{
			get { return _elementType.DeclaringType; }
		}

		public override Signature ResolutionScope
		{
			get { return _elementType.ResolutionScope; }
		}

		public override Signature Owner
		{
			get { return _elementType.Owner; }
		}

		public override TypeSignature ElementType
		{
			get { return _elementType; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.CustomModifier; }
		}

		#endregion

		#region Methods

		public override TypeSignature GetCustomModifier(out CustomModifierType modifierType)
		{
			modifierType = _modifierType;
			return _modifier;
		}

		public override bool GetSize(Module module, out int size)
		{
			return _elementType.GetSize(module, out size);
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _modifier);
			module.AddSignature(ref _elementType);
		}

		#endregion

		#region Static

		internal static CustomModifier LoadModOpt(IBinaryAccessor accessor, Module module)
		{
			return LoadMod(accessor, module, CustomModifierType.ModOpt);
		}

		internal static CustomModifier LoadModReq(IBinaryAccessor accessor, Module module)
		{
			return LoadMod(accessor, module, CustomModifierType.ModReq);
		}

		private static CustomModifier LoadMod(IBinaryAccessor accessor, Module module, CustomModifierType modType)
		{
			int token = MetadataToken.DecompressTypeDefOrRef(accessor.ReadCompressedInteger());

			var modifier = Load(module, token);

			var elementType = Load(accessor, module);

			return new CustomModifier(elementType, modifier, modType);
		}

		#endregion
	}
}
