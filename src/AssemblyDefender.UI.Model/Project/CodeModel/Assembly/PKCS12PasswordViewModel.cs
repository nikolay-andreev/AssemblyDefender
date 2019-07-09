using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.Net;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public class PKCS12PasswordViewModel : DialogViewModel
	{
		#region Fields

		private string _filePath;
		private string _password;

		#endregion

		#region Ctors

		internal PKCS12PasswordViewModel(ShellViewModel shell, string filePath)
			: base(shell)
		{
			_filePath = filePath;
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string FilePath
		{
			get { return _filePath; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Password
		{
			get { return _password; }
			set
			{
				_password = value;
				OnPropertyChanged("Password");
			}
		}

		#endregion

		#region Methods

		public override void Close(bool result)
		{
			if (result)
			{
				if (!IsPasswordValid())
				{
					AppService.UI.ShowMessageDialog(SR.PasswordNotValid, MessageDialogType.Error);
					return;
				}
			}

			base.Close(result);
		}

		private bool IsPasswordValid()
		{
			if (string.IsNullOrEmpty(_password))
				return false;

			if (!File.Exists(_filePath))
				return false;

			if (!StrongNameUtils.IsPasswordValid(_filePath, _password))
				return false;

			return true;
		}

		#endregion
	}
}
