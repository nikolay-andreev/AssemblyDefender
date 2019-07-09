using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace AssemblyDefender.UI
{
	public class TreeViewToggleButton : ToggleButton
	{
		static TreeViewToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TreeViewToggleButton),
				new FrameworkPropertyMetadata(typeof(TreeViewToggleButton)));
		}
	}
}
