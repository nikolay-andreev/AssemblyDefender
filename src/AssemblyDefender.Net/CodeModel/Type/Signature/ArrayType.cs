using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class ArrayType : TypeSignature
	{
		#region Fields

		private TypeSignature _elementType;
		private IReadOnlyList<ArrayDimension> _dimensions;

		#endregion

		#region Ctors

		private ArrayType()
		{
		}

		public ArrayType(TypeSignature elementType)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");

			_elementType = elementType;
			_dimensions = ReadOnlyList<ArrayDimension>.Empty;
		}

		public ArrayType(TypeSignature elementType, int rank)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");

			_elementType = elementType;
			_dimensions = ReadOnlyList<ArrayDimension>.Create(new ArrayDimension[rank > 0 ? rank : 0]);
		}

		public ArrayType(TypeSignature elementType, ArrayDimension[] dimensions)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");

			_elementType = elementType;
			_dimensions = ReadOnlyList<ArrayDimension>.Create(dimensions);
		}

		public ArrayType(TypeSignature elementType, IReadOnlyList<ArrayDimension> dimensions)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");

			_elementType = elementType;
			_dimensions = dimensions ?? ReadOnlyList<ArrayDimension>.Empty;
		}

		#endregion

		#region Properties

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

		public override TypeSignature ElementType
		{
			get { return _elementType; }
		}

		public override IReadOnlyList<ArrayDimension> ArrayDimensions
		{
			get { return _dimensions; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.Array; }
		}

		#endregion

		#region Methods

		public override bool GetSize(Module module, out int size)
		{
			size = 0;
			return false;
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _elementType);
		}

		#endregion

		#region Static

		internal static ArrayType LoadArray(IBinaryAccessor accessor, Module module)
		{
			var elementType = Load(accessor, module);

			var dimensions = LoadDimensions(accessor);

			return new ArrayType(elementType, dimensions);
		}

		internal static ArrayType LoadSzArray(IBinaryAccessor accessor, Module module)
		{
			var elementType = Load(accessor, module);

			return new ArrayType(elementType);
		}

		private static ArrayDimension[] LoadDimensions(IBinaryAccessor accessor)
		{
			int rank = accessor.ReadCompressedInteger();

			int numSizes = accessor.ReadCompressedInteger();
			int[] sizes = new int[numSizes];
			for (int i = 0; i < numSizes; i++)
			{
				sizes[i] = accessor.ReadCompressedInteger();
			}

			int numLoBounds = accessor.ReadCompressedInteger();
			int[] loBounds = new int[numLoBounds];
			for (int i = 0; i < numLoBounds; i++)
			{
				loBounds[i] = accessor.ReadCompressedInteger();
			}

			var dimensions = new ArrayDimension[rank];

			for (int i = 0; i < rank; i++)
			{
				int? lowerBound = null;
				if (loBounds.Length > i)
				{
					lowerBound = loBounds[i];
				}

				int? upperBound = null;
				if (sizes.Length > i)
				{
					upperBound = sizes[i] + lowerBound - 1;
				}

				dimensions[i] = new ArrayDimension(lowerBound, upperBound);
			}

			return dimensions;
		}

		#endregion
	}
}
