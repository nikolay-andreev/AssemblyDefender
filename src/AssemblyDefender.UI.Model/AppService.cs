using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model
{
	public static class AppService
	{
		#region Fields

		private static string _loadProjectFilePath;
		private static ShellViewModel _shell;
		private static IUIService _uiService;

		#endregion

		#region Properties

		public static ShellViewModel Shell
		{
			get { return _shell; }
		}

		public static IUIService UI
		{
			get { return _uiService; }
		}

		#endregion

		#region Methods

		public static void Run(IUIService uiService, string[] args)
		{
			_uiService = uiService;

#if (DEBUG != true)
			// Don't handle the exceptions in Debug mode because otherwise the Debugger wouldn't
			// jump into the code when an exception occurs.
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
#endif
			if (args.Length > 0)
			{
				_loadProjectFilePath = args[0];
			}

			_shell = new ShellViewModel();

			// Show shell
			UI.ShowShell();

			_uiService.BeginInvoke((Action)OnShellLoaded, null, DispatcherPriority.Background);
		}

		public static void AddProjectAssembly(string filePath)
		{
			var projectShell = _shell.Workspace as Project.ProjectShellViewModel;
			if (projectShell == null)
				return;

			projectShell.Project.AddAssembly(filePath);
		}

		internal static void GoWeb(string url)
		{
			Process.Start(url);
		}

		internal static void SelectFileInWindowsExplorer(string filePath)
		{
			string arguments;
			if (File.Exists(filePath))
			{
				arguments = string.Format("/select,\"{0}\"", filePath);
			}
			else
			{
				arguments = Path.GetDirectoryName(filePath);
			}

			Process.Start("explorer.exe", arguments);
		}

		internal static void Shutdown()
		{
			Shutdown(0);
		}

		internal static void Shutdown(int exitCode)
		{
			UI.Shutdown(exitCode);
		}

		internal static string GetExceptionMessage(Exception exception)
		{
			if (IsInternalError(exception))
				return Common.SR.InternalError;
			else
				return exception.Message;
		}

		private static bool IsInternalError(Exception exception)
		{
			if (exception == null)
				return true;

			if (string.IsNullOrEmpty(exception.Message))
				return true;

			if (exception is InvalidOperationException)
				return true;

			return false;
		}

		private static void OnShellLoaded()
		{
			// Load project
			if (!string.IsNullOrEmpty(_loadProjectFilePath))
			{
				_shell.ShowProject(_loadProjectFilePath);
			}
		}


		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exception = e.ExceptionObject as Exception;

			var errorViewModel = new ErrorViewModel(_shell, exception);
			_uiService.ShowError(errorViewModel);

			Environment.Exit(-1);
		}

		#endregion
	}
}
