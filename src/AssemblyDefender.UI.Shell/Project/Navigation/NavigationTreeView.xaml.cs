using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using AssemblyDefender.UI.Model;
using AssemblyDefender.UI.Model.Project;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Shell.Project
{
	/// <summary>
	/// Interaction logic for NavigationTreeView.xaml
	/// </summary>
	public partial class NavigationTreeView : UserControl
	{
		public NavigationTreeView()
		{
			InitializeComponent();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (App.Current != null)
			{
				App.Current.ScrollProjectNodeIntoView += OnScrollProjectNodeIntoView;
				e.Handled = true;
			}
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			if (App.Current != null)
			{
				App.Current.ScrollProjectNodeIntoView -= OnScrollProjectNodeIntoView;
				e.Handled = true;
			}
		}

		private void ImageAndTextPanel_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount > 1 && e.LeftButton == MouseButtonState.Pressed)
			{
				var item = WindowsUtils.FindVisualParent<NavigationListBoxItem>(e.OriginalSource as DependencyObject);
				if (item != null)
				{
					var viewModel = item.DataContext as NodeViewModel;
					if (viewModel != null)
					{
						viewModel.IsExpanded = !viewModel.IsExpanded;
					}
				}
			}
		}

		private void OnScrollProjectNodeIntoView(NodeViewModel node)
		{
			NavigationUtilities.ScrollIntoView(navListBox, node);
		}

		#region Drag & Drop

		private void Tree_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
			{
				e.Effects = DragDropEffects.Copy;
				e.Handled = true;
			}
		}

		private void Tree_Drop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop, true))
				return;

			var files = e.Data.GetData(DataFormats.FileDrop, true) as string[];
			if (files == null || files.Length == 0)
				return;

			foreach (var filePath in files)
			{
				AppService.AddProjectAssembly(filePath);
			}

			e.Handled = true;
		}

		#endregion
	}
}
