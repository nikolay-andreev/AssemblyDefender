using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AssemblyDefender.UI.Shell.Project
{
	public class NavigationListBoxItem : ListBoxItem
	{
		private NavigationListBox ParentListBox
		{
			get
			{
				return ParentSelector as NavigationListBox;
			}
		}

		private Selector ParentSelector
		{
			get
			{
				return (ItemsControl.ItemsControlFromItemContainer(this) as Selector);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			var listBox = ParentListBox;

			if (listBox.HandleKeyDown(e.Key))
				e.Handled = true;
			else
				base.OnKeyDown(e);
		}
	}
}
