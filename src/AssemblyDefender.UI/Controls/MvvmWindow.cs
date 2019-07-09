using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AssemblyDefender.UI
{
	public class MvvmWindow : Window
	{
		protected override void OnClosing(CancelEventArgs e)
		{
			var viewModel = DataContext as ViewModel;
			if (viewModel != null)
			{
				e.Cancel = !viewModel.CanDeactivate();
			}

			base.OnClosing(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			var viewModel = DataContext as ViewModel;
			if (viewModel != null)
			{
				viewModel.Deactivate();
			}

			base.OnClosed(e);
		}
	}
}
