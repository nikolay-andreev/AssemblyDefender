using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;

namespace AssemblyDefender.Net.Fusion
{
	public class AssemblyEnumerator : IEnumerable<AssemblyName>, IEnumerator<AssemblyName>
	{
		#region Fields

		private AssemblyName _current;
		private UnmanagedApi.IAssemblyEnum _assemblyEnum;

		#endregion

		#region Ctors

		public AssemblyEnumerator(UnmanagedApi.IAssemblyEnum assemblyEnum)
		{
			if (assemblyEnum == null)
				throw new ArgumentNullException("assemblyEnum");

			_assemblyEnum = assemblyEnum;
		}

		#endregion

		#region Properties

		public AssemblyName Current
		{
			get { return _current; }
		}

		object IEnumerator.Current
		{
			get { return _current; }
		}

		public UnmanagedApi.IAssemblyEnum IAssemblyEnum
		{
			get { return _assemblyEnum; }
		}

		#endregion

		#region Methods

		public bool MoveNext()
		{
			UnmanagedApi.IAssemblyName name;
			HRESULT.ThrowOnFailure(_assemblyEnum.GetNextAssembly(IntPtr.Zero, out name, 0));

			if (name != null)
				_current = new AssemblyName(name);
			else
				_current = null;

			return _current != null;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
		}

		public IEnumerator<AssemblyName> GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
