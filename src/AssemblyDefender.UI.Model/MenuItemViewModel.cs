using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model
{
	public class MenuItemViewModel : PropertyAwareObject
	{
		#region Fields

		private bool _isSeparator;
		private bool _isEnabled = true;
		private string _text;
		private string _toolTip;
		private ImageType _image;
		private ICommand _command;
		private object _commandParameter;
		private List<MenuItemViewModel> _items;

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsSeparator
		{
			get { return _isSeparator; }
			set
			{
				_isSeparator = value;
				OnPropertyChanged("IsSeparator");
			}
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
			get { return _text; }
			set
			{
				_text = value;
				OnPropertyChanged("Text");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string ToolTip
		{
			get { return _toolTip; }
			set
			{
				_toolTip = value;
				OnPropertyChanged("ToolTip");
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

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public List<MenuItemViewModel> Items
		{
			get
			{
				if (_items == null)
				{
					_items = new List<MenuItemViewModel>();
				}

				return _items;
			}
		}

		#endregion
	}
}
