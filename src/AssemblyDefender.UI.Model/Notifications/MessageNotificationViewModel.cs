using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model
{
	public class MessageNotificationViewModel : NotificationViewModel
	{
		private string _message;

		public MessageNotificationViewModel(ShellViewModel shell)
			: base(shell)
		{
			_image = ImageType.Info;
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Message
		{
			get { return _message; }
			set
			{
				_message = value;
				OnPropertyChanged("Message");
			}
		}
	}
}
