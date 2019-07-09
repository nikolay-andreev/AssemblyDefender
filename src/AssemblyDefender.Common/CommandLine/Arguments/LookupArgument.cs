using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.CommandLine
{
	public class LookupArgument<T> : Argument
	{
		private T _value;
		private bool _ignoreCase = true;
		private KeyValuePair<string, T>[] _lookups;

		public T Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public bool IgnoreCase
		{
			get { return _ignoreCase; }
			set { _ignoreCase = value; }
		}

		public KeyValuePair<string, T>[] Lookups
		{
			get { return _lookups; }
			set { _lookups = value; }
		}

		public override void SetValue(string value)
		{
			_value = _lookups.First(kvp => 0 == string.Compare(kvp.Key, value, _ignoreCase)).Value;
		}
	}
}
