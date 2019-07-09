using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedArrayType : ReferencedType
	{
		private IType _elementType;
		private IReadOnlyList<ArrayDimension> _dimensions;

		internal ReferencedArrayType(IType elementType, IReadOnlyList<ArrayDimension> dimensions)
			: base(elementType)
		{
			_elementType = elementType;
			_dimensions = dimensions;
		}

		public override string Name
		{
			get
			{
				string name = _elementType.Name;
				if (string.IsNullOrEmpty(name))
					return null;

				var builder = new StringBuilder();
				builder.Append(name);

				builder.Append("[");

				for (int i = 0; i < _dimensions.Count; i++)
				{
					builder.Append(",");
				}

				builder.Append("]");

				return builder.ToString();
			}
		}

		public override string Namespace
		{
			get { return _elementType.Namespace; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.Array; }
		}

		public override IType ElementType
		{
			get { return _elementType; }
		}

		public override IReadOnlyList<ArrayDimension> ArrayDimensions
		{
			get { return _dimensions; }
		}
	}
}
