using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AssemblyDefender.UI.Model;
using Microsoft.Win32;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Shell
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : System.Windows.Application, IUIService
	{
		private static App _current;
		internal event Action<Model.Project.NodeViewModel> ScrollProjectNodeIntoView;

		internal new static App Current
		{
			get { return _current; }
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			_current = this;
			AppService.Run(this, e.Args);
			base.OnStartup(e);
		}

		private Window Show<T>(ViewModel viewModel)
			where T : Window, new()
		{
			viewModel.Activate();

			var owner = MainWindow;

			var window = new T();
			window.DataContext = viewModel;

			if (owner != null)
				window.Owner = owner;

			window.Show();

			return window;
		}

		private bool? ShowDialog<T>(ViewModel viewModel)
			where T : Window, new()
		{
			viewModel.Activate();

			var owner = MainWindow;

			var window = new T();
			window.DataContext = viewModel;

			if (owner != null)
				window.Owner = owner;

			return window.ShowDialog();
		}

		void IUIService.ShowAppWait()
		{
			if (Mouse.OverrideCursor == null)
			{
				CursorChanger.ChangeAsync(Cursors.Wait, DispatcherPriority.Background);
			}
		}

		void IUIService.ShowShell()
		{
			Show<ShellWindow>(AppService.Shell);
		}

		string IUIService.ShowOpenFileDialog(string filter, string title, string initialDirectory)
		{
			var dialog = new OpenFileDialog();

			dialog.Multiselect = false;

			if (!string.IsNullOrEmpty(initialDirectory))
				dialog.InitialDirectory = initialDirectory;

			if (!string.IsNullOrEmpty(filter))
			{
				dialog.Filter = filter;
				dialog.FilterIndex = 1;
			}

			if (!string.IsNullOrEmpty(title))
				dialog.Title = title;
			else
				dialog.Title = Model.SR.DefaultWindowCaption;

			bool? result = dialog.ShowDialog(MainWindow);
			if (!result.HasValue || !result.Value)
				return null;

			return dialog.FileName;
		}

		string[] IUIService.ShowOpenFilesDialog(string filter, string title, string initialDirectory)
		{
			var dialog = new OpenFileDialog();

			dialog.Multiselect = true;

			if (!string.IsNullOrEmpty(initialDirectory))
				dialog.InitialDirectory = initialDirectory;

			if (!string.IsNullOrEmpty(filter))
			{
				dialog.Filter = filter;
				dialog.FilterIndex = 1;
			}

			if (!string.IsNullOrEmpty(title))
				dialog.Title = title;
			else
				dialog.Title = Model.SR.DefaultWindowCaption;

			bool? result = dialog.ShowDialog(MainWindow);
			if (!result.HasValue || !result.Value)
				return null;

			return dialog.FileNames;
		}

		string IUIService.ShowSaveFileDialog(string filter, string title, string initialDirectory)
		{
			var dialog = new SaveFileDialog();

			if (!string.IsNullOrEmpty(initialDirectory))
				dialog.InitialDirectory = initialDirectory;

			if (!string.IsNullOrEmpty(filter))
			{
				dialog.Filter = filter;
				dialog.FilterIndex = 1;
			}

			if (!string.IsNullOrEmpty(title))
				dialog.Title = title;
			else
				dialog.Title = Model.SR.DefaultWindowCaption;

			bool? result = dialog.ShowDialog(MainWindow);
			if (!result.HasValue || !result.Value)
				return null;

			return dialog.FileName;
		}

		string IUIService.ShowFolderBrowserDialog(string title)
		{
			var dialog = new FolderBrowserDialog();
			dialog.ShowNewFolderButton = true;
			dialog.ShowEditBox = true;
			dialog.ShowFullPathInEditBox = true;
			dialog.ShowBothFilesAndFolders = false;
			dialog.NewStyle = true;
			dialog.DontIncludeNetworkFoldersBelowDomainLevel = false;

			if (!string.IsNullOrEmpty(title))
				dialog.Title = title;
			else
				dialog.Title = Model.SR.DefaultWindowCaption;

			bool? result = dialog.ShowDialog(MainWindow);
			if (!result.HasValue || !result.Value)
				return null;

			return dialog.SelectedPath;
		}

		bool? IUIService.ShowMessageDialog(string message, MessageDialogType type, bool canCancel)
		{
			MessageBoxButton button;
			MessageBoxImage icon;

			switch (type)
			{
				case MessageDialogType.Information:
					button = MessageBoxButton.OK;
					icon = MessageBoxImage.Information;
					break;

				case MessageDialogType.Warning:
					button = MessageBoxButton.OK;
					icon = MessageBoxImage.Warning;
					break;

				case MessageDialogType.Error:
					button = MessageBoxButton.OK;
					icon = MessageBoxImage.Error;
					break;

				case MessageDialogType.Question:
					button = canCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo;
					icon = MessageBoxImage.Question;
					break;

				default:
					throw new InvalidOperationException();
			}

			MessageBoxResult result;
			if (MainWindow != null)
			{
				result = MessageBox.Show(
					MainWindow,
					message,
					Model.SR.DefaultWindowCaption,
					button,
					icon);
			}
			else
			{
				result = MessageBox.Show(
					message,
					Model.SR.DefaultWindowCaption,
					button,
					icon);
			}

			switch (result)
			{
				case MessageBoxResult.None:
					return null;

				case MessageBoxResult.OK:
					return true;

				case MessageBoxResult.Cancel:
					return null;

				case MessageBoxResult.Yes:
					return true;

				case MessageBoxResult.No:
					return false;

				default:
					throw new InvalidOperationException();
			}
		}

		void IUIService.ShowError(ErrorViewModel viewModel)
		{
			ShowDialog<ErrorWindow>(viewModel);
		}

		void IUIService.ShowAbout(AboutViewModel viewModel)
		{
			ShowDialog<AboutWindow>(viewModel);
		}

		void IUIService.ShowBuild(Model.Project.BuildViewModel viewModel)
		{
			ShowDialog<Project.BuildWindow>(viewModel);
		}

		void IUIService.ShowPKCS12Password(Model.Project.PKCS12PasswordViewModel viewModel)
		{
			ShowDialog<Project.PKCS12PasswordWindow>(viewModel);
		}

		void IUIService.ScrollProjectNodeIntoView(Model.Project.NodeViewModel viewModel)
		{
			var handler = ScrollProjectNodeIntoView;
			if (handler != null)
			{
				handler(viewModel);
			}
		}

		void IUIService.Invoke(Delegate method, object[] args, DispatcherPriority priority)
		{
			Dispatcher.Invoke(method, priority, args);
		}

		void IUIService.BeginInvoke(Delegate method, object[] args, DispatcherPriority priority)
		{
			Dispatcher.BeginInvoke(method, priority, args);
		}
	}
}
