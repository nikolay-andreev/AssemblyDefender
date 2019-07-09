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
	public class BuildErrorViewModel : ViewModel
	{
		#region Fields

		private string _message;
		private string _hint;
		private BuildViewModel _buildViewModel;

		#endregion

		#region Ctors

		public BuildErrorViewModel(BuildViewModel buildViewModel, string message, string hint = null)
			: base(buildViewModel)
		{
			_buildViewModel = buildViewModel;
			_message = message;
			_hint = hint;
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Message
		{
			get { return _message; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool HasHint
		{
			get { return !string.IsNullOrEmpty(_hint); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Hint
		{
			get { return _hint; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand CloseCommand
		{
			get { return _buildViewModel.DialogCloseCommand; }
		}

		#endregion
	}
}
