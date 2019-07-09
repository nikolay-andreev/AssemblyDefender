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
	public class SymbolReader : ISymbolReader
	{
		#region Fields

		private ISymUnmanagedReader _unmanaged;

		#endregion

		#region Ctors

		public SymbolReader(ISymUnmanagedReader unmanaged)
		{
			if (unmanaged == null)
				throw new ArgumentNullException("unmanaged");

			_unmanaged = unmanaged;
		}

		#endregion

		#region Properties

		public ISymUnmanagedReader Unmanaged
		{
			get { return _unmanaged; }
		}

		#endregion

		#region ISymbolReader Members

		public SymbolToken UserEntryPoint
		{
			get
			{
				int token;
				HRESULT.ThrowOnFailure(_unmanaged.GetUserEntryPoint(out token));

				return new SymbolToken(token);
			}
		}

		public ISymbolDocument GetDocument(string url, Guid language, Guid languageVendor, Guid documentType)
		{
			ISymUnmanagedDocument unmanagedDocument;
			HRESULT.ThrowOnFailure(_unmanaged.GetDocument(url, language, languageVendor, documentType, out unmanagedDocument));

			return new SymbolDocument(unmanagedDocument);
		}

		public ISymbolDocument[] GetDocuments()
		{
			int docCount;
			HRESULT.ThrowOnFailure(_unmanaged.GetDocuments(0, out docCount, null));

			var unmanagedDocuments = new ISymUnmanagedDocument[docCount];
			HRESULT.ThrowOnFailure(_unmanaged.GetDocuments(unmanagedDocuments.Length, out docCount, unmanagedDocuments));

			var documents = new ISymbolDocument[docCount];
			for (int i = 0; i < docCount; i++)
			{
				documents[i] = new SymbolDocument(unmanagedDocuments[i]);
			}

			return documents;
		}

		public ISymbolVariable[] GetGlobalVariables()
		{
			int varCount;
			HRESULT.ThrowOnFailure(_unmanaged.GetGlobalVariables(0, out varCount, null));

			var unmanagedVars = new ISymUnmanagedVariable[varCount];
			HRESULT.ThrowOnFailure(_unmanaged.GetGlobalVariables(unmanagedVars.Length, out varCount, unmanagedVars));

			var variables = new ISymbolVariable[varCount];
			for (int i = 0; i < varCount; i++)
			{
				variables[i] = new SymbolVariable(unmanagedVars[i]);
			}

			return variables;
		}

		public ISymbolMethod GetMethod(SymbolToken method, int version)
		{
			ISymUnmanagedMethod unmanagedMethod;
			HRESULT.ThrowOnFailure(_unmanaged.GetMethodByVersion(method.GetToken(), version, out unmanagedMethod));

			return new SymbolMethod(unmanagedMethod);
		}

		public ISymbolMethod GetMethod(SymbolToken method)
		{
			ISymUnmanagedMethod unmanagedMethod;
			HRESULT.ThrowOnFailure(_unmanaged.GetMethod(method.GetToken(), out unmanagedMethod));

			return new SymbolMethod(unmanagedMethod);
		}

		public ISymbolMethod GetMethodFromDocumentPosition(ISymbolDocument document, int line, int column)
		{
			var symDocument = document as SymbolDocument;
			if (symDocument == null)
			{
				throw new InvalidOperationException();
			}

			ISymUnmanagedMethod unmanagedMethod;
			HRESULT.ThrowOnFailure(_unmanaged.GetMethodFromDocumentPosition(symDocument.Unmanaged, line, column, out unmanagedMethod));

			return new SymbolMethod(unmanagedMethod);
		}

		public ISymbolNamespace[] GetNamespaces()
		{
			int namespaceCount;
			HRESULT.ThrowOnFailure(_unmanaged.GetGlobalVariables(0, out namespaceCount, null));

			var unmanagedNamespaces = new ISymUnmanagedNamespace[namespaceCount];
			HRESULT.ThrowOnFailure(_unmanaged.GetNamespaces(unmanagedNamespaces.Length, out namespaceCount, unmanagedNamespaces));

			var namespaces = new ISymbolNamespace[namespaceCount];
			for (int i = 0; i < namespaceCount; i++)
			{
				namespaces[i] = new SymbolNamespace(unmanagedNamespaces[i]);
			}

			return namespaces;
		}

		public byte[] GetSymAttribute(SymbolToken parent, string name)
		{
			int buffSize;
			HRESULT.ThrowOnFailure(_unmanaged.GetSymAttribute(parent.GetToken(), name, 0, out buffSize, null));

			byte[] buff = new byte[buffSize];

			HRESULT.ThrowOnFailure(_unmanaged.GetSymAttribute(parent.GetToken(), name, buff.Length, out buffSize, buff));

			return buff;
		}

		public ISymbolVariable[] GetVariables(SymbolToken parent)
		{
			int varCount;
			HRESULT.ThrowOnFailure(_unmanaged.GetVariables(parent.GetToken(), 0, out varCount, null));

			var unmanagedVars = new ISymUnmanagedVariable[varCount];
			HRESULT.ThrowOnFailure(_unmanaged.GetVariables(parent.GetToken(), unmanagedVars.Length, out varCount, unmanagedVars));

			var variables = new ISymbolVariable[varCount];
			for (int i = 0; i < varCount; i++)
			{
				variables[i] = new SymbolVariable(unmanagedVars[i]);
			}

			return variables;
		}

		#endregion
	}
}
