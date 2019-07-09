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
	public class SymbolScope : ISymbolScope
	{
		#region Fields

		private ISymUnmanagedScope _unmanaged;

		#endregion

		#region Ctors

		public SymbolScope(ISymUnmanagedScope unmanaged)
		{
			if (unmanaged == null)
				throw new ArgumentNullException("unmanaged");

			_unmanaged = unmanaged;
		}

		#endregion

		#region Properties

		public ISymUnmanagedScope Unmanaged
		{
			get { return _unmanaged; }
		}

		#endregion

		#region ISymbolScope Members

		public int EndOffset
		{
			get
			{
				int value;
				HRESULT.ThrowOnFailure(_unmanaged.GetEndOffset(out value));

				return value;
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

		public ISymbolMethod Method
		{
			get
			{
				ISymUnmanagedMethod unmanagedMethod;
				HRESULT.ThrowOnFailure(_unmanaged.GetMethod(out unmanagedMethod));

				return new SymbolMethod(unmanagedMethod);
			}
		}

		public ISymbolScope Parent
		{
			get
			{
				ISymUnmanagedScope unmanagedParent;
				HRESULT.ThrowOnFailure(_unmanaged.GetParent(out unmanagedParent));

				return new SymbolScope(unmanagedParent);
			}
		}

		public ISymbolScope[] GetChildren()
		{
			int size;
			HRESULT.ThrowOnFailure(_unmanaged.GetChildren(0, out size, null));

			var unmanagedScopes = new ISymUnmanagedScope[size];

			HRESULT.ThrowOnFailure(_unmanaged.GetChildren(unmanagedScopes.Length, out size, unmanagedScopes));

			var scopes = new ISymbolScope[size];
			for (int i = 0; i < size; i++)
			{
				scopes[i] = new SymbolScope(unmanagedScopes[i]);
			}

			return scopes;
		}

		public ISymbolVariable[] GetLocals()
		{
			int size;
			HRESULT.ThrowOnFailure(_unmanaged.GetLocals(0, out size, null));

			var unmanagedVars = new ISymUnmanagedVariable[size];

			HRESULT.ThrowOnFailure(_unmanaged.GetLocals(unmanagedVars.Length, out size, unmanagedVars));

			var variables = new ISymbolVariable[size];
			for (int i = 0; i < size; i++)
			{
				variables[i] = new SymbolVariable(unmanagedVars[i]);
			}

			return variables;
		}

		public ISymbolNamespace[] GetNamespaces()
		{
			int size;
			HRESULT.ThrowOnFailure(_unmanaged.GetNamespaces(0, out size, null));

			var unmanagedNamespaces = new ISymUnmanagedNamespace[size];

			HRESULT.ThrowOnFailure(_unmanaged.GetNamespaces(unmanagedNamespaces.Length, out size, unmanagedNamespaces));

			var namespaces = new ISymbolNamespace[size];
			for (int i = 0; i < size; i++)
			{
				namespaces[i] = new SymbolNamespace(unmanagedNamespaces[i]);
			}

			return namespaces;
		}

		#endregion
	}
}
