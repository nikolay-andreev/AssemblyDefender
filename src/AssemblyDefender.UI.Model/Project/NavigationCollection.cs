using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AssemblyDefender.UI.Model.Project
{
	public class NavigationCollection : IList<NodeViewModel>, INotifyCollectionChanged
	{
		#region Fields

		public event NotifyCollectionChangedEventHandler CollectionChanged;
		private List<NodeViewModel> _list = new List<NodeViewModel>();
		private bool _doNotTriggerEvents;

		#endregion

		#region Ctors

		internal NavigationCollection()
		{
		}

		#endregion

		#region Properties

		public NodeViewModel this[int index]
		{
			get { return _list[index]; }
			set
			{
				var oldItem = _list[index];
				_list[index] = value;

				if (!_doNotTriggerEvents)
				{
					OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, index);
				}
			}
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool DoNotTriggerEvents
		{
			get { return _doNotTriggerEvents; }
			set { _doNotTriggerEvents = value; }
		}

		#endregion

		#region Methods

		public void Add(NodeViewModel item)
		{
			_list.Add(item);

			if (!_doNotTriggerEvents)
			{
				OnCollectionChanged(NotifyCollectionChangedAction.Add, item, _list.Count - 1);
			}
		}

		public int IndexOf(NodeViewModel item)
		{
			return _list.IndexOf(item);
		}

		public bool Contains(NodeViewModel item)
		{
			return _list.Contains(item);
		}

		public void Insert(int index, NodeViewModel item)
		{
			_list.Insert(index, item);

			if (!_doNotTriggerEvents)
			{
				OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
			}
		}

		public void InsertRange(int index, List<NodeViewModel> items)
		{
			_list.InsertRange(index, items);

			if (!_doNotTriggerEvents)
			{
				if (items.Count > 100)
				{
					OnCollectionReset();
				}
				else
				{
					int currIndex = index;
					foreach (var item in items)
					{
						OnCollectionChanged(NotifyCollectionChangedAction.Add, item, currIndex++);
					}
				}
			}
		}

		public void RemoveAt(int index)
		{
			var item = _list[index];
			_list.RemoveAt(index);
			OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
		}

		public bool Remove(NodeViewModel item)
		{
			int index = IndexOf(item);
			if (index < 0)
				return false;

			RemoveAt(index);

			return true;
		}

		public void RemoveRange(int index, int count)
		{
			if (!_doNotTriggerEvents)
			{
				if (count > 100)
				{
					_list.RemoveRange(index, count);
					OnCollectionReset();
				}
				else
				{
					var items = new NodeViewModel[count];
					for (int i = 0; i < count; i++)
					{
						items[i] = _list[index + i];
					}

					_list.RemoveRange(index, count);

					for (int i = count - 1; i >= 0; i--)
					{
						OnCollectionChanged(NotifyCollectionChangedAction.Remove, items[i], index + i);
					}
				}
			}
			else
			{
				_list.RemoveRange(index, count);
			}
		}

		public void Clear()
		{
			_list.Clear();

			if (!_doNotTriggerEvents)
			{
				OnCollectionReset();
			}
		}

		public void CopyTo(NodeViewModel[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex);
		}

		public IEnumerator<NodeViewModel> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
		}

		internal void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
		}

		internal void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
		}

		internal void OnCollectionReset()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		internal void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
			{
				CollectionChanged(this, e);
			}
		}

		#endregion
	}
}
