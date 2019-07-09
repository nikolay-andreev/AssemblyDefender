using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class ReferencedMethodParameterCollection : IReadOnlyList<IMethodParameter>, IReadOnlyList<ITypeSignature>
	{
		#region Fields

		private IMethodParameter[] _items;

		#endregion

		#region Ctors

		internal ReferencedMethodParameterCollection(IMethodParameter[] items)
		{
			_items = items;
		}

		#endregion

		#region Properties

		public IMethodParameter this[int index]
		{
			get { return _items[index]; }
		}

		ITypeSignature IReadOnlyList<ITypeSignature>.this[int index]
		{
			get { return _items[index].Type; }
		}

		public int Count
		{
			get { return _items.Length; }
		}

		#endregion

		#region Methods

		public IEnumerator<IMethodParameter> GetEnumerator()
		{
			return new ArrayEnumerator<IMethodParameter>(_items);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<ITypeSignature> IEnumerable<ITypeSignature>.GetEnumerator()
		{
			return new TypeEnumerator(_items);
		}

		#endregion

		#region Nested types

		private struct TypeEnumerator : IEnumerable<ITypeSignature>, IEnumerator<ITypeSignature>
		{
			#region Fields

			private int _count;
			private int _index;
			private IMethodParameter[] _items;

			#endregion

			#region Ctors

			internal TypeEnumerator(IMethodParameter[] items)
			{
				_items = items;
				_count = items.Length;
				_index = -1;
			}

			#endregion

			#region Properties

			public ITypeSignature Current
			{
				get
				{
					if (_index < 0)
						return null;

					return _items[_index].Type;
				}
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			#endregion

			#region Methods

			public bool MoveNext()
			{
				if (_index + 1 == _count)
					return false;

				_index++;
				return true;
			}

			public void Reset()
			{
				_index = -1;
			}

			public void Dispose()
			{
			}

			IEnumerator<ITypeSignature> IEnumerable<ITypeSignature>.GetEnumerator()
			{
				return this;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this;
			}

			#endregion
		}

		#endregion




	}
}
