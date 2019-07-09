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
	/// Specifies the custom marshaler class when used with MarshalType or MarshalTypeRef. The MarshalCookie field
	/// can be used to pass additional information to the custom marshaler. You can use this member on any reference type.
	/// </summary>
	public class CustomMarshalType : MarshalType
	{
		#region Fields

		private string _guidString;
		private string _unmanagedTypeString;
		private string _typeName;
		private string _cookie;

		#endregion

		#region Ctors

		internal CustomMarshalType(CodeNode parent)
			: base(parent)
		{
		}

		#endregion

		#region Properties

		public string GuidString
		{
			get { return _guidString; }
			set
			{
				_guidString = value;
				OnChanged();
			}
		}

		public string UnmanagedTypeString
		{
			get { return _unmanagedTypeString; }
			set
			{
				_unmanagedTypeString = value;
				OnChanged();
			}
		}

		public string TypeName
		{
			get { return _typeName; }
			set
			{
				_typeName = value;
				OnChanged();
			}
		}

		public string Cookie
		{
			get { return _cookie; }
			set
			{
				_cookie = value;
				OnChanged();
			}
		}

		public override UnmanagedType UnmanagedType
		{
			get { return UnmanagedType.CustomMarshaler; }
		}

		#endregion

		#region Methods

		public override void CopyTo(MarshalType copy)
		{
			CopyTo((CustomMarshalType)copy);
		}

		public void CopyTo(CustomMarshalType copy)
		{
			copy._guidString = _guidString;
			copy._unmanagedTypeString = _unmanagedTypeString;
			copy._typeName = _typeName;
			copy._cookie = _cookie;
		}

		protected internal override void Load(IBinaryAccessor accessor)
		{
			if (accessor.IsEof())
				return;

			int length = accessor.ReadCompressedInteger();
			if (length > 0)
				_guidString = accessor.ReadString(length, Encoding.UTF8);

			if (accessor.IsEof())
				return;

			length = accessor.ReadCompressedInteger();
			if (length > 0)
				_unmanagedTypeString = accessor.ReadString(length, Encoding.UTF8);

			if (accessor.IsEof())
				return;

			length = accessor.ReadCompressedInteger();
			if (length > 0)
				_typeName = accessor.ReadString(length, Encoding.UTF8);

			if (accessor.IsEof())
				return;

			length = accessor.ReadCompressedInteger();
			if (length > 0)
				_cookie = accessor.ReadString(length, Encoding.UTF8);
		}

		#endregion
	}
}
