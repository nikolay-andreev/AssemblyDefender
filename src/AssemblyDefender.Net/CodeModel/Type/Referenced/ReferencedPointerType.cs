using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedPointerType : ReferencedType
	{
		private IType _elementType;

		internal ReferencedPointerType(IType elementType)
			: base(elementType)
		{
			_elementType = elementType;
		}

		public override string Name
		{
			get
			{
				string name = _elementType.Name;
				if (string.IsNullOrEmpty(name))
					return null;

				return name + "*";
			}
		}

		public override string Namespace
		{
			get { return _elementType.Namespace; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.Pointer; }
		}

		public override IType ElementType
		{
			get { return _elementType; }
		}
	}
}
