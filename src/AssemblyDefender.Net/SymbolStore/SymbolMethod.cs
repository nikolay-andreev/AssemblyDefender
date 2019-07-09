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
	public class SymbolMethod : ISymbolMethod
	{
		#region Fields

		private ISymUnmanagedMethod _unmanaged;

		#endregion

		#region Ctors

		public SymbolMethod(ISymUnmanagedMethod unmanaged)
		{
			if (unmanaged == null)
				throw new ArgumentNullException("unmanaged");

			_unmanaged = unmanaged;
		}

		#endregion

		#region Properties

		public ISymUnmanagedMethod Unmanaged
		{
			get { return _unmanaged; }
		}

		#endregion

		#region ISymbolMethod Members

		public ISymbolScope RootScope
		{
			get
			{
				ISymUnmanagedScope unmanagedScope;
				HRESULT.ThrowOnFailure(_unmanaged.GetRootScope(out unmanagedScope));

				return new SymbolScope(unmanagedScope);
			}
		}

		public int SequencePointCount
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetSequencePointCount(out value));

				return value;
			}
		}

		public SymbolToken Token
		{
			get
			{
				int token;
				HRESULT.ThrowOnFailure(_unmanaged.GetToken(out token));

				return new SymbolToken(token);
			}
		}

		public ISymbolNamespace GetNamespace()
		{
			ISymUnmanagedNamespace unmanagedNamespace;
			HRESULT.ThrowOnFailure(_unmanaged.GetNamespace(out unmanagedNamespace));

			return new SymbolNamespace(unmanagedNamespace);
		}

		public int GetOffset(ISymbolDocument document, int line, int column)
		{
			var symDocument = document as SymbolDocument;
			if (symDocument == null)
			{
				throw new InvalidOperationException();
			}

			int value;
			HRESULT.ThrowOnFailure(_unmanaged.GetOffset(symDocument.Unmanaged, line, column, out value));

			return value;
		}

		public ISymbolVariable[] GetParameters()
		{
			int varCount;
			HRESULT.ThrowOnFailure(_unmanaged.GetParameters(0, out varCount, null));

			var unmanagedVars = new ISymUnmanagedVariable[varCount];
			HRESULT.ThrowOnFailure(_unmanaged.GetParameters(unmanagedVars.Length, out varCount, unmanagedVars));

			var variables = new ISymbolVariable[varCount];
			for (int i = 0; i < varCount; i++)
			{
				variables[i] = new SymbolVariable(unmanagedVars[i]);
			}

			return variables;
		}

		public int[] GetRanges(ISymbolDocument document, int line, int column)
		{
			var symDocument = document as SymbolDocument;
			if (symDocument == null)
			{
				throw new InvalidOperationException();
			}

			int size;
			HRESULT.ThrowOnFailure(_unmanaged.GetRanges(symDocument.Unmanaged, line, column, 0, out size, null));

			int[] ranges = new int[size];
			HRESULT.ThrowOnFailure(_unmanaged.GetRanges(symDocument.Unmanaged, line, column, ranges.Length, out size, ranges));

			return ranges;
		}

		public ISymbolScope GetScope(int offset)
		{
			ISymUnmanagedScope unmanagedScope;
			HRESULT.ThrowOnFailure(_unmanaged.GetScopeFromOffset(offset, out unmanagedScope));

			return new SymbolScope(unmanagedScope);
		}

		public void GetSequencePoints(int[] offsets, ISymbolDocument[] documents, int[] lines, int[] columns, int[] endLines, int[] endColumns)
		{
			int size = documents.Length;

			var unmanagedDocs = new ISymUnmanagedDocument[size];
			HRESULT.ThrowOnFailure(_unmanaged.GetSequencePoints(size, out size, offsets, unmanagedDocs, lines, columns, endLines, endColumns));

			for (int i = 0; i < size; i++)
			{
				documents[i] = new SymbolDocument(unmanagedDocs[i]);
			}
		}

		public bool GetSourceStartEnd(ISymbolDocument[] docs, int[] lines, int[] columns)
		{
			var unmanagedDocs = new ISymUnmanagedDocument[docs.Length];
			for (int i = 0; i < docs.Length; i++)
			{
				docs[i] = new SymbolDocument(unmanagedDocs[i]);
			}

			bool value;
			HRESULT.ThrowOnFailure(_unmanaged.GetSourceStartEnd(unmanagedDocs, lines, columns, out value));

			return value;
		}

		#endregion
	}
}
