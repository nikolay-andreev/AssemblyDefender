using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model
{
	public class ShellViewModel : ViewModel
	{
		#region Fields

		private string _dialogTitle;
		private ViewModel _workspace;
		private MRUListViewModel _projectMRUList;
		private ObservableCollection<NotificationViewModel> _notifications = new ObservableCollection<NotificationViewModel>();

		#endregion

		#region Ctors

		internal ShellViewModel()
			: base(null)
		{
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string WindowTitle
		{
			get { return _dialogTitle; }
			set
			{
				if (!string.IsNullOrEmpty(value))
					_dialogTitle = string.Format("{0} - {1}", value, SR.DefaultWindowCaption);
				else
					_dialogTitle = SR.DefaultWindowCaption;

				OnPropertyChanged("WindowTitle");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ViewModel Workspace
		{
			get { return _workspace; }
			set { Show<ViewModel>(ref _workspace, value, "Workspace"); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public MRUListViewModel ProjectMRUList
		{
			get { return _projectMRUList; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ObservableCollection<NotificationViewModel> Notifications
		{
			get { return _notifications; }
		}

		#endregion

		#region Methods

		public bool ShowProject(string filePath)
		{
			var viewModel = Project.ProjectShellViewModel.LaodFile(filePath, this);
			if (viewModel == null)
				return false;

			return Show(viewModel);
		}

		public bool ShowNewProject()
		{
			var viewModel = Project.ProjectShellViewModel.CreateNew(this);
			return Show(viewModel);
		}

		public bool Show(ViewModel viewModel)
		{
			return Show(ref _workspace, viewModel, "Workspace");
		}

		public void New()
		{
			ShowNewProject();
		}

		public void Open()
		{
			string filePath = AppService.UI.ShowOpenFileDialog(
				Constants.ProjectFileFilter,
				SR.OpenProjectFileCaption);

			if (string.IsNullOrEmpty(filePath))
				return;

			ShowProject(filePath);
		}

		public void ShowStartPage()
		{
			Show(new StartPage.StartPageViewModel(this));
		}

		public void ShowAbout()
		{
			AppService.UI.ShowAbout(new AboutViewModel(this));
		}

		public void RemoveNotificationOfType(Type type)
		{
			for (int i = Notifications.Count - 1; i >= 0; i--)
			{
				var notification = Notifications[i];
				if (notification.GetType() == type)
				{
					Notifications.RemoveAt(i);
					break;
				}
			}
		}

		public override bool CanDeactivate()
		{
			if (_workspace != null)
			{
				return _workspace.CanDeactivate();
			}

			return true;
		}

		protected override void OnActivate()
		{
			string projectMRUKey = string.Format("{0}\\{1}", Constants.CurrentUserRegistryPath, "ProjectMRUList");
			_projectMRUList = new MRUListViewModel(this, projectMRUKey, 10);

			AttachEvents();

			ShowStartPage();

			base.OnActivate();
		}

		protected override void OnDeactivate()
		{
			if (_workspace != null)
			{
				_workspace.Deactivate();
			}

			_projectMRUList = null;

			base.OnDeactivate();
		}

		private void AttachEvents()
		{
			Commands.Exit.Subscribe(AppService.Shutdown);
			Commands.New.Subscribe(New);
			Commands.Open.Subscribe(Open);
			Commands.ViewStartPage.Subscribe(ShowStartPage);
			Commands.GoWeb.Subscribe(AppService.GoWeb);
			Commands.About.Subscribe(ShowAbout);
		}

		#endregion
	}
}
