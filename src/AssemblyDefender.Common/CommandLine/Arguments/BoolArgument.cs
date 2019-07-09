using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.CommandLine
{
	public class BoolArgument : Argument
	{
		private bool _value;
		private bool _allowSign;

		public bool Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public bool AllowSign
		{
			get { return _allowSign; }
			set { _allowSign = value; }
		}

		public override void SetValue(string value)
		{
			throw new InvalidOperationException();
		}
	}
}
