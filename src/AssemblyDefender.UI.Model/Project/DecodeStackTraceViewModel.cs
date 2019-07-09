using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;
using AD = AssemblyDefender;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public class DecodeStackTraceViewModel : ToolViewModel
	{
		#region Fields

		private string _text;
		private ICommand _decodeCommand;
		private BuildLog _buildLog;

		#endregion

		#region Ctors

		public DecodeStackTraceViewModel(ProjectShellViewModel projectShell)
			: base(projectShell)
		{
			Caption = SR.DecodeStackTrace;
			_decodeCommand = new DelegateCommand(Decode);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;
				OnPropertyChanged("Text");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand DecodeCommand
		{
			get { return _decodeCommand; }
		}

		#endregion

		#region Methods

		private void Decode()
		{
			if (string.IsNullOrEmpty(_text))
				return;

			if (_buildLog == null && !LoadBuildLog())
				return;

			Text = AssemblyDefenderUtils.DecodeStackTrace(_buildLog, _text);
		}

		private bool LoadBuildLog()
		{
			var projectShell = FindParent<ProjectShellViewModel>(true);
			if (projectShell.IsNew)
				return false;

			string filePath = Path.ChangeExtension(projectShell.FilePath, BuildLog.FileExtension);

			if (File.Exists(filePath))
			{
				_buildLog = BuildLog.LoadFile(filePath);
			}

			if (_buildLog == null)
			{
				AppService.UI.ShowMessageDialog(
					string.Format(AssemblyDefender.SR.LogFileNotValid, filePath),
					MessageDialogType.Error);
				return false;
			}

			return true;
		}

		#endregion
	}
}
