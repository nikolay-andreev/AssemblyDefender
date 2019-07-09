using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AssemblyDefender.UI.Model.Project;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Shell.Project
{
	/// <summary>
	/// Interaction logic for SearchView.xaml
	/// </summary>
	public partial class SearchView : UserControl
	{
		public SearchView()
		{
			InitializeComponent();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			searchTextBox.Focus();
			e.Handled = true;
		}

		public void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
		{
			var listItem = WindowsUtils.FindVisualParent<ListBoxItem>(e.OriginalSource as DependencyObject);
			if (listItem == null)
				return;

			var searchViewModel = listBox.DataContext as SearchViewModel;
			if (searchViewModel == null)
				return;

			var searchItem = listItem.DataContext as SearchItem;
			if (searchItem == null)
				return;

			searchItem.Show(searchViewModel.ProjectShell.Project);
			e.Handled = true;
		}
	}
}
