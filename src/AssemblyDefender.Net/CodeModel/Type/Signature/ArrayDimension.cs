using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public struct ArrayDimension
	{
		#region Fields

		private int? _lowerBound;
		private int? _upperBound;

		#endregion

		#region Ctors

		public ArrayDimension(int? lowerBound, int? upperBound)
		{
			_lowerBound = lowerBound;
			_upperBound = upperBound;
		}

		#endregion

		#region Properties

		public bool IsEmpty
		{
			get { return _lowerBound == null && _upperBound == null; }
		}

		public int? LowerBound
		{
			get { return _lowerBound; }
		}

		public int? UpperBound
		{
			get { return _upperBound; }
		}

		#endregion

		#region Methods

		public bool Equals(ArrayDimension other)
		{
			if (other._lowerBound != _lowerBound)
				return false;

			if (other._upperBound != _upperBound)
				return false;

			return true;
		}

		#endregion
	}
}
