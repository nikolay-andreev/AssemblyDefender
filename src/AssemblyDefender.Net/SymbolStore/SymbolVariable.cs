using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Net.SymbolStore.UnmanagedApi;

namespace AssemblyDefender.Net.SymbolStore
{
	[ComVisible(true)]
	public class SymbolVariable : ISymbolVariable
	{
		#region Fields

		private ISymUnmanagedVariable _unmanaged;

		#endregion

		#region Ctors

		public SymbolVariable(ISymUnmanagedVariable unmanaged)
		{
			if (unmanaged == null)
				throw new ArgumentNullException("unmanaged");

			_unmanaged = unmanaged;
		}

		#endregion

		#region Properties

		public ISymUnmanagedVariable Unmanaged
		{
			get { return _unmanaged; }
		}

		#endregion

		#region ISymbolVariable Members

		public string Name
		{
			get
			{
				int size;
				HRESULT.ThrowOnFailure(_unmanaged.GetName(0, out size, null));

				char[] nameChars = new char[size];

				HRESULT.ThrowOnFailure(_unmanaged.GetName(nameChars.Length, out size, nameChars));

				return new string(nameChars);
			}
		}

		public int StartOffset
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetStartOffset(out value));

				return value;
			}
		}

		public int EndOffset
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetEndOffset(out value));

				return value;
			}
		}

		public SymAddressKind AddressKind
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetAddressKind(out value));

				return (SymAddressKind)value;
			}
		}

		public int AddressField1
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetAddressField1(out value));

				return value;
			}
		}

		public int AddressField2
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetAddressField2(out value));

				return value;
			}
		}

		public int AddressField3
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetAddressField3(out value));

				return value;
			}
		}

		public object Attributes
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetAttributes(out value));

				return value;
			}
		}

		public byte[] GetSignature()
		{
			int size;
			HRESULT.ThrowOnFailure(_unmanaged.GetSignature(0, out size, null));
			byte[] data = new byte[size];
			HRESULT.ThrowOnFailure(_unmanaged.GetSignature(data.Length, out size, data));

			return data;
		}

		#endregion
	}
}
