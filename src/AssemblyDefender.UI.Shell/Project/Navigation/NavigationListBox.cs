using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AssemblyDefender.UI.Model.Project;

namespace AssemblyDefender.UI.Shell.Project
{
	public class NavigationListBox : ListBox
	{
		private bool IsControlKeyDown
		{
			get
			{
				return ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
			}
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new NavigationListBoxItem();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (HandleKeyDown(e.Key))
				e.Handled = true;
			else
				base.OnKeyDown(e);
		}

		internal bool HandleKeyDown(Key key)
		{
			if (IsControlKeyDown)
				return false;

			var node = SelectedItem as NodeViewModel;
			if (node == null)
				return false;

			switch (key)
			{
				case Key.Up:
					NavigationUtilities.MoveUp(this, node);
					return true;

				case Key.Down:
					NavigationUtilities.MoveDown(this, node);
					return true;

				case Key.Left:
					NavigationUtilities.MoveLeft(this, node);
					return true;

				case Key.Right:
					NavigationUtilities.MoveRight(this, node);
					return true;

				default:
					return false;
			}
		}
	}
}
