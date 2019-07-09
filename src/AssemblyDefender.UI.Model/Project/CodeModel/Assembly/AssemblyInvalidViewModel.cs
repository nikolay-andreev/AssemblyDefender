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
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public class AssemblyInvalidViewModel : NodeDetailsViewModel<AssemblyViewModel>
	{
		private string _errorMessage;

		public AssemblyInvalidViewModel(AssemblyViewModel parent, string errorMessage)
			: base(parent)
		{
			_errorMessage = errorMessage;
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string ErrorMessage
		{
			get { return _errorMessage; }
		}
	}
}
