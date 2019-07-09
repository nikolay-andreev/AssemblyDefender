using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public class BuildUnknownErrorViewModel : ViewModel
	{
		#region Fields

		private bool _isReportVisible;
		private string _message;
		private string _helpLink;
		private string _reportText;
		private BuildViewModel _buildViewModel;
		private ICommand _closeCommand;

		#endregion

		#region Ctors

		public BuildUnknownErrorViewModel(BuildViewModel buildViewModel, BuildErrorLog error)
			: base(buildViewModel)
		{
			_buildViewModel = buildViewModel;
			_message = error.Message;
			_helpLink = error.HelpLink;
			_reportText = error.ToString(true, true);
			_closeCommand = new DelegateCommand<bool>(Close);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsReportVisible
		{
			get { return _isReportVisible; }
			set
			{
				_isReportVisible = value;
				OnPropertyChanged("IsReportVisible");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Message
		{
			get { return _message; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool HasHelpLink
		{
			get { return !string.IsNullOrEmpty(_helpLink); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string HelpLink
		{
			get { return _helpLink; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string ReportText
		{
			get { return _reportText; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand CloseCommand
		{
			get { return _closeCommand; }
		}

		#endregion

		#region Methods

		private void Close(bool result)
		{
			_buildViewModel.Close(result);
		}

		#endregion
	}
}
