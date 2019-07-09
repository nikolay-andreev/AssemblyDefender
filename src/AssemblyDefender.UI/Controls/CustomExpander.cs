using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AssemblyDefender.UI
{
	public class CustomExpander : Expander
	{
		static CustomExpander()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(CustomExpander),
				new FrameworkPropertyMetadata(typeof(CustomExpander)));
		}
	}
}
