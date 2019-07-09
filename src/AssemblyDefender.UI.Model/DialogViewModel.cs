using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model
{
	public abstract class DialogViewModel : ViewModel
	{
		#region Fields

		private bool? _dialogResult;
		private ICommand _dialogCloseCommand;

		#endregion

		#region Ctors

		internal DialogViewModel(ViewModel parent)
			: base(parent)
		{
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool? DialogResult
		{
			get { return _dialogResult; }
			set
			{
				_dialogResult = value;
				OnPropertyChanged("DialogResult");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand DialogCloseCommand
		{
			get
			{
				if (_dialogCloseCommand == null)
				{
					_dialogCloseCommand = new DelegateCommand<bool>(Close);
				}

				return _dialogCloseCommand;
			}
		}

		#endregion

		#region Methods

		public virtual void Close(bool result)
		{
			DialogResult = result;
		}

		#endregion
	}
}
