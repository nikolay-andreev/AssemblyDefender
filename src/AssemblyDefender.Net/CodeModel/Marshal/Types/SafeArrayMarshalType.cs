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
	/// Safe array of type VariantType.
	/// </summary>
	public class SafeArrayMarshalType : MarshalType
	{
		#region Fields

		private UnmanagedVariantType _variantType = UnmanagedVariantType.VT_EMPTY;
		private string _userDefinedSubType;

		#endregion

		#region Ctors

		internal SafeArrayMarshalType(CodeNode parent)
			: base(parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Indicates the element type.
		/// </summary>
		public UnmanagedVariantType VariantType
		{
			get { return _variantType; }
			set
			{
				_variantType = value;
				OnChanged();
			}
		}

		/// <summary>
		/// Indicates the user-defined element type.
		/// </summary>
		public string UserDefinedSubType
		{
			get { return _userDefinedSubType; }
			set
			{
				_userDefinedSubType = value;
				OnChanged();
			}
		}

		public override UnmanagedType UnmanagedType
		{
			get { return UnmanagedType.SafeArray; }
		}

		#endregion

		#region Methods

		public override void CopyTo(MarshalType copy)
		{
			CopyTo((SafeArrayMarshalType)copy);
		}

		public void CopyTo(SafeArrayMarshalType copy)
		{
			copy._variantType = _variantType;
			copy._userDefinedSubType = _userDefinedSubType;
		}

		protected internal override void Load(IBinaryAccessor accessor)
		{
			if (accessor.IsEof())
				return;

			_variantType = (UnmanagedVariantType)accessor.ReadCompressedInteger();

			if (accessor.IsEof())
				return;

			int length = accessor.ReadCompressedInteger();
			if (length > 0)
				_userDefinedSubType = accessor.ReadString(length, Encoding.UTF8);
		}

		#endregion
	}
}
