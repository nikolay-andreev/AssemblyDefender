using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AD = AssemblyDefender;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public class ProjectAssemblyViewModel : ElementViewModel<ProjectDetailsViewModel, ProjectViewModel>
	{
		#region Fields

		private bool _isExpanded;
		private string _filePath;
		private string _outputFilePath;
		private DateTime _createdDate;
		private DateTime _lastModifiedDate;
		private long _fileSize;
		private Assembly _assembly;
		private AD.ProjectAssembly _projectAssembly;
		private ICommand _openCommand;
		private ICommand _removeCommand;

		#endregion

		#region Ctors

		public ProjectAssemblyViewModel(Assembly assembly, AD.ProjectAssembly projectAssembly, ProjectDetailsViewModel parent)
			: base(parent)
		{
			_assembly = assembly;
			_projectAssembly = projectAssembly;
			_filePath = _projectAssembly.FilePath;
			_openCommand = new DelegateCommand(Open);
			_removeCommand = new DelegateCommand(Remove);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				_isExpanded = value;

				OnPropertyChanged("IsExpanded");

				if (_isExpanded)
				{
					Load();
				}
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Name
		{
			get { return _assembly.Name; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string FilePath
		{
			get { return _filePath; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string OutputFilePath
		{
			get { return _outputFilePath; }
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
		public long FileSize
		{
			get { return _fileSize; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenameMembers
		{
			get { return _projectAssembly.RenameMembers; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateControlFlow
		{
			get { return _projectAssembly.ObfuscateControlFlow; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool EncryptIL
		{
			get { return _projectAssembly.EncryptIL; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateResources
		{
			get { return _projectAssembly.ObfuscateResources; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateStrings
		{
			get { return _projectAssembly.ObfuscateStrings; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RemoveUnusedMembers
		{
			get { return _projectAssembly.RemoveUnusedMembers; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool SealTypes
		{
			get { return _projectAssembly.SealTypes; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool DevirtualizeMethods
		{
			get { return _projectAssembly.DevirtualizeMethods; }
		}

		public Assembly Assembly
		{
			get { return _assembly; }
		}

		public AD.ProjectAssembly ProjectAssembly
		{
			get { return _projectAssembly; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand OpenCommand
		{
			get { return _openCommand; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		#endregion

		#region Methods

		public void Open()
		{
			Parent.Open(this);
		}

		public void Remove()
		{
			Parent.Remove(this);
		}

		private void Load()
		{
			_outputFilePath = ProjectUtils.GetAssemblyOutputFilePath(_projectAssembly);

			if (File.Exists(_filePath))
			{
				var fileInfo = new FileInfo(_filePath);
				_createdDate = fileInfo.CreationTime;
				_lastModifiedDate = fileInfo.LastWriteTime;
				_fileSize = fileInfo.Length;
			}

			OnPropertyChanged("OutputFilePath");
			OnPropertyChanged("CreatedDate");
			OnPropertyChanged("LastModifiedDate");
			OnPropertyChanged("FileSize");
		}

		#endregion
	}
}
