using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace AssemblyDefender.UI
{
	public class BoolValueExtension : MarkupExtension
	{
		private readonly bool _value;

		public BoolValueExtension(bool value)
		{
			_value = value;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return _value;
		}
	}
}
