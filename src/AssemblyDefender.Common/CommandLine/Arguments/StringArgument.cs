using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.CommandLine
{
	public class StringArgument : Argument
	{
		private string _value;

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override void SetValue(string value)
		{
			_value = value;
		}
	}
}
