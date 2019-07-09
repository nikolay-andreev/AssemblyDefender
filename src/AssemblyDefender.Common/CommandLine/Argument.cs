using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.CommandLine
{
	public abstract class Argument
	{
		private bool _required;
		private bool _atMostOnce;
		private bool _seenValue;

		/// <summary>
		/// Indicates that this argument is required. An error will be displayed if it is not present when parsing arguments.
		/// </summary>
		public bool Required
		{
			get { return _required; }
			set { _required = value; }
		}

		/// <summary>
		/// The argument is not required, but an error will be reported if it is specified more than once.
		/// </summary>
		public bool AtMostOnce
		{
			get { return _atMostOnce; }
			set { _atMostOnce = value; }
		}

		internal bool SeenValue
		{
			get { return _seenValue; }
			set { _seenValue = value; }
		}

		public abstract void SetValue(string value);
	}
}
