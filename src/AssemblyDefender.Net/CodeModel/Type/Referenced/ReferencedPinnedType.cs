using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedPinnedType : ReferencedType
	{
		private IType _elementType;

		internal ReferencedPinnedType(IType elementType)
			: base(elementType)
		{
			_elementType = elementType;
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
			get { return TypeElementCode.Pinned; }
		}

		public override IType ElementType
		{
			get { return _elementType; }
		}
	}
}
