using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model
{
	public class ActionViewModel : PropertyAwareObject
	{
		#region Fields

		private bool _isEnabled = true;
		private string _caption;
		private ImageType _image;
		private ICommand _command;
		private object _commandParameter;

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public virtual bool IsSeparator
		{
			get { return false; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				_isEnabled = value;
				OnPropertyChanged("IsEnabled");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Text
		{
			get { return _caption; }
			set
			{
				_caption = value;
				OnPropertyChanged("Text");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ImageType Image
		{
			get { return _image; }
			set
			{
				_image = value;
				OnPropertyChanged("Image");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand Command
		{
			get { return _command; }
			set
			{
				_command = value;
				OnPropertyChanged("Command");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public object CommandParameter
		{
			get { return _commandParameter; }
			set
			{
				_commandParameter = value;
				OnPropertyChanged("CommandParameter");
			}
		}

		#endregion
	}
}
