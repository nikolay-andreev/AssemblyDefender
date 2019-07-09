using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.UI;
using AssemblyDefender.Common;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model
{
	public class MRUListViewModel : PropertyAwareObject
	{
		#region Fields

		private ICommand _openCommand;
		private ICommand _clearCommand;
		private ShellViewModel _shell;
		private MRUList _list;
		private ObservableCollection<ActionViewModel> _items;

		#endregion

		#region Ctors

		public MRUListViewModel(ShellViewModel shell, string registryPath, int capacity)
		{
			_shell = shell;
			_list = new MRUList(registryPath, capacity);
			_items = new ObservableCollection<ActionViewModel>();
			_openCommand = new DelegateCommand<int>(Open);
			_clearCommand = new DelegateCommand(Clear);

			Load();
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool HasItems
		{
			get { return _items.Count > 0; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand ClearCommand
		{
			get { return _clearCommand; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ObservableCollection<ActionViewModel> Items
		{
			get { return _items; }
		}

		#endregion

		#region Methods

		public void Add(string item)
		{
			int count = _list.Count;

			_list.Add(item);

			_items.Clear();

			Load();

			if (count == 0)
			{
				OnPropertyChanged("HasItems");
			}
		}

		private void Open(int index)
		{
			_shell.ShowProject(_list[index]);
		}

		private void Clear()
		{
			_list.Clear();
			_items.Clear();
			OnPropertyChanged("HasItems");
		}

		private void Load()
		{
			if (_list.Count == 0)
				return;

			for (int i = 0; i < _list.Count; i++)
			{
				_items.Add(
					new ActionViewModel()
					{
						Text = _list[i],
						Command = _openCommand,
						CommandParameter = i,
					});
			}

			_items.Add(new ActionSeparatorViewModel());

			_items.Add(
				new ActionViewModel()
				{
					Text = SR.ClearListBrace,
					Command = _clearCommand,
				});
		}

		#endregion
	}
}
