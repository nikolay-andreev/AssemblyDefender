using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public class BuildBlobCollection : IEnumerable<BuildBlob>
	{
		#region Fields

		private List<BuildBlob> _list = new List<BuildBlob>();
		private BuildSection _section;

		#endregion

		#region Ctors

		internal BuildBlobCollection(BuildSection section)
		{
			_section = section;
		}

		#endregion

		#region Properties

		public BuildBlob this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int Add(BuildBlob item)
		{
			int priority = 1000;
			if (_list.Count > 0)
				priority += _list[_list.Count - 1].Priority;

			return Add(item, priority);
		}

		public int Add(BuildBlob item, int priority)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item.Attach(_section);
			item.Priority = priority;

			for (int i = _list.Count - 1; i >= 0; i--)
			{
				if (_list[i].Priority <= priority)
				{
					_list.Insert(i + 1, item);
					return i;
				}
			}

			_list.Insert(0, item);
			return 0;
		}

		public IEnumerator<BuildBlob> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
