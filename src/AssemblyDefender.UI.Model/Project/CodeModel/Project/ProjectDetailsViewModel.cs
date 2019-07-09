using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.UI;
using AssemblyDefender.Common;
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model.Project
{
	public class ProjectDetailsViewModel : NodeDetailsViewModel<ProjectViewModel>
	{
		#region Fields

		private bool _hasBuildLog;
		private string _filePath;
		private DateTime _createdDate;
		private DateTime _lastModifiedDate;
		private DateTime _lastBuildDate;
		private AD.Project _project;
		private ICommand _expandAllCommand;
		private ObservableCollection<ProjectAssemblyViewModel> _assemblies = new ObservableCollection<ProjectAssemblyViewModel>();

		#endregion

		#region Ctors

		public ProjectDetailsViewModel(ProjectViewModel parent)
			: base(parent)
		{
			_project = Node.ProjectNode;

			var projectShell = ProjectShell;
			_filePath = projectShell.FilePath;
			_createdDate = _project.CreatedDate;
			_lastModifiedDate = _project.LastModifiedDate;
			_expandAllCommand = new DelegateCommand<bool>(ExpandAll);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsNew
		{
			get { return string.IsNullOrEmpty(_filePath); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool HasBuildLog
		{
			get { return _hasBuildLog; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool HasAssemblies
		{
			get { return _assemblies.Count > 0; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string FilePath
		{
			get { return _filePath; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public DateTime CreatedDate
		{
			get { return _createdDate; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public DateTime LastModifiedDate
		{
			get { return _lastModifiedDate; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public DateTime LastBuildDate
		{
			get { return _lastBuildDate; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand ExpandAllCommand
		{
			get { return _expandAllCommand; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ObservableCollection<ProjectAssemblyViewModel> Assemblies
		{
			get { return _assemblies; }
		}

		#endregion

		#region Methods

		protected override void OnActivate()
		{
			var projectViewModel = Parent;
			projectViewModel.AssemblyAdded += OnAssemblyAdded;
			projectViewModel.AssemblyRemoved += OnAssemblyRemoved;

			Load();
			LoadBuildLog();
		}

		protected override void OnDeactivate()
		{
			var projectViewModel = Parent;
			projectViewModel.AssemblyAdded -= OnAssemblyAdded;
			projectViewModel.AssemblyRemoved -= OnAssemblyRemoved;
		}

		internal void Open(ProjectAssemblyViewModel assemblyViewModel)
		{
			var assemblyViewModel2 = Parent.FindAssembly(assemblyViewModel.Assembly);
			if (assemblyViewModel2 == null)
				return;

			assemblyViewModel2.Show();
		}

		internal void Remove(ProjectAssemblyViewModel assemblyViewModel)
		{
			if (!Parent.RemoveAssembly(assemblyViewModel.Assembly, true))
				return;

			_assemblies.Remove(assemblyViewModel);
		}

		private void Load()
		{
			var projectViewModel = Parent;

			for (int i = 0; i < projectViewModel.ChildCount; i++)
			{
				var assemblyViewModel = projectViewModel.GetChild(i) as AssemblyViewModel;
				if (assemblyViewModel == null)
					continue;

				if (assemblyViewModel.Assembly == null)
					continue;

				_assemblies.Add(
					new ProjectAssemblyViewModel(
							assemblyViewModel.Assembly,
							assemblyViewModel.ProjectAssembly,
							this));
			}

			OnPropertyChanged("HasAssemblies");
		}

		private void LoadBuildLog()
		{
			if (string.IsNullOrEmpty(_filePath))
				return;

			string logFilePath = Path.ChangeExtension(_filePath, BuildLog.FileExtension);
			if (!File.Exists(logFilePath))
				return;

			var log = BuildLog.LoadFile(logFilePath);
			if (log == null)
				return;

			_hasBuildLog = true;
			_lastBuildDate = log.BuildDate;
		}

		private void ExpandAll(bool isExpanded)
		{
			foreach (var assembly in _assemblies)
			{
				assembly.IsExpanded = isExpanded;
			}
		}

		private void OnAssemblyAdded(object sender, DataEventArgs<Assembly> e)
		{
			_assemblies.Clear();
			Load();
		}

		private void OnAssemblyRemoved(object sender, DataEventArgs<Assembly> e)
		{
			var assembly = e.Data;
			if (assembly == null)
				return;

			for (int i = _assemblies.Count - 1; i >= 0; i--)
			{
				var assemblyViewModel = _assemblies[i];
				if (assemblyViewModel.Assembly == assembly)
				{
					_assemblies.RemoveAt(i);
					OnPropertyChanged("HasAssemblies");
					break;
				}
			}
		}

		#endregion
	}
}
