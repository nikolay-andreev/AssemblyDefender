using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using AssemblyDefender.Common;

namespace AssemblyDefender.UI.Model
{
	public class ErrorViewModel : DialogViewModel
	{
		#region Fields

		private bool _isReportVisible;
		private string _message;
		private string _helpLink;
		private string _reportText;

		#endregion

		#region Ctors

		public ErrorViewModel(ShellViewModel shell, Exception exception)
			: base(shell)
		{
			if (string.IsNullOrEmpty(exception.Message) && exception is InvalidOperationException)
				_message = Common.SR.InternalError;
			else
				_message = exception.Message;

			_helpLink = exception.HelpLink;
			_reportText = CoreUtils.Print(exception, true, true);
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

		#endregion

		#region Methods

		public override void Close(bool result)
		{
			base.Close(result);
		}

		#endregion
	}
}
