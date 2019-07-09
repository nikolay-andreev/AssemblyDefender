using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class CustomAttributeNamedArgumentCollection : CodeNode, IEnumerable<CustomAttributeNamedArgument>
	{
		#region Fields

		private List<CustomAttributeNamedArgument> _list = new List<CustomAttributeNamedArgument>();

		#endregion

		#region Ctors

		internal CustomAttributeNamedArgumentCollection(CodeNode parent)
			: base(parent)
		{
			if (parent.IsNew)
			{
				IsNew = true;
			}
		}

		#endregion

		#region Properties

		public CustomAttributeNamedArgument this[int index]
		{
			get { return _list[index]; }
			set
			{
				_list[index] = value;
				OnChanged();
			}
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int IndexOf(CustomAttributeNamedArgument item)
		{
			return _list.IndexOf(item);
		}

		public void Add(CustomAttributeNamedArgument item)
		{
			_list.Add(item);
			OnChanged();
		}

		private void Insert(int index, CustomAttributeNamedArgument item)
		{
			_list.Insert(index, item);
			OnChanged();
		}

		public bool Remove(CustomAttributeNamedArgument item)
		{
			if (!_list.Remove(item))
				return false;

			OnChanged();
			return true;
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
			OnChanged();
		}

		public void Clear()
		{
			_list.Clear();
			OnChanged();
		}

		public void CopyTo(CustomAttributeNamedArgumentCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		public IEnumerator<CustomAttributeNamedArgument> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal void Load(IBinaryAccessor accessor)
		{
			int count = accessor.ReadUInt16();
			Load(accessor, count);
		}

		internal void Load(IBinaryAccessor accessor, int count)
		{
			for (int i = 0; i < count; i++)
			{
				var argument = CustomAttributeNamedArgument.Load(accessor, _module);
				_list.Add(argument);
			}
		}

		#endregion
	}
}
