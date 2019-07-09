using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	[Serializable]
	public struct ConvertEnumerator<TTarget, TSource> : IEnumerable<TTarget>, IEnumerator<TTarget>
	{
		#region Fields

		private TTarget _current;
		private IEnumerator<TSource> _inner;
		private Func<TSource, TTarget> _converter;

		#endregion

		#region Ctors

		public ConvertEnumerator(IEnumerator<TSource> inner, Func<TSource, TTarget> converter)
		{
			_inner = inner;
			_converter = converter;
			_current = default(TTarget);
		}

		#endregion

		#region Properties

		public TTarget Current
		{
			get { return _current; }
		}

		object IEnumerator.Current
		{
			get { return _current; }
		}

		#endregion

		#region Methods

		public bool MoveNext()
		{
			if (!_inner.MoveNext())
				return false;

			_current = _converter(_inner.Current);
			return true;
		}

		public void Reset()
		{
			_inner.Reset();
			_current = default(TTarget);
		}

		public void Dispose()
		{
			if (_inner != null)
			{
				_inner.Dispose();
				_inner = null;
			}

			_converter = null;
			_current = default(TTarget);
		}

		IEnumerator<TTarget> IEnumerable<TTarget>.GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		#endregion
	}
}
