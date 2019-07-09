using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Fixed-size array, of size Length bytes.
	/// </summary>
	public class ByValArrayMarshalType : MarshalType
	{
		#region Fields

		private int _length;

		#endregion

		#region Ctors

		internal ByValArrayMarshalType(CodeNode parent)
			: base(parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Indicates the number of elements in the array.
		/// </summary>
		public int Length
		{
			get { return _length; }
			set
			{
				_length = value;
				OnChanged();
			}
		}

		public override UnmanagedType UnmanagedType
		{
			get { return UnmanagedType.ByValArray; }
		}

		#endregion

		#region Methods

		public override void CopyTo(MarshalType copy)
		{
			CopyTo((ByValArrayMarshalType)copy);
		}

		public void CopyTo(ByValArrayMarshalType copy)
		{
			copy._length = _length;
		}

		protected internal override void Load(IBinaryAccessor accessor)
		{
			if (accessor.IsEof())
				return;

			_length = accessor.ReadCompressedInteger();
		}

		#endregion
	}
}
