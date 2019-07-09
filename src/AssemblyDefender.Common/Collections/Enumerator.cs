using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	[Serializable]
	public struct Enumerator<T> : IEnumerator<T>
	{
		#region Fields

		private IEnumerator _enumerator;

		#endregion

		#region Ctors

		public Enumerator(IEnumerator enumerator)
		{
			if (enumerator == null)
				throw new ArgumentNullException("enumerator");

			_enumerator = enumerator;
		}

		#endregion

		#region Properties

		public T Current
		{
			get { return (T)_enumerator.Current; }
		}

		object IEnumerator.Current
		{
			get { return _enumerator.Current; }
		}

		#endregion

		#region Methods

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			_enumerator.Reset();
		}

		public void Dispose()
		{
			((IDisposable)_enumerator).Dispose();
		}

		#endregion
	}
}
