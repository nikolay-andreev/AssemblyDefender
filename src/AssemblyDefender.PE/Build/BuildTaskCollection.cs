using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public class BuildTaskCollection : IEnumerable<BuildTask>
	{
		#region Fields

		private PEBuilder _builder;
		private List<BuildTask> _list = new List<BuildTask>();
		private Dictionary<Type, BuildTask> _taskByType = new Dictionary<Type, BuildTask>();

		#endregion

		#region Ctors

		internal BuildTaskCollection(PEBuilder builder)
		{
			_builder = builder;
		}

		#endregion

		#region Properties

		public BuildTask this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public T Get<T>(bool throwIfMissing = false)
			where T : BuildTask
		{
			BuildTask task;
			if (!_taskByType.TryGetValue(typeof(T), out task))
			{
				if (throwIfMissing)
				{
					throw new BuildException(string.Format(SR.BuildTaskNotFound, typeof(T).Name));
				}

				return null;
			}

			return (T)task;
		}

		public int Add(BuildTask item)
		{
			int priority = 1000;
			if (_list.Count > 0)
			{
				priority += _list[_list.Count - 1].Priority;
			}

			return Add(item, priority);
		}

		public int Add(BuildTask item, int priority)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item.Priority = priority;
			item.PE = _builder;

			_taskByType.Add(item.GetType(), item);

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

		public IEnumerator<BuildTask> GetEnumerator()
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
