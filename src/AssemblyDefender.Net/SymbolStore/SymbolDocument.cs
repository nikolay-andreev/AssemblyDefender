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
	public class SymbolDocument : ISymbolDocument
	{
		#region Fields

		private ISymUnmanagedDocument _unmanaged;

		#endregion

		#region Ctors

		public SymbolDocument(ISymUnmanagedDocument unmanaged)
		{
			if (unmanaged == null)
				throw new ArgumentNullException("unmanaged");

			_unmanaged = unmanaged;
		}

		#endregion

		#region Properties

		public ISymUnmanagedDocument Unmanaged
		{
			get { return _unmanaged; }
		}

		#endregion

		#region ISymbolDocument Members

		public Guid CheckSumAlgorithmId
		{
			get
			{
				Guid value;
				HRESULT.ThrowOnFailure(_unmanaged.GetCheckSumAlgorithmId(out value));

				return value;
			}
		}

		public Guid DocumentType
		{
			get
			{
				Guid value;
				HRESULT.ThrowOnFailure(_unmanaged.GetDocumentType(out value));

				return value;
			}
		}

		public bool HasEmbeddedSource
		{
			get
			{
				bool value;
				HRESULT.ThrowOnFailure(_unmanaged.HasEmbeddedSource(out value));

				return value;
			}
		}

		public Guid Language
		{
			get
			{
				Guid value;
				HRESULT.ThrowOnFailure(_unmanaged.GetLanguage(out value));

				return value;
			}
		}

		public Guid LanguageVendor
		{
			get
			{
				Guid value;
				HRESULT.ThrowOnFailure(_unmanaged.GetLanguageVendor(out value));

				return value;
			}
		}

		public int SourceLength
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetSourceLength(out value));

				return value;
			}
		}

		public string URL
		{
			get
			{
				int size;
				HRESULT.ThrowOnFailure(_unmanaged.GetURL(0, out size, null));

				char[] urlChars = new char[size];

				HRESULT.ThrowOnFailure(_unmanaged.GetURL(urlChars.Length, out size, urlChars));

				return new string(urlChars);
			}
		}

		public int FindClosestLine(int line)
		{
			int value;
			HRESULT.ThrowOnFailure(_unmanaged.FindClosestLine(line, out value));

			return value;
		}

		public byte[] GetCheckSum()
		{
			int size;
			HRESULT.ThrowOnFailure(_unmanaged.GetCheckSum(0, out size, null));

			byte[] data = new byte[size];

			HRESULT.ThrowOnFailure(_unmanaged.GetCheckSum(data.Length, out size, data));

			return data;
		}

		public byte[] GetSourceRange(int startLine, int startColumn, int endLine, int endColumn)
		{
			int size;
			HRESULT.ThrowOnFailure(_unmanaged.GetSourceRange(startLine, startColumn, endLine, endColumn, 0, out size, null));

			byte[] data = new byte[size];

			HRESULT.ThrowOnFailure(_unmanaged.GetSourceRange(startLine, startColumn, endLine, endColumn, data.Length, out size, data));

			return data;
		}

		#endregion
	}
}
