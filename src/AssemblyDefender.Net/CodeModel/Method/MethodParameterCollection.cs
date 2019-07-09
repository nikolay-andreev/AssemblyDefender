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
	public class MethodParameterCollection : CodeNodeCollection<MethodParameter>, IReadOnlyList<ITypeSignature>
	{
		#region Fields

		private MethodDeclaration _method;

		#endregion

		#region Ctors

		internal MethodParameterCollection(MethodDeclaration method)
			: base(method)
		{
			_method = method;

			if (method.IsNew)
			{
				IsNew = true;
			}
		}

		#endregion

		#region Properties

		public MethodDeclaration Method
		{
			get { return _method; }
		}

		#endregion

		#region Methods

		public void CopyTo(MethodParameterCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override MethodParameter CreateItem()
		{
			return new MethodParameter(this, _method, true);
		}

		internal void Load(IBinaryAccessor accessor, int count, int[] paramRIDs, int ridIndex)
		{
			var image = _module.Image;

			_list.Capacity = count;

			for (int i = 0; i < count; i++, ridIndex++)
			{
				int paramRID = (ridIndex < paramRIDs.Length) ? paramRIDs[ridIndex] : 0;

				var parameter = new MethodParameter(this, _method, false);
				parameter.Load(accessor, paramRID);
				_list.Add(parameter);
			}
		}

		#endregion

		#region IReadOnlyList<IType>

		ITypeSignature IReadOnlyList<ITypeSignature>.this[int index]
		{
			get { return _list[index].Type; }
		}

		IEnumerator<ITypeSignature> IEnumerable<ITypeSignature>.GetEnumerator()
		{
			return new TypeEnumerator(_list);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Nested types

		private struct TypeEnumerator : IEnumerable<TypeSignature>, IEnumerator<TypeSignature>
		{
			#region Fields

			private int _count;
			private int _index;
			private List<MethodParameter> _list;

			#endregion

			#region Ctors

			internal TypeEnumerator(List<MethodParameter> list)
			{
				_list = list;
				_count = list.Count;
				_index = -1;
			}

			#endregion

			#region Properties

			public TypeSignature Current
			{
				get
				{
					if (_index < 0)
						return null;

					return _list[_index].Type;
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

			IEnumerator<TypeSignature> IEnumerable<TypeSignature>.GetEnumerator()
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
