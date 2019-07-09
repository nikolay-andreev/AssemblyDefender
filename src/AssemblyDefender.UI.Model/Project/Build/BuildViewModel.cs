using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using AssemblyDefender.UI;
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model.Project
{
	public class BuildViewModel : DialogViewModel
	{
		#region Fields

		private string _dialogTitle;
		private ViewModel _workspace;
		private ProjectShellViewModel _projectShellViewModel;

		#endregion

		#region Ctors

		public BuildViewModel(ProjectShellViewModel projectShell)
			: base(projectShell)
		{
			_projectShellViewModel = projectShell;
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string ProjectFilePath
		{
			get { return _projectShellViewModel.FilePath; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public AD.Project ProjectNode
		{
			get { return _projectShellViewModel.Project.ProjectNode; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string WindowTitle
		{
			get { return _dialogTitle; }
			set
			{
				_dialogTitle = value;
				OnPropertyChanged("WindowTitle");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ViewModel Workspace
		{
			get { return _workspace; }
			set { Show<ViewModel>(ref _workspace, value, "Workspace"); }
		}

		#endregion

		#region Methods

		private void ShowProgress()
		{
			WindowTitle = SR.BuildWindowCaption;
			Show(new BuildProgressViewModel(this));
		}

		public void ShowError(string message, string hint)
		{
			WindowTitle = SR.BuildErrorWindowCaption;
			Show(new BuildErrorViewModel(this, message, hint));
		}

		public void ShowError(BuildErrorLog error)
		{
			WindowTitle = SR.BuildErrorWindowCaption;
			
			if (string.IsNullOrEmpty(error.Hint))
			{
				Show(new BuildUnknownErrorViewModel(this, error));
			}
			else
			{
				Show(new BuildErrorViewModel(this, error.Message, error.Hint));
			}
		}

		public void ShowBuildError()
		{
			if (!ShowBuildErrorFromLog())
			{
				Show(new BuildErrorViewModel(this, Common.SR.InternalError));
			}
		}

		public bool ShowBuildErrorFromLog()
		{
			string projectFilePath = _projectShellViewModel.FilePath;
			if (string.IsNullOrEmpty(projectFilePath))
				return false;

			string logFilePath = Path.ChangeExtension(projectFilePath, BuildLog.FileExtension);
			if (!File.Exists(logFilePath))
				return false;

			var log = BuildLog.LoadFile(logFilePath);
			if (log == null)
				return false;

			if (log.Error == null)
				return false;

			ShowError(log.Error);
			return true;
		}

		private bool Show(ViewModel viewModel)
		{
			return Show(ref _workspace, viewModel, "Workspace");
		}

		protected override void OnActivate()
		{
			ShowProgress();
		}

		protected override void OnDeactivate()
		{
			if (_workspace != null)
			{
				_workspace.Deactivate();
			}
		}

		#endregion
	}
}
