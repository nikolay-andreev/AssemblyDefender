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
	public class SymbolNamespace : ISymbolNamespace
	{
		#region Fields

		private ISymUnmanagedNamespace _unmanaged;

		#endregion

		#region Ctors

		public SymbolNamespace(ISymUnmanagedNamespace unmanaged)
		{
			if (unmanaged == null)
				throw new ArgumentNullException("unmanaged");

			_unmanaged = unmanaged;
		}

		#endregion

		#region Properties

		public ISymUnmanagedNamespace Unmanaged
		{
			get { return _unmanaged; }
		}

		#endregion

		#region ISymbolNamespace Members

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

		public ISymbolVariable[] GetVariables()
		{
			int size;
			HRESULT.ThrowOnFailure(_unmanaged.GetVariables(0, out size, null));

			var unmanagedVars = new ISymUnmanagedVariable[size];

			HRESULT.ThrowOnFailure(_unmanaged.GetVariables(unmanagedVars.Length, out size, unmanagedVars));

			var variables = new ISymbolVariable[size];
			for (int i = 0; i < size; i++)
			{
				variables[i] = new SymbolVariable(unmanagedVars[i]);
			}

			return variables;
		}

		#endregion
	}
}
