using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class PrimitiveMarshalType : MarshalType
	{
		#region Fields

		private int? _iidParameterIndex;
		private UnmanagedType _unmanagedType;

		#endregion

		#region Ctors

		internal PrimitiveMarshalType(CodeNode parent, UnmanagedType unmanagedType)
			: base(parent)
		{
			_unmanagedType = unmanagedType;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Specifies the parameter index of the unmanaged iid_is attribute used by COM.
		/// </summary>
		public int? IIdParameterIndex
		{
			get { return _iidParameterIndex; }
			set
			{
				_iidParameterIndex = value;
				OnChanged();
			}
		}

		public override UnmanagedType UnmanagedType
		{
			get { return _unmanagedType; }
		}

		#endregion

		#region Methods

		public override void CopyTo(MarshalType copy)
		{
			CopyTo((PrimitiveMarshalType)copy);
		}

		public void CopyTo(PrimitiveMarshalType copy)
		{
			copy._iidParameterIndex = _iidParameterIndex;
			copy._unmanagedType = _unmanagedType;
		}

		protected internal override void Load(IBinaryAccessor accessor)
		{
			if (accessor.IsEof())
				return;

			if (_unmanagedType == UnmanagedType.IUnknown ||
				_unmanagedType == UnmanagedType.IDispatch ||
				_unmanagedType == UnmanagedType.Interface)
			{
				_iidParameterIndex = accessor.ReadCompressedInteger();
			}
		}

		#endregion
	}
}
