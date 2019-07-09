using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Net.SymbolStore.UnmanagedApi;
using SymbolStoreGuids = AssemblyDefender.Net.SymbolStore.UnmanagedApi.SymbolStoreNative;

namespace AssemblyDefender.Net.SymbolStore
{
	[ComVisible(true)]
	public class SymbolBinder : ISymbolBinder, ISymbolBinder1
	{
		#region Fields

		private ISymUnmanagedBinder _unmanaged;

		#endregion

		#region Ctors

		public SymbolBinder()
		{
			Guid clsid = SymbolStoreGuids.CLSID_CorSymBinderGuid;
			Guid iid = typeof(ISymUnmanagedBinder).GUID;
			_unmanaged = CoreUtils.CoCreateInstance<ISymUnmanagedBinder>(clsid, iid, true);
		}

		public SymbolBinder(ISymUnmanagedBinder unmanaged)
		{
			if (unmanaged == null)
				throw new ArgumentNullException("unmanaged");

			_unmanaged = unmanaged;
		}

		#endregion

		#region Properties

		public ISymUnmanagedBinder Unmanaged
		{
			get { return _unmanaged; }
		}

		#endregion

		#region ISymbolBinder Members

		public ISymbolReader GetReader(int importer, string filename, string searchPath)
		{
			IntPtr ptr = new IntPtr(importer);
			return GetReader(ptr, filename, searchPath);
		}

		#endregion

		#region ISymbolBinder1 Members

		public ISymbolReader GetReader(IntPtr importer, string filename, string searchPath)
		{
			object objImporter = Marshal.GetObjectForIUnknown(importer);

			ISymUnmanagedReader unmanagedReader;
			HRESULT.ThrowOnFailure(_unmanaged.GetReaderForFile(objImporter, filename, searchPath, out unmanagedReader));

			return new SymbolReader(unmanagedReader);
		}

		#endregion
	}
}
