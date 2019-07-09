using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace AssemblyDefender.UI.Model
{
	public interface IUIService
	{
		void ShowAppWait();

		void ShowShell();

		string ShowOpenFileDialog(string filter = null, string title = null, string initialDirectory = null);

		string[] ShowOpenFilesDialog(string filter = null, string title = null, string initialDirectory = null);

		string ShowSaveFileDialog(string filter = null, string title = null, string initialDirectory = null);

		string ShowFolderBrowserDialog(string title = null);

		bool? ShowMessageDialog(string message, MessageDialogType type = MessageDialogType.Information, bool canCancel = false);

		void ShowError(ErrorViewModel viewModel);

		void ShowAbout(AboutViewModel viewModel);

		void ShowBuild(Project.BuildViewModel viewModel);

		void ShowPKCS12Password(Project.PKCS12PasswordViewModel viewModel);

		void ScrollProjectNodeIntoView(Project.NodeViewModel viewModel);

		void Invoke(Delegate method, object[] args = null, DispatcherPriority priority = DispatcherPriority.Normal);

		void BeginInvoke(Delegate method, object[] args = null, DispatcherPriority priority = DispatcherPriority.Normal);

		void Shutdown(int exitCode);
	}
}
