using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public class BuildFixupCollection : IEnumerable<BuildFixup>
	{
		#region Fields

		private PEBuilder _builder;
		private List<BuildFixup> _list = new List<BuildFixup>();

		#endregion

		#region Ctors

		internal BuildFixupCollection(PEBuilder builder)
		{
			_builder = builder;
		}

		#endregion

		#region Properties

		public BuildFixup this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public int Add(BuildFixup item)
		{
			int priority = 1000;
			if (_list.Count > 0)
				priority += _list[_list.Count - 1].Priority;

			return Add(item, priority);
		}

		public int Add(BuildFixup item, int priority)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item.Priority = priority;
			item.PE = _builder;

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

		public IEnumerator<BuildFixup> GetEnumerator()
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
