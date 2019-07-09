using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public abstract class ToolViewModel : ViewModel
	{
		#region Fields

		private string _caption;
		private ICommand _closeCommand;
		private ProjectShellViewModel _projectShell;

		#endregion

		#region Ctors

		internal ToolViewModel(ProjectShellViewModel projectShell)
			: base(projectShell)
		{
			_projectShell = projectShell;
			_closeCommand = new DelegateCommand(Close);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Caption
		{
			get { return _caption; }
			set
			{
				_caption = value;
				OnPropertyChanged("Caption");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand CloseCommand
		{
			get { return _closeCommand; }
		}

		public ShellViewModel Shell
		{
			get { return _projectShell.Shell; }
		}

		public ProjectShellViewModel ProjectShell
		{
			get { return _projectShell; }
		}

		#endregion

		#region Methods

		public void Close()
		{
			_projectShell.SelectedTool = null;
		}

		#endregion
	}
}
