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
	/// Fixed-size system string of size Length bytes. Applicable to field marshaling only.
	/// </summary>
	public class ByValTStrMarshalType : MarshalType
	{
		#region Fields

		private int _length;

		#endregion

		#region Ctors

		internal ByValTStrMarshalType(CodeNode parent)
			: base(parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Indicates the the number of characters (not bytes) in a string to import.
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
			get { return UnmanagedType.ByValTStr; }
		}

		#endregion

		#region Methods

		public override void CopyTo(MarshalType copy)
		{
			CopyTo((ByValTStrMarshalType)copy);
		}

		public void CopyTo(ByValTStrMarshalType copy)
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
