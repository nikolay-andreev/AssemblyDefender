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
	public class SymbolDocumentWriter : ISymbolDocumentWriter
	{
		#region Fields

		private ISymUnmanagedDocumentWriter _unmanaged;

		#endregion

		#region Ctors

		public SymbolDocumentWriter(ISymUnmanagedDocumentWriter unmanaged)
		{
			if (unmanaged == null)
				throw new ArgumentNullException("unmanaged");

			_unmanaged = unmanaged;
		}

		#endregion

		#region Properties

		public ISymUnmanagedDocumentWriter Unmanaged
		{
			get { return _unmanaged; }
		}

		#endregion

		#region ISymbolDocumentWriter Members

		public void SetCheckSum(Guid algorithmId, byte[] checkSum)
		{
			HRESULT.ThrowOnFailure(_unmanaged.SetCheckSum(algorithmId, checkSum.Length, checkSum));
		}

		public void SetSource(byte[] source)
		{
			HRESULT.ThrowOnFailure(_unmanaged.SetSource(source.Length, source));
		}

		#endregion
	}
}
