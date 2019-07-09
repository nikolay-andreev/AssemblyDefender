using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class PinnedType : TypeSignature
	{
		#region Fields

		private TypeSignature _elementType;

		#endregion

		#region Ctors

		private PinnedType()
		{
		}

		public PinnedType(TypeSignature elementType)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");

			_elementType = elementType;
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
			get { return TypeElementCode.Pinned; }
		}

		#endregion

		#region Methods

		public override bool GetSize(Module module, out int size)
		{
			return _elementType.GetSize(module, out size);
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _elementType);
		}

		#endregion

		#region Static

		internal static PinnedType LoadPinned(IBinaryAccessor accessor, Module module)
		{
			var elementType = Load(accessor, module);

			return new PinnedType(elementType);
		}

		#endregion
	}
}
