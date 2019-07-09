using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.Collections
{
	/// <summary>
	/// Priority queue implementation.
	/// </summary>
	/// <typeparam name="P">Specifies the type of priority in the queue.</typeparam>
	/// <typeparam name="V">Specifies the type of elements in the queue.</typeparam>
	public class PriorityQueue<P, V>
	{
		#region Fields

		private int _count;
		private SortedDictionary<P, Queue<V>> _list;

		#endregion

		#region Ctors

		public PriorityQueue()
		{
			_list = new SortedDictionary<P, Queue<V>>();
		}

		public PriorityQueue(IComparer<P> comparer)
		{
			_list = new SortedDictionary<P, Queue<V>>(comparer);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of elements contained in the Queue.
		/// </summary>
		public int Count
		{
			get { return _count; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds an object to the end of the queue.
		/// </summary>
		/// <param name="priority">The priority of object added to the queue.</param>
		/// <param name="value">The object to add to the queue. The value can be null for reference types.</param>
		public void Enqueue(P priority, V value)
		{
			Queue<V> queue;
			if (!_list.TryGetValue(priority, out queue))
			{
				queue = new Queue<V>();
				_list.Add(priority, queue);
			}

			queue.Enqueue(value);
			_count++;
		}

		/// <summary>
		/// Removes and returns the object at the beginning of the queue.
		/// </summary>
		/// <returns>The object that is removed from the beginning of the queue.</returns>
		public V Dequeue()
		{
			// Will throw if there isn’t any first element!
			var pair = _list.First();

			var value = pair.Value.Dequeue();
			if (pair.Value.Count == 0)
			{
				// Nothing left of the top priority.
				_list.Remove(pair.Key);
			}

			_count--;

			return value;
		}

		/// <summary>
		/// Returns the object at the beginning of the queue without removing it.
		/// </summary>
		/// <returns>The object at the beginning of the queue.</returns>
		public V Peek()
		{
			return _list.First().Value.Peek();
		}

		/// <summary>
		/// Removes all objects from the queue.
		/// </summary>
		public void Clear()
		{
			_list.Clear();
		}

		#endregion
	}
}
