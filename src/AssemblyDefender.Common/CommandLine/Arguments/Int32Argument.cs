using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.CommandLine
{
	public class Int32Argument : Argument
	{
		private int _value;

		public int Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override void SetValue(string value)
		{
			_value = int.Parse(value);
		}
	}
}
