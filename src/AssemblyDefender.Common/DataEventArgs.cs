using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common
{
	/// <summary>
	/// Generic arguments class to pass to event handlers that need to receive data.
	/// </summary>
	/// <typeparam name="TData">The type of data to pass.</typeparam>
	public class DataEventArgs<TData> : EventArgs
	{
		private TData _data;

		/// <summary>
		/// Initializes the DataEventArgs class.
		/// </summary>
		/// <param name="data">Information related to the event.</param>
		/// <exception cref="ArgumentNullException">The data is null.</exception>
		public DataEventArgs(TData data)
		{
			_data = data;
		}

		/// <summary>
		/// Gets the information related to the event.
		/// </summary>
		public TData Data
		{
			get { return _data; }
		}

		/// <summary>
		/// Provides a string representation of the argument data.
		/// </summary>
		public override string ToString()
		{
			return _data.ToString();
		}
	}
}
