using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class BaseRelocationEntry : ICloneable
	{
		#region Fields

		private BaseRelocationType _type;
		private uint _offset;
		internal BaseRelocationBlock _parent;

		#endregion

		#region Ctors

		public BaseRelocationEntry()
		{
		}

		public BaseRelocationEntry(BaseRelocationType type, uint offset)
		{
			_type = type;
			_offset = offset;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Stored in the high 4 bits of the WORD, a value that indicates the type of base relocation to be applied.
		/// </summary>
		public BaseRelocationType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		/// <summary>
		/// Stored in the remaining 12 bits of the WORD, an offset from the starting address that was specified in the
		/// Page RVA field for the block. This offset specifies where the base relocation is to be applied.
		/// </summary>
		public uint Offset
		{
			get { return _offset; }
			set { _offset = value; }
		}

		public BaseRelocationBlock Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public BaseRelocationEntry Clone()
		{
			var copy = new BaseRelocationEntry();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(BaseRelocationEntry copy)
		{
			copy._type = _type;
			copy._offset = _offset;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}
