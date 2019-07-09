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
	/// Fixed-size array of a native type ArraySubType.
	/// </summary>
	public class LPArrayMarshalType : MarshalType
	{
		#region Fields

		private UnmanagedType _arraySubType = UnmanagedType.Max;
		private int? _arrayLength;
		private int? _lengthParamIndex;
		private int? _iidParameterIndex;

		#endregion

		#region Ctors

		internal LPArrayMarshalType(CodeNode parent)
			: base(parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Specifies the element type of the unmanaged LPArray
		/// </summary>
		public UnmanagedType ArraySubType
		{
			get { return _arraySubType; }
			set
			{
				_arraySubType = value;
				OnChanged();
			}
		}

		/// <summary>
		/// Indicates the number of elements in the fixed-length array.
		/// </summary>
		public int? ArrayLength
		{
			get { return _arrayLength; }
			set
			{
				_arrayLength = value;
				OnChanged();
			}
		}

		/// <summary>
		/// Indicates which parameter contains the count of array elements, much like size_is in COM, and is zero-based.
		/// </summary>
		public int? LengthParamIndex
		{
			get { return _lengthParamIndex; }
			set
			{
				_lengthParamIndex = value;
				OnChanged();
			}
		}

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
			get { return UnmanagedType.LPArray; }
		}

		#endregion

		#region Methods

		public override void CopyTo(MarshalType copy)
		{
			CopyTo((LPArrayMarshalType)copy);
		}

		public void CopyTo(LPArrayMarshalType copy)
		{
			copy._arraySubType = _arraySubType;
			copy._arrayLength = _arrayLength;
			copy._lengthParamIndex = _lengthParamIndex;
			copy._iidParameterIndex = _iidParameterIndex;
		}

		protected internal override void Load(IBinaryAccessor accessor)
		{
			_arraySubType = (UnmanagedType)accessor.ReadByte();

			// The value MAX is used to indicate "no info."
			if (_arraySubType == UnmanagedType.Max)
				return;

			if (accessor.IsEof())
				return;

			if (_arraySubType == UnmanagedType.IUnknown ||
				_arraySubType == UnmanagedType.IDispatch ||
				_arraySubType == UnmanagedType.Interface)
			{
				_iidParameterIndex = accessor.ReadCompressedInteger();
			}

			if (accessor.IsEof())
				return;

			_lengthParamIndex = accessor.ReadCompressedInteger();

			if (accessor.IsEof())
				return;

			_arrayLength = accessor.ReadCompressedInteger();
		}

		#endregion
	}
}
