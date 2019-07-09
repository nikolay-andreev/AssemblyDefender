using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model.Project
{
	public class SearchViewModel : ToolViewModel
	{
		#region Fields

		private bool _matchCase;
		private string _searchText;
		private bool _isChanged;
		private bool _isRunning;
		private bool _cancellationPending;
		private Action _searchAction;
		private Action _searchCompletedAction;
		private Action<List<SearchItem>> _addItemsAction;
		private SearchItem[] _allItems;
		private ObservableCollection<SearchItem> _items;
		private object _lockObject = new object();

		#endregion

		#region Ctors

		public SearchViewModel(ProjectShellViewModel projectShell)
			: base(projectShell)
		{
			Caption = SR.Search;

			_searchAction = (Action)OnSearch;
			_searchCompletedAction = (Action)OnSearchCompleted;
			_addItemsAction = (Action<List<SearchItem>>)AddItems;
			_items = new ObservableCollection<SearchItem>();

			ProjectShell.HierarchyChanged += OnHierarchyChanged;
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsLoading
		{
			get { return _isRunning; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool MatchCase
		{
			get { return _matchCase; }
			set
			{
				if (_matchCase != value)
				{
					_matchCase = value;
					OnPropertyChanged("MatchCase");
					BeginRun();
				}
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string SearchText
		{
			get { return _searchText; }
			set
			{
				if (_searchText != value)
				{
					_searchText = value;
					OnPropertyChanged("SearchText");
					BeginRun();
				}
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ObservableCollection<SearchItem> Items
		{
			get { return _items; }
		}

		#endregion

		#region Methods

		protected override void OnActivate()
		{
			BeginRun();
			base.OnActivate();
		}

		protected override void OnDeactivate()
		{
			if (_isRunning)
			{
				BeginCancel();
			}

			_allItems = null;
			_items.Clear();
			base.OnDeactivate();
		}

		private void BeginRun()
		{
			if (!IsActive)
				return;

			_isChanged = true;

			if (!_isRunning)
			{
				_isRunning = true;
				OnPropertyChanged("IsLoading");
				_searchAction.BeginInvoke(null, null);
			}
		}

		private void BeginCancel()
		{
			_cancellationPending = true;
		}

		private void OnSearch()
		{
			var allItems = _allItems;
			if (allItems == null)
			{
				lock (_lockObject)
				{
					if (_allItems == null)
					{
						LoadItems();
					}

					allItems = _allItems;
				}
			}

			while (_isChanged)
			{
				_isChanged = false;
				string searchText = _searchText;
				var comparison = _matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

				// Filter
				var list = new List<SearchItem>();
				if (!string.IsNullOrEmpty(searchText))
				{
					searchText = searchText.Trim();

					foreach (var item in allItems)
					{
						if (_isChanged || _cancellationPending)
							break;

						if (item.Match(searchText, comparison))
						{
							list.Add(item);
						}
					}
				}
				else
				{
					list.AddRange(allItems);
				}

				if (_cancellationPending)
					break;

				if (_isChanged)
					continue;

				// Add
				lock (_items)
				{
					AppService.UI.Invoke(
						_addItemsAction,
						new object[] { list });
				}
			}

			AppService.UI.Invoke(_searchCompletedAction);
		}

		private void OnSearchCompleted()
		{
			if (_cancellationPending)
			{
				_cancellationPending = false;
				_isRunning = false;
				_isChanged = false;
			}
			else
			{
				_isRunning = false;

				if (_isChanged)
				{
					BeginRun();
				}
			}

			OnPropertyChanged("IsLoading");
		}

		private void AddItems(List<SearchItem> list)
		{
			_items.Clear();

			for (int i = 0; i < list.Count; i++)
			{
				_items.Add(list[i]);
			}
		}

		private void LoadItems()
		{
			var items = new List<SearchItem>();

			// Collect all types
			CollectTypes(items);

			// Sort
			items.Sort(CompareItems);

			_allItems = items.ToArray();
		}

		private int CompareItems(SearchItem x, SearchItem y)
		{
			return StringLogicalComparer.Default.Compare(x.Caption, y.Caption);
		}

		private void CollectTypes(List<SearchItem> items)
		{
			var projectViewModel = ProjectShell.Project;
			for (int i = 0; i < projectViewModel.ChildCount; i++)
			{
				var assemblyViewModel = projectViewModel.GetChild(i) as AssemblyViewModel;
				if (assemblyViewModel == null)
					continue;

				var assembly = assemblyViewModel.Assembly;
				if (assembly == null)
					continue;

				foreach (var module in assembly.Modules)
				{
					CollectTypes(items, module.Types);
				}
			}
		}

		private void CollectTypes(List<SearchItem> items, TypeDeclarationCollection types)
		{
			foreach (var type in types)
			{
				items.Add(new SearchTypeItem(type));
				CollectTypes(items, type.NestedTypes);
			}
		}

		private void OnHierarchyChanged(object sender, EventArgs e)
		{
			if (!IsActive)
				return;

			if (_isRunning)
			{
				BeginCancel();
				_isRunning = false;
			}

			lock (_lockObject)
			{
				_allItems = null;
			}

			BeginRun();
		}

		#endregion
	}
}
