using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model
{
	public abstract class NotificationViewModel : PropertyAwareObject
	{
		protected ImageType _image;
		protected ICommand _removeCommand;
		protected ShellViewModel _shell;

		protected NotificationViewModel(ShellViewModel shell)
		{
			_shell = shell;
			_removeCommand = new DelegateCommand(Remove);
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ImageType Image
		{
			get { return _image; }
			set { _image = value; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}

		public void Remove()
		{
			_shell.Notifications.Remove(this);
		}
	}
}
